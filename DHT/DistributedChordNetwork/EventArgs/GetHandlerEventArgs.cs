namespace DHT.DistributedChordNetwork.EventArgs
{
    public class GetHandlerEventArgs:System.EventArgs
    {
        public DhtProtocolCommandDto DhtProtocolCommandDto { get; set; }
    }
}