using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DHT.DistributedChordNetwork;
using DHT.DistributedChordNetwork.EventArgs;
using DHT.DistributedChordNetwork.Networking;
using Microsoft.Extensions.Options;
using Serilog;

namespace DHT
{
    public class DistributedHashtable : IDistributedHashtable
    {
        private readonly IDhtRelayServiceAdapter _relayServiceAdapter;
        private readonly IFingerTable _fingerTable;
        private Node _node;
        private readonly IOptions<DhtSettings> _options;
        private readonly IGenerateKey _generateKey;

        public DistributedHashtable(IDhtRelayServiceAdapter relayServiceAdapter, IFingerTable fingerTable, Node node,
            IOptions<DhtSettings> options, IGenerateKey generateKey)
        {
            _relayServiceAdapter = relayServiceAdapter;
            _fingerTable = fingerTable;
            _node = node;
            _options = options;
            _generateKey = generateKey;
        }


        private void Join(uint id, string ipAddress, int port)
        {
            var bootstrap = new NodeDto {Id = id, IpAddress = ipAddress, Port = port};
            _node.Join(bootstrap);
        }

        public string Get(string key)
        {
            uint hashedKey = (uint)_generateKey.Generate(key);
            Console.WriteLine(hashedKey);
            _node.Get(hashedKey);
            bool isCalled = false;
            string value = null;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            _node.GetResponseEventHandler += (sender, args) =>
            {
                GetResponseHandlerEventArgs eventArgs = (GetResponseHandlerEventArgs)args;
                value = eventArgs.Value;
                isCalled = true;
            };
            while (!isCalled && stopwatch.Elapsed < TimeSpan.FromSeconds(10))
            {
                //Performance sleep
                Thread.Sleep(100);
            }

            return value;
        }

        public void Put(string key, string value)
        {
            uint hashedKey = (uint)_generateKey.Generate(key);
            _node.Put(hashedKey, value);
        }

        private void Create()
        {
            _node.Create();
        }

        public void Run(string[] args)
        {

            var ip = args[0];
            int port = int.Parse(args[1]);
            var address = string.Concat(ip, ":", port);
            var selfKey = _generateKey.Generate(address);

            _fingerTable.CreateFingerTable((uint)selfKey);
            _relayServiceAdapter.ConnectionUrl = address;
            NodeDto self = new NodeDto {Id = (uint)selfKey, IpAddress = ip, Port = port};

            _node.Id = (uint)selfKey;
            Console.WriteLine($"my key is {selfKey}");
            Log.Logger.Information($"my key is {selfKey}");
            _node.IpAddress = self.IpAddress;
            _node.Port = self.Port;

            if (!_options.Value.BootstrapUrls.Contains(address))
            {
                var otherAddress = _options.Value.BootstrapUrls.FirstOrDefault();
                var otherKey = (uint)_generateKey.Generate(otherAddress);
                Join(otherKey, otherAddress.Split(":")[0], Convert.ToInt32(otherAddress.Split(":")[1]));
            }
            else Create();
            
            _node.Start();
        }
    }
}