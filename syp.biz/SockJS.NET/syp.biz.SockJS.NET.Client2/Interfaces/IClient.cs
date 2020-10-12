using System;
using System.Threading;
using System.Threading.Tasks;

namespace syp.biz.SockJS.NET.Client2.Interfaces
{
    public interface IClient : IDisposable
    {
        event EventHandler Connected;
        event EventHandler Disconnected;
        event EventHandler<string> Message;

        ConnectionState State { get; }

        Task Connect(CancellationToken token);
        Task Connect();
        Task Disconnect();

        Task Send(string data);
        Task Send(string data, CancellationToken token);
    }
}
