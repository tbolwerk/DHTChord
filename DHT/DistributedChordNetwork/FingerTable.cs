using System;
using System.Collections.Generic;

namespace DHT
{
    public class FingerTable : IFingerTable
    {
        private readonly Dictionary<int, NodeDto> _fingerTable;


        public FingerTable(NodeDto successor)
        {
            _fingerTable = new Dictionary<int, NodeDto> {{0, successor}};
        }

        public void AddEntry(NodeDto node)
        {
            _fingerTable[0] = node;
        }
        public NodeDto ClosestPrecedingNode(int id)
        {
            return _fingerTable[0];
        }
    }
}