using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Configuration
    {
        public ICollection<ITransportFactory>? TransportFactories { get; set; }
        public Uri? BaseEndpoint { get; set; }
        public WebHeaderCollection? DefaultHeaders { get; set; }
        public ILogger? Logger { get; set; }
        public TimeSpan? InfoReceiverTimeout { get; set; }
        public ICredentials? Credentials { get; set; }
        public IWebProxy? Proxy { get; set; }
        public X509CertificateCollection? ClientCertificates { get; set; }
        public RemoteCertificateValidationCallback? RemoteCertificateValidator { get; set; }
        public CookieContainer? Cookies { get; set; }
        public TimeSpan? KeepAliveInterval { get; set; }

        internal Factory.ReadOnlySockJsConfiguration AsReadonly() => new Factory.ReadOnlySockJsConfiguration(this);

        public static class Factory
        {
            public static Configuration BuildDefault(string baseEndpoint)
            {
                return BuildDefault(new Uri(baseEndpoint ?? throw new ArgumentNullException(nameof(baseEndpoint))));
            }

            public static Configuration BuildDefault(Uri baseEndpoint)
            {
                if (baseEndpoint is null) throw new ArgumentNullException(nameof(baseEndpoint));

                return new Configuration
                {
                    BaseEndpoint = baseEndpoint,
                    TransportFactories = ReflectTransportFactories(),
                    DefaultHeaders = new WebHeaderCollection(),
                    Logger = new Implementations.NullLogger(),
                    InfoReceiverTimeout = TimeSpan.FromSeconds(8),
                    Credentials = null,
                    Proxy = null,
                    ClientCertificates = null,
                    RemoteCertificateValidator = null,
                    Cookies = null,
                    KeepAliveInterval = null,
                };
            }

            private static ICollection<ITransportFactory> ReflectTransportFactories()
            {
                var factories = typeof(SockJS)
                    .Assembly
                    .DefinedTypes
                    .Where(t => !t.IsAbstract)
                    .Where(t => typeof(ITransportFactory).IsAssignableFrom(t))
                    .Select(Activator.CreateInstance)
                    .OfType<ITransportFactory>()
                    .OrderByDescending(f => f.Priority)
                    .ToArray();
                return factories;
            }

            internal class ReadOnlySockJsConfiguration
            {
                public ReadOnlySockJsConfiguration(Configuration config)
                {
                    this.TransportFactories = config.TransportFactories ?? ReflectTransportFactories();
                    this.BaseEndpoint = config.BaseEndpoint ?? throw new ArgumentNullException(nameof(Configuration.BaseEndpoint));
                    this.DefaultHeaders = config.DefaultHeaders ?? new WebHeaderCollection();
                    this.Logger = config.Logger ?? new Implementations.NullLogger();
                    this.InfoReceiverTimeout = config.InfoReceiverTimeout ?? TimeSpan.FromSeconds(8);
                    this.Credentials = config.Credentials;
                    this.Proxy = config.Proxy;
                    this.ClientCertificates = config.ClientCertificates;
                    this.RemoteCertificateValidator = config.RemoteCertificateValidator;
                    this.Cookies = config.Cookies;
                    this.KeepAliveInterval = config.KeepAliveInterval;
                }

                public ICollection<ITransportFactory> TransportFactories { get; }
                public Uri BaseEndpoint { get; }
                public WebHeaderCollection DefaultHeaders { get; }
                public ILogger Logger { get; }
                public TimeSpan InfoReceiverTimeout { get; }
                public ICredentials? Credentials { get; }
                public IWebProxy? Proxy { get; }
                public X509CertificateCollection? ClientCertificates { get; }
                public RemoteCertificateValidationCallback? RemoteCertificateValidator { get; }
                public CookieContainer? Cookies { get; }
                public TimeSpan? KeepAliveInterval { get; }
            }
        }
    }
}