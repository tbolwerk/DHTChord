using System;
namespace DHT.DistributedChordNetwork
{
    public class TimeOutScheduler : ITimeOutScheduler
    {
        private readonly ITimeOutTimerFactory _timerFactory;

        public TimeOutScheduler(ITimeOutTimerFactory timerFactory)
        {
            _timerFactory = timerFactory;
        }

        public void AddTimeOutTimer(object origin, int maxAttempts, double timeOutInSeconds, Action onRetry,
            Action onTimeOut)
        {
            _timerFactory.Create(origin, timeOutInSeconds, maxAttempts, onRetry, onTimeOut);
        }

        public void StartTimer(object origin)
        {
            var timer = _timerFactory.Get(origin);
            timer?.Start();
        }

        public void StopTimer(object origin)
        {
            var timer = _timerFactory.Get(origin);
            timer?.Stop();
        }
    }

    public interface ITimeOutScheduler
    {
        public void AddTimeOutTimer(object origin, int maxAttempts, double timeOutInSeconds, Action onRetry,
            Action onTimeOut);

        public void StartTimer(object origin);
        public void StopTimer(object origin);
    }
}