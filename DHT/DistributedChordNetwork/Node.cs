using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Timers;

namespace DHT
{
    public class Node : NodeDto
    {
        private IFingerTable _fingerTable;
        private readonly IDhtRelayServiceAdapter _relayServiceAdapter;

        public Node(IDhtRelayServiceAdapter relayServiceAdapter, IFingerTable fingerTable)
        {
            _fingerTable = fingerTable;
            _relayServiceAdapter = relayServiceAdapter;
            _relayServiceAdapter.NotifyHandler += NotifyHandler;
            _relayServiceAdapter.FindSuccessorHandler += FindSuccessorHandler;
            _relayServiceAdapter.FoundSuccessorHandler += FoundSuccessorHandler;
            _relayServiceAdapter.StabilizeHandler += StabilizeHandler;
            _relayServiceAdapter.StabilizeResponseHandler += StabilizeResponseHandler;

            Timer t = new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds); // Set the time (5 mins in this case)
            t.AutoReset = true;
            t.Elapsed += OnStabilize;
            t.Start();
        }

        private async void StabilizeResponseHandler(object? sender, EventArgs e)
        {
            Console.WriteLine("Stabilize response handler");

            StabilizeResponseEventArgs eventArgs = (StabilizeResponseEventArgs) e;
            var predecessorOfSuccessor = eventArgs.PredecessorOfSuccessor;
            if (predecessorOfSuccessor.Id != this.Id)
            {
                Console.WriteLine("predecessorOfSuccessor =  " + predecessorOfSuccessor);
                // if (Successor.Id >= predecessorOfSuccessor.Id)
                // {
                Successor = predecessorOfSuccessor;
                await Notify(Successor, this);
                // }
            }
        }

        private async void StabilizeHandler(object? sender, EventArgs e)
        {
            StabilizeEventArgs eventArgs = (StabilizeEventArgs) e;
            var stabilizingNode = eventArgs.DestinationNode;
            if (Predecessor == null) Predecessor = stabilizingNode;
            var command = new DhtProtocolCommandDto
                {Command = DhtCommand.STABILIZE_RESPONSE, NodeDto = Predecessor, Key = Predecessor.Id};
            await _relayServiceAdapter.SendRPCCommand(stabilizingNode, command);
        }


        private void FoundSuccessorHandler(object? sender, EventArgs e)
        {
            FoundSuccessorEventArgs eventArgs = (FoundSuccessorEventArgs) e;
            Successor = eventArgs.SuccessorNode;
            Console.WriteLine("Successor found:" + Successor.Id);
            Task.Run((() => Notify(Successor, this)));
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

            this.Successor = this;
            _fingerTable.AddEntry(this);
            this.Predecessor = null;
        }

        private async void FindSuccessorHandler(object? sender, EventArgs e)
        {
            FindSuccessorEventArgs eventArgs = (FindSuccessorEventArgs) e;
            var successor = await FindSuccessor(eventArgs.Key, Successor, eventArgs.DestinationNode);
        }

        private bool IsThisNodeMyPredecessor(NodeDto possiblePredecessor, NodeDto currentPredecessor, NodeDto self)
        {
            if (Predecessor == null) return true;
            return
                (self.Id > possiblePredecessor.Id && currentPredecessor.Id < possiblePredecessor.Id);
        }

        private void NotifyHandler(object? sender, EventArgs e)
        {
            NotifyEventArgs eventArgs = (NotifyEventArgs) e;
            Console.WriteLine("Node thinks it might be our predecessor " + eventArgs.NodeDto.Id);
            if (IsThisNodeMyPredecessor(eventArgs.NodeDto, Predecessor, this) && eventArgs.NodeDto != Successor && this.Id != eventArgs.NodeDto.Id) // Not exactly sure if this works
            {
                Predecessor = eventArgs.NodeDto;
                Console.WriteLine("And it is " + Predecessor.Id);
            }

            // This can only happen with the bootstrap node
            if (Successor == this)
            {
                Successor = eventArgs.NodeDto;
                Successor = eventArgs.NodeDto;
                Task.Run((() => Notify(Successor, this)));
                Console.WriteLine("I am a bootstrap node " + Id + "  \n My successor node is " + Successor.Id +
                                  " \n and my predecessor node is " + Predecessor.Id);
            }
        }

        public async Task Stabilize()
        {
            Console.WriteLine(this);
            // Otherwise bootstrap node will call on upon itself and block other messages.
            if (Successor == this) return;
            var command = new DhtProtocolCommandDto {Command = DhtCommand.STABILIZE, Key = Id, NodeDto = this};
            await _relayServiceAdapter.SendRPCCommand(Successor, command);
            // var x = await FindSuccessor(this.Id, Successor, this);
            // Console.WriteLine("Stabilize has found succesor " + x.Id);
            //
            // if ((x.Predecessor == null) || x.Predecessor.Id != Id && x.Id == Successor.Id)
            // {
            //     Console.WriteLine("Stabilize is changing successor with succesor predecessor " + x.Predecessor.Id);
            //
            //     Successor = x.Predecessor;
            //     bool isSuccessfullyNotified = await Notify(Successor, this);
            //     if (!isSuccessfullyNotified) await Stabilize();
            // }
        }

        private void OnStabilize(object sender, ElapsedEventArgs e)
        {
            Task.Run(Stabilize);
        }

        private async Task<bool> Notify(NodeDto connectingNode, NodeDto self)
        {
            NodeDto predecessorOfSuccessor = await _relayServiceAdapter.Notify(connectingNode, self);
            return predecessorOfSuccessor.Id == this.Id;
        }

        public async Task Join(NodeDto node)
        {
            Predecessor = null;
            
            Successor = await FindSuccessor(Id, node, this);
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
        /// <returns>"NodeDto"</returns>
        public async Task<NodeDto> FindSuccessor(int id, NodeDto connectingNode, NodeDto destinationNode)
        {
            if (Predecessor != null && IsIdInBetween(id, Predecessor.Id, this.Id))
            {
                var requestDto = new DhtProtocolCommandDto
                    {Command = DhtCommand.FOUND_SUCCESSOR, NodeDto = this, Key = this.Id};
                //send found request back

                var x = await _relayServiceAdapter.SendRPCCommand(destinationNode, requestDto);
                return this;
            }
            else if ((Successor != null && IsIdInBetween(connectingNode.Id, this.Id, Successor.Id)) ||
                     Successor == this)
            {
                var requestDto = new DhtProtocolCommandDto
                    {Command = DhtCommand.FOUND_SUCCESSOR, NodeDto = connectingNode, Key = connectingNode.Id};
                //send found request back

                var x = await _relayServiceAdapter.SendRPCCommand(destinationNode, requestDto);
                return connectingNode;
            }
            else
            {
                var requestDto = new DhtProtocolCommandDto
                    {Command = DhtCommand.FIND_SUCCESSOR, NodeDto = this, Key = id};
                //Forward query

                // TODO: Replace Successor with closest preceding node for optimization
                var x = await _relayServiceAdapter.SendRPCCommand(connectingNode, requestDto);
                return x;
            }
        }

        public void Start()
        {
            _relayServiceAdapter.Start();
        }
    }
}