using syp.biz.SockJS.NET.Client.Transports.Lib.Driver;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Lib.Sender
{
    internal class XhrCorsObject : XhrDriver
    {
        private XhrCorsObject(string method, string url, string payload, ITransportOptions opts) : base(method, url, payload, opts) { }

        public new static bool Enabled => XhrDriver.Enabled && XhrDriver.SupportsCors;

        public static IAjaxObject Build(string method, string url, string payload, ITransportOptions opts = default)
        {
            var obj = new XhrCorsObject(method, url, payload, opts);
            return obj;
        }
    }
}
