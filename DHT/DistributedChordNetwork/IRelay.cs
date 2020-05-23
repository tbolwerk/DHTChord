namespace DHT
{
    public interface IRelay
    {
        bool connected { get; set; }
        
        void connect(string ip, int port);

        void close();

        void send(string message);

        void send(byte[] data);

        byte[] receive();
    }

    public class RelayImpl : IRelay
    {
        public bool connected { get; set; }
        public void connect(string ip, int port)
        {
            throw new System.NotImplementedException();
        }

        public void close()
        {
            throw new System.NotImplementedException();
        }

        public void send(string message)
        {
            throw new System.NotImplementedException();
        }

        public void send(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public byte[] receive()
        {
            throw new System.NotImplementedException();
        }
    }
}