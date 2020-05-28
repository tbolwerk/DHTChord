using System;
using System.Threading.Tasks;
using System.Timers;

namespace DHT
{
    public class TimeOutScheduler
    {
        public event EventHandler TimeOutHandler;
        public event EventHandler RetryHandler;
        private readonly int _maxAttempts;
        private Timer _timer;
        private int _retry;
        private double timeOutInSeconds;

        public TimeOutScheduler(double timeOutInSeconds, int maxAttempts)
        {
            this.timeOutInSeconds = timeOutInSeconds;
            _maxAttempts = maxAttempts;
            _timer = new Timer(timeOutInSeconds);
            _retry = 0;
            _timer.Elapsed += OnTimeElapsed;
            _timer.AutoReset = false;
        }

        private void OnTimeElapsed(object sender, ElapsedEventArgs e)
        {
            if (_retry < _maxAttempts)
            {
                _retry += 1;
                OnRetryHandler();
                Stop();
                Start();
            }
            else
            {
                OnTimeOutHandler();
            }
        }


        public void Start()
        {

            if (!_timer.Enabled)
            {
                _timer.Start();
            }
        }

        public void Stop()
        {
            _timer.Interval += timeOutInSeconds;
            _timer.Stop();
        }

        protected virtual void OnTimeOutHandler()
        {
            Console.WriteLine("Timeout handler");
            _retry = 0;
            TimeOutHandler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRetryHandler()
        {
            Console.WriteLine("Retry handler");

            RetryHandler?.Invoke(this, EventArgs.Empty);
        }
    }
}