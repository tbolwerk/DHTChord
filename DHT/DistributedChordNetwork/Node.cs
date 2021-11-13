using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DHT.DistributedChordNetwork.EventArgs;
using DHT.DistributedChordNetwork.Networking;
using Microsoft.Extensions.Options;

namespace DHT.DistributedChordNetwork
{
    public class Node : NodeDto
    {
        public NodeDto BootStrapNode { get; set; }
        private readonly IOptions<DhtSettings> _options;
        private readonly IFingerTable _fingerTable;
        private readonly IDhtActions _dhtActions;
        private readonly ITimeOutScheduler _timeOutScheduler;
        private readonly ISchedule _scheduler;

        private const string OriginSuccessor = "successor";
        private const string OriginPredecessor = "predecessor";

        private NodeDto _successor;
        private NodeDto _predecessor;

        public readonly Dictionary<uint, string>
            Hashtable = new Dictionary<uint, string>(); // Key: Public key, Value: IP address

        public override NodeDto? Successor
        {
            get => _successor;
            set
            {
                _successor = value;
                _timeOutScheduler.StopTimer(OriginSuccessor);
            }
        }

        public override NodeDto Predecessor
        {
            get => _predecessor;
            set
            {
                _predecessor = value;
                _timeOutScheduler.StopTimer(OriginPredecessor);
            }
        }

        public Node(IOptions<DhtSettings> options, INetworkAdapter networkAdapter, IFingerTable fingerTable,
            IDhtActions dhtActions,
            ITimeOutScheduler timeOutScheduler,
            ISchedule scheduler, IStabilize stabilize, ICheckPredecessor checkPredecessor)
        {
            _options = options;
            _fingerTable = fingerTable;
            _dhtActions = dhtActions;
            _timeOutScheduler = timeOutScheduler;
            _scheduler = scheduler;
            networkAdapter.NotifyHandler += NotifyHandler;
            networkAdapter.FindSuccessorHandler += FindSuccessorHandler;
            networkAdapter.FoundSuccessorHandler += FoundSuccessorHandler;
            networkAdapter.GetHandler += GetHandler;
            networkAdapter.GetReponseHandler += GetResponseHandler;
            networkAdapter.PutHandler += PutHandler;
            networkAdapter.RemoveDataFromExpiredReplicasHandler += RemoveDataFromExpiredReplicasHandler;
            networkAdapter.RemoveDataFromExpiredReplicasResponseHandler += RemoveDataFromExpiredReplicasResponseHandler;


            checkPredecessor.Node = this;
            stabilize.Node = this;
            fingerTable.Node = this;
            
            // Create();
        }

        public event EventHandler GetResponseEventHandler;


        private void RemoveDataFromExpiredReplicasResponseHandler(object? sender, System.EventArgs e)
        {
            GetHandlerEventArgs eventArgs = (GetHandlerEventArgs) e;

            if (Hashtable.ContainsKey(eventArgs.DhtProtocolCommandDto.Key))
            {
                Hashtable.Remove(eventArgs.DhtProtocolCommandDto.Key);
            }
        }

        private void RemoveDataFromExpiredReplicasHandler(object? sender, System.EventArgs e)
        {
            GetHandlerEventArgs eventArgs = (GetHandlerEventArgs) e;
            var protocolDto = eventArgs.DhtProtocolCommandDto;


            if (protocolDto.CurrentNumberOfReplicas <= _options.Value.Replicas)
            {
                if ((IAmTheSuccessorOf(protocolDto.Key) || protocolDto.Key == this.Id))
                {
                    if (this.Id == protocolDto.NodeDto.Id || Successor == null)
                    {
                        return;
                    }

                    protocolDto.CurrentNumberOfReplicas += 1;
                    protocolDto.Key = Successor.Id;
                }

                _dhtActions.ForwardRequest(Successor, protocolDto);
            }
            else
            {
                _dhtActions.RemoveDataFromExpiredReplicasReponse(protocolDto.NodeDto, protocolDto.KeyToAdd);
            }
        }

        private void GetResponseHandler(object? sender, System.EventArgs e)
        {
            EventHandler handler = GetResponseEventHandler;
            handler?.Invoke(sender, e);
        }

        private void PutHandler(object? sender, System.EventArgs e)
        {
            PutHandlerEventArgs eventArgs = (PutHandlerEventArgs) e;
            var protocolDto = eventArgs.ProtocolCommandDto;
            if (Successor != null)
            {
                if ((IAmTheSuccessorOf(protocolDto.Key) || protocolDto.Key == this.Id))
                {
                    protocolDto.CurrentNumberOfReplicas += 1;
                    Put(Successor.Id, protocolDto.Value, Successor, protocolDto.NodeDto,
                        protocolDto.CurrentNumberOfReplicas, protocolDto.KeyToAdd);
                }
                else
                {
                    _dhtActions.ForwardRequest(Successor, protocolDto);
                }
            }
        }


        private void GetHandler(object? sender, System.EventArgs e)
        {
            GetHandlerEventArgs eventArgs = (GetHandlerEventArgs) e;
            var protocolDto = eventArgs.DhtProtocolCommandDto;
            string value = GetValueByKey(protocolDto.Key);
            if (IAmTheSuccessorOf(protocolDto.Key))
            {
                _dhtActions.GetResponse(Successor, protocolDto.NodeDto, protocolDto.Key, value);
            }
            else
            {
                _dhtActions.ForwardRequest(Successor, protocolDto);
            }
        }

        private void FoundSuccessorHandler(object? sender, System.EventArgs e)
        {
            //TODO: create own handler for fix fingers
            FoundSuccessorEventArgs eventArgs = (FoundSuccessorEventArgs) e;
            if (eventArgs == null) throw new Exception("found successor event is null");
            Console.WriteLine($"this is successor id {eventArgs?.SuccessorNode?.Id} found for key {eventArgs?.Key}");

            if (eventArgs?.Key == Id) //Found successor for this node
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
            FindSuccessorEventArgs eventArgs = (FindSuccessorEventArgs) e;
            FindSuccessor(eventArgs.Key, Successor, eventArgs.DestinationNode);
        }

        private bool IsThisNodeMyPredecessor(NodeDto possiblePredecessor, NodeDto currentPredecessor, NodeDto self)
        {
            if (currentPredecessor == null) return true;
            if (IsBootStrapNode && possiblePredecessor.Id > currentPredecessor.Id) return true;
            if (currentPredecessor.Id > this.Id && possiblePredecessor.Id < currentPredecessor.Id &&
                !IsBootStrapNode) return true;
            return
                (self.Id > possiblePredecessor.Id && currentPredecessor.Id < possiblePredecessor.Id);
        }

        private void NotifyHandler(object? sender, System.EventArgs e)
        {
            NotifyEventArgs eventArgs = (NotifyEventArgs) e;
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

        public bool IsBootStrapNode
        {
            get => BootStrapNode.Id.Equals(Id);
        }


        public void Join(NodeDto node)
        {
            Predecessor = null;
            BootStrapNode = node;
            FindSuccessor(Id, BootStrapNode, this);
        }

        public void Get(uint key)
        {
            // var closest = _fingerTable.ClosestPrecedingNode(searchKey);
            _dhtActions.Get(key, Successor, this);
        }

        public void Put(uint key, string value)
        {
            // var closest = _fingerTable.ClosestPrecedingNode(key);
            Put(key, value, Successor, this, 0, key);
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
            Console.WriteLine($"Searching id {id} for {destinationNode} currently looking in {connectingNode}");
            // Am I the Successor? 
            if (IAmTheSuccessorOf(id))
            {
                //send found request back
                _dhtActions.FoundSuccessor(destinationNode, id, this);
            }
            // Is my Successor the successor? 
            else if (IsMySuccessorTheSuccessorOf(id))
            {
                //send found request back
                _dhtActions.FoundSuccessor(destinationNode, id, Successor);
            }
            else
            {
                //Forward query

                // TODO: Replace Successor with closest preceding node for optimization
                _dhtActions.FindSuccessor(connectingNode, id, destinationNode);
            }
        }

        private bool IsMySuccessorTheSuccessorOf(uint id)
        {
            return (Successor != null && IsIdInBetween(id, this.Id, Successor.Id)) ||
                   (Successor != null && Successor.Id.Equals(Id));
        }

        private bool IAmTheSuccessorOf(uint id)
        {
            return (Predecessor != null && IsIdInBetween(id, Predecessor.Id, this.Id)) ||
                   (IsBootStrapNode && Predecessor?.Id < id);
        }

        private void Put(uint key, string value, NodeDto connectingNode, NodeDto destinationNode,
            int currentNumberOfReplicas, uint keyToAdd)
        {
            // If currentNumberOfReplicas < 2, store value at Node (connectingNode)
            if (currentNumberOfReplicas <= _options.Value.Replicas + 1)
            {
                // Add value to dictionary
                if (currentNumberOfReplicas > 0)
                {
                    Hashtable[keyToAdd] = value;
                }

                _dhtActions.Put(connectingNode, destinationNode, key, value, currentNumberOfReplicas, keyToAdd);
                Console.WriteLine(this);
            }
            // else forward to destinationNode
            else
            {
                _dhtActions.PutResponse(connectingNode, destinationNode, key, value);
            }
        }

        private string GetValueByKey(uint id)
        {
            string value = null;
            Hashtable.TryGetValue(id, out value);
            // value not found
            return value;
        }

        public void Start()
        {
            Task.Run(() => _dhtActions.Start());
            _scheduler.Run();
        }

        public string DictionaryString()
        {
            string dictionaryString = null;
            foreach (var pair in Hashtable)
            {
                dictionaryString += $"Key: {pair.Key}, Value: {pair.Value}\n";
            }

            return dictionaryString;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var keyValuePair in Hashtable)
            {
                sb.Append("[");
                sb.Append($"(Key:{keyValuePair.Key} \t Value:{keyValuePair.Value})\n");
                sb.Append("]");
            }

            return
                $" SuccessorId {_successor?.Id} || MyId {Id} || PredecessorId {_predecessor?.Id} \n MY VALUES ARE: \n{sb.ToString()} \n " +
                $"Dictionary: {DictionaryString()}";
            // // return base.ToString();
            // return base.ToString() + " FingerTable: \n " + _fingerTable.ToString();
        }
    }
}