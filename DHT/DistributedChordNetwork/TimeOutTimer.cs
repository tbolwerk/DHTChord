using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Microsoft.Extensions.Options;

namespace DHT.DistributedChordNetwork
{
    public class TimeOutTimer
    {
        public object Origin { get; }
        private readonly double _timeOutInSeconds;
        private readonly int _maxAttempts;
        private readonly Timer _timer;
        private int _retry;
        public event EventHandler TimeOutHandler;
        public event EventHandler RetryHandler;

        public TimeOutTimer()
        {
        }

        public TimeOutTimer(IOptions<DhtSettings> options)
        {
            _timeOutInSeconds = options.Value.TimeToLiveInSeconds;
            _maxAttempts = options.Value.MaxRetryAttempts;
            _retry = 0;
            _timer = new Timer(_timeOutInSeconds);
        }

        public TimeOutTimer(object origin, in double timeOutInSeconds, in int maxAttempts)
        {
            Origin = origin;
            _timeOutInSeconds = timeOutInSeconds;
            _maxAttempts = maxAttempts;
            _retry = 0;

            _timer = new Timer(timeOutInSeconds);

            _timer.Elapsed += (sender, e) => OnTimeElapsed(sender, e, origin);
            _timer.AutoReset = false;
        }

        private void OnTimeElapsed(object sender, ElapsedEventArgs e, object origin)
        {
            if (_retry < _maxAttempts && origin == Origin)
            {
                _retry += 1;
                OnRetryHandler(origin);
                Stop();
                Start();
            }
            else if(origin == Origin)
            {
                OnTimeOutHandler(origin);
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
            if (_timer.Enabled)
            {
                _timer.Stop();
            }
        }

        protected virtual void OnTimeOutHandler(object origin)
        {
            Console.WriteLine("Timeout handler");
            _retry = 0;
            TimeOutEventArgs eventArgs = new TimeOutEventArgs {Origin = origin};
            TimeOutHandler?.Invoke(this, eventArgs);
        }

        protected virtual void OnRetryHandler(object origin)
        {
            Console.WriteLine("Retry handler");
            TimeOutEventArgs eventArgs = new TimeOutEventArgs {Origin = origin};
            RetryHandler?.Invoke(this, eventArgs);
        }
    }

    public class TimeOutTimerFactory : ITimeOutTimerFactory
    {
        private readonly List<TimeOutTimer> _timers;

        public TimeOutTimerFactory()
        {
            _timers = new List<TimeOutTimer>();
        }

        public TimeOutTimer? Get(object origin)
        {
            return _timers.FirstOrDefault(timeOutTimer => timeOutTimer.Origin.Equals(origin));
        }

        public void Create(object origin, in double timeOutInSeconds, in int maxAttempts, Action onRetry,
            Action onTimeOut)
        {
            TimeOutTimer? timer = null;
            if (_timers.Count > 0)
            {
                timer = Get(origin);
            }

            if (timer == null)
            {
                timer = new TimeOutTimer(origin, timeOutInSeconds, maxAttempts);
                timer.RetryHandler +=  (sender, args) =>
                {
                    if (args.GetType() == typeof(TimeOutEventArgs))
                    {
                        TimeOutEventArgs timeOutEventArgs = (TimeOutEventArgs)args;
                        if (timeOutEventArgs.Origin == timer.Origin)
                        {
                            onRetry();
                        }
                    }
                };
                timer.TimeOutHandler += (sender, args) =>
                {
                    if (args.GetType() == typeof(TimeOutEventArgs))
                    {
                        TimeOutEventArgs timeOutEventArgs = (TimeOutEventArgs)args;
                        if (timeOutEventArgs.Origin == timer.Origin)
                        {
                            onTimeOut();
                        }
                    }
                };
                _timers.Add(timer);
            }
        }
    }

    public interface ITimeOutTimerFactory
    {
        TimeOutTimer? Get(object origin);
        void Create(object origin, in double timeOutInSeconds, in int maxAttempts, Action onRetry, Action onTimeOut);
    }

    public class TimeOutEventArgs : System.EventArgs
    {
        public object Origin { get; set; }
    }
}