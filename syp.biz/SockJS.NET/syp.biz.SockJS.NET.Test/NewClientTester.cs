using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Client2;

namespace syp.biz.SockJS.NET.Test
{
    internal class NewClientTester
    {
        public async Task Execute()
        {
            var tcs = new TaskCompletionSource<bool>();
            SockJS2.SetLogger(new ConsoleLogger());
            var sockJs = new SockJS2("http://localhost:9999/echo");
            sockJs.OpenEvent += (sender, e) =>
            {
                Console.WriteLine("****************** Main: Open");
                sockJs.Send(JsonConvert.SerializeObject(new { foo = "bar" }));
                sockJs.Send("test");
            };
            sockJs.MessageEvent += (sender, e) =>
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
            };
            sockJs.CloseEvent += (sender, e) =>
            {
                Console.WriteLine($"****************** Main: Closed");
                tcs.TrySetResult(e.wasClean);
            };

            await sockJs.StartAsync();

            await tcs.Task;
        }
    }
}
