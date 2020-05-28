using System.Threading.Tasks;

namespace DHT
{
    public interface IFingerTable
    {
        FingerTableEntry[] FingerTableEntries { get; }
        public void AddEntry(NodeDto node, int id);
        public void CreateFingerTable(int id);
        public NodeDto ClosestPrecedingNode(int id);
        public void FixFingers(int id, NodeDto connectionNode, Node destinationNode,
            IDhtRelayServiceAdapter relayServiceAdapter);

        public bool Include(int id);

        void AddEntries(NodeDto successor,  int id);
        string ToString();
    }
}