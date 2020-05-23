using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DHT
{
    public class DhtRelayServiceAdapter : IDhtRelayServiceAdapter
    {
        private readonly IRelay _relay;
        public Task<DhtProtocolCommandDto> ClientAsync(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto)
        {
            throw new NotImplementedException();
        }

        public event EventHandler NotifyHandler;
        public event EventHandler FindSuccessorHandler;
        public event EventHandler FoundSuccessorHandler;
        public event EventHandler StabilizeHandler;
        public event EventHandler StabilizeResponseHandler;
        public void Start()
        {
            throw new NotImplementedException();
        }


        public DhtRelayServiceAdapter(IRelay relay)
        {
            _relay = relay;
        }


        public async Task<DhtProtocolCommandDto> ListeningForRequests()
        {
            DhtProtocolCommandDto protocolCommandDto = null;
            while (protocolCommandDto == null)
            {
                if (_relay.connected)
                {
                    byte[] response = await Task.Run((() => _relay.receive()));
                    string responseMessage = Encoding.UTF8.GetString(response);
                    protocolCommandDto = JsonSerializer.Deserialize<DhtProtocolCommandDto>(responseMessage);
                    if (protocolCommandDto.Command == DhtCommand.FIND_SUCCESSOR)
                    {
                        NotifyEventArgs eventArgs = new NotifyEventArgs {NodeDto = protocolCommandDto.NodeDto};
                        OnFindSuccessor(eventArgs);
                    }

                    if (protocolCommandDto.Command == DhtCommand.NOTIFY)
                    {
                        NotifyEventArgs eventArgs = new NotifyEventArgs {NodeDto = protocolCommandDto.NodeDto};
                        OnNotify(eventArgs);
                    }
                }
            }

            return protocolCommandDto;
        }


        public async Task<NodeDto> SendRPCCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto)
        {
            _relay.connect(connectingNode.IpAddress, connectingNode.Port);
            if (_relay.connected)
            {
                _relay.send(JsonSerializer.Serialize(protocolCommandDto));
            }
            else
            {
                throw new Exception("Cannot connect with " + connectingNode.IpAddress);
            }

            DhtProtocolCommandDto dhtProtocolCommandDto = await ListeningForRequests();
            _relay.close();
            return dhtProtocolCommandDto.NodeDto;
        }

        public async Task<NodeDto> GetSuccessor(NodeDto node)
        {
            _relay.connect(node.IpAddress, node.Port);

            if (_relay.connected)
            {
                _relay.send(JsonSerializer.Serialize(new DhtProtocolCommandDto {Command = DhtCommand.FIND_SUCCESSOR}));
            }
            else
            {
                throw new Exception("Cannot connect with successor");
            }

            DhtProtocolCommandDto dhtProtocolCommandDto = await ListeningForRequests();

            _relay.close();
            return dhtProtocolCommandDto.NodeDto;
        }

        public async Task<NodeDto> Notify(NodeDto connectingNode, NodeDto node)
        {
            _relay.connect(node.IpAddress, node.Port);

            if (_relay.connected)
            {
                _relay.send(JsonSerializer.Serialize(new DhtProtocolCommandDto {Command = DhtCommand.NOTIFY}));
            }
            else
            {
                throw new Exception("Cannot notify, no connection");
            }

            DhtProtocolCommandDto dhtProtocolCommandDto = await ListeningForRequests();

            _relay.close();

            return dhtProtocolCommandDto.NodeDto;
        }

        protected virtual void OnNotify(EventArgs e)
        {
            EventHandler handler = NotifyHandler;
            handler?.Invoke(this, e);
        }

        protected virtual void OnFindSuccessor(EventArgs e)
        {
            EventHandler handler = NotifyHandler;
            handler?.Invoke(this, e);
        }
    }
}