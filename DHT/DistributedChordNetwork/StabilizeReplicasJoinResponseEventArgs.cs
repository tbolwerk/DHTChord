using System;
using System.Collections.Generic;

namespace DHT
{
    public class StabilizeReplicasJoinResponseEventArgs:EventArgs
    {
        public uint Key { get; set; }
        public IEnumerable<KeyValuePair<uint, string>> Dictionary { get; set; }
    }
}