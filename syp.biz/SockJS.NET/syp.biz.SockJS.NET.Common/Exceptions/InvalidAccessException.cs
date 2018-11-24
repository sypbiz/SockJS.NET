using System;
using System.Runtime.Serialization;

namespace syp.biz.SockJS.NET.Common.Exceptions
{
    [Serializable]
    public class InvalidAccessException : Exception
    {
        public InvalidAccessException() { }

        public InvalidAccessException(string message) : base(message) { }

        public InvalidAccessException(string message, Exception inner) : base(message, inner) { }

        protected InvalidAccessException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class InvalidStateException : Exception
    {
        public InvalidStateException() { }

        public InvalidStateException(string message) : base(message) { }

        public InvalidStateException(string message, Exception inner) : base(message, inner) { }

        protected InvalidStateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
