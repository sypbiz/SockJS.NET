namespace syp.biz.SockJS.NET.Common.Interfaces
{
    public interface ITransport : IEventEmitter
    {
        string TransportName { get; }
        void Close();
        void Send(string message);
    }
}