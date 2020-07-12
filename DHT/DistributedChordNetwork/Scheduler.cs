using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Options;

namespace DHT.DistributedChordNetwork
{
    public class Scheduler : ISchedule
    {
        private int _interval;
        private readonly Queue<SchedulerTask> _tasks;

        public Scheduler(IOptions<DhtSettings> options)
        {
            _interval = options.Value.IntervalBetweenPeriodicCallsInSeconds;
            _tasks = new Queue<SchedulerTask>();
        }

        public void Run()
        {
            SchedulerTask enqueued = _tasks.Dequeue();
            enqueued.Timer.Start();
            enqueued.Timer.AutoReset = false;
            enqueued.Timer.Elapsed += (sender, args) =>
            {
                enqueued.Action.Invoke();
                enqueued.Timer.Stop();

                if (_interval == 0)
                {
                    _interval = (int)enqueued.Timer.Interval;
                }

                _tasks.Enqueue(new SchedulerTask
                {
                    Action = enqueued.Action, Timer = new Timer {Interval = enqueued.Timer.Interval}
                });
                Task.Delay((int)enqueued.Timer.Interval).ContinueWith((task => Run()));
                enqueued.Timer.Dispose();
            };
        }

        public void Enqueue(Timer timer, Action action)
        {
            _tasks.Enqueue(new SchedulerTask {Timer = timer, Action = action});
        }
    }

    public interface ISchedule
    {
        void Enqueue(Timer timer, Action action);
        void Run();
    }
}