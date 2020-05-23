using System;
using System.Threading.Tasks;

namespace DHT
{
    public interface IDhtRelayServiceAdapter
    {
        public Task<NodeDto> SendRPCCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto);
        public Task<NodeDto> GetSuccessor(NodeDto node);
        public Task<NodeDto> Notify(NodeDto connectingNode, NodeDto node);
        Task<DhtProtocolCommandDto> ListeningForRequests();
        Task<DhtProtocolCommandDto> ClientAsync(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto);
        public event EventHandler NotifyHandler;
        public event EventHandler FindSuccessorHandler;
        public event EventHandler FoundSuccessorHandler;
        public event EventHandler StabilizeHandler;
        public event EventHandler StabilizeResponseHandler;

        void Start();
    }
}