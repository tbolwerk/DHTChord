
using System;
using System.Collections.Generic;

namespace DHT.DistributedChordNetwork.Networking
{
    public interface IDhtRelayServiceAdapter
    {
        public void SendRpcCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto);
        public Queue<Action> RpcCalls { get; }

        string? ConnectionUrl { get; set; }
        void Run();
    }
}