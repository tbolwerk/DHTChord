using System;
using System.Timers;

namespace DHT.DistributedChordNetwork
{
    public class SchedulerTask
    {
        public Timer Timer { get; set; }
        public Action Action { get; set; }
    }
}