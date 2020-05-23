using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DHT.Formatting;
using NetMQ;
using NetMQ.Sockets;

namespace DHT
{
    public class DhtRelayNetMqAdapter : IDhtRelayServiceAdapter
    {
        private readonly string _connectionUrl;

        public DhtRelayNetMqAdapter(string connectionUrl)
        {
            _connectionUrl = connectionUrl;
        }

        public async Task<NodeDto> SendRPCCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto)
        {
            var response = await ClientAsync(connectingNode, protocolCommandDto);

            return response.NodeDto;
        }

        public Task<NodeDto> GetSuccessor(NodeDto node)
        {
            throw new NotImplementedException();
        }

        public async Task<DhtProtocolCommandDto> ClientAsync(NodeDto connectingNode,
            DhtProtocolCommandDto protocolCommandDto)
        {
            DhtProtocolCommandDto response = null;

            using (var client = new RequestSocket())
            {
                client.Connect("tcp://" + connectingNode.IpAddress + ":" + connectingNode.Port);
                client.SendFrame(JsonCustomFormatter.SerializeObject(protocolCommandDto, 2));
                var responseString = await client.ReceiveFrameStringAsync();
                response = JsonSerializer.Deserialize<DhtProtocolCommandDto>(responseString.Item1);
            }

            return response;
        }

        public async Task<NodeDto> Notify(NodeDto connectingNode, NodeDto node)
        {
            var response = await ClientAsync(connectingNode,
                new DhtProtocolCommandDto {Command = DhtCommand.NOTIFY, NodeDto = node});

            return response.NodeDto;
        }

        public Task<DhtProtocolCommandDto> ListeningForRequests()
        {
            using (var server = new ResponseSocket())
            {
                server.Bind("tcp://" + _connectionUrl);

                while (true)
                {
                    var responseMessage = server.ReceiveFrameString();


                    var requestDto = JsonSerializer.Deserialize<DhtProtocolCommandDto>(responseMessage);
                    if (requestDto.Command == DhtCommand.STABILIZE_RESPONSE)
                    {
                        StabilizeResponseEventArgs eventArgs = new StabilizeResponseEventArgs
                        {
                            PredecessorOfSuccessor = requestDto.NodeDto
                        };
                        OnStabilizeResponseHandler(eventArgs);
                    }
                    
                    if (requestDto.Command == DhtCommand.STABILIZE)
                    {
                        StabilizeEventArgs eventArgs = new StabilizeEventArgs
                        {
                            DestinationNode = requestDto.NodeDto
                        };
                        OnStabilizeHandler(eventArgs);
                    }

                    if (requestDto.Command == DhtCommand.FOUND_SUCCESSOR)
                    {
                        FoundSuccessorEventArgs eventArgs = new FoundSuccessorEventArgs
                            {SuccessorNode = requestDto.NodeDto, Key = requestDto.Key};

                        OnFoundSuccessor(eventArgs);
                    }

                    if (requestDto.Command == DhtCommand.FIND_SUCCESSOR)
                    {
                        FindSuccessorEventArgs eventArgs = new FindSuccessorEventArgs
                            {Key = requestDto.Key, DestinationNode = requestDto.NodeDto};
                        OnFindSuccessor(eventArgs);
                    }

                    if (requestDto.Command == DhtCommand.NOTIFY)
                    {
                        NotifyEventArgs eventArgs = new NotifyEventArgs {NodeDto = requestDto.NodeDto};
                        OnNotify(eventArgs);
                    }

                    server.SendFrame("World");
                }
            }
        }

        protected virtual void OnNotify(EventArgs e)
        {
            EventHandler handler = NotifyHandler;
            handler?.Invoke(this, e);
        }

        protected virtual void OnFindSuccessor(EventArgs e)
        {
            EventHandler handler = FindSuccessorHandler;
            handler?.Invoke(this, e);
        }

        protected virtual void OnFoundSuccessor(EventArgs e)
        {
            EventHandler handler = FoundSuccessorHandler;
            handler?.Invoke(this, e);
        }

        public event EventHandler NotifyHandler;
        public event EventHandler FindSuccessorHandler;
        public event EventHandler FoundSuccessorHandler;
        public event EventHandler StabilizeHandler;
        public event EventHandler StabilizeResponseHandler;
        public void Start()
        {
            using var runtime = new NetMQRuntime();
            runtime.Run(ListeningForRequests());
        }

        protected virtual void OnStabilizeHandler(EventArgs e)
        {
            EventHandler handler = StabilizeHandler;
            handler?.Invoke(this, e);
        }

        protected virtual void OnStabilizeResponseHandler(EventArgs e)
        {
            EventHandler handler = StabilizeResponseHandler;
            handler?.Invoke(this, e);
        }
    }
}