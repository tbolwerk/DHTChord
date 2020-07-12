namespace DHT.DistributedChordNetwork.EventArgs
{
    public class FindSuccessorEventArgs : System.EventArgs
    {
        public NodeDto DestinationNode { get; set; }
        public uint Key { get; set; }
    }
}