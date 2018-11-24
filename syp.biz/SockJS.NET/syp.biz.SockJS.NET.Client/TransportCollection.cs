using System;
using System.Collections.Generic;
using System.Linq;
using syp.biz.SockJS.NET.Common.DTO;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client
{
    internal class TransportCollection
    {
        private HashSet<ITransportFactory> _main = new HashSet<ITransportFactory>();
        private HashSet<ITransportFactory> _facade = new HashSet<ITransportFactory>();
        private TransportCollection() { }

        public IEnumerable<ITransportFactory> Main
        {
            get => this._main;
            set => this._main = new HashSet<ITransportFactory>(value);
        }
        public IEnumerable<ITransportFactory> Facade
        {
            get => this._facade;
            set => this._facade = new HashSet<ITransportFactory>(value);
        }

        public static TransportCollection FilterToEnabled(string[] transportsWhitelist, InfoDto info)
        {
            var transports = new TransportCollection();
            if (transportsWhitelist is null) transportsWhitelist = Array.Empty<string>();

            foreach (var factory in Transports.TransportFactoryFactory.GetTransportFactories())
            {
                if (factory is null) continue;
                if (transportsWhitelist.Length > 0 && !transportsWhitelist.Contains(factory.TransportName))
                {
                    Log.Debug($"{nameof(FilterToEnabled)}: Not in whitelist {factory.TransportName}");
                    continue;
                }

                if (!factory.Enabled(info))
                {
                    Log.Debug($"{nameof(FilterToEnabled)}: Disabled {factory.TransportName}");
                    continue;
                }

                Log.Debug($"{nameof(FilterToEnabled)}: Enabled {factory.TransportName}");
                transports._main.Add(factory);
                if (factory.FacadeTransport != null) transports._facade.Add(factory.FacadeTransport);
            }

            return transports;
        }
    }
}