using System;

namespace DHT
{
    public class CheckPredecessorResponseEventArgs : EventArgs
    {
        public NodeDto Predecessor { get; set; }
    }
}