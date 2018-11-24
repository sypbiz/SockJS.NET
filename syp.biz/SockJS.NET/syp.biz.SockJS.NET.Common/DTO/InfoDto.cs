using Newtonsoft.Json;

namespace syp.biz.SockJS.NET.Common.DTO
{
    public class InfoDto
    {
        public string BaseUrl { get; set; }

        [JsonProperty(PropertyName = "entropy")]
        public long Entropy { get; set; }

        [JsonProperty(PropertyName = "origins")]
        public string[] Origins { get; set; }

        [JsonProperty(PropertyName = "cookie_needed")]
        public bool CookieNeeded { get; set; }

        [JsonProperty(PropertyName = "websocket")]
        public bool WebSocket { get; set; }
        /*
{
    "entropy":296547767,
    "origins":["*:*"],
    "cookie_needed":true,
    "websocket":true
}
         */
    }
}