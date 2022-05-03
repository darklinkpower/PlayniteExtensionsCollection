using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ImporterforAnilist
{
    public class MalSyncRateLimiter
    {
        private readonly DispatcherTimer Timer;
        private int apiReqRemaining;

        public MalSyncRateLimiter()
        {
            //MalSync Api allows 20 requests without limits. After 20, it opens a new request slot each 5 seconds.
            // Using 17 for safety
            apiReqRemaining = 17;

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromMilliseconds(5000);
            Timer.Tick += new EventHandler(Timer_Tick);
        }

        public void WaitForSlot()
        {
            Thread.Sleep(5000);
            if (apiReqRemaining == 0)
            {
                Thread.Sleep(5000);
            }

            Decrease();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (apiReqRemaining < 17)
            {
                apiReqRemaining++;
            }

            if (apiReqRemaining == 17)
            {
                Timer.Stop();
            }
        }

        internal void Decrease()
        {
            if (apiReqRemaining > 0)
            {
                apiReqRemaining--;
            }

            if (apiReqRemaining < 17 && !Timer.IsEnabled)
            {
                Timer.Start();
            }
        }
    }
}