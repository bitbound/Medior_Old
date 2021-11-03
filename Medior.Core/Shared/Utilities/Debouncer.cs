using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Medior.Core.Shared.Utilities
{
    public static class Debouncer
    {
        private static readonly ConcurrentDictionary<object, Timer> _timers = new();

        public static void Debounce(object key, TimeSpan wait, Action action)
        {
            if (_timers.TryRemove(key, out var timer))
            {
                timer.Stop();
                timer.Dispose();
            }

            timer = new Timer(wait.TotalMilliseconds);
            timer.Elapsed += (s, e) => action();
            _timers.TryAdd(key, timer);
            timer.Start();
        }
    }
}
