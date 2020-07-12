namespace DHT.DistributedChordNetwork
{
    public interface IDhtNetworkRequestAction
    {
        public void ForwardRequest(NodeDto connectingNode, DhtProtocolCommandDto request);
        public void Notify(NodeDto connectingNode, uint key, NodeDto destinationNode);

        public void FindSuccessor(NodeDto connectingNode, uint key, NodeDto destinationNode);

        public void CheckPredecessor(NodeDto predecessor, uint key, NodeDto self);

        public void Stabilize(NodeDto connectingNode, uint key, NodeDto destinationNode);

        void Get(in uint key, NodeDto? connectingNode, Node destinationNode);

        public void Put(NodeDto connectingNode, NodeDto destinationNode, uint key, string value, int currentNumberOfReplicas, uint keyToAdd);

        public void RemoveDataFromExpiredReplicas(NodeDto connectingNode, NodeDto destinationNode, uint key,
            uint keyToFind, int currentNumberOfReplicas);
    }
}