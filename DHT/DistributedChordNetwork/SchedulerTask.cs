using System;
using System.Threading.Tasks;
using System.Timers;

namespace DHT
{
    public class SchedulerTask
    {
        public Timer Timer { get; set; }
        public Action Action { get; set; }
    }
}