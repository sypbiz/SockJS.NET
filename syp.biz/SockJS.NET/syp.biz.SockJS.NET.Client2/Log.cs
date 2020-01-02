using System.Diagnostics;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client2
{
    internal static class Log
    {
        private static ILogger _log = new DefaultDebugger();

        internal static ILogger Logger
        {
            get => _log;
            set => _log = value ?? new DefaultDebugger();
        }

        [DebuggerStepThrough, DebuggerNonUserCode]
        public static void Debug(string message) => Logger.Debug(message);

        [DebuggerStepThrough, DebuggerNonUserCode]
        public static void Info(string message) => Logger.Info(message);

        [DebuggerStepThrough, DebuggerNonUserCode]
        public static void Error(string message) => Logger.Error(message);
    }
}
