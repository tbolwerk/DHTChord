using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DHT.Formatting;
using NetMQ;
using NetMQ.Sockets;

namespace DHT
{
    public sealed class DhtRelayNetMqAdapter : IDhtRelayServiceAdapter
    {
        private readonly string _connectionUrl;

        public DhtRelayNetMqAdapter(string connectionUrl)
        {
            _connectionUrl = connectionUrl;
        }

        public void SendRpcCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto)
        {
            Client(connectingNode, protocolCommandDto);
        }


        public void Client(NodeDto connectingNode,
            DhtProtocolCommandDto protocolCommandDto)
        {
            try
            {
                if (connectingNode == null) throw new NullReferenceException("Connecting node is null");
                using (var client = new RequestSocket())
                {
                    var address = "tcp://" + connectingNode.IpAddress + ":" + connectingNode.Port;
                    client.Connect(address);
                    client.TrySendFrame(JsonCustomFormatter.SerializeObject(protocolCommandDto, 2));
                }
            }
            catch (NetMQException e)
            {
                Console.WriteLine(e.ErrorCode);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
            }
        }

        public void Notify(NodeDto connectingNode, NodeDto node)
        {
            Client(connectingNode,
                new DhtProtocolCommandDto {Command = DhtCommand.NOTIFY, NodeDto = node, Key = node.Id});
        }

        public Task<DhtProtocolCommandDto> ServerAsync()
        {
            using (var server = new ResponseSocket())
            {
                server.Bind("tcp://" + _connectionUrl);

                while (true)
                {
                    if (server.TryReceiveFrameString(out string responseMessage))
                    {
                        var dhtProtocolCommandDto = JsonSerializer.Deserialize<DhtProtocolCommandDto>(responseMessage);

                        switch (dhtProtocolCommandDto.Command)
                        {
                            case DhtCommand.NOTIFY:
                                NotifyEventArgs notifyEventArgs =
                                    new NotifyEventArgs {NodeDto = dhtProtocolCommandDto.NodeDto};
                                OnNotify(notifyEventArgs);
                                break;
                            case DhtCommand.STABILIZE:
                                StabilizeEventArgs stabilizeEventArgs =
                                    new StabilizeEventArgs {DestinationNode = dhtProtocolCommandDto.NodeDto};
                                OnStabilizeHandler(stabilizeEventArgs);
                                break;
                            case DhtCommand.FIND_SUCCESSOR:
                                FindSuccessorEventArgs findSuccessorEventArgs = new FindSuccessorEventArgs
                                {
                                    Key = dhtProtocolCommandDto.Key, DestinationNode = dhtProtocolCommandDto.NodeDto
                                };
                                OnFindSuccessor(findSuccessorEventArgs);
                                break;
                            case DhtCommand.FOUND_SUCCESSOR:
                                FoundSuccessorEventArgs foundSuccessorEventArgs = new FoundSuccessorEventArgs
                                {
                                    SuccessorNode = dhtProtocolCommandDto.NodeDto, Key = dhtProtocolCommandDto.Key
                                };
                                OnFoundSuccessor(foundSuccessorEventArgs);
                                break;
                            case DhtCommand.CHECK_PREDECESSOR:
                                CheckPredecessorEventArgs checkPredecessorEventArgs = new CheckPredecessorEventArgs
                                {
                                    DestinationNode = dhtProtocolCommandDto.NodeDto
                                };
                                OnCheckPredecessorHandler(checkPredecessorEventArgs);
                                break;
                            case DhtCommand.STABILIZE_RESPONSE:
                                StabilizeResponseEventArgs stabilizeResponseEventArgs = new StabilizeResponseEventArgs
                                {
                                    PredecessorOfSuccessor = dhtProtocolCommandDto.NodeDto
                                };
                                OnStabilizeResponseHandler(stabilizeResponseEventArgs);
                                break;
                            case DhtCommand.CHECK_PREDECESSOR_RESPONSE:
                                CheckPredecessorResponseEventArgs checkPredecessorResponseEventArgs =
                                    new CheckPredecessorResponseEventArgs {Predecessor = dhtProtocolCommandDto.NodeDto};
                                OnCheckPredecessorResponseHandler(checkPredecessorResponseEventArgs);
                                break;
                            default:
                                break;
                        }

                        server.SendFrameEmpty();
                    }
                }
            }
        }


        private void OnNotify(EventArgs e)
        {
            EventHandler handler = NotifyHandler;
            handler?.Invoke(this, e);
        }

        private void OnFindSuccessor(EventArgs e)
        {
            EventHandler handler = FindSuccessorHandler;
            handler?.Invoke(this, e);
        }

        private void OnFoundSuccessor(EventArgs e)
        {
            EventHandler handler = FoundSuccessorHandler;
            handler?.Invoke(this, e);
        }

        public event EventHandler NotifyHandler;
        public event EventHandler FindSuccessorHandler;
        public event EventHandler FoundSuccessorHandler;
        public event EventHandler StabilizeHandler;
        public event EventHandler StabilizeResponseHandler;
        public event EventHandler CheckPredecessorHandler;
        public event EventHandler CheckPredecessorResponseHandler;

        public void Start()
        {
            NetMQConfig.MaxSockets = 4048;
            using NetMQRuntime runtime = new NetMQRuntime();
            runtime.Run(ServerAsync());
        }

        private void OnStabilizeHandler(EventArgs e)
        {
            EventHandler handler = StabilizeHandler;
            handler?.Invoke(this, e);
        }

        private void OnStabilizeResponseHandler(EventArgs e)
        {
            EventHandler handler = StabilizeResponseHandler;
            handler?.Invoke(this, e);
        }

        private void OnCheckPredecessorHandler(EventArgs e)
        {
            EventHandler handler = CheckPredecessorHandler;
            handler?.Invoke(this, e);
        }

        private void OnCheckPredecessorResponseHandler(EventArgs e)
        {
            EventHandler handler = CheckPredecessorResponseHandler;
            handler?.Invoke(this, e);
        }
    }
}