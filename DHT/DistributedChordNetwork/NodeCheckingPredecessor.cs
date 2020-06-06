using System;
using System.Timers;
using Microsoft.Extensions.Options;

namespace DHT
{
    public class NodeCheckingPredecessor : ICheckPredecessor
    {
        private readonly IDhtActions _dhtActions;
        private readonly ITimeOutScheduler _timeOutScheduler;
        private readonly ISchedule _scheduler;
        private string ORIGIN_PREDECESSOR = "predecessor";
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

            _timeOutScheduler.AddTimeOutTimer(ORIGIN_PREDECESSOR, dhtSettings.MaxRetryAttempts, timeOut,
                CheckPredecessor, TimeOutCheckPredecessorHandler);

            scheduler.Enqueue(
                new Timer(TimeSpan.FromSeconds(dhtSettings.CheckPredecessorCallInSeconds).TotalMilliseconds),
                CheckPredecessor);
        }

        private void CheckPredecessorResponseHandler(object? sender, EventArgs e)
        {
            CheckPredecessorResponseEventArgs eventArgs = (CheckPredecessorResponseEventArgs)e;
            Node.Predecessor = eventArgs.Predecessor;
            Console.WriteLine("Check predecessor response handler : " + Node);
        }

        private void CheckPredecessorHandler(object? sender, EventArgs e)
        {
            CheckPredecessorEventArgs eventArgs = (CheckPredecessorEventArgs)e;
            NodeDto destinationNode = eventArgs.DestinationNode;
            _dhtActions.CheckPredecessorResponse(destinationNode, Node.Id, Node);
        }

        public void CheckPredecessor()
        {
            if (Node.Predecessor == null) return;
            _timeOutScheduler.StartTimer(ORIGIN_PREDECESSOR);
            Console.WriteLine("Im called CheckPredecessor");
            _dhtActions.CheckPredecessor(Node.Predecessor, Node.Id, Node);
        }

        private void TimeOutCheckPredecessorHandler()
        {
            Console.WriteLine("No response from predecessor, so its dead we reset it to null");
            Node.Predecessor = null;
        }
    }

    public interface ICheckPredecessor
    {
        public Node? Node { set; }
        public void CheckPredecessor();
    }
}