using System;

namespace DHT
{
    public class FindSuccessorEventArgs : EventArgs
    {
        public NodeDto DestinationNode { get; set; }
        public int Key { get; set; }
    }
}