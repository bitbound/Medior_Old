using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Timer = System.Timers.Timer;

namespace Medior.Utilities
{
    public static class Debouncer
    {
        private static readonly ConcurrentDictionary<object, Timer> _timers = new();

        public static void Debounce(TimeSpan wait, Action action, [CallerMemberName] string key = "")
        {
            if (_timers.TryRemove(key, out var timer))
            {
                timer.Stop();
                timer.Dispose();
            }

            timer = new Timer(wait.TotalMilliseconds);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => action();
            _timers.TryAdd(key, timer);
            timer.Start();
        }
    }
}
