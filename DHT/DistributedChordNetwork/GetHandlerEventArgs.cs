using System;

namespace DHT
{
    public class GetHandlerEventArgs:EventArgs
    {
        public DhtProtocolCommandDto DhtProtocolCommandDto { get; set; }
    }
}