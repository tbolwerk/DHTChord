namespace DHT.DistributedChordNetwork.EventArgs
{
    public class GetResponseHandlerEventArgs:System.EventArgs
    {
        public NodeDto DestinationNode { get; set; }
        public uint Key { get; set; }
        public string Value { get; set; }
    }
}