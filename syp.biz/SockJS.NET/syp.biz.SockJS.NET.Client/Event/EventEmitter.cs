using System;
using System.Collections.Concurrent;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Event
{
    internal abstract class EventEmitter : EventTarget, IEventEmitter
    {
        protected sealed override ConcurrentDictionary<string, EventHandler<object[]>[]> Listeners { get; } = new ConcurrentDictionary<string, EventHandler<object[]>[]>();

        public void RemoveAllListeners(string eventType = null)
        {
            if (!string.IsNullOrWhiteSpace(eventType))
            {
                this.Listeners.TryRemove(eventType, out _);
            }
            else
            {
                this.Listeners.Clear();
            }
        }

        public void Once(string eventType, EventHandler<object[]> listener)
        {
            var fired = false;

            void handler(object sender, object[] args)
            {
                this.RemoveListener(eventType, handler);
                if (!fired)
                {
                    fired = true;
                    listener.Invoke(this, args);
                }
            }
            this.On(eventType, handler);
        }

        protected void Emit(string eventType, params object[] args)
        {
            if (!this.Listeners.TryGetValue(eventType, out var listeners)) return;
            foreach (var listener in listeners)
            {
                listener.Invoke(this, args);
            }
        }

        public void On(string eventType, EventHandler<object[]> listener) => base.AddEventListener(eventType, listener);
        protected void AddListener(string eventType, EventHandler<object[]> listener) => base.AddEventListener(eventType, listener);
        protected void RemoveListener(string eventType, EventHandler<object[]> listener) => base.RemoveEventListener(eventType, listener);
    }
}
