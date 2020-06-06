using System;

namespace DHT
{
    public class GetResponseHandlerEventArgs:EventArgs
    {
        public NodeDto DestinationNode { get; set; }
        public uint Key { get; set; }
        public string Value { get; set; }
    }
}