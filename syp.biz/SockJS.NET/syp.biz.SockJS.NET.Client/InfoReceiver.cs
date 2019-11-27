using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using syp.biz.SockJS.NET.Client.Event;
using syp.biz.SockJS.NET.Common.DTO;

namespace syp.biz.SockJS.NET.Client
{
    //TODO: finish
    internal class InfoReceiver: EventEmitter
    {
        private readonly string _baseUrl;

        public InfoReceiver(string baseUrl)
        {
            Log.Debug($"{nameof(InfoReceiver)}: {baseUrl}");
            this._baseUrl = baseUrl;
            this.DoInfoRequest(baseUrl).ConfigureAwait(false);
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


        private async Task DoInfoRequest(string baseUrl)
        {
            await Task.Delay(0);
            var baseUri = new Uri(baseUrl);
            var url = new Uri(baseUri, $"{baseUri.AbsolutePath}/info?t={DateTimeOffset.Now.ToUnixTimeMilliseconds()}").OriginalString;
            Log.Debug($"{nameof(this.DoInfoRequest)}: {url}");

            try
            {
                using (var client = new HttpClient())
                using (var cts = new CancellationTokenSource(InfoReceiver.Timeout))
                {
                    var stopwatch = Stopwatch.StartNew();
                    var response = await client.GetAsync(url, cts.Token);
                    response.EnsureSuccessStatusCode();
                    stopwatch.Stop();

                    var content = await response.Content.ReadAsStringAsync();
                    var info = JsonConvert.DeserializeObject<InfoDto>(content);
                    var rtt = stopwatch.ElapsedMilliseconds;

                    Log.Debug($"{nameof(this.DoInfoRequest)}: Finish {content} {rtt}");
                    this.Cleanup(true);
                    this.Emit("finish", info, rtt);
                }
            }
            catch (TaskCanceledException)
            {
                this.OnTimeout();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                this.OnTimeout();
            }
        }

        private void OnTimeout()
        {
            Log.Debug($"{nameof(this.OnTimeout)}: Timeout");
            this.Cleanup(false);
            this.Emit("finish");
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
            this.RemoveAllListeners();
            this.Cleanup(false);
        }
    }
}