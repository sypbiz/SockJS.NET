using System;
using System.Collections.Generic;
using syp.biz.SockJS.NET.Client.Polyfills;
using syp.biz.SockJS.NET.Common.Extensions;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Lib
{
    internal abstract class AjaxBasedTransport : SenderReceiver
    {
        protected AjaxBasedTransport(string transportUrl, string urlSuffix, Polling.ReceiverFactory receiver, AjaxObjectFactory ajaxObject) 
            : base(transportUrl, urlSuffix, CreateAjaxSender(ajaxObject), receiver, ajaxObject) { }

        protected AjaxBasedTransport() : base() { }

        private static SenderDelegate CreateAjaxSender(AjaxObjectFactory ajaxObject)
        {
            return (url, payload, callback) =>
            {
                Log.Debug($"{nameof(CreateAjaxSender)}: Create ajax sender {url} {payload}");
                var opt = new TransportOptions();
                if (!payload.IsNullOrEmpty()) opt.Headers = new Dictionary<string, string>
                {
                    {"Content-type", "text/plain"}
                };
                var ajaxUrl = new Uri(url).AddPath("/xhr_send");
                var xo = ajaxObject("POST", ajaxUrl.OriginalString, payload, opt);
                xo.Once("finish", (sender, e) =>
                {
                    var status = (int)e[0];
                    Log.Debug($"{nameof(CreateAjaxSender)}: Finish {status}");
                    xo = null;
                    if (status != 200 && status != 204)
                    {
                        callback(new Exception($"http status {status}"));
                        return;
                    }

                    callback(null);
                });

                return () =>
                {
                    Log.Debug($"{nameof(CreateAjaxSender)}: Abort");
                    xo.Close();
                    xo = null;
                    var err = new CodedException("Aborted", 1000);
                    callback(err);
                };
            };
        }
    }
}
