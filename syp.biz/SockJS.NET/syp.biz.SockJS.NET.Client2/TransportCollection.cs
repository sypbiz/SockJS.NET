using System;
using System.Collections.Generic;
using System.Linq;
using syp.biz.SockJS.NET.Common.DTO;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client2
{
    internal class TransportCollection
    {
        private HashSet<ITransportFactory2> _main = new HashSet<ITransportFactory2>();
        private HashSet<ITransportFactory2> _facade = new HashSet<ITransportFactory2>();
        private TransportCollection() { }

        public IEnumerable<ITransportFactory2> Main
        {
            get => this._main;
            set => this._main = new HashSet<ITransportFactory2>(value);
        }
        public IEnumerable<ITransportFactory2> Facade
        {
            get => this._facade;
            set => this._facade = new HashSet<ITransportFactory2>(value);
        }

        public static TransportCollection FilterToEnabled(string[]? transportsWhitelist, InfoDto info)
        {
            var transports = new TransportCollection();
            if (transportsWhitelist is null) transportsWhitelist = Array.Empty<string>();

            // TODO: finish
//            foreach (var factory in Transports.TransportFactoryFactory.GetTransportFactories())
//            {
//                if (factory is null) continue;
//                if (transportsWhitelist.Length > 0 && !transportsWhitelist.Contains(factory.TransportName))
//                {
//                    Log.Debug($"{nameof(FilterToEnabled)}: Not in whitelist {factory.TransportName}");
//                    continue;
//                }
//
//                if (!factory.Enabled(info))
//                {
//                    Log.Debug($"{nameof(FilterToEnabled)}: Disabled {factory.TransportName}");
//                    continue;
//                }
//
//                Log.Debug($"{nameof(FilterToEnabled)}: Enabled {factory.TransportName}");
//                transports._main.Add(factory);
//                if (factory.FacadeTransport != null) transports._facade.Add(factory.FacadeTransport);
//            }

            return transports;
        }
    }
}