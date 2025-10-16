using System;
using System.Collections.Generic;

namespace ICXK3
{
    public static class Locator
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T instance) where T : class => _services[typeof(T)] = instance;
        public static T Resolve<T>() where T : class => (T)_services[typeof(T)];
        public static bool TryResolve<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }
    }
}
