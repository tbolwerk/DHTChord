using System;

namespace DHT
{
    public class StabilizeResponseEventArgs : EventArgs
    {
        public NodeDto PredecessorOfSuccessor { get; set; }
    }
}