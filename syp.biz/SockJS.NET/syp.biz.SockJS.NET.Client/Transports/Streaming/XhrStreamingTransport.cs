using System;
using syp.biz.SockJS.NET.Client.Transports.Lib;
using syp.biz.SockJS.NET.Client.Transports.Lib.Receiver;
using syp.biz.SockJS.NET.Client.Transports.Lib.Sender;
using syp.biz.SockJS.NET.Common.DTO;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Streaming
{
    internal class XhrStreamingTransport: AjaxBasedTransport, ITransportFactory
    {
        public XhrStreamingTransport() : base() { }

        private XhrStreamingTransport(string transportUrl) : base(transportUrl, "/xhr_streaming", XhrReceiver.Build, XhrCorsObject.Build)
        {
            if (!XhrCorsObject.Enabled)
            {
                throw new Exception("Transport created when disabled");
            }
            /* NOT RUNNING IN BROWSER
 if (!XHRLocalObject.enabled && !XHRCorsObject.enabled) {
    throw new Error('Transport created when disabled');
  }
  AjaxBasedTransport.call(this, transUrl, '/xhr_streaming', XhrReceiver, XHRCorsObject);
             */
        }

        public ITransportFactory FacadeTransport => null;
        public long RoundTrips => 2; // preflight, ajax
        public override string TransportName => "xhr-streaming";
        public bool Enabled(InfoDto info)
        {
            // BUG: this transport is timing out
            // disabled due to above bug
            return false; 
//            return XhrCorsObject.Enabled;
            /* NOT RUNNING IN BROWSER
 if (info.nullOrigin) {
    return false;
  }
  // Opera doesn't support xhr-streaming #60
  // But it might be able to #92
  if (browser.isOpera()) {
    return false;
  }

  return XHRCorsObject.enabled;
             */
        }

        public static bool NeedBody => false;
        /* NOT USING BROWSER
 // Safari gets confused when a streaming ajax request is started
// before onload. This causes the load indicator to spin indefinitely.
// Only require body when used in a browser
XhrStreamingTransport.needBody = !!global.document;
         */

        public ITransport Build(string transportName, string transportUrl, string originalTransportUrl, ITransportOptions options)
        {
            return new XhrStreamingTransport(transportUrl);
        }
    }
}
