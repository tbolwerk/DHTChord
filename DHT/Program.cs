using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DHT
{
    class Program
    {
        public static void Main(string[] args)
        {
            // TODO: replace STUB used for relay impl
            NodeDto self = new NodeDto {Id = int.Parse(args[0]), IpAddress = args[1], Port = int.Parse(args[2])};
            var dht = new DistributedHashtable(self.Id, self.IpAddress, self.Port, new DhtRelayNetMqAdapter(self.IpAddress+":"+self.Port));
            if (args.Length > 3) dht.Join(int.Parse(args[3]), args[4], int.Parse(args[5]));
            else dht.Create();
            dht.Start();
        }
    }
}