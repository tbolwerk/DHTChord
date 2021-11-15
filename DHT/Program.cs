using System;
using System.IO;
using System.Threading.Tasks;
using DHT.ConsistentHash;
using DHT.DistributedChordNetwork;
using DHT.DistributedChordNetwork.Networking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DHT
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Setup the container
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File($"{Directory.GetCurrentDirectory()}/DHT-{args[0]}-{args[1]}.log")
                .MinimumLevel.Debug()
                .CreateLogger();

            var serviceProvider = new ServiceCollection()
                .AddOptions<DhtSettings>().Configure(ConfigureSettings)
                .Services.AddLogging(configure =>
                {
                    configure.AddSerilog()
                        .AddEventLog();
                    configure.AddConsole()
                        .AddEventLog();
                })
                .AddSingleton<ISchedule, Scheduler>()
                .AddSingleton<ITimeOutTimerFactory, TimeOutTimerFactory>()
                .AddTransient(typeof(ITimeOutScheduler), typeof(TimeOutScheduler))
                .AddTransient(typeof(IDhtActions), typeof(DhtActions))
                .AddTransient<IHash, Sha1Hash>()
                .AddTransient<IGenerateKey, KeyGenerator>()
                // .AddTransient<IRelaySocket,MessageSocket>()
                .AddSingleton<ISchedule,Scheduler>()
                .AddSingleton<ITimeOutTimerFactory,TimeOutTimerFactory>()
                .AddTransient(typeof(ITimeOutScheduler),typeof(TimeOutScheduler))
                .AddTransient(typeof(IDhtActions),typeof(DhtActions))
                .AddTransient<IHash,Sha1Hash>()
                .AddTransient<IGenerateKey,KeyGenerator>()
                .AddSingleton(typeof(IFingerTable), typeof(FingerTable))
                .AddSingleton(typeof(INetworkAdapter), typeof(NetworkAdapter))
                // .AddSingleton(typeof(IDhtRelayServiceAdapter), typeof(DhtRelayServiceAdapter)) // enable integration with server
                .AddSingleton(typeof(IDhtRelayServiceAdapter), typeof(DhtRelayNetMqAdapter))
                // .AddSingleton(typeof(IDhtRelayServiceAdapter), typeof(DhtRelayAdapter))
                .AddSingleton<IStabilize, NodeStabilizing>()
                .AddSingleton<ICheckPredecessor, NodeCheckingPredecessor>()
                .AddSingleton(typeof(Node), typeof(Node))
                .AddSingleton<IDistributedHashtable, DistributedHashtable>()

                // Add other dependencies here ...
                .BuildServiceProvider();


            
            // Run our host
            var dht = serviceProvider.GetService<IDistributedHashtable>();
            Task.Run(()=>dht.Run(args));
            uint i = 0;
            Console.WriteLine("Enter command: \n * put \n * get");

            while (true)
            {
                var input = Console.ReadLine();
                if (input != null)
                {
                    if (input.StartsWith("/"))
                    {
                        if (input.Split("/")[1].Contains("get"))
                        {
                            Console.WriteLine("Put specify key:");
                            var key = Console.ReadLine();     
                            Console.WriteLine($"I found: {dht.Get(key)} for key:{key}");
                        } 
                        else if (input.Split("/")[1].Contains("put"))
                        {
                            Console.WriteLine("Put specify key:");
                            var key = Console.ReadLine();     
                            Console.WriteLine("Put specify value:");
                            var value = Console.ReadLine();
                            dht.Put(key,value);
                            Console.WriteLine($"+ Key: {key} and Value:{value}");
                        }
                        else
                        {
                            Console.WriteLine("Enter command: \n * put \n * get");
                        }
                    }
                }
            }
        }


        private static void ConfigureSettings(DhtSettings settings)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();
            config.Bind("dhtSettings", settings);
        }
    }
}