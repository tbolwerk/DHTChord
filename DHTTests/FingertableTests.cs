using System;
using DHT;
using NUnit.Framework;

namespace DHTTests
{
    public class FingertableTests
    {
        [Test]
        public void CreateFingerTableValidInputParameterTest()
        {
            IFingerTable fingerTable = new FingerTable(128);
            fingerTable.CreateFingerTable(80);

            FingerTableEntry[] fingerTableEntries = new FingerTableEntry[7];
            fingerTableEntries[0] = new FingerTableEntry { Start = 81, IntervalBegin = 81, IntervalEnd = 82, Successor = null };
            fingerTableEntries[1] = new FingerTableEntry { Start = 82, IntervalBegin = 82, IntervalEnd = 84, Successor = null };
            fingerTableEntries[2] = new FingerTableEntry { Start = 84, IntervalBegin = 84, IntervalEnd = 88, Successor = null };
            fingerTableEntries[3] = new FingerTableEntry { Start = 88, IntervalBegin = 88, IntervalEnd = 96, Successor = null };
            fingerTableEntries[4] = new FingerTableEntry { Start = 96, IntervalBegin = 96, IntervalEnd = 112, Successor = null };
            fingerTableEntries[5] = new FingerTableEntry { Start = 112, IntervalBegin = 112, IntervalEnd = 16, Successor = null };
            fingerTableEntries[6] = new FingerTableEntry { Start = 16, IntervalBegin = 16, IntervalEnd = 80, Successor = null };

            Assert.AreEqual(fingerTableEntries, fingerTable.FingerTableEntries);
        }

        [Test]
        public void CreateFingerTableInvalidInputParameterTest()
        {
            IFingerTable fingerTable = new FingerTable(128);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => fingerTable.CreateFingerTable(-1));
            Assert.AreEqual("ID index out of range: -1 (Parameter 'id')", exception.Message);
        }

        [Test]
        public void ClosestPrecedingNodeValidInputParametersTest()
        {
            // Create fingertable where maxnodes in chord ring is 128, this means every node has a fingertable of 7 entries
            IFingerTable fingerTable = new FingerTable(128);

            // Create fingertable for node 7
            fingerTable.CreateFingerTable(7);

            fingerTable.AddEntry(new NodeDto { Id = 12 }, 8);
            fingerTable.AddEntry(new NodeDto { Id = 12 }, 9);
            fingerTable.AddEntry(new NodeDto { Id = 12 }, 11);
            fingerTable.AddEntry(new NodeDto { Id = 15 }, 15);
            fingerTable.AddEntry(new NodeDto { Id = 12 }, 23);
            fingerTable.AddEntry(new NodeDto { Id = 12 }, 39);
            fingerTable.AddEntry(new NodeDto { Id = 7 }, 71);

            var actual = fingerTable.ClosestPrecedingNode(28);

            var expected = 12;
            Assert.AreEqual(expected, actual.Id);
        }

        [Test]
        public void ClosestPrecedingNodeInvalidInputParametersTest()
        {
            IFingerTable fingerTable = new FingerTable(128);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => fingerTable.ClosestPrecedingNode(-1));

            Assert.AreEqual("ID index out of range: -1 (Parameter 'id')", exception.Message);
        }
    }
}