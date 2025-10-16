using System;
using System.Collections.Generic;

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subs = new();

    public void Publish<T>(T evt)
    {
        if (_subs.TryGetValue(typeof(T), out var list))
            foreach (var d in list) (d as Action<T>)?.Invoke(evt);
    }

    public void Subscribe<T>(Action<T> handler)
    {
        if (!_subs.TryGetValue(typeof(T), out var list))
        {
            list = new List<Delegate>();
            _subs[typeof(T)] = list;
        }
        list.Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        if (_subs.TryGetValue(typeof(T), out var list))
            list.Remove(handler);
    }
}
