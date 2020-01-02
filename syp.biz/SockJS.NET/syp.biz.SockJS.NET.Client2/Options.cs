using System;
using System.Collections.Generic;
using System.Threading;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client2
{
    public class Options
    {
        public string[] Transports { get; set; }
        public IDictionary<string, ITransportOptions> TransportOptions { get; set; }
        public Func<string> SessionIdGenerator { get; set; }
        public string Server { get; set; }
        public CancellationToken? CancellationToken { get; set; }
    }
}