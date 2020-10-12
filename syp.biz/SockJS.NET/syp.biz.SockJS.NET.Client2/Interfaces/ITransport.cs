using System;
using System.Threading;
using System.Threading.Tasks;

namespace syp.biz.SockJS.NET.Client2.Interfaces
{
    public interface ITransport : IDisposable
    {
        event EventHandler<string> Message;
        event EventHandler Connected;
        event EventHandler Disconnected;
        
        string Name { get; }

        Task Connect(CancellationToken token);
        Task Connect();
        Task Disconnect();

        Task Send(string data);
        Task Send(string data, CancellationToken token);
    }
}