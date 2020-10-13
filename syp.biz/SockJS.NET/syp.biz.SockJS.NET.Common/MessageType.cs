using System.ComponentModel;

namespace syp.biz.SockJS.NET.Common
{
    public enum MessageType
    {
        [Description("o")] Open,
        [Description("h")] Heartbeat,
        [Description("a")] ArrayMessage,
        [Description("m")] SingleMessage,
        [Description("c")] Close,
        Unknown
    }
}
