using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using syp.biz.SockJS.NET.Common.Interfaces;

namespace syp.biz.SockJS.NET.Client.Transports
{
    internal static class TransportFactoryFactory
    {
        private static readonly HashSet<ITransportFactory> TransportFactories = new HashSet<ITransportFactory>();

        static TransportFactoryFactory() => LoadInternals();

        private static IEnumerable<ITransportFactory> ScanTypes(IEnumerable<Type> types)
        {
            return types.Where(t => typeof(ITransportFactory).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(Activator.CreateInstance)
                .OfType<ITransportFactory>();
        }

        private static IEnumerable<ITransportFactory> ScanAssembly(Assembly assembly) => ScanTypes(assembly.GetExportedTypes());

        private static void LoadInternals() => AddTransports(ScanTypes(typeof(TransportFactoryFactory).Assembly.GetTypes()));

        private static void AddTransports(IEnumerable<ITransportFactory> transportFactories)
        {
            foreach (var factory in transportFactories) TransportFactories.Add(factory);
        }

        public static IEnumerable<ITransportFactory> GetTransportFactories() => TransportFactories;

        public static void RegisterTransportsInAssembly(Assembly assembly) => AddTransports(ScanAssembly(assembly));

        public static void RegisterTransportsTypes(params Type[] types) => AddTransports(ScanTypes(types));
    }
}