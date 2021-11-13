using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Serilog;

namespace DHT.DistributedChordNetwork.Networking
{
    //https://github.com/StatTag/JupyterKernelManager/issues/7
    //To many instances started at the same time can cause it to shut down
    //TODO: should replace with async router + dealer sockets. 
    public sealed class DhtRelayNetMqAdapter : IDhtRelayServiceAdapter
    {
        private readonly INetworkAdapter _networkAdapter;
        public string? ConnectionUrl { get; set; }


        public DhtRelayNetMqAdapter(INetworkAdapter networkAdapter)
        {
            _networkAdapter = networkAdapter;
        }

        public void SendRpcCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto)
        {
            if (connectingNode == null) return;
            Client(connectingNode, protocolCommandDto);
        }

        public Queue<Action> RpcCalls => _networkAdapter.RpcCalls;


        public void Client(NodeDto connectingNode,
            DhtProtocolCommandDto protocolCommandDto)
        {
            var cleanAddress = connectingNode.IpAddress.Replace("127.0.0.1", "localhost");
            var address = $"tcp://{cleanAddress}:{connectingNode.Port}";
            RequestSocket client = null;
            // client = _clients.FirstOrDefault(socket => socket.Options.LastEndpoint.Equals(address));
            // if (client == null)
            // {
            //     client = new RequestSocket();
            //     _clients.Add(client);
            // }
            client = new RequestSocket();
            try
            {
                client.Connect(address);
                client.TrySendFrame(protocolCommandDto.ToString());
                client.TryReceiveSignal(out bool signal);
            }
            catch (NetMQException e)
            {
                Log.Logger.Error(e, e.Message);
                Log.Debug(e.ErrorCode.ToString());
                Log.Debug(e.StackTrace);
                Log.Debug(e.Message);
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception, exception.Message);
                Log.Debug(exception.Message);
            }
            finally
            {
                // _clients.Remove(client);
                // client.Disconnect(address);
                // client.Dispose();
            }
        }


        public void ServerAsync()
        {
            using var server = new ResponseSocket();
            var url = ConnectionUrl.Replace("127.0.0.1", "*");
            server.Bind($"tcp://{url}");

            while (server.TryReceiveFrameString(timeout: TimeSpan.FromMinutes(5), out string responseMessage))
            {
                _networkAdapter.Receive(responseMessage);
                server.TrySignalOK();
            }
        }
        public void Run()
        {
            NetMQConfig.MaxSockets = 1024;
            using NetMQRuntime runtime = new NetMQRuntime();
            runtime.Run(Task.Run(ServerAsync));
        }
    }
}