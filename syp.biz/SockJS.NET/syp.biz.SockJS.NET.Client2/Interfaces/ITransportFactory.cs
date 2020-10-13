using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace syp.biz.SockJS.NET.Client2.Interfaces
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface ITransportFactory
    {
        string Name { get; }
        bool Enabled { get; set; }
        uint Priority { get; set; }

        Task<ITransport> Build(ITransportConfiguration config);
    }
}