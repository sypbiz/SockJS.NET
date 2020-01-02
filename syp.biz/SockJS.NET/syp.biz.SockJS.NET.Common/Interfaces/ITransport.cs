using System;
using System.Threading;
using System.Threading.Tasks;

namespace syp.biz.SockJS.NET.Common.Interfaces
{
    // TODO: cleanup
    public interface ITransport : IEventEmitter
    {
        string TransportName { get; }
        void Close();
        void Send(string message);
    }

    public interface ITransport2: IDisposable
    {
        // TODO: remove `Event` suffixes
        event EventHandler<string> MessageEvent;
        event EventHandler<(int code, string reason)> CloseEvent;

        string TransportName { get; }

        Task Close();
        Task Send(string message);
        Task TryConnect(CancellationToken cancel);
    }
}