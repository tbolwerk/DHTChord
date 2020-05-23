using System;
using System.Threading.Tasks;
using System.Timers;
using NetMQ;

namespace DHT
{
    public class DistributedHashtable : IDistributedHashtable
    {
        private Node _node;

        public DistributedHashtable(int id, string ipAddress, int port, IDhtRelayServiceAdapter relay)
        {
            var fingerTable = new FingerTable(null);
            var relayAdapter = relay;
            this._node = new Node(relayAdapter, fingerTable) {Id = id, IpAddress = ipAddress, Port = port};
        }

        public void Start()
        {

            _node.Start();
         
            
        }

        public Task Join(int id, string ipAddress, int port)
        {
            var bootstrap = new NodeDto {Id = id, IpAddress = ipAddress, Port = port};
            return _node.Join(bootstrap);
        }

        public void Create()
        {
            _node.Create();
        }
    }
}