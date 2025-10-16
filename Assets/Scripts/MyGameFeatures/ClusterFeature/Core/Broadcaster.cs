using System;
using System.Collections.Generic;
using UnityEngine;

namespace ICXK3
{
    public interface IService { }

    public interface IBroadcaster : IService
    {
        void Broadcast<T>(T arg);
        void Add<T>(Action<T> action);
        void Remove<T>(Action<T> action);
    }

    // Semplice event bus type-safe (Observer pattern)
    public class Broadcaster : IBroadcaster
    {
        private readonly Dictionary<Type, List<Delegate>> _map = new();

        public void Broadcast<T>(T arg)
        {
            var t = typeof(T);
            if (!_map.TryGetValue(t, out var list)) return;
            // Snapshot per sicurezza in caso di unsubscribe durante l'iterazione
            var copy = list.ToArray();
            foreach (var d in copy)
            {
                try { (d as Action<T>)?.Invoke(arg); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        public void Add<T>(Action<T> action)
        {
            var t = typeof(T);
            if (!_map.TryGetValue(t, out var list))
            {
                list = new List<Delegate>();
                _map[t] = list;
            }
            if (!list.Contains(action)) list.Add(action);
        }

        public void Remove<T>(Action<T> action)
        {
            var t = typeof(T);
            if (_map.TryGetValue(t, out var list))
            {
                list.Remove(action);
            }
        }
    }
}
