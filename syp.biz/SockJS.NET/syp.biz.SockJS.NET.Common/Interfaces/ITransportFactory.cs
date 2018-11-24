using syp.biz.SockJS.NET.Common.DTO;

namespace syp.biz.SockJS.NET.Common.Interfaces
{
    public interface ITransportFactory
    {
        ITransportFactory FacadeTransport { get; }
        long RoundTrips { get; }
        string TransportName { get; }
        bool Enabled(InfoDto info);
        ITransport Build(string transportName, string transportUrl, string originalTransportUrl, ITransportOptions options);
    }
}