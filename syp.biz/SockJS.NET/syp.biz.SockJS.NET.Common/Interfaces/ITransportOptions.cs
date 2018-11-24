using System.Collections.Generic;

namespace syp.biz.SockJS.NET.Common.Interfaces
{
    public interface ITransportOptions
    {
        IDictionary<string, string> Headers { get; set; }
    }
}