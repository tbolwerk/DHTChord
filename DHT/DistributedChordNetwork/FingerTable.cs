using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DHT.Formatting;

namespace DHT
{
    public class FingerTable : IFingerTable
    {
        public FingerTableEntry[] FingerTableEntries { get; }

        private readonly int _numberOfFingerTableEntries;
        private readonly int _numDHT;

        public FingerTable(int maxNumNodes)
        {
            _numberOfFingerTableEntries = (int)Math.Ceiling(Math.Log(maxNumNodes - 1) / Math.Log(2));
            _numDHT = (int)Math.Pow(2, _numberOfFingerTableEntries);
            FingerTableEntries = new FingerTableEntry[_numberOfFingerTableEntries];
        }

        public FingerTable(int maxNumNodes, int id)
        {
            _numberOfFingerTableEntries = (int)Math.Ceiling(Math.Log(maxNumNodes - 1) / Math.Log(2));
            _numDHT = (int)Math.Pow(2, _numberOfFingerTableEntries);
            FingerTableEntries = new FingerTableEntry[_numberOfFingerTableEntries];
            CreateFingerTable(id);
        }


        /// <summary>
        /// Add entry to the fingertable. The parameter 'node' is added to the fingerTable entry by its id (start) value.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="id"></param>
        /// <returns>"void"</returns>
        public void AddEntry(NodeDto node, int id)
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
        public void CreateFingerTable(int id)
        {
            if (id < 0 || id > _numDHT)
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"ID index out of range: {id}");
            }

            for (int i = 0; i < _numberOfFingerTableEntries; i++)
            {
                FingerTableEntries[i] = new FingerTableEntry {Start = ((id + (int)Math.Pow(2, i)) % _numDHT)};
            }

            for (int i = 0; i < _numberOfFingerTableEntries - 1; i++)
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
        public NodeDto ClosestPrecedingNode(int id)
        {
            if (id < 0 || id > _numDHT)
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"ID index out of range: {id}");
            }

            for (int i = _numberOfFingerTableEntries - 1; i >= 0; i--)
            {
                if (id >= FingerTableEntries[i].IntervalBegin && id < FingerTableEntries[i].IntervalEnd &&
                    FingerTableEntries[i].Successor != null)
                {
                    return FingerTableEntries[i].Successor;
                }
            }

            return FingerTableEntries[_numberOfFingerTableEntries - 1].Successor;
        }

        /// <summary>
        /// Initialize fingertable by calling FindSuccessor for the start value of fingertable entries.
        /// <param name="id"></param>
        /// <param name="connectionNode"></param>
        /// <param name="destinationNode"></param>
        /// <param name="relayServiceAdapter"></param>
        /// <returns>"void"</returns>
        public void FixFingers(int id, NodeDto connectionNode, Node destinationNode,
            IDhtRelayServiceAdapter relayServiceAdapter)
        {
            FingerTableEntries[0].Successor = connectionNode;

            for (int i = 1; i < _numberOfFingerTableEntries; i++)
            {
                 destinationNode.FindSuccessor(FingerTableEntries[i].Start, connectionNode, destinationNode);
            }     
        }

        public bool Include(int id)
        {
            return FingerTableEntries.FirstOrDefault(x => x.Start.Equals(id)) != null;
        }

        public void AddEntries(NodeDto successor, int id)
        {
            
            for (int i = 1; i < _numberOfFingerTableEntries; i++)
            {
                if (FingerTableEntries[i].Successor == null)
                { 
                    FingerTableEntries[i].Successor = successor;
                }else

                if (FingerTableEntries[i].Start == id)
                {
                    FingerTableEntries[i].Successor = successor;
                    return;
                }
            }
        }

        public override string ToString()
        {
             return JsonCustomFormatter.SerializeObject(this, 3);
        }
    }
}