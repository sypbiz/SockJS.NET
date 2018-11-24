using System;

namespace syp.biz.SockJS.NET.Common.Interfaces
{
    public interface IEventTarget
    {
        void AddEventListener(string eventType, EventHandler<object[]> listener);
        void RemoveEventListener(string eventType, EventHandler<object[]> listener);
    }
}