using System;
using System.Collections.Generic;

namespace DHT
{
    public interface INetworkAdapter
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


        public void Send();
        void Receive(string responseMessage);
    }
}