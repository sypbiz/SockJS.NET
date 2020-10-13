using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using syp.biz.SockJS.NET.Common.DTO;

namespace syp.biz.SockJS.NET.Common.Interfaces
{
    public interface ITransportConfiguration
    {
        Uri BaseEndpoint { get; }
        ILogger Logger { get; }
        InfoDto Info { get; }
        WebHeaderCollection DefaultRequestHeaders { get; }
        ICredentials? Credentials { get; }
        IWebProxy? Proxy { get; }
        X509CertificateCollection? ClientCertificates { get; }
        RemoteCertificateValidationCallback? RemoteCertificateValidator { get; }
        CookieContainer? Cookies { get; }
        TimeSpan? KeepAliveInterval { get; }
    }
}