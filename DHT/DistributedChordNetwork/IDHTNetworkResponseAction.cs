namespace DHT.DistributedChordNetwork
{
    public interface IDhtNetworkResponseAction
    {
        public void StabilizeResponse(NodeDto destinationNode, uint key, NodeDto myPredecessor);
        public void CheckPredecessorResponse(NodeDto destinationNode, uint key, NodeDto myPredecessor);
        public void FoundSuccessor(NodeDto destinationNode, uint key, NodeDto foundSuccessor);
        public void PutResponse(NodeDto connectingNode, NodeDto destinationNode, uint key,string value);
        public void GetResponse( NodeDto connectingNode, NodeDto destinationNode, uint key, string value);
        public void RemoveDataFromExpiredReplicasReponse(NodeDto connectingNode, uint keyToRemove);
    }
}