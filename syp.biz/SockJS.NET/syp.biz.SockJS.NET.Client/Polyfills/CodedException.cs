using System;
using System.Runtime.Serialization;

namespace syp.biz.SockJS.NET.Client.Polyfills
{
    [Serializable]
    public class CodedException : Exception
    {
        public CodedException(int code) => this.Code = code;

        public CodedException(string message, int code) : base(message) => this.Code = code;

        public CodedException(string message, int code,  Exception inner) : base(message, inner) => this.Code = code;

        protected CodedException(int code, SerializationInfo info, StreamingContext context) : base(info, context) => this.Code = code;

        public int Code { get; }
    }
}
