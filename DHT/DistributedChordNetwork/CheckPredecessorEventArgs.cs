using System;

namespace DHT
{
    public class CheckPredecessorEventArgs:EventArgs
    {
        public NodeDto DestinationNode { get; set; }
    }
}