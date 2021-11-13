using System;
using System.Collections.Generic;
using System.Text.Json;
using DHT.DistributedChordNetwork.EventArgs;
using Serilog;

namespace DHT.DistributedChordNetwork.Networking
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
        public event EventHandler RemoveDataFromExpiredReplicasHandler;
        public event EventHandler RemoveDataFromExpiredReplicasResponseHandler;

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
                    Log.Debug(action.Target?.ToString());
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, ToString());
                    Log.Debug(e.Message);
                    Log.Debug(e.StackTrace);
                    Log.Debug(e.InnerException?.Message);
                }
            }
        }

        public void Receive(string responseMessage)
        {
            var dhtProtocolCommandDto = JsonSerializer.Deserialize<DhtProtocolCommandDto>(responseMessage);
            Log.Debug(dhtProtocolCommandDto.ToString());
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
                case DhtCommand.REMOVE_DATA_FROM_EXPIRED_REPLICAS:
                    GetHandlerEventArgs removeDataFromExpiredReplicasEventArgs = new GetHandlerEventArgs
                    {
                        DhtProtocolCommandDto = dhtProtocolCommandDto
                    };
                    OnRemoveDataFromExpiredReplicas(removeDataFromExpiredReplicasEventArgs);
                    break;
                case DhtCommand.REMOVE_DATA_FROM_EXPIRED_REPLICAS_RESPONSE:
                    GetHandlerEventArgs removeDataFromExpiredReplicasResponseEventArgs = new GetHandlerEventArgs
                    {
                        DhtProtocolCommandDto = dhtProtocolCommandDto
                    };
                    OnRemoveDataFromExpiredReplicasResponseHandler(removeDataFromExpiredReplicasResponseEventArgs);
                    break;
                default:
                    break;
            }
        }

        private void OnRemoveDataFromExpiredReplicas(GetHandlerEventArgs e)
        {
            EventHandler handler = RemoveDataFromExpiredReplicasHandler;
            handler?.Invoke(this, e);
        }

        private void OnRemoveDataFromExpiredReplicasResponseHandler(GetHandlerEventArgs e)
        {
            EventHandler handler = RemoveDataFromExpiredReplicasResponseHandler;
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

        private void OnPut(System.EventArgs e)
        {
            EventHandler handler = PutHandler;
            handler?.Invoke(this, e);
        }

        private void OnNotify(System.EventArgs e)
        {
            EventHandler handler = NotifyHandler;
            handler?.Invoke(this, e);
        }

        private void OnFindSuccessor(System.EventArgs e)
        {
            EventHandler handler = FindSuccessorHandler;
            handler?.Invoke(this, e);
        }

        private void OnFoundSuccessor(System.EventArgs e)
        {
            EventHandler handler = FoundSuccessorHandler;
            handler?.Invoke(this, e);
        }

        private void OnStabilizeHandler(System.EventArgs e)
        {
            EventHandler handler = StabilizeHandler;
            handler?.Invoke(this, e);
        }

        private void OnStabilizeResponseHandler(System.EventArgs e)
        {
            EventHandler handler = StabilizeResponseHandler;
            handler?.Invoke(this, e);
        }

        private void OnCheckPredecessorHandler(System.EventArgs e)
        {
            EventHandler handler = CheckPredecessorHandler;
            handler?.Invoke(this, e);
        }

        private void OnCheckPredecessorResponseHandler(System.EventArgs e)
        {
            EventHandler handler = CheckPredecessorResponseHandler;
            handler?.Invoke(this, e);
        }

        public override string ToString()
        {
            return RpcCalls.ToArray().ToString() ?? string.Empty;
        }
    }
}