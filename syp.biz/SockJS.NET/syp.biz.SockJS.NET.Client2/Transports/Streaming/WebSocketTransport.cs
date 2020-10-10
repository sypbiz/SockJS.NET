using System;
using System.Threading;
using System.Threading.Tasks;
using syp.biz.SockJS.NET.Common.DTO;
using syp.biz.SockJS.NET.Common.Extensions;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client2.Transports.Streaming
{
    internal class WebSocketTransport : ITransport2, ITransportFactory2
    {
        public WebSocketTransport() { }

        private WebSocketTransport(string transportUrl, string originalTransportUrl, ITransportOptions options)
        {
            if (!WebSocketTransport.IsEnabled) throw new Exception("Transport created when disabled");

            Log.Debug($"{nameof(WebSocketTransport)}: Constructor {transportUrl}");

            var uri = new Uri(transportUrl).AddPath("/websocket");
            uri = new UriBuilder(uri)
            {
                Scheme = uri.Scheme == "https" ? "wss" : "ws"
            }.Uri;

            this.Uri = uri;

            this.Ws = null;  // TODO: finish: new WebSocketDriver(this.Url, Array.Empty<string>(), options);
            // TODO: finish:
            // this.Ws.MessageEvent += this.WebSocketOnMessage;
            // this.Ws.CloseEvent += this.WebSocketOnClose;
            // this.Ws.ErrorEvent += this.WebSocketOnError;
        }

        private static bool IsEnabled => true; // TODO: finish: WebSocketDriver.IsSupported;

        private TBD? Ws { get; set; } // WebSocketDriver
        private Uri? Uri { get; }

        private void Cleanup()
        {
            Log.Debug(nameof(this.Cleanup));
            var ws = this.Ws;
            // TODO: finidh: ws?.RemoveAllListeners();
            this.Ws = null;
            this.Dispose();
        }

        #region Implementation of IDisposable
        public void Dispose()
        {
            this.MessageEvent = null;
            this.CloseEvent = null;
        }
        #endregion Implementation of IDisposable

        #region Implementation of ITransportFactory2
        public ITransportFactory2? FacadeTransport => null;
        public string TransportName => "websocket";

        // In theory, ws should require 1 round trip.
        // Most likely a ws connection requires a separate SSL connection, in which case 2 round trips are an absolute minimum.
        public long RoundTrips => 2;

        public bool Enabled(InfoDto info)
        {
            if (info.WebSocket) return true;

            Log.Debug($"{nameof(this.Enabled)}: Disabled from server 'websocket'");
            return false;
        }

        public ITransport2 Build(string transportUrl, string originalTransportUrl, ITransportOptions options, CancellationToken cancel)
        {
            throw new NotImplementedException();
        }
        #endregion Implementation of ITransportFactory2

        #region Implementation of ITransport2
        public event EventHandler<string>? MessageEvent;
        public event EventHandler<(int code, string reason)>? CloseEvent;

        public async Task Close()
        {
            Log.Debug(nameof(this.Close));
            var ws = this.Ws;
            this.Cleanup();
            // TODO: finish: ws?.Close().IgnoreAwait();
        }

        public async Task Send(string message)
        {
            var msg = $"[{message}]";
            Log.Debug($"{nameof(this.Send)}: {msg}");
            // TODO: finish: this.Ws.Send(msg).IgnoreAwait();
        }

        public async Task TryConnect(CancellationToken cancel)
        {
            throw new NotImplementedException();
        }
        #endregion Implementation of ITransport2
    }
}
