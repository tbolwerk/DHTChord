using DHT.DistributedChordNetwork.Networking;

namespace DHT.DistributedChordNetwork
{
    public class DhtActions : IDhtActions
    {
        private readonly IDhtRelayServiceAdapter _relayServiceAdapter;

        public DhtActions(IDhtRelayServiceAdapter relayServiceAdapter)
        {
            _relayServiceAdapter = relayServiceAdapter;
        }

        public void Notify(NodeDto connectingNode, uint key, NodeDto destinationNode)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto {NodeDto = destinationNode, Key = key, Command = DhtCommand.NOTIFY};
            EnqueueRpcCall(connectingNode, protocolCommandDto);
        }

        public void FindSuccessor(NodeDto connectingNode, uint key, NodeDto destinationNode)
        {
            var protocolCommandDto = new DhtProtocolCommandDto
            {
                Command = DhtCommand.FIND_SUCCESSOR, Key = key, NodeDto = destinationNode
            };
            EnqueueRpcCall(connectingNode, protocolCommandDto);
        }

        public void Stabilize(NodeDto connectingNode, uint key, NodeDto destinationNode)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto {Command = DhtCommand.STABILIZE, Key = key, NodeDto = destinationNode};
            EnqueueRpcCall(connectingNode, protocolCommandDto);
        }

        public void CheckPredecessorResponse(NodeDto destinationNode, uint key, NodeDto myPredecessor)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto
                {
                    Command = DhtCommand.CHECK_PREDECESSOR_RESPONSE, Key = key, NodeDto = myPredecessor
                };
            EnqueueRpcCall(destinationNode, protocolCommandDto);
        }

        public void FoundSuccessor(NodeDto destinationNode, uint key, NodeDto foundSuccessor)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto {Command = DhtCommand.FOUND_SUCCESSOR, Key = key, NodeDto = foundSuccessor};
            EnqueueRpcCall(destinationNode, protocolCommandDto);
        }


        public void ForwardRequest(NodeDto connectingNode, DhtProtocolCommandDto request)
        {
            EnqueueRpcCall(connectingNode, request);
        }

        public void Put(NodeDto connectingNode, NodeDto destinationNode, uint key, string value,
            int currentNumberOfReplicas, uint keyToAdd)
        {
            var protocolCommandDto = new DhtProtocolCommandDto
            {
                Command = DhtCommand.PUT,
                NodeDto = destinationNode,
                Key = key,
                Value = value,
                KeyToAdd = keyToAdd,
                CurrentNumberOfReplicas = currentNumberOfReplicas,
            };

            EnqueueRpcCall(connectingNode, protocolCommandDto);
        }

        public void PutResponse(NodeDto connectingNode, NodeDto destinationNode, uint key, string value)
        {
            var protocolCommandDto = new DhtProtocolCommandDto
            {
                Command = DhtCommand.PUT_RESPONSE, NodeDto = connectingNode, Key = key, Value = value,
            };

            EnqueueRpcCall(destinationNode, protocolCommandDto);
        }

        public void GetResponse(NodeDto connectingNode, NodeDto destinationNode, uint key,
            string value)
        {
            var protocolCommandDto = new DhtProtocolCommandDto
            {
                Command = DhtCommand.GET_RESPONSE, NodeDto = destinationNode, Key = key, Value = value
            };

            EnqueueRpcCall(destinationNode, protocolCommandDto);
        }

        public void Start()
        {
            _relayServiceAdapter.Run();
        }

        public void Get(in uint key, NodeDto? connectingNode, Node destinationNode)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto {Command = DhtCommand.GET, Key = key, NodeDto = destinationNode};
            EnqueueRpcCall(connectingNode, protocolCommandDto);
        }

        public void RemoveDataFromExpiredReplicas(NodeDto connectingNode, NodeDto destinationNode, uint key,
            uint keyToFind, int currentNumberOfReplicas)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto
                {
                    Command = DhtCommand.REMOVE_DATA_FROM_EXPIRED_REPLICAS,
                    Key = key,
                    NodeDto = destinationNode,
                    KeyToAdd = keyToFind,
                    CurrentNumberOfReplicas = currentNumberOfReplicas
                };

            EnqueueRpcCall(connectingNode, protocolCommandDto);
        }

        public void RemoveDataFromExpiredReplicasReponse(NodeDto connectingNode, uint keyToRemove)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto
                {
                    Command = DhtCommand.REMOVE_DATA_FROM_EXPIRED_REPLICAS_RESPONSE, Key = keyToRemove
                };

            EnqueueRpcCall(connectingNode, protocolCommandDto);
        }

        public void StabilizeResponse(NodeDto destinationNode, uint key, NodeDto myPredecessor)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto {Command = DhtCommand.STABILIZE_RESPONSE, Key = key, NodeDto = myPredecessor};
            EnqueueRpcCall(destinationNode, protocolCommandDto);
        }

        public void CheckPredecessor(NodeDto predecessor, uint key, NodeDto self)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto {Command = DhtCommand.CHECK_PREDECESSOR, Key = key, NodeDto = self};
            EnqueueRpcCall(predecessor, protocolCommandDto);
        }

        private void EnqueueRpcCall(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto)
        {
            _relayServiceAdapter.SendRpcCommand(connectingNode, protocolCommandDto);
        }
    }
}