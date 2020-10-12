using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using syp.biz.SockJS.NET.Client2.Interfaces;
using syp.biz.SockJS.NET.Common.DTO;

namespace syp.biz.SockJS.NET.Client2.Implementations
{
    internal class InfoReceiver
    {
        private readonly SockJsConfiguration.Factory.ReadOnlySockJsConfiguration _config;
        private readonly ILogger _log;
        private readonly Uri _baseUrl;

        public InfoReceiver(SockJsConfiguration.Factory.ReadOnlySockJsConfiguration config)
        {
            this._config = config;
            this._log = config.Logger;
            this._baseUrl = config.BaseEndpoint;
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

            var stopwatch = Stopwatch.StartNew();
            var response = await client.GetAsync(url, cts.Token);
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