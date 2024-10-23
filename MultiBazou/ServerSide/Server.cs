using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using MultiBazou.ClientSide;
using MultiBazou.ServerSide.Data;
using MultiBazou.ServerSide.Handle;
using MultiBazou.Shared;
using UnityEngine;

namespace MultiBazou.ServerSide
{
     public class Server
    {
        private static int MaxPlayers { get; set; }
        private static int Port { get; set; }
        public static readonly Dictionary<int, ServerClient> clients = new Dictionary<int, ServerClient>();

        public delegate void PacketHandler(int fromClient, Packet packet);
        public static readonly Dictionary<int, DateTime> lastClientActivity = new Dictionary<int, DateTime>();
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener _tcpListener;
        private static UdpClient _udpListener;
        private static bool _isStopping;

        public static void Start()
        {
            MaxPlayers = Plugin.MaxPlayer;
            Port = Plugin.Port;

            Plugin.log.LogInfo("Starting server...");
            InitializeServerData();

            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

            _udpListener = new UdpClient(Port);
            _udpListener.BeginReceive(UDPReceiveCallback, null);

            Plugin.log.LogInfo($"Server started successfully !");
            ServerData.isRunning = true;
            _isStopping = false;
            
            Application.runInBackground = true;
            
            Client.Instance.ConnectToServer("127.0.0.1");

            // TODO: use coroutine
            // IDEA: this is basically useless tbh, why should we disconnect the client for being inactive???
           CheckForInactiveClientsRoutine();
        }

        private static void CheckForInactiveClients()
        {
            const int maxInactivityDelay = 60;
            foreach (
                    var clientId 
                     in from 
                         entry 
                         in 
                         lastClientActivity 
                     let clientId = entry.Key 
                     let lastActivity = entry.Value 
                     where (DateTime.Now - lastActivity).TotalSeconds > maxInactivityDelay 
                     select clientId
                )
            {
                ServerSend.DisconnectClient(clientId, "Client Inactive for too long...");
                clients.Remove(clientId);
                lastClientActivity.Remove(clientId);
            }
        }


        private static IEnumerator CheckForInactiveClientsRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(10);
                CheckForInactiveClients();
            }
        }

        public static void Stop()
        {
            if(!ServerData.isRunning) return;
            
            Application.runInBackground = false;
            foreach (int id in Server.clients.Keys)
            {
                ServerSend.DisconnectClient(id, "Server is shutting down.");
            }
            
            ServerData.ResetData();
            _isStopping = true;

            _udpListener?.Close();
            _tcpListener?.Stop();

            clients?.Clear();
            packetHandlers?.Clear();

            ModUI.Instance.window = GUIWindow.Main;
            Plugin.log.LogInfo("Server Closed.");

        }

        private static void TcpConnectCallback(IAsyncResult result)
        {
            if(_isStopping)
                return;
            
            var client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

           foreach (var clientID in clients.Keys.Where(clientID => clients[clientID].tcp.Socket == null))
           {
               clients[clientID].tcp.Connect(client);
               return;
           }

           Plugin.log.LogInfo($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
        }
        
        private static void UDPReceiveCallback(IAsyncResult result)
        {
                if(_isStopping)
                    return;
                try
                {
                    var receivedIP = new IPEndPoint(IPAddress.Any, 0);
                    var data = _udpListener.EndReceive(result, ref receivedIP);
                    _udpListener.BeginReceive(UDPReceiveCallback, null);
                    
                    if (data.Length < 4)
                        return;
                    

                    using (var packet = new Packet(data))
                    {
                        var clientId = packet.ReadInt();
                        if (clientId == 0)
                            return;
                        
                        if (clients[clientId].udp.EndPoint == null)
                        {
                            clients[clientId].udp.Connect(receivedIP);
                            return;
                        }
                        
                        
                        if (clients[clientId].udp.EndPoint.ToString() == receivedIP.ToString())
                        {
                            clients[clientId].udp.HandleData(packet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.log.LogInfo($"Error receiving UDP data: {ex}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    _udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Plugin.log.LogInfo($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new ServerClient(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)PacketTypes.empty, ServerHandle.Empty},
                {(int)PacketTypes.welcome, ServerHandle.WelcomeReceived},
                {(int)PacketTypes.readyState, ServerHandle.ReadyState},
                {(int)PacketTypes.keepAlive, ServerHandle.KeepAlive},
                {(int)PacketTypes.disconnect, ServerHandle.Disconnect},
                
                {(int)PacketTypes.playerPosition, ServerHandle.PlayerPosition},
                {(int)PacketTypes.playerInitialPos, ServerHandle.PlayerInitialPosition},
                {(int)PacketTypes.playerRotation, ServerHandle.PlayerRotation},
                {(int)PacketTypes.playerSceneChange, ServerHandle.PlayerSceneChange},
            };
            Plugin.log.LogInfo("Initialized Packets!");
        }
    }
}