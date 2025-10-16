using System;
using System.Collections.Generic;

public static class ServiceRegistry
{
    static readonly Dictionary<Type, object> map = new();

    public static void Reset() => map.Clear();
    public static void Register<T>(T impl) => map[typeof(T)] = impl!;
    public static T Resolve<T>() => (T)map[typeof(T)];
}
