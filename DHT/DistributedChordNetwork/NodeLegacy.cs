using System;
using System.Timers;
using DHT.DistributedChordNetwork.EventArgs;
using DHT.DistributedChordNetwork.Networking;
using Microsoft.Extensions.Options;
using RelayService.DataAccessService.RoutingDataAccess.DHT.DistributedChordNetwork;

namespace DHT.DistributedChordNetwork
{
    /// <summary>
    /// Deprecated
    /// </summary>
    public class NodeLegacy : NodeDto
    {
        public NodeDto BootStrapNode { get; set; }
        private readonly IFingerTable _fingerTable;
        private readonly IDhtActions _dhtActions;
        private readonly ITimeOutScheduler _timeOutScheduler;

        const string ORIGIN_SUCCESSOR = "successor";
        const string ORIGIN_PREDECESSOR = "predecessor";

        private NodeDto _successor;
        private NodeDto _predecessor;

        public override NodeDto? Successor
        {
            get => _successor;
            set
            {
                _successor = value;
                _timeOutScheduler.StopTimer(ORIGIN_SUCCESSOR);
            }
        }

        public override NodeDto? Predecessor
        {
            get => _predecessor;
            set
            {
                _predecessor = value;
                _timeOutScheduler.StopTimer(ORIGIN_PREDECESSOR);
            }
        }

        public NodeLegacy(INetworkAdapter networkAdapter, IFingerTable fingerTable,
            IOptions<DhtSettings> options, IDhtActions dhtActions, ITimeOutScheduler timeOutScheduler,
            ISchedule scheduler)
        {
            _fingerTable = fingerTable;
            _dhtActions = dhtActions;
            _timeOutScheduler = timeOutScheduler;
            networkAdapter.NotifyHandler += NotifyHandler;
            networkAdapter.FindSuccessorHandler += FindSuccessorHandler;
            networkAdapter.FoundSuccessorHandler += FoundSuccessorHandler;
            networkAdapter.StabilizeHandler += StabilizeHandler;
            networkAdapter.StabilizeResponseHandler += StabilizeResponseHandler;
            networkAdapter.CheckPredecessorHandler += CheckPredecessorHandler;
            networkAdapter.CheckPredecessorResponseHandler += CheckPredecessorResponseHandler;

            DhtSettings dhtSettings = options.Value;

            double timeOut = TimeSpan.FromSeconds(dhtSettings.TimeToLiveInSeconds).TotalMilliseconds;
            _timeOutScheduler.AddTimeOutTimer(ORIGIN_SUCCESSOR, dhtSettings.MaxRetryAttempts, timeOut, Stabilize,
                OnTimeOutStabilizeHandler);
            _timeOutScheduler.AddTimeOutTimer(ORIGIN_PREDECESSOR, dhtSettings.MaxRetryAttempts, timeOut,
                CheckPredecessor, TimeOutCheckPredecessorHandler);

            scheduler.Enqueue(new Timer(TimeSpan.FromSeconds(dhtSettings.StabilizeCallInSeconds).TotalMilliseconds),
                Stabilize);
            scheduler.Enqueue(
                new Timer(TimeSpan.FromSeconds(dhtSettings.CheckPredecessorCallInSeconds).TotalMilliseconds),
                CheckPredecessor);
            scheduler.Enqueue(new Timer(TimeSpan.FromSeconds(dhtSettings.FixFingersCallInSeconds).TotalMilliseconds),
                FixFingers);
            scheduler.Run();
        }

        public void FixFingers()
        {
            for (int i = 1; i < _fingerTable.FingerTableEntries.Length; i++)
            {
                var next = _fingerTable.FingerTableEntries[i].Start;
                Console.WriteLine("fix fingers called next = " + next);
                FindSuccessor(next, _fingerTable.FingerTableEntries[i - 1].Successor, this);
            }
        }

        private void CheckPredecessorResponseHandler(object? sender, System.EventArgs e)
        {
            CheckPredecessorResponseEventArgs eventArgs = (CheckPredecessorResponseEventArgs)e;
            Predecessor = eventArgs.Predecessor;
            Console.WriteLine("Check predecessor response handler : " + this);
        }

        private void CheckPredecessorHandler(object? sender, System.EventArgs e)
        {
            CheckPredecessorEventArgs eventArgs = (CheckPredecessorEventArgs)e;
            NodeDto destinationNode = eventArgs.DestinationNode;
            _dhtActions.CheckPredecessorResponse(destinationNode, Id, this);
        }

        private void StabilizeResponseHandler(object? sender, System.EventArgs e)
        {
            Console.WriteLine("Stabilize response handler " + this);
            StabilizeResponseEventArgs eventArgs = (StabilizeResponseEventArgs)e;
            var predecessorOfSuccessor = eventArgs.PredecessorOfSuccessor;
            Successor = Successor;
            if (predecessorOfSuccessor.Id != this.Id)
            {
                Console.WriteLine("predecessorOfSuccessor =  " + predecessorOfSuccessor);
                Successor = predecessorOfSuccessor;
                _dhtActions.Notify(Successor, Id, this);
            }
        }

        private void StabilizeHandler(object? sender, System.EventArgs e)
        {
            StabilizeEventArgs eventArgs = (StabilizeEventArgs)e;
            Console.WriteLine("Stabilize handler is called by " + eventArgs.DestinationNode);

            var stabilizingNode = eventArgs.DestinationNode;
            if (Predecessor == null) Predecessor = stabilizingNode;
            if ((Predecessor.Id <= stabilizingNode.Id && IsBootStrapNode) ||
                (Predecessor.Id <= stabilizingNode.Id && stabilizingNode.Id < Id) ||
                (Predecessor.Id > Id && Predecessor.Id <= stabilizingNode.Id))
            {
                Predecessor = stabilizingNode;
            }

            Console.WriteLine("My predecessor is : " + Predecessor);
            _dhtActions.StabilizeResponse(eventArgs.DestinationNode, Predecessor.Id, _predecessor);
            Console.WriteLine("My predecessor is send to : " + eventArgs.DestinationNode);
        }


        public void CheckPredecessor()
        {
            if (Predecessor == null) return;
            _timeOutScheduler.StartTimer(ORIGIN_PREDECESSOR);
            Console.WriteLine("Im called CheckPredecessor");
            _dhtActions.CheckPredecessor(Predecessor, Id, this);
        }

        private void TimeOutCheckPredecessorHandler()
        {
            Console.WriteLine("No response from predecessor, so its dead we reset it to null");
            this.Predecessor = null;
        }

        private void FoundSuccessorHandler(object? sender, System.EventArgs e)
        {
            //TODO: create own handler for fix fingers
            FoundSuccessorEventArgs eventArgs = (FoundSuccessorEventArgs)e;
            Console.WriteLine($"this is successor id {eventArgs?.SuccessorNode?.Id} found for key {eventArgs.Key}");

            if (eventArgs.Key == Id) //Found successor for this node
            {
                Successor = eventArgs.SuccessorNode;
                Console.WriteLine("Successor found:" + Successor?.Id);
                _dhtActions.Notify(Successor, Id, this);
                _fingerTable.FingerTableEntries[0].Successor = eventArgs.SuccessorNode;
                _fingerTable.AddEntries(eventArgs.SuccessorNode, eventArgs.Key);
            }
            // If key is equal to key in fingertable successor
            //TODO: evalute this function
            else if (_fingerTable.Include(eventArgs.Key))
            {
                _fingerTable.FingerTableEntries[0].Successor = Successor;
                _fingerTable.AddEntries(eventArgs.SuccessorNode, eventArgs.Key);
            }
        }

        public bool IsConnected()
        {
            return Successor != null;
        }

        public void Create()
        {
            if (IsConnected())
            {
                throw new Exception("Already connected");
            }

            BootStrapNode = this;
            _fingerTable.CreateFingerTable(Id);
            this.Successor = this;
            _fingerTable.AddEntry(this, 0);
            this.Predecessor = null;
        }

        private void FindSuccessorHandler(object? sender, System.EventArgs e)
        {
            FindSuccessorEventArgs eventArgs = (FindSuccessorEventArgs)e;
            FindSuccessor(eventArgs.Key, Successor, eventArgs.DestinationNode);
        }

        private bool IsThisNodeMyPredecessor(NodeDto possiblePredecessor, NodeDto currentPredecessor, NodeDto self)
        {
            if (currentPredecessor == null) return true;
            if (IsBootStrapNode && possiblePredecessor.Id > currentPredecessor.Id) return true;
            if (currentPredecessor.Id > this.Id && possiblePredecessor.Id < currentPredecessor.Id) return true;
            return
                (self.Id > possiblePredecessor.Id && currentPredecessor.Id < possiblePredecessor.Id);
        }

        private void NotifyHandler(object? sender, System.EventArgs e)
        {
            NotifyEventArgs eventArgs = (NotifyEventArgs)e;
            Console.WriteLine($"Node thinks it might be our {Id} predecessor {eventArgs.NodeDto.Id}");
            Console.WriteLine(this);
            if (IsThisNodeMyPredecessor(eventArgs.NodeDto, Predecessor, this) && eventArgs.NodeDto != Successor &&
                this.Id != eventArgs.NodeDto.Id) // Not exactly sure if this works
            {
                Predecessor = eventArgs.NodeDto;
                Console.WriteLine("And it is " + Predecessor.Id);
            }

            // This can only happen with the bootstrap node
            if (Successor.Id.Equals(Id))
            {
                Successor = eventArgs.NodeDto;
                Predecessor = eventArgs.NodeDto;
                _dhtActions.Notify(Successor, Id, this);
                Console.WriteLine("I am a bootstrap node " + Id + "  \n My successor node is " + Successor.Id +
                                  " \n and my predecessor node is " + Predecessor.Id);
                BootStrapNode = this;
            }
        }

        public bool IsBootStrapNode { get => BootStrapNode.Id.Equals(Id); }

        public void Stabilize()
        {
            // TODO: bootstrap node should have the predecessor with largest id.
            // Otherwise bootstrap node will call on upon itself and block other messages.
            if (Successor == null)
            {
                _dhtActions.FindSuccessor(BootStrapNode, Id, this);
                return;
            }

            if (Successor.Id.Equals(Id)) return;

            _dhtActions.Stabilize(Successor, Id, this);
            _timeOutScheduler.StartTimer(ORIGIN_SUCCESSOR);
            Console.WriteLine($"Stabilize {this}");
        }

        private void OnTimeOutStabilizeHandler()
        {
            Console.WriteLine($"Stabilize timeout {this}");
            NodeDto nextClosestSuccessor = null;
            //TODO: fix connecting node, should be closest successor node from finger table!
            if (IsBootStrapNode)
            {
                if (nextClosestSuccessor == null || nextClosestSuccessor.Id == Id)
                {
                    Successor = Predecessor;
                }
                else
                {
                    Successor = nextClosestSuccessor;
                }
            }
            else
            {
                //TODO: fix next closest preceding successor in order to relaibly fail nodes, otherwise they will go back to their previous successor who might be offline, so fingertable needs to be updates aswell.
                if (nextClosestSuccessor == null || Successor == null || nextClosestSuccessor?.Id == Id ||
                    nextClosestSuccessor?.Id == Successor?.Id)
                {
                    Successor = BootStrapNode;
                }
                else
                {
                    Successor = nextClosestSuccessor;
                }
            }

            Console.WriteLine($"Stabilize timeout successor is {Successor}");
            _dhtActions.Notify(Successor, Id, this);
        }


        public void Join(NodeDto node)
        {
            Predecessor = null;
            BootStrapNode = node;
            FindSuccessor(Id, BootStrapNode, this);
        }

        private bool IsIdInBetween(uint id, uint left, uint right)
        {
            return id > left && id <= right;
        }

        /// <summary>
        /// Search for successor containing id. When successor with the id is found, return the result to given node.
        /// For example, when node searches its own successor input should be "FindSuccessor(this.id, this)"
        /// When looking for a result in the distributed hashtable input should be "FindSuccessor(key.id, this)"
        /// This will be given as input in our request dto and is send in a forward query if closest preceding node has
        /// no record if the id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="connectingNode"></param>
        /// <param name="destinationNode"></param>
        /// <param name="node"></param>
        public void FindSuccessor(uint id, NodeDto connectingNode, NodeDto destinationNode)
        {
            // Am I the Successor? 
            if ((Predecessor != null && IsIdInBetween(id, Predecessor.Id, this.Id)) ||
                (IsBootStrapNode && Predecessor?.Id < id))
            {
                //send found request back
                _dhtActions.FoundSuccessor(destinationNode, id, this);
            }
            // Is my Successor the successor? 
            else if ((Successor != null && IsIdInBetween(id, this.Id, Successor.Id)) ||
                     (Successor != null && Successor.Id.Equals(Id)))
            {
                //send found request back
                _dhtActions.FoundSuccessor(destinationNode, id, connectingNode);
            }
            else
            {
                //Forward query

                // TODO: Replace Successor with closest preceding node for optimization
                _dhtActions.FindSuccessor(connectingNode, id, destinationNode);
            }
        }

        public void Start()
        {
            _dhtActions.Start();
        }


        public override string ToString()
        {
            return $" SuccessorId {_successor?.Id} || MyId {Id} || PredecessorId {_predecessor?.Id}";
            // // return base.ToString();
            // return base.ToString() + " FingerTable: \n " + _fingerTable.ToString();
        }
    }
}