using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using syp.biz.SockJS.NET.Client;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Test
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    internal class Client2Tester : ITestModule
    {
        public async Task Execute()
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                var config = SockJsConfiguration.Factory.BuildDefault();
                config.Logger = new ConsoleLogger();
                config.BaseEndpoint = new Uri("http://localhost:9999/echo");
                config.DefaultHeaders = new WebHeaderCollection
                {
                    {HttpRequestHeader.UserAgent, "Test"},
                    {"application-key", "foo-bar"}
                };
                var sockJs = (IClient)new Client.SockJS(config);

                sockJs.Connected += async (sender, e) =>
                {
                    try
                    {
                        Console.WriteLine("****************** Main: Open");
                        await sockJs.Send(JsonConvert.SerializeObject(new { foo = "bar" }));
                        await sockJs.Send("test");
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                };
                sockJs.Message += async (sender, msg) =>
                {
                    try
                    {
                        Console.WriteLine($"****************** Main: Message: {msg}");
                        if (msg != "test") return;
                        Console.WriteLine("****************** Main: Got back echo -> sending shutdown");
                        //                                sockJs.Send("shutdown");
                        //                            }
                        //                            else if (dataString == "ok")
                        //                            {
                        //                                Console.WriteLine($"****************** Main: Got back shutdown confirmation");
                        await sockJs.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                };
                sockJs.Disconnected += (sender, e) =>
                {
                    try
                    {
                        Console.WriteLine("****************** Main: Closed");
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                };

                await sockJs.Connect();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                throw;
            }

            await tcs.Task;
        }
    }
}
