using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using syp.biz.SockJS.NET.Common.DTO;

namespace syp.biz.SockJS.NET.Client2
{
    //TODO: finish
    internal class InfoReceiver
    {
        public event EventHandler<(InfoDto info, long roundTripTime)?>? Finish;

        private readonly string _baseUrl;

        public InfoReceiver(string baseUrl)
        {
            Log.Debug($"{nameof(InfoReceiver)}: {baseUrl}");
            this._baseUrl = baseUrl;
        }

        public static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(8);

        /* NOT RUNNING IN BROWSER
        InfoReceiver._getReceiver = function(baseUrl, url, urlInfo) {
          // determine method of CORS support (if needed)
          if (urlInfo.sameOrigin) {
            return new InfoAjax(url, XHRLocal);
          }
          if (XHRCors.enabled) {
            return new InfoAjax(url, XHRCors);
          }
          if (XDR.enabled && urlInfo.sameScheme) {
            return new InfoAjax(url, XDR);
          }
          if (InfoIframe.enabled()) {
            return new InfoIframe(baseUrl, url);
          }
          return new InfoAjax(url, XHRFake);
        };
        */

        public Task<(InfoDto info, long roundTripTime)> Execute() => this.DoInfoRequest(this._baseUrl);

        private async Task<(InfoDto info, long roundTripTime)> DoInfoRequest(string baseUrl)
        {
            await Task.Delay(0);
            var baseUri = new Uri(baseUrl);
            var url = new Uri(baseUri, $"{baseUri.AbsolutePath}/info?t={DateTimeOffset.Now.ToUnixTimeMilliseconds()}").OriginalString;
            Log.Debug($"{nameof(this.DoInfoRequest)}: {url}");

            try
            {
                using var client = new HttpClient();
                using var cts = new CancellationTokenSource(InfoReceiver.Timeout);

                var stopwatch = Stopwatch.StartNew();
                var response = await client.GetAsync(url, cts.Token);
                response.EnsureSuccessStatusCode();
                stopwatch.Stop();

                var content = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<InfoDto>(content);
                var roundTripTime = stopwatch.ElapsedMilliseconds;

                Log.Debug($"{nameof(this.DoInfoRequest)}: Finish {content} {roundTripTime}");
                this.Cleanup(true);

                var result = (info, roundTripTime);
                this.Finish?.Invoke(this, result);
                return result;
            }
            catch (TaskCanceledException)
            {
                this.OnTimeout();
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                this.OnTimeout();
                throw;
            }
        }

        private void OnTimeout()
        {
            Log.Debug($"{nameof(this.OnTimeout)}: Timeout");
            this.Cleanup(false);
            this.Finish?.Invoke(this, null);
        }

        private void Cleanup(bool wasClean)
        {
            Log.Debug(nameof(this.Cleanup));
            /* NOT RUNNING IN BROWSER
              clearTimeout(this.timeoutRef);
              this.timeoutRef = null;
              if (!wasClean && this.xo) {
                this.xo.close();
              }
              this.xo = null;
             */
        }

        public void Close()
        {
            Log.Debug(nameof(this.Close));
            this.Finish = null;
            this.Cleanup(false);
        }
    }
}