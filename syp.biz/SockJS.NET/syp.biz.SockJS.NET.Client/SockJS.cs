using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Common;
using syp.biz.SockJS.NET.Common.DTO;
using syp.biz.SockJS.NET.Common.Exceptions;
using syp.biz.SockJS.NET.Common.Extensions;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client
{
    public class SockJS : EventTarget
    {
        private CancellationTokenSource _transportTimeoutCancellationToken;
        private ReadyState _readyState;

        public SockJS(string url, ICollection<string> protocols = null, Options options = null)
        {
            this.ReadyState = ReadyState.CONNECTING;
            this.Extensions = string.Empty;
            this.Protocol = string.Empty;
            if (options is null) options = new Options();
            this.TransportsWhitelist = options.Transports;
            this.TransportOptions = options.TransportOptions ?? new Dictionary<string, ITransportOptions>();
            this.GenerateSessionId = options.SessionIdGenerator ?? DefaultSessionIdGenerator;
            this.Server = options.Server ?? Common.Utils.Random.GetNumberString(1000);
            if (protocols is null) protocols = Array.Empty<string>();

            ParseAndValidateUrl(ref url);
            CheckPortAccess(ref protocols);
            CheckProtocols(ref protocols);
            ConvertOrigin();
            RemoveTrailingSlash(ref url);
            this.Url = url;
            this.StartConnectionInBackground();
        }

        public static string Version { get; } = typeof(SockJS).Assembly.GetName().Version.ToString(3);

        private ReadyState ReadyState
        {
            get => this._readyState;
            set
            {
                var previous = this._readyState;
                this._readyState = value;
                if (previous != value) this.DispatchEvent(new Event.Event("stateChanged"), previous, value);
            }
        }
        private string Extensions { get; set; }
        private string Protocol { get; set; }
        private string[] TransportsWhitelist { get; }
        private IDictionary<string, ITransportOptions> TransportOptions { get; }
        private Func<string> GenerateSessionId { get; }
        private string Server { get; }
        private string Url { get; }
        private InfoReceiver Ir { get; set; }
        private long RoundTripTimeout { get; set; }
        private string TransportUrl { get; set; }
        private ITransport Transport { get; set; }
        private ConcurrentQueue<ITransportFactory> Transports { get; set; }

        public static void SetLogger(ILogger logger) => Log.Logger = logger;

        public void Send(string data)
        {
            // NOT RUNNING IN BROWSER
            //#13 - convert anything non-string to string
            //TODO this currently turns objects into [object Object]
            //if (typeof data !== 'string') {
            //    data = '' + data;
            //}

            if (this.ReadyState == ReadyState.CONNECTING) throw new InvalidStateException("The connection has not been established yet");

            if (this.ReadyState != ReadyState.OPEN) return;
            this.Transport.Send(Common.Utils.Escape.Quote(data));
        }

        /// <summary>
        /// Step 1 of WS spec - parse and validate the url. Issue #8
        /// </summary>
        private static void ParseAndValidateUrl(ref string url)
        {
            var parsedUrl = new Uri(url);
            if (parsedUrl.Host.IsNullOrWhiteSpace() || parsedUrl.Scheme.IsNullOrWhiteSpace())
            {
                throw new SyntaxErrorException($"The URL '{url}' is invalid");
            }

            if (!parsedUrl.Fragment.IsNullOrWhiteSpace())
            {
                throw new SyntaxErrorException("The URL must not contain a fragment");
            }

            if (parsedUrl.Scheme != "http" && parsedUrl.Scheme != "https")
            {
                throw new SyntaxErrorException($"The URL's scheme must be either 'http' or 'https'. '{parsedUrl.Scheme}' is not allowed.");
            }
        }

        /// <summary>
        /// Step 3 - check port access - no need here
        /// Step 4 - parse protocols argument
        /// </summary>
        /// <param name="protocols"></param>
        private static void CheckPortAccess(ref ICollection<string> protocols)
        {
            if (protocols is null) protocols = Array.Empty<string>();
        }

        /// <summary>
        /// Step 5 - check protocols argument
        /// </summary>
        private static void CheckProtocols(ref ICollection<string> protocols)
        {
            var set = new HashSet<string>();
            foreach (var proto in protocols)
            {
                if (proto.IsNullOrWhiteSpace()) throw new SyntaxErrorException($"The protocols entry '{proto ?? "null"}' is invalid.");
                if (!set.Add(proto)) throw new SyntaxErrorException($"The protocols entry '{proto}' is duplicated.");
            }
        }

        /// <summary>
        /// Step 6 - convert origin
        /// </summary>
        private static void ConvertOrigin()
        {
            /* NOT RUNNING IN BROWSER
            var o = urlUtils.getOrigin(loc.href);
            this._origin = o ? o.toLowerCase() : null;
            */
        }

        /// <summary>
        /// remove the trailing slash
        /// </summary>
        private static void RemoveTrailingSlash(ref string url)
        {
            var uriBuilder = new UriBuilder(new Uri(url));
            uriBuilder.Path = uriBuilder.Path.TrimEnd('/');
            url = uriBuilder.Uri.OriginalString;
        }

        /// <summary>
        /// Step 7 - start connection in background
        /// obtain server info
        /// http://sockjs.github.io/sockjs-protocol/sockjs-protocol-0.3.3.html#section-26
        /// </summary>
        private void StartConnectionInBackground()
        {
            /* NOT RUNNING IN BROWSER
            this._urlInfo = {
                nullOrigin: !browser.hasDomain()
                , sameOrigin: urlUtils.isOriginEqual(this.url, loc.href)
                , sameScheme: urlUtils.isSchemeEqual(this.url, loc.href)
            };
            */
            this.Ir = new InfoReceiver(this.Url);
            this.Ir.On("finish", this.ReceiveInfo);
        }

        private static string DefaultSessionIdGenerator() => Common.Utils.Random.GetString(8);

        private static bool UserSetCode(int code) => code == 1000 || (code >= 3000 && code <= 4999);

        public void Close(int code = 1000, string reason = "Normal closure")
        {
            // Step 1
            if (!UserSetCode(code)) throw new InvalidAccessException("Invalid code");

            // Step 2.4 states the max is 123 bytes, but we are just checking length
            if (reason?.Length > 123) throw new SyntaxErrorException($"{nameof(reason)} argument has an invalid length");

            // Step 3.1
            if (this.ReadyState == ReadyState.CLOSING || this.ReadyState == ReadyState.CLOSED) return;

            // TODO look at docs to determine how to set this
            const bool wasClean = true;
            this.Close(1000, reason, wasClean);
        }

        private void ReceiveInfo(object sender, object[] args)
        {
            var info = args[0] as InfoDto;
            var rtt = (long)args[1];
            Log.Debug($"{nameof(this.ReceiveInfo)}: {rtt}");
            this.Ir = null;
            if (info is null)
            {
                this.Close(1002, "Cannot connect to server");
                return;
            }

            // establish a round-trip timeout (RTO) based on the
            // round-trip time (RTT)
            this.RoundTripTimeout = this.CountRoundTripTimeout(rtt);

            // allow server to override url used for the actual transport
            this.TransportUrl = !info.BaseUrl.IsNullOrWhiteSpace() ? info.BaseUrl : this.Url;
            // info = objectUtils.extend(info, this._urlInfo);
            Log.Debug($"{nameof(this.ReceiveInfo)}: {nameof(info)} {info}");

            // determine list of desired and supported transports
            var enabledTransports = TransportCollection.FilterToEnabled(this.TransportsWhitelist, info);
            this.Transports = new ConcurrentQueue<ITransportFactory>(enabledTransports.Main);
            Log.Debug($"{nameof(this.ReceiveInfo)}: {this.Transports.Count} enabled transports");

            this.Connect();
        }

        private void Connect()
        {
            while (this.Transports.TryDequeue(out var transport))
            {
                Log.Debug($"{nameof(this.Connect)}: Attempt {transport.TransportName}");
                /* NOT RUNNING IN BROWSER
                if (transport.NeedBody)
                {
                    if (!global.document.body ||
                      (typeof global.document.readyState !== 'undefined' &&
                        global.document.readyState !== 'complete' &&
                        global.document.readyState !== 'interactive')) {
                        debug('waiting for body');
                        this._transports.unshift(Transport);
                        eventUtils.attachEvent('load', this._connect.bind(this));
                        return;
                    }
                */

                // calculate timeout based on RTO and round trips. Default to 5s
                var timeoutMs = (this.RoundTripTimeout * transport.RoundTrips);// || 5000;

                // this._transportTimeoutId = setTimeout(this._transportTimeout.bind(this), timeoutMs);
                this._transportTimeoutCancellationToken = new CancellationTokenSource();
                Task.Delay(TimeSpan.FromMilliseconds(timeoutMs), this._transportTimeoutCancellationToken.Token).ContinueWith((t, o) => this.TransportTimeout(), this._transportTimeoutCancellationToken);

                Log.Debug($"{nameof(this.Connect)}: Using timeout {timeoutMs}");

                var transportUrl = $"{this.TransportUrl}/{this.Server}/{this.GenerateSessionId()}";
                var options = this.TransportOptions.GetValueOrDefault(transport.TransportName);
                Log.Debug($"{nameof(this.Connect)}: Transport url {transportUrl}");
                //                var transportObj = new Transport(transport.TransportName, transportUrl, this.TransportUrl, options);
                var transportObj = transport.Build(transport.TransportName, transportUrl, this.TransportUrl, options);
                transportObj.On("message", this.OnTransportMessage);
                transportObj.Once("closed", this.TransportClose);
                // transportObj.transportName = Transport.transportName; // moved to constructor
                this.Transport = transportObj;
                return;
            }

            this.Close(2000, "All transports failed", false);
        }

        private void TransportTimeout()
        {
            Log.Debug(nameof(this.TransportTimeout));
            if (this.ReadyState != ReadyState.CONNECTING) return;

            this.Transport?.Close();
            this.TransportClose(this, 2007, "Transport timed out");
        }

        private void OnTransportMessage(object sender, object[] args)
        {
            var msg = (string)args[0];
            Log.Debug($"{nameof(this.OnTransportMessage)}, {msg}");
            var type = msg.Substring(0, 1);
            var content = msg.Substring(1);

            // first check for messages that don't need a payload
            switch (type)
            {
                case "o":
                    this.OnOpen();
                    return;
                case "h":
                    Log.Debug($"{nameof(this.OnTransportMessage)}: Heartbeat {this.Transport}");
                    this.DispatchEvent(new Event.Event("heartbeat"));
                    return;
            }

            if (content.IsNullOrWhiteSpace())
            {
                Log.Debug($"{nameof(this.OnTransportMessage)}: Empty payload {content}");
                return;
            }

            JToken payload = null;
            try
            {
                payload = JsonConvert.DeserializeObject(content) as JToken;
            }
            catch
            {
                Log.Debug($"{nameof(this.OnTransportMessage)}: Bad JSON {content}");
            }

            var payloadCollection = payload as ICollection<JToken>;
            switch (type)
            {
                case "a":
                    if (payloadCollection != null)
                    {
                        foreach (var item in payloadCollection)
                        {
                            Log.Debug($"{nameof(this.OnTransportMessage)}: Message {this.Transport} {item}");
                            this.DispatchEvent(new TransportMessageEvent(item));
                        }
                    }
                    break;
                case "m":
                    Log.Debug($"{nameof(this.OnTransportMessage)}: Message {this.Transport} {payload}");
                    this.DispatchEvent(new TransportMessageEvent(payload));
                    break;
                case "c":
                    if (payloadCollection != null && payloadCollection.Count == 2)
                    {
                        var items = new JToken[2];
                        payloadCollection.CopyTo(items, 0);
                        this.Close((int)items[0], (string)items[1], true);
                    }
                    break;
            }
        }

        private void TransportClose(object sender, params object[] args)
        {
            var code = (int)args[0];
            var reason = args[1] as string;
            Log.Debug($"{nameof(this.TransportClose)}: {this.Transport?.TransportName} {code} {reason}");
            if (this.Transport != null)
            {
                this.Transport.RemoveAllListeners();
                this.Transport = null;
            }

            if (!UserSetCode(code) && code != 2000 && this.ReadyState == ReadyState.CONNECTING)
            {
                this.Connect();
                return;
            }

            this.Close(code, reason);
        }

        private void OnOpen()
        {
            Log.Debug($"{nameof(this.OnOpen)}: Open {this.Transport?.TransportName} {this.ReadyState}");
            if (this.ReadyState == ReadyState.CONNECTING)
            {
                var tokenSource = this._transportTimeoutCancellationToken;
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                    this._transportTimeoutCancellationToken = null;
                }

                this.ReadyState = ReadyState.OPEN;
                // this.transport = this._transport.transportName;
                Log.Debug($"{nameof(this.OnOpen)}: connected {this.Transport?.TransportName}");
                this.DispatchEvent(new Event.Event("open"));
            }
            else
            {
                // The server might have been restarted, and lost track of our connection.
                this.Close(1006, "Server lost session");
            }
        }

        private void Close(int code, string reason, bool wasClean)
        {
            Log.Debug($"{nameof(this.Close)}: {this.Transport} {code} {reason} {wasClean} {this.ReadyState}");
            var forceFail = false;

            if (this.Ir != null)
            {
                forceFail = true;
                this.Ir.Close();
                this.Ir = null;
            }

            if (this.Transport != null)
            {
                this.Transport.Close();
                this.Transport = null;
                // this.transport = null;
            }

            if (this.ReadyState == ReadyState.CLOSED) throw new InvalidStateException("SockJS has already been closed");

            this.ReadyState = ReadyState.CLOSING;
            Task.Factory.StartNew(() =>
            {
                this.ReadyState = ReadyState.CLOSED;
                if (forceFail) this.DispatchEvent(new Event.Event("error"));

                var e = new CloseEvent
                {
                    WasClean = wasClean, // || false
                    Code = code, // || 1000
                    Reason = reason
                };
                this.DispatchEvent(e);

                Log.Debug($"{nameof(this.Close)}: Disconnected");
                // this.onmessage = this.onclose = this.onerror = null;
            });
        }

        /// <summary>
        /// See: http://www.erg.abdn.ac.uk/~gerrit/dccp/notes/ccid2/rto_estimator/
        /// and RFC 2988
        /// </summary>
        private long CountRoundTripTimeout(long rtt)
        {
            // In a local environment, when using IE8/9 and the `jsonp-polling`
            // transport the time needed to establish a connection (the time that pass
            // from the opening of the transport to the call of `_dispatchOpen`) is
            // around 200msec (the lower bound used in the article above) and this
            // causes spurious timeouts. For this reason we calculate a value slightly
            // larger than that used in the article.
            return rtt > 100
                ? 4 * rtt // rto > 400msec
                : 300 + rtt; // 300msec < rto <= 400msec
        }
    }
}
