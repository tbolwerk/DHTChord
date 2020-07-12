
namespace DHT.DistributedChordNetwork.Networking
{
    public interface IDhtRelayServiceAdapter
    {
        public void SendRpcCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto);

        string? ConnectionUrl { get; set; }
        void Run();
    }
}