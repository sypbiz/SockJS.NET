using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using syp.biz.SockJS.NET.Common;
using syp.biz.SockJS.NET.Common.Extensions;

namespace syp.biz.SockJS.NET.Client2
{
    internal class MessageParser
    {
        private static readonly IDictionary<string, MessageType> MessageTypeConversion = EnumExtensions.GetDescriptionMembers<MessageType>();

        public MessageParser(string originalMessage)
        {
            this.OriginalMessage = originalMessage;
            this.Type = ResolveMessageType(originalMessage.Substring(0, 1));
            this.Content = originalMessage.Substring(1);
            this.Payload = new Lazy<JToken?>(this.ParsePayload);
        }

        public MessageType Type { get; }
        public string Content { get; }
        public Lazy<JToken?> Payload { get;  }

        private string OriginalMessage { get; }

        private static MessageType ResolveMessageType(string code)
        {
            if (MessageTypeConversion.TryGetValue(code, out var type)) return type;
            Log.Error($"{nameof(ResolveMessageType)}: Invalid message type code '{code}'");
            return MessageType.Unknown;
        }

        private JToken? ParsePayload()
        {
            try
            {
                return JsonConvert.DeserializeObject(this.Content) as JToken;
            }
            catch
            {
                Log.Debug($"{nameof(this.ParsePayload)}: Bad JSON {this.Content}");
                return null;
            }
        }
    }
}
