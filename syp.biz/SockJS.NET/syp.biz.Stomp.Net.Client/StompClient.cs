using System;
using System.Threading;
using System.Threading.Tasks;
using syp.biz.SockJS.NET.Client;

namespace syp.biz.Stomp.Net.Client
{
    public class StompClient : SockJS.NET.Client.SockJS, IStompClient
    {
        #region Constructors
        public StompClient(string baseEndpoint) : 
            base(baseEndpoint)
        {
        }

        public StompClient(Uri baseEndpoint) : 
            base(baseEndpoint)
        {
        }

        public StompClient(Configuration config) : 
            base(config)
        {
        }
        #endregion

        public async Task SetApiKey(string apiKey) => await SetApiKey(apiKey, CancellationToken.None);
        public async Task SetApiKey(string apiKey, CancellationToken token)
        {
            var message = $"CONNECT\napiKey:{apiKey}\naccept-version:1.1,1.0\nheart-beat:10000,10000\n\n\u0000";
            await Send(message, token);
        }

        public async Task Subscribe(string destination) => await Subscribe(destination, CancellationToken.None);
        public async Task Subscribe(string destination, CancellationToken token)
        {
            var message = $"SUBSCRIBE\nid:sub-0\ndestination:{destination}\n\n\u0000";
            await Send(message, token);
        }
    }
}
