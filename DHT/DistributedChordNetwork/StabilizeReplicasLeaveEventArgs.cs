using System;
using System.Collections.Generic;

namespace DHT
{
    public class StabilizeReplicasLeaveEventArgs:EventArgs
    {
        public NodeDto DestinationNode { get; set; }
        public uint Key { get; set; }
        public IEnumerable<KeyValuePair<uint, string>> Dictionary { get; set; }
        public int CurrentNumberOfReplicas { get; set; }
    }
}