using System;

namespace DHT
{
    public class NotifyEventArgs : EventArgs
    {
        public NodeDto NodeDto { get; set; }
    }
}