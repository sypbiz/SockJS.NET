using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using syp.biz.SockJS.NET.Client2.Interfaces;

namespace syp.biz.SockJS.NET.Client2
{
    public class SockJsConfiguration
    {
        public ICollection<ITransportFactory>? TransportFactories { get; set; }
        public Uri? BaseEndpoint { get; set; }
        public NameValueCollection? DefaultHeaders { get; set; }
        public ILogger? Logger { get; set; }
        public TimeSpan? InfoReceiverTimeout { get; set; }

        internal Factory.ReadOnlySockJsConfiguration AsReadonly() => new Factory.ReadOnlySockJsConfiguration(this);

        public static class Factory
        {
            public static SockJsConfiguration BuildDefault()
            {
                return new SockJsConfiguration
                {
                    TransportFactories = ReflectTransportFactories(),
                    BaseEndpoint = null,
                    DefaultHeaders = new NameValueCollection(),
                    Logger = new Implementations.NullLogger(),
                    InfoReceiverTimeout = TimeSpan.FromSeconds(8),
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
                public ReadOnlySockJsConfiguration(SockJsConfiguration config)
                {
                    this.TransportFactories = config.TransportFactories ?? ReflectTransportFactories();
                    this.BaseEndpoint = config.BaseEndpoint ?? throw new ArgumentNullException(nameof(SockJsConfiguration.BaseEndpoint));
                    this.DefaultHeaders = config.DefaultHeaders ?? new NameValueCollection();
                    this.Logger = config.Logger ?? new Implementations.NullLogger();
                    this.InfoReceiverTimeout = config.InfoReceiverTimeout ?? TimeSpan.FromSeconds(8);
                }

                public ICollection<ITransportFactory> TransportFactories { get; }
                public Uri BaseEndpoint { get; }
                public NameValueCollection DefaultHeaders { get; }
                public ILogger Logger { get; }
                public TimeSpan InfoReceiverTimeout { get; }
            }
        }
    }
}