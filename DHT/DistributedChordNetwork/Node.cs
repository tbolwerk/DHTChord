using System;
using System.Threading.Tasks;
using System.Timers;

namespace DHT
{
    public class Node : NodeDto
    {
        private NodeDto BootStrapNode { get; set; }
        private readonly IFingerTable _fingerTable;
        private readonly IDhtRelayServiceAdapter _relayServiceAdapter;


        private readonly TimeOutScheduler _timeOutSuccessor;
        private readonly TimeOutScheduler _timeOutPredecessor;
        private NodeDto _successor;
        private NodeDto _predecessor;

        public override NodeDto Successor
        {
            get => _successor;
            set
            {
                _successor = value;
                _timeOutSuccessor.Stop();
            }
        }

        public override NodeDto Predecessor
        {
            get => _predecessor;
            set
            {
                _predecessor = value;
                _timeOutPredecessor.Stop();
            }
        }

        public Node(IDhtRelayServiceAdapter relayServiceAdapter, IFingerTable fingerTable)
        {
            _fingerTable = fingerTable;
            _relayServiceAdapter = relayServiceAdapter;
            _relayServiceAdapter.NotifyHandler += NotifyHandler;
            _relayServiceAdapter.FindSuccessorHandler += FindSuccessorHandler;
            _relayServiceAdapter.FoundSuccessorHandler += FoundSuccessorHandler;
            _relayServiceAdapter.StabilizeHandler += StabilizeHandler;
            _relayServiceAdapter.StabilizeResponseHandler += StabilizeResponseHandler;
            _relayServiceAdapter.CheckPredecessorHandler += CheckPredecessorHandler;
            _relayServiceAdapter.CheckPredecessorResponseHandler += CheckPredecessorResponseHandler;

            double timeOut = TimeSpan.FromSeconds(5).TotalMilliseconds;
            _timeOutSuccessor = new TimeOutScheduler(timeOut, 1); 
            _timeOutSuccessor.RetryHandler += (sender, args) => Stabilize();
            _timeOutSuccessor.TimeOutHandler += OnTimeOutStabilizeHandler;

            _timeOutPredecessor = new TimeOutScheduler(timeOut, 0); // 1 less attempt than successor, in order to improve performance
            _timeOutPredecessor.RetryHandler += (sender, args) => CheckPredecessor();
            _timeOutPredecessor.TimeOutHandler += TimeOutCheckPredecessorHandler;

            Scheduler scheduler = new Scheduler(1);
            scheduler.Enqueue(new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds), Stabilize);
            scheduler.Enqueue(new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds), CheckPredecessor);
            scheduler.Enqueue(new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds), FixFingers);
            scheduler.Run();
        }

        private void FixFingers()
        {
            for (int i = 1; i < _fingerTable.FingerTableEntries.Length; i++)
            {
                var next = _fingerTable.FingerTableEntries[i - 1].Start;
                Console.WriteLine("fix fingers called next = " + next);
                FindSuccessor(next, _fingerTable.FingerTableEntries[i - 1].Successor, this);
            }
        }

        private void CheckPredecessorResponseHandler(object? sender, EventArgs e)
        {
            CheckPredecessorResponseEventArgs eventArgs = (CheckPredecessorResponseEventArgs)e;
            Predecessor = eventArgs.Predecessor;
            Console.WriteLine("Check predecessor response handler : " + this);
        }

        private void CheckPredecessorHandler(object? sender, EventArgs e)
        {
            CheckPredecessorEventArgs eventArgs = (CheckPredecessorEventArgs)e;
            NodeDto destinationNode = eventArgs.DestinationNode;
            DhtProtocolCommandDto commandDto = new DhtProtocolCommandDto
            {
                Command = DhtCommand.CHECK_PREDECESSOR_RESPONSE, NodeDto = this
            };
            _relayServiceAdapter.SendRpcCommand(destinationNode, commandDto);
        }

        private void StabilizeResponseHandler(object? sender, EventArgs e)
        {
            Console.WriteLine("Stabilize response handler " + this);
            StabilizeResponseEventArgs eventArgs = (StabilizeResponseEventArgs)e;
            var predecessorOfSuccessor = eventArgs.PredecessorOfSuccessor;
            Successor = Successor;
            if (predecessorOfSuccessor.Id != this.Id)
            {
                Console.WriteLine("predecessorOfSuccessor =  " + predecessorOfSuccessor);
                Successor = predecessorOfSuccessor;
                Notify(Successor, this);
            }
        }

        private void StabilizeHandler(object? sender, EventArgs e)
        {
            StabilizeEventArgs eventArgs = (StabilizeEventArgs)e;
            Console.WriteLine("Stabilize handler is called by " + eventArgs.DestinationNode);

            var stabilizingNode = eventArgs.DestinationNode;
            if (Predecessor == null) Predecessor = stabilizingNode;
            if ((Predecessor.Id <= stabilizingNode.Id && IsBootStrapNode) ||
                (Predecessor.Id <= stabilizingNode.Id && stabilizingNode.Id < Id))
            {
                Predecessor = stabilizingNode;
            }

            var command = new DhtProtocolCommandDto
            {
                Command = DhtCommand.STABILIZE_RESPONSE, NodeDto = Predecessor, Key = Predecessor.Id
            };
            Console.WriteLine("My predecessor is : " + Predecessor);
            _relayServiceAdapter.SendRpcCommand(eventArgs.DestinationNode, command);
            Console.WriteLine("My predecessor is send to : " + eventArgs.DestinationNode);
        }


        private void CheckPredecessor()
        {
            if (Predecessor == null) return;


            DhtProtocolCommandDto command = new DhtProtocolCommandDto
            {
                Command = DhtCommand.CHECK_PREDECESSOR, NodeDto = this
            };
            _timeOutPredecessor.Start();
            Console.WriteLine("Im called CheckPredecessor");

            _relayServiceAdapter.SendRpcCommand(Predecessor, command);
        }

        private void TimeOutCheckPredecessorHandler(object sender, EventArgs e)
        {
            Console.WriteLine("No response from predecessor, so its dead we reset it to null");
            this.Predecessor = null;
        }

        private void FoundSuccessorHandler(object? sender, EventArgs e)
        {
            //TODO: create own handler for fix fingers
            FoundSuccessorEventArgs eventArgs = (FoundSuccessorEventArgs)e;
            if (eventArgs.Key == Id) //Found successor for this node
            {
                Successor = eventArgs.SuccessorNode;
                Console.WriteLine("Successor found:" + Successor.Id);
                Task.Run((() => Notify(Successor, this)));
                _fingerTable.FingerTableEntries[0].Successor = eventArgs.SuccessorNode;
                _fingerTable.AddEntries(eventArgs.SuccessorNode, eventArgs.Key);
            }
            // If key is equal to key in fingertable successor
            //TODO: evalute this function
            else if (_fingerTable.Include(eventArgs.Key))
            {
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

        private void FindSuccessorHandler(object? sender, EventArgs e)
        {
            FindSuccessorEventArgs eventArgs = (FindSuccessorEventArgs)e;
            FindSuccessor(eventArgs.Key, Successor, eventArgs.DestinationNode);
        }

        private bool IsThisNodeMyPredecessor(NodeDto possiblePredecessor, NodeDto currentPredecessor, NodeDto self)
        {
            if (currentPredecessor == null) return true;
            if (IsBootStrapNode && possiblePredecessor.Id > currentPredecessor.Id) return true;
            if (currentPredecessor.Id > this.Id && possiblePredecessor.Id > this.Id &&
                possiblePredecessor.Id < currentPredecessor.Id) return true;
            return
                (self.Id > possiblePredecessor.Id && currentPredecessor.Id < possiblePredecessor.Id);
        }

        private void NotifyHandler(object? sender, EventArgs e)
        {
            NotifyEventArgs eventArgs = (NotifyEventArgs)e;

            Console.WriteLine($"Node thinks it might be our predecessor {eventArgs.NodeDto.Id}");
            if (IsThisNodeMyPredecessor(eventArgs.NodeDto, Predecessor, this) && eventArgs.NodeDto != Successor &&
                this.Id != eventArgs.NodeDto.Id) // Not exactly sure if this works
            {
                Predecessor = eventArgs.NodeDto;
                Console.WriteLine("And it is " + Predecessor.Id);
            }

            // This can only happen with the bootstrap node
            if (Successor == this)
            {
                Successor = eventArgs.NodeDto;
                Predecessor = eventArgs.NodeDto;
                Notify(Successor, this);
                Console.WriteLine("I am a bootstrap node " + Id + "  \n My successor node is " + Successor.Id +
                                  " \n and my predecessor node is " + Predecessor.Id);
                IsBootStrapNode = true;
            }
        }

        public bool IsBootStrapNode { get; set; }

        public void Stabilize()
        {
            // TODO: bootstrap node should have the predecessor with largest id.
            Console.WriteLine("1Stabilize " + this);
            // Otherwise bootstrap node will call on upon itself and block other messages.
            if (Successor == this) return;
            var command = new DhtProtocolCommandDto {Command = DhtCommand.STABILIZE, Key = Id, NodeDto = this};
            Console.WriteLine("2Stabilize");

            // Task.Run(() => _timeOutSuccessor.Start());
            Console.WriteLine("3Stabilize");
            _relayServiceAdapter.SendRpcCommand(Successor, command);
            _timeOutSuccessor.Start();
        }


        private void OnTimeOutStabilizeHandler(object? sender, EventArgs e)
        {
            Console.WriteLine("Stabilize timeout " + this);
            var nextClosestSuccessor = _fingerTable.ClosestPrecedingNode(Id);
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
            {//TODO: fix next closest preceding successor in order to relaibly fail nodes, otherwise they will go back to their previous successor who might be offline, so fingertable needs to be updates aswell.
                if (nextClosestSuccessor == null || nextClosestSuccessor.Id == Id ||
                    nextClosestSuccessor.Id == Successor.Id)
                {
                    Successor = BootStrapNode;
                }
                else
                {
                    Successor = nextClosestSuccessor;
                }
            }

            Task.Run(() => Notify(Successor, this));
        }

        private void Notify(NodeDto connectingNode, NodeDto self)
        {
            _relayServiceAdapter.Notify(connectingNode, self);
        }

        public void Join(NodeDto node)
        {
            Predecessor = null;
            BootStrapNode = node;
            FindSuccessor(Id, node, this);
        }

        private bool IsIdInBetween(int id, int left, int right)
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
        public void FindSuccessor(int id, NodeDto connectingNode, NodeDto destinationNode)
        {
            // Am I the Successor? 
            if ((Predecessor != null && IsIdInBetween(id, Predecessor.Id, this.Id)) ||
                (IsBootStrapNode && Predecessor?.Id < id))
            {
                var requestDto = new DhtProtocolCommandDto
                {
                    Command = DhtCommand.FOUND_SUCCESSOR, NodeDto = this, Key = id
                };
                //send found request back

                _relayServiceAdapter.SendRpcCommand(destinationNode, requestDto);
            }
            // Is my Successor the successor? 
            else if ((Successor != null && IsIdInBetween(id, this.Id, Successor.Id)) ||
                     Successor == this)
            {
                var requestDto = new DhtProtocolCommandDto
                {
                    Command = DhtCommand.FOUND_SUCCESSOR, NodeDto = connectingNode, Key = id
                };
                //send found request back

                _relayServiceAdapter.SendRpcCommand(destinationNode, requestDto);
            }
            else
            {
                var requestDto = new DhtProtocolCommandDto
                {
                    Command = DhtCommand.FIND_SUCCESSOR, NodeDto = destinationNode, Key = id
                };
                //Forward query

                // TODO: Replace Successor with closest preceding node for optimization
                _relayServiceAdapter.SendRpcCommand(connectingNode, requestDto);
            }
        }

        public void Start()
        {
            _relayServiceAdapter.Start();
        }

        public override string ToString()
        {
            return base.ToString();
            // return base.ToString() + " FingerTable: \n " + _fingerTable.ToString();
        }
    }
}