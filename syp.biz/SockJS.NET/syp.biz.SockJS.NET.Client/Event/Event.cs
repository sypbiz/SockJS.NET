using System;
using syp.biz.SockJS.NET.Common.Extensions;

namespace syp.biz.SockJS.NET.Client.Event
{
    public class Event
    {
        public const int CAPTURING_PHASE = 1;
        public const int AT_TARGET = 2;
        public const int BUBBLING_PHASE = 3;

        public Event(string eventType)
        {
            if (eventType.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(eventType));
            this.Type = eventType;
        }

        public string Type { get; private set; }
        public bool Bubbles { get; private set; }
        public bool Cancelable { get; private set; }
        public long TimeStamp { get; private set; }

        protected Event InitEvent(string eventType, bool canBubble, bool cancelable)
        {
            this.Type = eventType;
            this.Bubbles = canBubble;
            this.Cancelable = cancelable;
            this.TimeStamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
            return this;
        }

        public virtual void StopPropagation() { }
        public virtual void PreventDefault() { }
    }
}
