using System;
using syp.biz.SockJS.NET.Common.DTO;

namespace syp.biz.SockJS.NET.Client2.Interfaces
{
    public interface ITransportConfiguration
    {
        Uri BaseEndpoint { get; }
        ILogger Logger { get; }
        InfoDto Info { get; }
    }
}