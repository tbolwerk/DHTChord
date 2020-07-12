using System;
using System.Linq;
using System.Timers;
using DHT.DistributedChordNetwork.EventArgs;
using DHT.DistributedChordNetwork.Networking;
using Microsoft.Extensions.Options;
using RelayService.DataAccessService.RoutingDataAccess.DHT.DistributedChordNetwork;

namespace DHT.DistributedChordNetwork
{
    public class NodeStabilizing : IStabilize
    {
        private readonly IOptions<DhtSettings> _options;
        private readonly IDhtActions _dhtActions;
        private readonly ITimeOutScheduler _timeOutScheduler;
        private const string OriginSuccessor = "successor";

        public Node? Node { get; set; }

        public NodeStabilizing(INetworkAdapter networkAdapter,
            IOptions<DhtSettings> options, IDhtActions dhtActions, ITimeOutScheduler timeOutScheduler,
            ISchedule scheduler)
        {
            _options = options;
            _dhtActions = dhtActions;
            _timeOutScheduler = timeOutScheduler;
            networkAdapter.StabilizeHandler += StabilizeHandler;
            networkAdapter.StabilizeResponseHandler += StabilizeResponseHandler;

            var dhtSettings = _options.Value;

            double timeOut = TimeSpan.FromSeconds(dhtSettings.TimeToLiveInSeconds).TotalMilliseconds;
            _timeOutScheduler.AddTimeOutTimer(OriginSuccessor, dhtSettings.MaxRetryAttempts + 1, timeOut, Stabilize,
                OnTimeOutStabilizeHandler);

            scheduler.Enqueue(new Timer(TimeSpan.FromSeconds(dhtSettings.StabilizeCallInSeconds).TotalMilliseconds),
                Stabilize);
        }

        private void StabilizeHandler(object? sender, System.EventArgs e)
        {
            StabilizeEventArgs eventArgs = (StabilizeEventArgs)e;
            // Console.WriteLine("Stabilize handler is called by " + eventArgs.DestinationNode);

            var stabilizingNode = eventArgs.DestinationNode;
            if (Node.Predecessor == null) Node.Predecessor = stabilizingNode;
            if ((Node.Predecessor.Id <= stabilizingNode.Id && Node.Predecessor.Id.Equals(Node.Id)) ||
                (Node.Predecessor.Id <= stabilizingNode.Id && stabilizingNode.Id < Node.Id) ||
                (Node.Predecessor.Id > Node.Id && Node.Predecessor.Id <= stabilizingNode.Id))
            {
                Node.Predecessor = stabilizingNode;
            }

            // Console.WriteLine("My predecessor is : " + Node.Predecessor);
            _dhtActions.StabilizeResponse(eventArgs.DestinationNode, Node.Predecessor.Id, Node.Predecessor);
            // Console.WriteLine("My predecessor is send to : " + eventArgs.DestinationNode);
        }

        public void Stabilize()
        {
            // TODO: bootstrap node should have the predecessor with largest id.
            // Otherwise bootstrap node will call on upon itself and block other messages.
            if (Node.Successor == null)
            {
                _dhtActions.FindSuccessor(Node.BootStrapNode, Node.Id, Node);
                return;
            }

            if (Node.Successor.Id.Equals(Node.Id)) return;

            _dhtActions.Stabilize(Node.Successor, Node.Id, Node);
            _timeOutScheduler.StartTimer(OriginSuccessor);
            // Console.WriteLine($"Stabilize {Node}");
        }

        private void OnTimeOutStabilizeHandler()
        {
            // Console.WriteLine($"Stabilize timeout {Node}");
            NodeDto nextClosestSuccessor = null;
            //TODO: fix connecting node, should be closest successor node from finger table!
            Node.Successor = Node.BootStrapNode;
            _dhtActions.Notify(Node.Successor, Node.Id, Node);

            return;
            if (Node.BootStrapNode.Id.Equals(Node.Id))
            {
                if (nextClosestSuccessor == null || nextClosestSuccessor.Id == Node.Id)
                {
                    Node.Successor = Node.Predecessor;
                }
                else
                {
                    Node.Successor = nextClosestSuccessor;
                }
            }
            else
            {
                //TODO: fix next closest preceding successor in order to relaibly fail nodes, otherwise they will go back to their previous successor who might be offline, so fingertable needs to be updates aswell.
                if (nextClosestSuccessor == null || Node.Successor == null || nextClosestSuccessor?.Id == Node.Id ||
                    nextClosestSuccessor?.Id == Node.Successor?.Id)
                {
                    Node.Successor = Node.BootStrapNode;
                }
                else
                {
                    Node.Successor = nextClosestSuccessor;
                }
            }

            // Console.WriteLine($"Stabilize timeout successor is {Node.Successor}");
            _dhtActions.Notify(Node.Successor, Node.Id, Node);
        }

        private void StabilizeResponseHandler(object? sender, System.EventArgs e)
        {
            // Console.WriteLine("Stabilize response handler " + Node);
            StabilizeResponseEventArgs eventArgs = (StabilizeResponseEventArgs)e;
            var predecessorOfSuccessor = eventArgs.PredecessorOfSuccessor;
            _timeOutScheduler.StopTimer(OriginSuccessor);
            if (predecessorOfSuccessor.Id != Node.Id)
            {
                // Console.WriteLine("predecessorOfSuccessor =  " + predecessorOfSuccessor);
                Node.Successor = predecessorOfSuccessor;
                _dhtActions.Notify(Node.Successor, Node.Id, Node);
            }
            else
            {
                if (Node.Successor != null && Node.Predecessor != null)
                {
                    StabilizeReplicaData();
                    RemoveDataFromExpiredReplicas();
                }
            }
        }

        private void RemoveDataFromExpiredReplicas()
        {
            //TODO: filter dictionary on replica entries only
            
            foreach (var KeyValuePair in Node.Hashtable)
            {
                _dhtActions.RemoveDataFromExpiredReplicas(Node.Successor, Node, KeyValuePair.Key, KeyValuePair.Key, 0);
            }
        }

        private void StabilizeReplicaData()
            {
                if (Node.Hashtable.Count() > 0)
                {
                    foreach (var KeyValuePair in Node.Hashtable)
                    {
                        _dhtActions.Put(Node.Successor, Node, KeyValuePair.Key, KeyValuePair.Value, 0,
                            KeyValuePair.Key);
                    }
                }
            }
        }

        public interface IStabilize
        {
            public Node? Node { set; }
            public void Stabilize();
        }
    }