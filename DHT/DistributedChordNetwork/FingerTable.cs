using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DHT.Formatting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DHT
{
    public class FingerTable : IFingerTable
    {
        public FingerTableEntry[] FingerTableEntries { get; }

        private readonly uint _numberOfFingerTableEntries;
        private readonly uint _numDHT;
        public Node? Node { get; set; }

        public FingerTable(IOptions<DhtSettings> options, ISchedule scheduler)
        {
            var maxNumNodes = options.Value.MaxNumberOfNodes;
            _numberOfFingerTableEntries = (uint)Math.Ceiling(Math.Log(maxNumNodes - 1) / Math.Log(2));
            _numDHT = (uint)Math.Pow(2, _numberOfFingerTableEntries);
            FingerTableEntries = new FingerTableEntry[_numberOfFingerTableEntries];

            scheduler.Enqueue(new Timer(TimeSpan.FromSeconds(options.Value.FixFingersCallInSeconds).TotalMilliseconds),
                FixFingers);
        }

        public void FixFingers()
        {
            for (int i = 1; i < FingerTableEntries.Length; i++)
            {
                var next = FingerTableEntries[i].Start;
                // Console.WriteLine("fix fingers called next = " + next);
                Node?.FindSuccessor(next, FingerTableEntries[i - 1].Successor, Node);
            }
        }

        /// <summary>
        /// Add entry to the fingertable. The parameter 'node' is added to the fingerTable entry by its id (start) value.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <returns>"void"</returns>
        public void AddEntry(NodeDto node, uint id)
        {
            for (int i = 0; i < _numberOfFingerTableEntries; i++)
            {
                if (FingerTableEntries[i].Start == id)
                {
                    FingerTableEntries[i].Successor = node;
                    return;
                }
            }
        }


        /// <summary>
        /// Create fingertable for specific node (id).
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <returns>"void"</returns>
        public void CreateFingerTable(uint id)
        {
            if (id < 0 || id > _numDHT)
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"ID index out of range: {id}");
            }

            for (uint i = 0; i < _numberOfFingerTableEntries; i++)
            {
                FingerTableEntries[i] = new FingerTableEntry {Start = (uint)((id + (uint)Math.Pow(2, i)) % _numDHT)};
            }

            for (uint i = 0; i < _numberOfFingerTableEntries - 1; i++)
            {
                FingerTableEntries[i].SetInterval(FingerTableEntries[i].Start, FingerTableEntries[i + 1].Start);
            }

            FingerTableEntries[_numberOfFingerTableEntries - 1].SetInterval(
                FingerTableEntries[_numberOfFingerTableEntries - 1].Start, FingerTableEntries[0].Start - 1);
        }

        /// <summary>
        /// Find closest preceding node for specific node id.
        /// <param name="id"></param>
        /// <returns>"void"</returns>
        public NodeDto ClosestPrecedingNode(uint id)
        {
            if (id < 0 || id > _numDHT)
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"ID index out of range: {id}");
            }

            if (FingerTableEntriesHasBeenInitialized())
            {
                for (uint i = _numberOfFingerTableEntries - 1; i >= 0; i--)
                {
                    if (id >= FingerTableEntries[i].IntervalBegin && id < FingerTableEntries[i].IntervalEnd &&
                        FingerTableEntries[i].Successor != null)
                    {
                        return FingerTableEntries[i].Successor;
                    }
                }

                return FingerTableEntries[_numberOfFingerTableEntries - 1].Successor;
            }

            // If fingertable entries are NOT initialized, return null 
            return null;
        }

        private bool FingerTableEntriesHasBeenInitialized()
        {
            return FingerTableEntries.All(entry => entry.Successor != null);
        }

        /// <summary>
        /// Initialize fingertable by calling FindSuccessor for the start value of fingertable entries.
        /// <param name="id"></param>
        /// <param name="connectionNode"></param>
        /// <param name="destinationNode"></param>
        /// <param name="relayServiceAdapter"></param>
        /// <returns>"void"</returns>
        public void FixFingers(uint id, NodeDto connectionNode, Node destinationNode,
            IDhtRelayServiceAdapter relayServiceAdapter)
        {
            FingerTableEntries[0].Successor = connectionNode;

            for (int i = 1; i < _numberOfFingerTableEntries; i++)
            {
                destinationNode.FindSuccessor(FingerTableEntries[i].Start, connectionNode, destinationNode);
            }
        }

        public bool Include(uint id)
        {
            return FingerTableEntries.FirstOrDefault(x => x.Start.Equals(id)) != null;
        }

        public void AddEntries(NodeDto successor, uint id)
        {
            for (int i = 0; i < _numberOfFingerTableEntries; i++)
            {
                if (FingerTableEntries[i].Successor == null)
                {
                    FingerTableEntries[i].Successor = successor;
                }
                else if (FingerTableEntries[i].Start == id)
                {
                    FingerTableEntries[i].Successor = successor;
                    return;
                }
            }
        }

        public override string ToString()
        {
            return FingerTableEntries.ToArray().ToString();
        }
    }
}