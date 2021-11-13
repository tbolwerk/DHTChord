using System;
using System.Timers;
using DHT.DistributedChordNetwork.EventArgs;
using DHT.DistributedChordNetwork.Networking;
using Microsoft.Extensions.Options;
using Serilog;

namespace DHT.DistributedChordNetwork
{
    public class NodeCheckingPredecessor : ICheckPredecessor
    {
        private readonly IDhtActions _dhtActions;
        private readonly ITimeOutScheduler _timeOutScheduler;
        private readonly ISchedule _scheduler;
        private const string OriginPredecessor = "predecessor";
        public Node? Node { get; set; }

        public NodeCheckingPredecessor(IDhtActions dhtActions, INetworkAdapter networkAdapter,
            IOptions<DhtSettings> options, ITimeOutScheduler timeOutScheduler, ISchedule scheduler)
        {
            _dhtActions = dhtActions;
            _timeOutScheduler = timeOutScheduler;
            _scheduler = scheduler;
            networkAdapter.CheckPredecessorHandler += CheckPredecessorHandler;
            networkAdapter.CheckPredecessorResponseHandler += CheckPredecessorResponseHandler;
            var dhtSettings = options.Value;

            double timeOut = TimeSpan.FromSeconds(dhtSettings.TimeToLiveInSeconds).TotalMilliseconds;

            _timeOutScheduler.AddTimeOutTimer(OriginPredecessor, dhtSettings.MaxRetryAttempts, timeOut,
                CheckPredecessor, TimeOutCheckPredecessorHandler);
            
            scheduler.Enqueue(
                new Timer(TimeSpan.FromSeconds(dhtSettings.CheckPredecessorCallInSeconds).TotalMilliseconds),
                CheckPredecessor);
        }

        private void CheckPredecessorResponseHandler(object? sender, System.EventArgs e)
        {
            CheckPredecessorResponseEventArgs eventArgs = (CheckPredecessorResponseEventArgs)e;
            Node.Predecessor = eventArgs.Predecessor;
            Log.Debug("Check predecessor response handler : " + Node);
        }

        private void CheckPredecessorHandler(object? sender, System.EventArgs e)
        {
            CheckPredecessorEventArgs eventArgs = (CheckPredecessorEventArgs)e;
            NodeDto destinationNode = eventArgs.DestinationNode;
            _dhtActions.CheckPredecessorResponse(destinationNode, Node.Id, Node);
        }

        public void CheckPredecessor()
        {
            if (Node.Predecessor == null)
            {
                _timeOutScheduler.StopTimer(OriginPredecessor);
                return;
            }

            _timeOutScheduler.StartTimer(OriginPredecessor);
            Log.Debug("Im called CheckPredecessor");
            _dhtActions.CheckPredecessor(Node.Predecessor, Node.Id, Node);
        }

        private void TimeOutCheckPredecessorHandler()
        {
            Log.Debug("No response from predecessor, so its dead we reset it to null");
            Node.Predecessor = null;
        }
    }

    public interface ICheckPredecessor
    {
        public Node? Node { set; }
        public void CheckPredecessor();
    }
}