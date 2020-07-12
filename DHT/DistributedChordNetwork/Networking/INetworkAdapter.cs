using System;
using System.Collections.Generic;

namespace DHT.DistributedChordNetwork.Networking
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
        public event EventHandler RemoveDataFromExpiredReplicasHandler;
        public event EventHandler RemoveDataFromExpiredReplicasResponseHandler;

        public void Send();
        void Receive(string responseMessage);
    }
}