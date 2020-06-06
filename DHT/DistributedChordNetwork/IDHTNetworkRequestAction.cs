using System.Collections.Generic;

namespace DHT
{
    public interface IDHTNetworkRequestAction
    {
        public void ForwardRequest(NodeDto connectingNode, DhtProtocolCommandDto request);
        public void Notify(NodeDto connectingNode, uint key, NodeDto successorNode);

        public void FindSuccessor(NodeDto connectingNode, uint key, NodeDto destinationNode);

        public void CheckPredecessor(NodeDto predecessor, uint key, NodeDto self);

        public void Stabilize(NodeDto connectingNode, uint key, NodeDto destinationNode);

        
        public void Put(NodeDto connectingNode, NodeDto destinationNode, uint key, string value, int currentNumberOfReplicas);
        public void StabilizeReplicasJoin(NodeDto connectingNode, NodeDto destinationNode, uint key);

        public void StabilizeReplicasLeave(NodeDto connectingNode, NodeDto destinationNode, uint key,
            KeyValuePair<uint, string>[] dictionary, int currentNumberOfReplicas);

    }
}