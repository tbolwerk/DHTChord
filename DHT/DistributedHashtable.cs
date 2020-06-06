using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NetMQ;
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


        public void Join(uint id, string ipAddress, int port)
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
            }

            return value;
        }

        public void Put(string key, string value)
        {
            uint hashedKey = (uint)_generateKey.Generate(key);
            _node.Put(hashedKey, value);
        }

        public void Create()
        {
            _node.Create();
        }

        public void Run(string[] args)
        {
            var selfKey = _generateKey.Generate(string.Concat(args[1], ":", int.Parse(args[2])));

            _fingerTable.CreateFingerTable((uint)selfKey);
            _relayServiceAdapter.ConnectionUrl = $"{args[1]}:{args[2]}";
            NodeDto self = new NodeDto {Id = (uint)int.Parse(args[0]), IpAddress = args[1], Port = int.Parse(args[2])};

            _node.Id = (uint)selfKey;
            Console.WriteLine($"my key is {selfKey}");
            Log.Logger.Information($"my key is {selfKey}");
            _node.IpAddress = self.IpAddress;
            _node.Port = self.Port;
            if (args.Length > 3)
            {
                var otherKey = (uint)_generateKey.Generate(string.Concat(args[4], ":", int.Parse(args[5])));
                Join(otherKey, args[4], int.Parse(args[5]));
            }
            else Create();

            _node.Start();
        }
    }
}