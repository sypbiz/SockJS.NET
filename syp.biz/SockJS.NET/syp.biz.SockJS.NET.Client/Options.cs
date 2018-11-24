using System;
using System.Collections.Generic;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client
{
    public class Options
    {
        [Obsolete("ProtocolWhitelist is DEPRECATED. Use Transports instead.")]
        public string[] ProtocolWhitelist { get; set; }

        public string[] Transports { get; set; }
        public IDictionary<string, ITransportOptions> TransportOptions { get; set; }
        public Func<string> SessionIdGenerator { get; set; }
        public string Server { get; set; }
    }
}