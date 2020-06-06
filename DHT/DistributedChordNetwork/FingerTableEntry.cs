using System;
using System.Collections.Generic;
using System.Text;

namespace DHT
{
    public class FingerTableEntry
    {
        public uint Start { get; set; }
        public uint IntervalBegin { get; set; }
        public uint IntervalEnd { get; set; } 
        public NodeDto Successor { get; set; }

        public FingerTableEntry()
        {

        }

        public void SetInterval(uint intervalBegin, uint intervalEnd)
        {
            IntervalBegin = intervalBegin;
            IntervalEnd = intervalEnd;
        }

        public override bool Equals(object obj)
        {
            return obj is FingerTableEntry entry &&
                   Start == entry.Start &&
                   IntervalBegin == entry.IntervalBegin &&
                   IntervalEnd == entry.IntervalEnd &&
                   EqualityComparer<NodeDto>.Default.Equals(Successor, entry.Successor);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, IntervalBegin, IntervalEnd, Successor);
        }
    }
}
