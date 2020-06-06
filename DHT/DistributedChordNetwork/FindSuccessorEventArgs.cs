using System;

namespace DHT
{
    public class FindSuccessorEventArgs : EventArgs
    {
        public NodeDto DestinationNode { get; set; }
        public uint Key { get; set; }
    }
}