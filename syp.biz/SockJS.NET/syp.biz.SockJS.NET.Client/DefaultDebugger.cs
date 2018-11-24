using syp.biz.SockJS.NET.Common.Interfaces;
using Logger = System.Diagnostics.Debug;

namespace syp.biz.SockJS.NET.Client
{
    internal class DefaultDebugger : ILogger
    {
        private const string PREFIX = "[SockJS.NET]";
        public void Debug(string message) => Logger.WriteLine($"{PREFIX} [DBG] {message}");

        public void Info(string message) => Logger.WriteLine($"{PREFIX} [INF] {message}");

        public void Error(string message) => Logger.WriteLine($"{PREFIX} [ERR] {message}");
    }
}