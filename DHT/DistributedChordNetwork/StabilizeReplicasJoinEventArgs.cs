using System;
using System.Collections.Generic;

namespace DHT
{
    public class StabilizeReplicasJoinEventArgs: EventArgs
    {
        public NodeDto DestinationNode { get; set; }
        public uint Key { get; set; }
    }
}