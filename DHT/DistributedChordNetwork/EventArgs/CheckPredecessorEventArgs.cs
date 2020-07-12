namespace DHT.DistributedChordNetwork.EventArgs
{
    public class CheckPredecessorEventArgs:System.EventArgs
    {
        public NodeDto DestinationNode { get; set; }
    }
}