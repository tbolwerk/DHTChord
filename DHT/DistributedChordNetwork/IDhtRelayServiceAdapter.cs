using System;
using System.Threading.Tasks;

namespace DHT
{
    public interface IDhtRelayServiceAdapter
    {
        public void SendRpcCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto);

        string? ConnectionUrl { get; set; }
        void Run();
    }
}