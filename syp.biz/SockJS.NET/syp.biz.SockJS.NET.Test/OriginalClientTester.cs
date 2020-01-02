using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using syp.biz.SockJS.NET.Client.Event;

namespace syp.biz.SockJS.NET.Test
{
    internal class OriginalClientTester
    {
        public Task Execute()
        {
            var tcs = new TaskCompletionSource<bool>();
            Client.SockJS.SetLogger(new ConsoleLogger());
            var sockJs = new Client.SockJS("http://localhost:9999/echo");
            sockJs.Start();
            sockJs.AddOpenEventListener((sender, e) =>
            {
                Console.WriteLine("****************** Main: Open");
                sockJs.Send(JsonConvert.SerializeObject(new { foo = "bar" }));
                sockJs.Send("test");
            });
            sockJs.AddMessageEventListener((sender, e) =>
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
            sockJs.AddCloseEventListener((sender, e) =>
            {
                Console.WriteLine($"****************** Main: Closed");
                tcs.TrySetResult(true);
            });

            return tcs.Task;
        }
    }
}
