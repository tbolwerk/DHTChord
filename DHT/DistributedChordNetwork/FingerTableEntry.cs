using System;
using System.Collections.Generic;
using System.Text;

namespace DHT
{
    public class FingerTableEntry
    {
        public int Start { get; set; }
        public int IntervalBegin { get; set; }
        public int IntervalEnd { get; set; } 
        public NodeDto Successor { get; set; }

        public FingerTableEntry()
        {

        }

        public void SetInterval(int intervalBegin, int intervalEnd)
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
