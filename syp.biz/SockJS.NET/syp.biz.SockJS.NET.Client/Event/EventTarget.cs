using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Event
{
    public abstract class EventTarget : IEventTarget
    {
        protected virtual ConcurrentDictionary<string, EventHandler<object[]>[]> Listeners { get; } = new ConcurrentDictionary<string, EventHandler<object[]>[]>();

        public void AddEventListener(string eventType, EventHandler<object[]> listener)
        {
            var arr = this.Listeners.GetOrAdd(eventType, _ => Array.Empty<EventHandler<object[]>>());

            if (!arr.Contains(listener))
            {
                // Make a copy so as not to interfere with a current dispatchEvent.
                arr = arr.Concat(new[] { listener }).ToArray();
            }

            this.Listeners[eventType] = arr;
        }

        public void RemoveEventListener(string eventType, EventHandler<object[]> listener)
        {
            if (!this.Listeners.TryGetValue(eventType, out var arr)) return;
            arr = arr.Where(l => l != listener).ToArray();
            if (arr.Any())
            {
                this.Listeners[eventType] = arr;
            }
            else
            {
                this.Listeners.TryRemove(eventType, out _);
            }
        }

        protected void DispatchEvent(Event @event, params object[] args)
        {
            var eventType = @event.Type;
            var argBuilder = new LinkedList<object>(args);
            argBuilder.AddFirst(@event);
            args = argBuilder.ToArray();

            if (this.Listeners.TryGetValue(eventType, out var listeners))
            {
                foreach (var listener in listeners)
                {
                    listener.Invoke(this, args);
                }
            }
        }
    }
}
