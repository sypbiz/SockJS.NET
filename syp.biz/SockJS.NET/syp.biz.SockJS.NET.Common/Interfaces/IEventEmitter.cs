using System;

namespace syp.biz.SockJS.NET.Common.Interfaces
{
    public interface IEventEmitter: IEventTarget
    {
        void Once(string eventType, EventHandler<object[]> listener);
        void On(string eventType, EventHandler<object[]> listener);
        void RemoveAllListeners(string eventType = null);
    }
}