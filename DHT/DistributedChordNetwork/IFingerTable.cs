using System.Threading.Tasks;

namespace DHT
{
    public interface IFingerTable
    {
        FingerTableEntry[] FingerTableEntries { get; }
        public void AddEntry(NodeDto node, uint id);
        public void CreateFingerTable(uint id);
        public NodeDto ClosestPrecedingNode(uint id);
        public void FixFingers(uint id, NodeDto connectionNode, Node destinationNode,
            IDhtRelayServiceAdapter relayServiceAdapter);

        public bool Include(uint id);

        void AddEntries(NodeDto successor, uint id);
        public Node? Node { get; set; }

        string ToString();
    }
}