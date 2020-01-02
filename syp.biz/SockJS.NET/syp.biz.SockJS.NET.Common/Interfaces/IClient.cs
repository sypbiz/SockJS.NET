using System;

namespace syp.biz.SockJS.NET.Common.Interfaces
{
    public interface IClient
    {
        void Start();
        void Send(string data);
        void Close(int code, string reason);
    }

    public interface IClientEventEmitter
    {
        void AddOpenEventListener(EventHandler<object[]> handler);
        void RemoveOpenEventListener(EventHandler<object[]> handler);

        void AddMessageEventListener(EventHandler<object[]> handler);
        void RemoveMessageEventListener(EventHandler<object[]> handler);

        void AddCloseEventListener(EventHandler<object[]> handler);
        void RemoveCloseEventListener(EventHandler<object[]> handler);

        void AddStateChangeEventListener(EventHandler<object[]> handler);
        void RemoveStateChangeEventListener(EventHandler<object[]> handler);
    }

    public interface IClientEvents
    {
        // TODO: remove `Event` suffix
        event EventHandler<object[]> OpenEvent;
        event EventHandler<object[]> MessageEvent;
        event EventHandler<(int code, string reason, bool wasClean)> CloseEvent;
        event EventHandler<(ReadyState previous, ReadyState current)> StateChangeEvent;
    }
}
