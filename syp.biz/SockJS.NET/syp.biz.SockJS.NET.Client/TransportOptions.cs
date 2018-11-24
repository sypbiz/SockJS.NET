using System.Collections.Generic;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client
{
    internal class TransportOptions : ITransportOptions
    {
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}