using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace syp.biz.SockJS.NET.Client.Interfaces
{
    public interface IClient : IDisposable
    {
        event EventHandler Connected;
        event EventHandler Disconnected;
        event EventHandler<string> Message;

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        ConnectionState State { get; }

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        Task Connect(CancellationToken token);
        Task Connect();
        Task Disconnect();

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
        Task Send(string data, CancellationToken token);
        Task Send(string data);
    }
}
