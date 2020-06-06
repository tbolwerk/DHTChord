using System.Collections.Generic;
using System.Collections.Specialized;
using DHT.Formatting;

namespace DHT
{
    public class DhtProtocolCommandDto
    {
        public uint Key { get; set; }
        public DhtCommand Command { get; set; }
        public NodeDto NodeDto { get; set; }
        public string Value { get; set; }
        public int CurrentNumberOfReplicas { get; set; }
        public IEnumerable<KeyValuePair<uint, string>> Dictionary { get; set; }

        public override string ToString()
        {
           return new JsonCustomFormatter().SerializeObject(this, 2);
        }
    }

   
}