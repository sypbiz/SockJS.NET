using System;
using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Common.DTO;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Streaming
{
    // TODO: finish implementation
    internal class WebSocketTransport : EventEmitter, ITransport, ITransportFactory
    {
        public WebSocketTransport() { }

        private WebSocketTransport(string transportUrl, string originalTransportUrl, ITransportOptions options) { }

        public string TransportName => "websocket";
        public ITransport FacadeTransport => null;

        // In theory, ws should require 1 round trip.
        // But in chrome, this is not very stable over SSL.
        // Most likely a ws connection requires a separate SSL connection, in which case 2 round trips are an absolute minimum.
        public long RoundTrips => 2;

        public void Send(string message)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public bool Enabled(InfoDto info)
        {
            // TODO: finish implementation
            return false;
//            if (info.WebSocket) return true;
//
//            Log.Debug($"{nameof(this.Enabled)}: Disabled from server 'websocket'");
//            return false;
        }

        ITransportFactory ITransportFactory.FacadeTransport => null;

        public ITransport Build(string transportName, string transportUrl, string originalTransportUrl, ITransportOptions options)
        {
            return new WebSocketTransport(transportUrl, originalTransportUrl, options);
        }
    }
}
