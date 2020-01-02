using System;
using System.Diagnostics;
using System.Threading.Tasks;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // await new OriginalClientTester().Execute();
                await new NewClientTester().Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }

    internal class ConsoleLogger : ILogger
    {
        [DebuggerStepThrough, DebuggerNonUserCode]
        public void Debug(string message) => Console.WriteLine($"{DateTime.Now:s} [DBG] {message}");

        [DebuggerStepThrough, DebuggerNonUserCode]
        public void Info(string message) => Console.WriteLine($"{DateTime.Now:s} [INF] {message}");

        [DebuggerStepThrough, DebuggerNonUserCode]
        public void Error(string message) => Console.WriteLine($"{DateTime.Now:s} [ERR] {message}");
    }
}
