using System;
using System.Collections.Generic;

namespace DHT
{
    public class DhtActions : IDhtActions
    {
        private readonly INetworkAdapter _networkAdapter;
        private readonly IDhtRelayServiceAdapter _relayServiceAdapter;

        public DhtActions(INetworkAdapter networkAdapter, IDhtRelayServiceAdapter relayServiceAdapter)
        {
            _networkAdapter = networkAdapter;
            _relayServiceAdapter = relayServiceAdapter;
        }

        public void Notify(NodeDto connectingNode, uint key, NodeDto successorNode)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto {NodeDto = successorNode, Key = key, Command = DhtCommand.NOTIFY};
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

      

        public void ForwardRequest(NodeDto connectingNode,DhtProtocolCommandDto protocolCommandDto)
        {
            EnqueueRpcCall(connectingNode,protocolCommandDto);
        }

        public void Put(NodeDto connectingNode, NodeDto destinationNode, uint key, string value,
            int currentNumberOfReplicas
             )
        {
            var protocolCommandDto = new DhtProtocolCommandDto
            {
                Command = DhtCommand.PUT,
                NodeDto = destinationNode,
                Key = key,
                Value = value,
                CurrentNumberOfReplicas = currentNumberOfReplicas + 1,
            };
            
            EnqueueRpcCall(connectingNode, protocolCommandDto);
        }

        public void StabilizeReplicasJoin(NodeDto connectingNode, NodeDto destinationNode, uint key)
        {
            DhtProtocolCommandDto protocolCommandDto =
                new DhtProtocolCommandDto {Command = DhtCommand.STABILIZE_REPLICAS_JOIN, NodeDto = destinationNode, Key = key};

            EnqueueRpcCall(connectingNode, protocolCommandDto); 
        }

        public void StabilizeReplicasLeave(NodeDto connectingNode, NodeDto destinationNode, uint key, KeyValuePair<uint, string>[] dictionary, int currentNumberOfReplicas)
        {
            var protocolCommandDto = new DhtProtocolCommandDto
            {
                Command = DhtCommand.STABILIZE_REPLICAS_LEAVE,
                NodeDto = destinationNode,
                Key = key,
                Dictionary =  dictionary,
                CurrentNumberOfReplicas = currentNumberOfReplicas+1
            };

            EnqueueRpcCall(connectingNode, protocolCommandDto);
            
        }

        public void PutResponse(NodeDto connectingNode, NodeDto destinationNode, uint key, string value)
        {
            var protocolCommandDto = new DhtProtocolCommandDto
            {
                Command = DhtCommand.PUT_RESPONSE,
                NodeDto = connectingNode,
                Key = key,
                Value = value,
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

        public void StabilizeReplicasJoinResponse(NodeDto connectingNode, uint key, IEnumerable<KeyValuePair<uint, string>> dictionary)
        {
            DhtProtocolCommandDto protocolCommandDto = new DhtProtocolCommandDto
            {
                Dictionary = dictionary, Key = key, Command = DhtCommand.STABILIZE_REPLICAS_JOIN_RESPONSE
            };

            EnqueueRpcCall(connectingNode, protocolCommandDto);
        }
        
        public void Start()
        {
            _relayServiceAdapter.Run();
        }

        public void Get(in uint key, NodeDto? successor, Node destinationNode)
        {
            var protocolCommandDto =
                new DhtProtocolCommandDto {Command = DhtCommand.GET, Key = key, NodeDto = destinationNode};
            EnqueueRpcCall(successor, protocolCommandDto);
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
            void Action() => _relayServiceAdapter.SendRpcCommand(connectingNode, protocolCommandDto);
            _networkAdapter.RpcCalls.Enqueue(Action);
        }
    }
}