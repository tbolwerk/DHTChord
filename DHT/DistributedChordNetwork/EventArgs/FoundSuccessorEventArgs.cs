namespace DHT.DistributedChordNetwork.EventArgs
{
    public class FoundSuccessorEventArgs: System.EventArgs
    {
            public NodeDto SuccessorNode { get; set; }
            public uint Key { get; set; }
        }
}