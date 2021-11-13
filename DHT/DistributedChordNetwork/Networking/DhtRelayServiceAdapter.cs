// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Text;
// using System.Threading.Tasks;
// using DHT;
// using RelaySocket;
// using Serilog;
//
// namespace RelayService.DataAccessService.RoutingDataAccess.DHT.DistributedChordNetwork.Networking
// {
//     public class DhtRelayServiceAdapter : IDhtRelayServiceAdapter
//     {
//         private readonly INetworkAdapter _networkAdapter;
//         private Dictionary<string, IRelaySocket> _connections;
//         public string? ConnectionUrl { get; set; }
//
//         public DhtRelayServiceAdapter(INetworkAdapter networkAdapter)
//         {
//             _networkAdapter = networkAdapter;
//             _connections = new Dictionary<string, IRelaySocket>();
//         }
//
//         public void SendRpcCommand(NodeDto connectingNode, DhtProtocolCommandDto protocolCommandDto)
//         {
//             string endpoint = $"{connectingNode.IpAddress}:{connectingNode.Port}";
//             if (!_connections.TryGetValue(endpoint, out IRelaySocket socket))
//             {
//                 Console.Out.WriteLine("try new connection");
//                 socket = new MessageSocket();
//                 _connections.Add(endpoint, socket);
//             }
//
//             if (!socket.Connected)
//             {
//                 Connect(connectingNode, socket);
//             }
//
//             if (socket.Connected)
//             {
//                 byte[] bytes = Encoding.UTF8.GetBytes(protocolCommandDto.ToString());
//                 socket.Send(bytes);
//             }
//
//             if (socket.RemoteEndPoint == null ||
//                 !socket.RemoteEndPoint.Address.ToString().Equals(connectingNode.IpAddress))
//             {
//                 Console.Out.WriteLine("close");
//                 socket.Close();
//                 _connections.Remove(endpoint);
//             }
//         }
//
//         private void Connect(NodeDto connectingNode, IRelaySocket socket)
//         {
//             try
//             {
//                 Console.Out.WriteLine("connecting to: " + connectingNode.IpAddress + ":" + connectingNode.Port);
//                 IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(connectingNode.IpAddress), connectingNode.Port);
//                 socket.Connect(endpoint);
//             }
//             catch (Exception e)
//             {
//                 Log.Debug(e.Message);
//             }
//         }
//
//         public void Run()
//         {
//             try
//             {
//                 int port = int.Parse(ConnectionUrl?.Split(":")[1]);
//                 IRelaySocket serverSocket = new MessageSocket();
//                 serverSocket.Bind(port);
//                 serverSocket.Listen();
//                 Task.Run(() =>
//                 {
//                     while (true)
//                     {
//                         IRelaySocket connectedSocket = serverSocket.Accept();
//                         Console.Out.WriteLine("");
//                         Task.Run(() => Receive(connectedSocket));
//                     }
//                 });
//             }
//             catch (Exception e)
//             {
//                 Log.Logger.Error(e, e.Message);
//                 Log.Debug(e.Message);
//             }
//         }
//
//         private void Receive(IRelaySocket socket)
//         {
//             if (socket == null)
//             {
//                 return;
//             }
//
//             while (socket.Connected)
//             {
//                 byte[] bytes = socket.Receive();
//                 string result = Encoding.UTF8.GetString(bytes);
//
//                 _networkAdapter.Receive(result);
//             }
//
//             socket.Close();
//         }
//     }
// }