namespace DHT.DistributedChordNetwork.EventArgs
{
    public class PutHandlerEventArgs :System.EventArgs
    {
        public DhtProtocolCommandDto? ProtocolCommandDto { get; set; }
    }
}