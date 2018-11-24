namespace syp.biz.SockJS.NET.Common.Interfaces
{
    public delegate IAjaxObject AjaxObjectFactory(string method, string url, string payload, ITransportOptions opts = default);

    public interface IAjaxObject : IEventEmitter
    {
        void Close();
    }
}
