using System.Threading;
using syp.biz.SockJS.NET.Common.DTO;

namespace syp.biz.SockJS.NET.Common.Interfaces
{
    // TODO: cleanup
    public interface ITransportFactory
    {
        ITransportFactory FacadeTransport { get; }
        long RoundTrips { get; }
        string TransportName { get; }
        bool Enabled(InfoDto info);
        ITransport Build(string transportName, string transportUrl, string originalTransportUrl, ITransportOptions options);
    }

    public interface ITransportFactory2
    {
        ITransportFactory2? FacadeTransport { get; }
        long RoundTrips { get; }
        string TransportName { get; }
        bool Enabled(InfoDto info);
        ITransport2 Build(string transportUrl, string originalTransportUrl, ITransportOptions options, CancellationToken cancel);
    }
}