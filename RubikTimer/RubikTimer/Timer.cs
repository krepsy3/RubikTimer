using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RubikTimer
{
    class Timer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void UpdateProperty(string propertyName) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        private DispatcherTimer timer;
        private Stopwatch watch;
        private bool countdown;

        private TimeSpan _timeproperty;
        public TimeSpan Timeproperty { get { return _timeproperty; } private set { _timeproperty = value; UpdateProperty("Timeproperty"); UpdateProperty("Time"); } }
        private TimeSpan temptime;
        public DateTime Time { get { return (new DateTime(0).Add(Timeproperty)); } }

        public bool IsCounting { get { return timer.IsEnabled; } }

        public Timer()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(500);
            timer.Tick += Tick;
            Timeproperty = new TimeSpan(0);
            watch = new Stopwatch();
        }

        private void Tick(object sender, EventArgs e)
        {
            if (countdown)
            {
                if (temptime <= watch.Elapsed)
                {
                    StopTime();
                    Timeproperty = new TimeSpan(0);
                }
                else
                {
                    Timeproperty = temptime - watch.Elapsed;
                }
            }

            else
            {
                Timeproperty = temptime + watch.Elapsed;
            }
        }

        public void SetCountdown(long ticks)
        {
            Timeproperty = new TimeSpan(ticks);
            temptime = new TimeSpan(ticks);
            countdown = true;
        }
        public void SetCountdown(int second, int millisecond) { SetCountdown(new DateTime(1, 1, 1, 0, 0, second, millisecond).Ticks); }
        public void SetCountdown(int minute, int second, int millisecond) { SetCountdown(new DateTime(1, 1, 1, 0, minute, second, millisecond).Ticks); }
        public void SetCountdown(int hour, int minute, int second, int millisecond) { SetCountdown(new DateTime(1, 1, 1, hour, minute, second, millisecond).Ticks); }

        public void Countdown(long ticks)
        {
            SetCountdown(ticks);
            watch.Start();
            timer.Start();
        }
        public void Countdown(int second, int millisecond) { Countdown(new DateTime(1, 1, 1, 0, 0, second, millisecond).Ticks); }
        public void Countdown(int minute, int second, int millisecond) { Countdown(new DateTime(1, 1, 1, 0, minute, second, millisecond).Ticks); }
        public void Countdown(int hour, int minute, int second, int millisecond) { Countdown(new DateTime(1, 1, 1, hour, minute, second, millisecond).Ticks); }
        public void Countdown()
        {
            watch.Start();
            timer.Start();
        }

        public void SetTime(long ticks)
        {
            countdown = false;
            Timeproperty = new TimeSpan(ticks);
            temptime = new TimeSpan(ticks);
        }
        public void SetTime(int second, int millisecond) { SetTime(new DateTime(1, 1, 1, 0, 0, second, millisecond).Ticks); }
        public void SetTime(int minute, int second, int millisecond) { SetTime(new DateTime(1, 1, 1, 0, minute, second, millisecond).Ticks); }
        public void SetTime(int hour, int minute, int second, int millisecond) { SetTime(new DateTime(1, 1, 1, hour, minute, second, millisecond).Ticks); }

        public void StartTime(long ticks)
        {
            SetTime(ticks);
            watch.Start();
            timer.Start();
        }
        public void StartTime() { StartTime(0); }
        public void StartTime(int second, int millisecond) { StartTime(new DateTime(1, 1, 1, 0, 0, second, millisecond).Ticks); }
        public void StartTime(int minute, int second, int millisecond) { StartTime(new DateTime(1, 1, 1, 0, minute, second, millisecond).Ticks); }
        public void StartTime(int hour, int minute, int second, int millisecond) { StartTime(new DateTime(1, 1, 1, hour, minute, second, millisecond).Ticks); }


        public void StopTime()
        {
            watch.Stop();
            timer.Stop();
        }

        public void ResetTime()
        {
            watch.Reset();
            Timeproperty = new TimeSpan(0);
        }
    }
}