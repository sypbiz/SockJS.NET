using System;
using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Client.Transports.Lib.Driver;
using syp.biz.SockJS.NET.Common.DTO;
using syp.biz.SockJS.NET.Common.Extensions;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Streaming
{
    internal class WebSocketTransport : EventEmitter, ITransport, ITransportFactory
    {
        public WebSocketTransport() { }

        private WebSocketTransport(string transportUrl, string originalTransportUrl, ITransportOptions options)
        {
            if (!WebSocketTransport.IsEnabled)
            {
                throw new Exception("Transport created when disabled");
            }
            Log.Debug($"{nameof(WebSocketTransport)}: Constructor {transportUrl}");

            var url = new Uri(transportUrl).AddPath("/websocket");
            url = new UriBuilder(url)
            {
                Scheme = url.Scheme == "https" ? "wss" : "ws"
            }.Uri;

            this.Url = url;

            this.Ws = new WebSocketDriver(this.Url, Array.Empty<string>(), options);
            this.Ws.On("message", this.WebSocketOnMessage);
            /* NOT RUNNING IN BROWSER
// Firefox has an interesting bug. If a websocket connection is
// created after onunload, it stays alive even when user
// navigates away from the page. In such situation let's lie -
// let's not open the ws connection at all. See:
// https://github.com/sockjs/sockjs-client/issues/28
// https://bugzilla.mozilla.org/show_bug.cgi?id=696085
this.unloadRef = utils.unloadAdd(function() {
debug('unload');
self.ws.close();
});
            */
            this.Ws.On("close", this.WebSocketOnClose);
            this.Ws.On("error", this.WebSocketOnError);
        }

        private Uri Url { get; }
        private WebSocketDriver Ws { get; set; }

        private void WebSocketOnMessage(object sender, object[] e)
        {
            var data = e[0] as string;
            Log.Debug($"{nameof(this.WebSocketOnMessage)}: {data}");
            this.Emit("message", data);
        }

        private void WebSocketOnClose(object sender, object[] e)
        {
            var code = (int)e[0];
            var reason = e[1] as string;
            Log.Debug($"{nameof(this.WebSocketOnClose)}: {code} {reason}");
            this.Cleanup();
        }

        private void WebSocketOnError(object sender, object[] e)
        {
            Log.Debug($"{nameof(this.WebSocketOnError)}: {e[0]}");
            this.Emit("close", 1006, "WebSocket connection broken");
            this.Cleanup();
        }

        private static bool IsEnabled => WebSocketDriver.IsSupported;

        public string TransportName => "websocket";
        public ITransport FacadeTransport => null;

        // In theory, ws should require 1 round trip.
        // But in chrome, this is not very stable over SSL.
        // Most likely a ws connection requires a separate SSL connection, in which case 2 round trips are an absolute minimum.
        public long RoundTrips => 2;

        public void Send(string message)
        {
            var msg = $"[{message}]";
            Log.Debug($"{nameof(this.Send)}: {msg}");
            this.Ws.Send(msg).IgnoreAwait();
        }

        public void Close()
        {
            Log.Debug(nameof(this.Close));
            var ws = this.Ws;
            this.Cleanup();
            ws?.Close().IgnoreAwait();
        }

        private void Cleanup()
        {
            Log.Debug(nameof(this.Cleanup));
            var ws = this.Ws;
            ws?.RemoveAllListeners();
            /* NO RUNNING IN BROWSER
  utils.unloadDel(this.unloadRef);
  this.unloadRef = this.ws = null;
             */
            this.Ws = null;
            this.RemoveAllListeners();
        }

        public bool Enabled(InfoDto info)
        {
            if (info.WebSocket) return true;
            
            Log.Debug($"{nameof(this.Enabled)}: Disabled from server 'websocket'");
            return false;
        }

        ITransportFactory ITransportFactory.FacadeTransport => null;

        public ITransport Build(string transportName, string transportUrl, string originalTransportUrl, ITransportOptions options)
        {
            return new WebSocketTransport(transportUrl, originalTransportUrl, options);
        }
    }
}
