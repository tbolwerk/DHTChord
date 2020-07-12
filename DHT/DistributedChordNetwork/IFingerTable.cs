using DHT.DistributedChordNetwork.Networking;
using RelayService.DataAccessService.RoutingDataAccess.DHT.DistributedChordNetwork;

namespace DHT.DistributedChordNetwork
{
    public interface IFingerTable
    {
        FingerTableEntry[] FingerTableEntries { get; }
        public void AddEntry(NodeDto node, uint id);
        public void CreateFingerTable(uint id);
        public NodeDto ClosestPrecedingNode(uint id);
        public void FixFingers(uint id, NodeDto connectionNode, Node destinationNode,
            IDhtRelayServiceAdapter relayServiceAdapter);
        public void FixFingers();
        public bool Include(uint id);

        void AddEntries(NodeDto successor, uint id);
        public Node? Node { get; set; }

        string ToString();
    }
}