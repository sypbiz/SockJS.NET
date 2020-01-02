using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using syp.biz.SockJS.NET.Common;
using syp.biz.SockJS.NET.Common.DTO;
using syp.biz.SockJS.NET.Common.Exceptions;
using syp.biz.SockJS.NET.Common.Extensions;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client2
{
    public class SockJS2 : IClient, IClientEvents
    {
        public event EventHandler<object[]> OpenEvent;
        public event EventHandler<object[]> MessageEvent;
        public event EventHandler<(int code, string reason, bool wasClean)> CloseEvent;
        public event EventHandler<(ReadyState previous, ReadyState current)> StateChangeEvent;
        private event EventHandler<JToken?> TransportMessageEvent;

        private readonly CancellationTokenSource _cancellation;
        private ReadyState _readyState;

        public SockJS2(string url, ICollection<string>? protocols = null, Options? options = null)
        {
            if (options is null) options = new Options();
            this._cancellation = options.CancellationToken.HasValue
                ? CancellationTokenSource.CreateLinkedTokenSource(options.CancellationToken.Value)
                : new CancellationTokenSource();

            this.ReadyState = ReadyState.CONNECTING;
            //this.Extensions = string.Empty;
            //this.Protocol = string.Empty;
            //this.TransportsWhitelist = options.Transports;
            this.TransportOptions = options.TransportOptions ?? new Dictionary<string, ITransportOptions>();
            this.GenerateSessionId = options.SessionIdGenerator ?? DefaultSessionIdGenerator;
            this.Server = options.Server ?? Common.Utils.Random.GetNumberString(1000);
            if (protocols is null) protocols = Array.Empty<string>();

            ParseAndValidateUrl(ref url);
            CheckProtocols(ref protocols);
            RemoveTrailingSlash(ref url);
            this.Url = url;
        }

        public static string Version => typeof(SockJS2).Assembly.GetName().Version.ToString(3);

        private ReadyState ReadyState
        {
            get => this._readyState;
            set
            {
                var previous = this._readyState;
                this._readyState = value;
                if (previous != value)
                {
                    //this.DispatchEvent(new Event.Event("stateChanged"), previous, value);
                    this.StateChangeEvent?.Invoke(this, (previous, value));
                }
            }
        }
        private string Url { get; }
        private InfoDto ServerInfo { get; set; }
        private long RoundTripTime { get; set; }
        private long RoundTripTimeout { get; set; }
        private string TransportUrl { get; set; }
        private ConcurrentQueue<ITransportFactory2> Transports { get; set; }
        private string Server { get; }
        private Func<string> GenerateSessionId { get; }
        private IDictionary<string, ITransportOptions> TransportOptions { get; }
        private ITransport2? Transport { get; set; }

        //private string[] TransportsWhitelist { get; }

        public static void SetLogger(ILogger logger) => Log.Logger = logger;
        private static string DefaultSessionIdGenerator() => Common.Utils.Random.GetString(8);
        private static bool UserSetCode(int code) => code == 1000 || (code >= 3000 && code <= 4999);

        /// <summary>
        /// Establish a round-trip timeout (RTO) based on the round-trip time (RTT)
        /// </summary>
        /// <param name="roundTripTime">The measured round-trip time</param>
        /// <returns>Round-trip timeout</returns>
        /// <remarks>
        /// See: http://www.erg.abdn.ac.uk/~gerrit/dccp/notes/ccid2/rto_estimator/
        /// and RFC 2988
        /// </remarks>
        private static long GetRoundTripTimeout(long roundTripTime)
        {
            // In a local environment, when using IE8/9 and the `jsonp-polling`
            // transport the time needed to establish a connection (the time that pass
            // from the opening of the transport to the call of `_dispatchOpen`) is
            // around 200msec (the lower bound used in the article above) and this
            // causes spurious timeouts. For this reason we calculate a value slightly
            // larger than that used in the article.
            return roundTripTime > 100
                ? 4 * roundTripTime // rto > 400 milliseconds
                : 300 + roundTripTime; // 300 < rto <= 400 milliseconds
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
        /// remove the trailing slash
        /// </summary>
        private static void RemoveTrailingSlash(ref string url)
        {
            var uriBuilder = new UriBuilder(new Uri(url));
            uriBuilder.Path = uriBuilder.Path.TrimEnd('/');
            url = uriBuilder.Uri.OriginalString;
        }

        public void Start() => this.StartAsync().ConfigureAwait(false);

        public void Send(string data)
        {
            throw new NotImplementedException();
        }

        void IClient.Close(int code, string reason)
        {
            throw new NotImplementedException();
        }

        public async Task StartAsync()
        {
            var infoReceiver = new InfoReceiver(this.Url);
            var (info, roundTripTime) = await infoReceiver.Execute();
            await this.ProcessServerInfo(info, roundTripTime);
        }

        public void Close(int code = 1000, string reason = "Normal closure")
        {
            // Step 1
            if (!UserSetCode(code)) throw new InvalidAccessException("Invalid code");

            // Step 2.4 states the max is 123 bytes, but we are just checking length
            if (reason.Length > 123) throw new SyntaxErrorException($"{nameof(reason)} argument has an invalid length");

            // Step 3.1
            if (this.ReadyState == ReadyState.CLOSING || this.ReadyState == ReadyState.CLOSED) return;

            // TODO look at docs to determine how to set this
            const bool wasClean = true;
            this.Close(1000, reason, wasClean);
        }

        private async Task ProcessServerInfo(InfoDto info, long roundTripTime)
        {
            Log.Debug($"{nameof(this.ProcessServerInfo)}: {nameof(info)} {info}");

            this.ServerInfo = info;
            this.RoundTripTime = roundTripTime;
            this.RoundTripTimeout = GetRoundTripTimeout(roundTripTime);

            // allow server to override url used for the actual transport
            this.TransportUrl = !info.BaseUrl.IsNullOrWhiteSpace() ? info.BaseUrl : this.Url;

            this.UpdateTransports(info);

            await this.Connect();
        }

        private void UpdateTransports(InfoDto info)
        {
            // determine list of desired and supported transports
            var enabledTransports = TransportCollection.FilterToEnabled(null /* TODO: this.TransportsWhitelist */, info);
            this.Transports = new ConcurrentQueue<ITransportFactory2>(enabledTransports.Main);
            Log.Debug($"{nameof(this.UpdateTransports)}: {this.Transports.Count} enabled transports");
        }

        private async Task Connect()
        {
            while (this.Transports.TryDequeue(out var factory))
            {
                Log.Debug($"{nameof(this.Connect)}: Attempt {factory.TransportName}");

                try
                {
                    var transportUrl = $"{this.TransportUrl}/{this.Server}/{this.GenerateSessionId()}";
                    var options = this.TransportOptions.GetValueOrDefault(factory.TransportName);
                    var transport = factory.Build(transportUrl, this.TransportUrl, options, this._cancellation.Token);

                    using var cancel = CancellationTokenSource.CreateLinkedTokenSource(this._cancellation.Token);
                    cancel.CancelAfter(TimeSpan.FromMilliseconds(this.RoundTripTimeout * factory.RoundTrips));
                    await transport.TryConnect(cancel.Token);

                    transport.MessageEvent += this.OnTransportMessage;
                    transport.CloseEvent += this.OnTransportClose;

                    return;
                }
                catch (Exception e)
                {
                    Log.Error($"{nameof(this.Connect)}: {factory.TransportName} failed: {e.Message}");
                    Log.Debug($"{nameof(this.Connect)}: {factory.TransportName} failed: {e}");
                }
            }

            this.Close(2000, "All transports failed", false);
        }

        private void OnTransportMessage(object sender, string message)
        {
            Log.Debug($"{nameof(this.OnTransportMessage)}: {message}");
            var parser = new MessageParser(message);

            // first check for messages that don't need a payload
            switch (parser.Type)
            {
                case MessageType.Open:
                    this.OnOpen();
                    return;
                case MessageType.Heartbeat:
                    Log.Debug($"{nameof(this.OnTransportMessage)}: Heartbeat {this.Transport}");
                    // TODO: is needed?
                    // this.DispatchEvent(new Event.Event("heartbeat"));
                    return;
            }

            if (parser.Content.IsNullOrWhiteSpace())
            {
                Log.Debug($"{nameof(this.OnTransportMessage)}: Empty payload");
                return;
            }

            var payload = parser.Payload.Value;
//            var payloadCollection = payload as ICollection<JToken>;
            switch (parser.Type)
            {
                case MessageType.ArrayMessage when payload is ICollection<JToken> payloadCollection:
//                    if (payloadCollection == null) break;
                    foreach (var item in payloadCollection)
                    {
                        Log.Debug($"{nameof(this.OnTransportMessage)}: Message {sender} {item}");
                        this.TransportMessageEvent?.Invoke(this, item);
                    }
                    break;

                case MessageType.SingleMessage:
                    Log.Debug($"{nameof(this.OnTransportMessage)}: Message {sender} {payload}");
                    this.TransportMessageEvent?.Invoke(this, payload);
                    break;

                case MessageType.Close when payload is ICollection<JToken> payloadCollection && payloadCollection.Count == 2:
                    var items = payloadCollection.Take(2).ToArray();
                    this.Close((int)items[0], (string)items[1], true);
                    break;
                default:
                    Log.Debug($"{nameof(this.OnTransportMessage)}: Message {sender} {parser.Type} {payload?.GetType().Name}");
                    break;
            }
        }

        private void OnOpen()
        {
            Log.Debug($"{nameof(this.OnOpen)}: Open {this.Transport?.TransportName} {this.ReadyState}");
            // TODO: finish
//            if (this.ReadyState == ReadyState.CONNECTING)
//            {
//                var tokenSource = this._transportTimeoutCancellationToken;
//                if (tokenSource != null)
//                {
//                    tokenSource.Cancel();
//                    this._transportTimeoutCancellationToken = null;
//                }
//
//                this.ReadyState = ReadyState.OPEN;
//                // this.transport = this._transport.transportName;
//                Log.Debug($"{nameof(this.OnOpen)}: connected {this.Transport?.TransportName}");
//                this.DispatchEvent(new Event.Event("open"));
//            }
//            else
//            {
//                // The server might have been restarted, and lost track of our connection.
//                this.Close(1006, "Server lost session");
//            }
        }

        private void OnTransportClose(object sender, (int code, string reason) args)
        {
            var (code, reason) = args;
            Log.Debug($"{nameof(this.OnTransportClose)}: {this.Transport?.TransportName} {code} {reason}");
            if (this.Transport != null)
            {
                this.Transport.Dispose();
                this.Transport = null;
            }

            if (!UserSetCode(code) && code != 2000 && this.ReadyState == ReadyState.CONNECTING)
            {
                this.Connect().ConfigureAwait(false);
                return;
            }

            this.Close(code, reason);
        }

        //private void TransportTimeout()
        //{
        //    Log.Debug(nameof(this.TransportTimeout));
        //    if (this.ReadyState != ReadyState.CONNECTING) return;

        //    this.Transport?.Close();
        //    this.TransportClose(this, 2007, "Transport timed out");
        //}

        private void Close(int code, string reason, bool wasClean)
        {
            Log.Debug($"{nameof(this.CloseEvent)}: {this.Transport} {code} {reason} {wasClean} {this.ReadyState}");
//            var forceFail = false;

//            if (this.Ir != null)
//            {
//                forceFail = true;
//                this.Ir.Close();
//                this.Ir = null;
//            }

            if (this.Transport != null)
            {
                this.Transport.Dispose();
                this.Transport = null;
            }

            if (this.ReadyState == ReadyState.CLOSED) throw new InvalidStateException("SockJS has already been closed");

            this.ReadyState = ReadyState.CLOSING;
            Task.Factory.StartNew(() =>
            {
                this.ReadyState = ReadyState.CLOSED;
//                if (forceFail) this.DispatchEvent(new Event.Event("error"));

                this.CloseEvent?.Invoke(this, (code, reason, wasClean));

                Log.Debug($"{nameof(this.CloseEvent)}: Disconnected");
                // this.onmessage = this.onclose = this.onerror = null;
            });
        }
    }
}
