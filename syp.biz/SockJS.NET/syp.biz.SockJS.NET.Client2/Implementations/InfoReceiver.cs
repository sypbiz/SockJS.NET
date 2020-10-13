using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using syp.biz.SockJS.NET.Common.DTO;
using syp.biz.SockJS.NET.Common.Extensions;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Implementations
{
    internal class InfoReceiver
    {
        private readonly SockJsConfiguration.Factory.ReadOnlySockJsConfiguration _config;
        private readonly ILogger _log;

        public InfoReceiver(SockJsConfiguration.Factory.ReadOnlySockJsConfiguration config)
        {
            this._config = config;
            this._log = config.Logger;
        }

        public async Task<InfoDto> GetInfo()
        {
            this._log.Debug($"{nameof(this.GetInfo)}: Base URL: {this._config.BaseEndpoint}");
            await Task.Delay(0);
            var baseUri = this._config.BaseEndpoint;
            var url = new Uri(baseUri, $"{baseUri.AbsolutePath}/info?t={DateTimeOffset.Now.ToUnixTimeMilliseconds()}").OriginalString;
            this._log.Debug($"{nameof(this.GetInfo)}: Info URL: {url}");

            using var client = new HttpClient();
            using var cts = new CancellationTokenSource(this._config.InfoReceiverTimeout);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            this._config.DefaultHeaders?
                .OfType<string>()
                .Select(header => (header, value: this._config.DefaultHeaders[header]))
                .ForEach(i => request.Headers.Add(i.header, i.value));

            var stopwatch = Stopwatch.StartNew();
            var response = await client.SendAsync(request, cts.Token);
            response.EnsureSuccessStatusCode();
            stopwatch.Stop();

            var content = await response.Content.ReadAsStringAsync();
            var info = JsonConvert.DeserializeObject<InfoDto>(content);
            info.RoundTripTime = stopwatch.ElapsedMilliseconds;

            this._log.Debug($"{nameof(this.GetInfo)}: Finish {content} {info.RoundTripTime}");
            return info;
        }
    }
}