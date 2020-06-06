using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DHT.Formatting;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using Serilog;

namespace DHT
{
    //https://github.com/StatTag/JupyterKernelManager/issues/7
    //To many instances started at the same time can cause it to shut down
    //TODO: should replace with async router + dealer sockets. 
    public sealed class DhtRelayNetMqAdapter : IDhtRelayServiceAdapter
    {
        private readonly INetworkAdapter _networkAdapter;
        public string? ConnectionUrl { get; set; }

        public List<RequestSocket> _clients;

        public DhtRelayNetMqAdapter(INetworkAdapter networkAdapter)
        {
            _networkAdapter = networkAdapter;
            _clients = new List<RequestSocket>();
        }
        
        public void SendRpcCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto)
        {
            if (connectingNode == null) return;
            Client(connectingNode,protocolCommandDto);
        }


        public void Client(NodeDto connectingNode,
            DhtProtocolCommandDto protocolCommandDto)
        {
            var address = $"tcp://{connectingNode.IpAddress}:{connectingNode.Port}";
            RequestSocket client = null;
            client = _clients.FirstOrDefault(socket => socket.Options.LastEndpoint.Equals(address));
            if (client == null)
            {
                client = new RequestSocket();
                _clients.Add(client);
            }

            try
            {
                client.Connect(address);
                client.TrySendFrame(protocolCommandDto.ToString());
                client.TryReceiveSignal(out bool signal);
            }
            catch (NetMQException e)
            {
                Log.Logger.Error(e,e.Message);
                Console.WriteLine(e.ErrorCode);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception,exception.Message);
                Console.WriteLine(exception.Message);
            }
            finally
            {
                _clients.Remove(client);
                client.Disconnect(address);
                client.Dispose();
            }
        }


    

        public Task<DhtProtocolCommandDto> ServerAsync()
        {
            using var server = new ResponseSocket();
            server.Bind($"tcp://{ConnectionUrl}");
            while (true)
            {
                _networkAdapter.Send();
                if (server.TryReceiveFrameString(out string responseMessage))
                {
                    _networkAdapter.Receive(responseMessage);
                    server.TrySignalOK();
                }
            }
        }


        public void Run()
        {
            NetMQConfig.MaxSockets = 1024;
            using NetMQRuntime runtime = new NetMQRuntime();
            runtime.Run(ServerAsync());
        }
    }
}