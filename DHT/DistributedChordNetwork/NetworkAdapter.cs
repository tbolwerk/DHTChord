using System;
using System.Collections.Generic;
using System.Text.Json;
using DHT.Formatting;
using Serilog;

namespace DHT
{
    public class NetworkAdapter : INetworkAdapter
    {
        public Queue<Action> RpcCalls { get; }
        public event EventHandler NotifyHandler;
        public event EventHandler FindSuccessorHandler;
        public event EventHandler FoundSuccessorHandler;
        public event EventHandler StabilizeHandler;
        public event EventHandler StabilizeResponseHandler;
        public event EventHandler CheckPredecessorHandler;
        public event EventHandler CheckPredecessorResponseHandler;
        public event EventHandler GetHandler;
        public event EventHandler GetReponseHandler;
        public event EventHandler PutHandler;
        public event EventHandler PutReponseHandler;
        public event EventHandler StabilizeReplicasHandler;
        public event EventHandler StabilizeReplicasResponseHandler;
        public event EventHandler StabilizeReplicasLeaveHandler;
        
        public NetworkAdapter()
        {
            RpcCalls = new Queue<Action>();
        }

        public void Send()
        {
            if (RpcCalls.TryDequeue(out Action? action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, ToString());
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(e.InnerException?.Message);
                }
            }
        }

        public void Receive(string responseMessage)
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
                case DhtCommand.GET:
                    GetHandlerEventArgs getHandlerEventArgs = new GetHandlerEventArgs
                    {
                        DhtProtocolCommandDto = dhtProtocolCommandDto
                    };
                    OnGet(getHandlerEventArgs);
                    break;
                case DhtCommand.GET_RESPONSE:
                    GetResponseHandlerEventArgs getResponseHandlerEventArgs = new GetResponseHandlerEventArgs
                    {
                        DestinationNode = dhtProtocolCommandDto.NodeDto,
                        Key = dhtProtocolCommandDto.Key,
                        Value = dhtProtocolCommandDto.Value,
                    };
                    OnGetResponseHandler(getResponseHandlerEventArgs);
                    break;
                case DhtCommand.PUT:
                    PutHandlerEventArgs putHandlerEventArgs = new PutHandlerEventArgs
                    {
                        ProtocolCommandDto = dhtProtocolCommandDto
                    };
                    OnPut(putHandlerEventArgs);
                    break;
                case DhtCommand.PUT_RESPONSE:
                    GetResponseHandlerEventArgs putReponseHandlerEventArgs = new GetResponseHandlerEventArgs
                    {
                        DestinationNode = dhtProtocolCommandDto.NodeDto,
                        Key = dhtProtocolCommandDto.Key,
                        Value = dhtProtocolCommandDto.Value,
                    };
                    OnPutResponseHandler(putReponseHandlerEventArgs);
                    break;
                case DhtCommand.STABILIZE_REPLICAS_JOIN:
                    StabilizeReplicasJoinEventArgs stabilizeReplicasJoinEventArgs = new StabilizeReplicasJoinEventArgs
                    {
                        DestinationNode = dhtProtocolCommandDto.NodeDto, Key = dhtProtocolCommandDto.Key,
                    };
                    OnStabilizeReplicasHandler(stabilizeReplicasJoinEventArgs);
                    break;
                case DhtCommand.STABILIZE_REPLICAS_JOIN_RESPONSE:
                    StabilizeReplicasJoinResponseEventArgs stabilizeReplicasJoinResponseEventArgs = new StabilizeReplicasJoinResponseEventArgs
                    {
                        Dictionary = dhtProtocolCommandDto.Dictionary, Key = dhtProtocolCommandDto.Key,
                    };
                    OnStabilizeReplicasResponseHandler(stabilizeReplicasJoinResponseEventArgs);
                    break;
                case DhtCommand.STABILIZE_REPLICAS_LEAVE:
                    StabilizeReplicasLeaveEventArgs stabilizeReplicasLeaveEventArgs = new StabilizeReplicasLeaveEventArgs
                    {
                        DestinationNode = dhtProtocolCommandDto.NodeDto, 
                        Key = dhtProtocolCommandDto.Key,
                        Dictionary = dhtProtocolCommandDto.Dictionary,
                        CurrentNumberOfReplicas = dhtProtocolCommandDto.CurrentNumberOfReplicas
                    };
                    OnStabilizeReplicasLeaveHandler(stabilizeReplicasLeaveEventArgs);
                    break;
                default:
                    break;
            }
        }
        
        private void OnStabilizeReplicasHandler(StabilizeReplicasJoinEventArgs e)
        {
            EventHandler handler = StabilizeReplicasHandler;
            handler?.Invoke(this, e);
        }
        
        private void OnStabilizeReplicasLeaveHandler(StabilizeReplicasLeaveEventArgs e)
        {
            EventHandler handler = StabilizeReplicasLeaveHandler;
            handler?.Invoke(this, e);
        }
        
        private void OnStabilizeReplicasResponseHandler(StabilizeReplicasJoinResponseEventArgs e)
        {
            EventHandler handler = StabilizeReplicasResponseHandler;
            handler?.Invoke(this, e);
        }

        private void OnGetResponseHandler(GetResponseHandlerEventArgs e)
        {
            EventHandler handler = GetReponseHandler;
            handler?.Invoke(this, e);
        }

        private void OnGet(GetHandlerEventArgs e)
        {
            EventHandler handler = GetHandler;
            handler?.Invoke(this, e);
        }

        private void OnPutResponseHandler(GetResponseHandlerEventArgs e)
        {
            EventHandler handler = PutReponseHandler;
            handler?.Invoke(this, e);
        }

        private void OnPut(EventArgs e)
        {
            EventHandler handler = PutHandler;
            handler?.Invoke(this, e);
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

        public override string ToString()
        {
            return RpcCalls.ToArray().ToString();
        }
    }
}