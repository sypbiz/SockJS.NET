using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client2.Transports
{
    internal static class TransportFactoryFactory
    {
        private static readonly HashSet<ITransportFactory2> TransportFactories = new HashSet<ITransportFactory2>();

        static TransportFactoryFactory() => LoadInternals();

        private static IEnumerable<ITransportFactory2> ScanTypes(IEnumerable<Type> types)
        {
            return types.Where(t => typeof(ITransportFactory2).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(Activator.CreateInstance)
                .OfType<ITransportFactory2>();
        }

        private static IEnumerable<ITransportFactory2> ScanAssembly(Assembly assembly) => ScanTypes(assembly.GetExportedTypes());

        private static void LoadInternals() => AddTransports(ScanTypes(typeof(TransportFactoryFactory).Assembly.GetTypes()));

        private static void AddTransports(IEnumerable<ITransportFactory2> transportFactories)
        {
            foreach (var factory in transportFactories) TransportFactories.Add(factory);
        }

        public static IEnumerable<ITransportFactory2> GetTransportFactories() => TransportFactories;

        public static void RegisterTransportsInAssembly(Assembly assembly) => AddTransports(ScanAssembly(assembly));

        public static void RegisterTransportsTypes(params Type[] types) => AddTransports(ScanTypes(types));
    }
}