using System.Threading;
using System.Threading.Tasks;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.Stomp.Net.Client
{
    public interface IStompClient : IClient
    {
        Task SetApiKey(string apiKey, CancellationToken token);
        Task SetApiKey(string apiKey);
        Task Subscribe(string destination, CancellationToken token);
        Task Subscribe(string destination);
    }
}