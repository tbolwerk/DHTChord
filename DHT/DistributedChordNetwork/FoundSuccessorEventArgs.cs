using System;

namespace DHT
{
    public class FoundSuccessorEventArgs: EventArgs
    {
            public NodeDto SuccessorNode { get; set; }
            public int Key { get; set; }
        }
}