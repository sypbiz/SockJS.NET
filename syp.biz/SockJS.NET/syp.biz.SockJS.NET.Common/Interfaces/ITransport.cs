using System;
using System.Threading;
using System.Threading.Tasks;

namespace syp.biz.SockJS.NET.Common.Interfaces
{
    public interface ITransport : IDisposable
    {
        event EventHandler<string> Message;
        event EventHandler Disconnected;
        
        Task Connect(CancellationToken token);
        Task Disconnect();

        Task Send(string data, CancellationToken token);
    }
}