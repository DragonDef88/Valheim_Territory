using System;
using System.Collections.Generic;

namespace ClanTerritory.Core
{
    internal static class ServiceContainer
    {
        private static readonly Dictionary<Type, object> Services =
            new Dictionary<Type, object>();

        public static void Register<TService>(TService service) where TService : class
        {
            if (service == null)
                throw new ArgumentNullException("service");

            Services[typeof(TService)] = service;
        }

        public static TService Get<TService>() where TService : class
        {
            object service;

            if (!Services.TryGetValue(typeof(TService), out service))
                throw new InvalidOperationException("Service is not registered: " + typeof(TService).FullName);

            return (TService)service;
        }

        public static bool TryGet<TService>(out TService service) where TService : class
        {
            object value;

            if (Services.TryGetValue(typeof(TService), out value))
            {
                service = value as TService;
                return service != null;
            }

            service = null;
            return false;
        }

        public static bool Has<TService>() where TService : class
        {
            return Services.ContainsKey(typeof(TService));
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}