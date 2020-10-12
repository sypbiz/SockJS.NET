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
            var config = syp.biz.SockJS.NET.Client2.SockJsConfiguration.Factory.BuildDefault();
            config.Logger = new ConsoleLogger();
            config.BaseEndpoint = new Uri("http://localhost:9999/echo");
            var sockJs = new syp.biz.SockJS.NET.Client2.SockJS(config);

            sockJs.Connected += (sender, e) =>
            {
                Console.WriteLine("****************** Main: Open");
                sockJs.Send(JsonConvert.SerializeObject(new { foo = "bar" }));
                sockJs.Send("test");
            };
            sockJs.Message += (sender, msg) =>
            {
                Console.WriteLine($"****************** Main: Message: {msg}");
                if (msg != "test") return;
                Console.WriteLine($"****************** Main: Got back echo -> sending shutdown");
                //                                sockJs.Send("shutdown");
                //                            }
                //                            else if (dataString == "ok")
                //                            {
                //                                Console.WriteLine($"****************** Main: Got back shutdown confirmation");
                sockJs.Disconnect().ConfigureAwait(false);
            };
            sockJs.Disconnected += (sender, e) =>
            {
                Console.WriteLine($"****************** Main: Closed");
                tcs.TrySetResult(true);
            };

            await sockJs.Connect();

            await tcs.Task;
        }
    }
}
