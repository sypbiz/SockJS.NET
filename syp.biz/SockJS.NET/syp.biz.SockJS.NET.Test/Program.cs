using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Test
{
    class Program
    {
        static async Task Main(/* string[] args */)
        {
            try
            {
                var testModules = typeof(Program).Assembly.GetTypes()
                    .Where(t => !t.IsAbstract && typeof(ITestModule).IsAssignableFrom(t))
                    .OrderBy(t => t.Name)
                    .ToImmutableArray();

                foreach (var type in testModules)
                {
                    Console.WriteLine($"Testing '{type.Name}'...");
                    var module = (ITestModule) Activator.CreateInstance(type);
                    await module!.Execute();
                }
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

    internal interface ITestModule
    {
        Task Execute();
    }

    internal class ConsoleLogger : ILogger, syp.biz.SockJS.NET.Client2.Interfaces.ILogger
    {
        [DebuggerStepThrough, DebuggerNonUserCode]
        public void Debug(string message) => Console.WriteLine($"{DateTime.Now:s} [DBG] {message}");

        [DebuggerStepThrough, DebuggerNonUserCode]
        public void Info(string message) => Console.WriteLine($"{DateTime.Now:s} [INF] {message}");

        [DebuggerStepThrough, DebuggerNonUserCode]
        public void Error(string message) => Console.WriteLine($"{DateTime.Now:s} [ERR] {message}");
    }
}
