using DHT.Formatting;

namespace DHT.DistributedChordNetwork
{
    public class NodeDto
    {
        public uint Id { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public virtual NodeDto? Predecessor { get; set; }
        public virtual NodeDto? Successor { get; set; }
        public override string ToString()
        {
            return new JsonCustomFormatter().SerializeObject(this, 2);
        }
    }
}