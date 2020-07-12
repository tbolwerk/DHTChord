namespace DHT.DistributedChordNetwork.EventArgs
{
    public class CheckPredecessorResponseEventArgs : System.EventArgs
    {
        public NodeDto Predecessor { get; set; }
    }
}