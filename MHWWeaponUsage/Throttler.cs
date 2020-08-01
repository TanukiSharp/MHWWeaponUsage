using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHWWeaponUsage
{
    public class Throttler
    {
        private readonly TimerCallback action;
        private readonly int timeBuffer;

        private Timer timer;

        public Throttler(Action action, int timeBuffer)
        {
            SynchronizationContext context = SynchronizationContext.Current;

            var contextFunc = new SendOrPostCallback(_ => action());
            var timerCallback = new TimerCallback(_ => context.Send(contextFunc, null));

            this.action = timerCallback;
            this.timeBuffer = timeBuffer;
        }

        public void Reset()
        {
            if (timer == null)
                timer = new Timer(action, null, timeBuffer, 0);
            else
                timer.Change(timeBuffer, 0);
        }
    }
}
