using syp.biz.SockJS.NET.Client.Transports.Lib;
using syp.biz.SockJS.NET.Client.Transports.Lib.Receiver;
using syp.biz.SockJS.NET.Client.Transports.Lib.Sender;
using syp.biz.SockJS.NET.Common.DTO;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Polling
{
    internal class XhrPollingTransport : AjaxBasedTransport, ITransportFactory
    {
        public XhrPollingTransport() : base() { }

        private XhrPollingTransport(string transportUrl) : base(transportUrl, "/xhr", XhrReceiver.Build, XhrCorsObject.Build) { }
        
        public override string TransportName => "xhr-polling";

        public ITransport FacadeTransport => null;

        public long RoundTrips => 2;

        public bool Enabled(InfoDto info) => true;

        ITransportFactory ITransportFactory.FacadeTransport => null;

        public ITransport Build(string transportName, string transportUrl, string originalTransportUrl, ITransportOptions options)
        {
            return new XhrPollingTransport(transportUrl);
        }
    }
}
