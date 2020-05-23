namespace DHT
{
    public interface IFingerTable
    {
        public void AddEntry(NodeDto node);
        public NodeDto ClosestPrecedingNode(int id);
    }
}