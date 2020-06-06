using System;

namespace DHT
{
    public class PutHandlerEventArgs :EventArgs
    {
        public DhtProtocolCommandDto? ProtocolCommandDto { get; set; }
    }
}