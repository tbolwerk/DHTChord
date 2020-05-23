using System;

namespace DHT
{
    public class StabilizeEventArgs : EventArgs
    {
        public NodeDto DestinationNode { get; set; }
    }
}