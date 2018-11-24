using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Client.Polyfills;
using syp.biz.SockJS.NET.Common.Extensions;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports.Lib.Driver
{
    internal class XhrDriver : EventEmitter, IAjaxObject, IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly HttpClient _client;

        protected XhrDriver(string method, string url, string payload, ITransportOptions opts)
        {
            Log.Debug($"{nameof(XhrDriver)}: {method} {url} {payload}");
            var parsedUrl = new Uri(url);

            /* NOT USING NODE.JS
              var options = {
                method: method
              , hostname: parsedUrl.hostname.replace(/\[|\]/g, '')
              , port: parsedUrl.port
              , path: parsedUrl.pathname + (parsedUrl.query || '')
              , headers: opts && opts.headers
              , agent: false
              };

              var protocol = parsedUrl.protocol === 'https:' ? https : http;
             */

            this._client = new HttpClient();
            var request = new HttpRequestMessage();
            if (opts?.Headers != null) foreach (var header in opts.Headers) request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            request.Method = new HttpMethod(method);
            request.RequestUri = parsedUrl;
            if (!payload.IsNullOrEmpty()) request.Content = new StringContent(payload, Encoding.UTF8);

            var response = this._client.SendAsync(request, this._cts.Token);
            response.ContinueWith(t => t.Result.EnsureSuccessStatusCode())
                .ContinueWith(t => t.Result.Content.ReadAsStreamAsync().Result)
                .ContinueWith(t =>
                {
                    var builder = new StringBuilder();
                    var buffer = new byte[1024];
                    var read = buffer.Length;
                    while (read == buffer.Length)
                    {
                        read = t.Result.ReadAsync(buffer, 0, buffer.Length, this._cts.Token).Result;
                        var chunk = Encoding.UTF8.GetString(buffer, 0, read);
                        Log.Debug($"{nameof(XhrDriver)}: Data {chunk}");
                        builder.Append(chunk);
                        this.Emit("chunk", 200, builder.ToString());
                    }

                    return builder.ToString();
                })
                .ContinueWith(t =>
                {
                    Log.Debug($"{nameof(XhrDriver)}: End");
                    this.Emit("finish", (int)response.Result.StatusCode, t.Result);
                    request.Dispose();
                    request = null;
                })
                .ContinueWith(t =>
                {
                    Log.Debug($"{nameof(XhrDriver)}: Error {t.Exception?.GetBaseException() ?? t.Exception}");
                    this.Emit("finish", 0, (t.Exception?.GetBaseException() ?? t.Exception)?.Message);
                }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.NotOnCanceled);
        }

        protected static bool Enabled => true;
        protected static bool SupportsCors => true;

        public void Close()
        {
            Log.Debug($"{nameof(this.Close)}: Close");
            this.RemoveAllListeners();
            if (this._client != null) this.Dispose();
        }

        public void Dispose()
        {
            this._cts?.Cancel();
            this._cts?.Dispose();
            this._client?.Dispose();
        }
    }
}
