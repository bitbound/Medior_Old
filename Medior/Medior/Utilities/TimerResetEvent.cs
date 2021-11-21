using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Utilities
{
    public class TimerResetEvent
    {
        private readonly AutoResetEvent _resetEvent;
        private readonly System.Timers.Timer _timer;

        public TimerResetEvent(bool initialState, TimeSpan setEvery)
        {
            _resetEvent = new AutoResetEvent(initialState);
            _timer = new System.Timers.Timer(setEvery.TotalMilliseconds);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void WaitOne()
        {
            _resetEvent.WaitOne();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            _resetEvent.Set();
        }
    }
}
