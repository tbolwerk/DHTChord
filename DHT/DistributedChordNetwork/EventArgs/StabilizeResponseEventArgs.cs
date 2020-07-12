namespace DHT.DistributedChordNetwork.EventArgs
{
    public class StabilizeResponseEventArgs : System.EventArgs
    {
        public NodeDto PredecessorOfSuccessor { get; set; }
    }
}