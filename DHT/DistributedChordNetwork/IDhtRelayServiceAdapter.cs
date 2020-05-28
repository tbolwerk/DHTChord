using System;
using System.Threading.Tasks;

namespace DHT
{
    public interface IDhtRelayServiceAdapter
    {
        public void SendRpcCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto);
        public void Notify(NodeDto connectingNode, NodeDto node);
        Task<DhtProtocolCommandDto> ServerAsync();
        void Client(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto);
        public event EventHandler NotifyHandler;
        public event EventHandler FindSuccessorHandler;
        public event EventHandler FoundSuccessorHandler;
        public event EventHandler StabilizeHandler;
        public event EventHandler StabilizeResponseHandler;
        public event EventHandler CheckPredecessorHandler;
        public event EventHandler CheckPredecessorResponseHandler;

        void Start();
    }
}