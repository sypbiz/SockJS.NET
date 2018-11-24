using System;
using System.Diagnostics;
using System.Linq;
using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Client.SockJS.SetLogger(new ConsoleLogger());
                var sockJs = new Client.SockJS("http://localhost:9999/echo");
                sockJs.AddEventListener("open", (sender, e) =>
                {
                    Console.WriteLine("****************** Main: Open");
                    sockJs.Send("test");
                });
                sockJs.AddEventListener("message", (sender, e) =>
                {
                    Console.WriteLine($"****************** Main: Message: {string.Join(",", e.Select(o => o?.ToString()))}");
                    if (e[0] is TransportMessageEvent msg)
                    {
                        var dataString = msg.Data.ToString();
                        if (dataString == "test")
                        {
                            Console.WriteLine($"****************** Main: Got back echo -> sending shutdown");
//                                sockJs.Send("shutdown");
//                            }
//                            else if (dataString == "ok")
//                            {
//                                Console.WriteLine($"****************** Main: Got back shutdown confirmation");
                            sockJs.Close();
                        }
                    }
                });
                sockJs.AddEventListener("close", (sender, e) =>
                {
                    Console.WriteLine($"****************** Main: Closed");
                });
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
