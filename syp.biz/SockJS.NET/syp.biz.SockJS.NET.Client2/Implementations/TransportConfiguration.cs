using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using syp.biz.SockJS.NET.Client2.Interfaces;
using syp.biz.SockJS.NET.Common.DTO;

namespace syp.biz.SockJS.NET.Client2.Implementations
{
    internal class TransportConfiguration : ITransportConfiguration
    {
        private readonly SockJsConfiguration.Factory.ReadOnlySockJsConfiguration _config;

        public TransportConfiguration(SockJsConfiguration.Factory.ReadOnlySockJsConfiguration config, InfoDto info)
        {
            this.Info = info;
            this._config = config;
        }

        #region Implementation of ITransportConfiguration
        public Uri BaseEndpoint => this._config.BaseEndpoint;
        public ILogger Logger => this._config.Logger;
        public InfoDto Info { get; }
        public WebHeaderCollection DefaultRequestHeaders => this._config.DefaultHeaders;
        public ICredentials? Credentials => this._config.Credentials;
        public IWebProxy? Proxy => this._config.Proxy;
        public X509CertificateCollection? ClientCertificates => this._config.ClientCertificates;
        public RemoteCertificateValidationCallback? RemoteCertificateValidator => this._config.RemoteCertificateValidator;
        public CookieContainer? Cookies => this._config.Cookies;
        public TimeSpan? KeepAliveInterval => this._config.KeepAliveInterval;
        #endregion Implementation of ITransportConfiguration
    }
}
