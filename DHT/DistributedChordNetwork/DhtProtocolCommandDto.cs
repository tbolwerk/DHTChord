using System.Collections.Specialized;

namespace DHT
{
    public class DhtProtocolCommandDto
    {
        public int Key { get; set; }
        public DhtCommand Command { get; set; }
        public NodeDto NodeDto { get; set; }
        
    }

   
}