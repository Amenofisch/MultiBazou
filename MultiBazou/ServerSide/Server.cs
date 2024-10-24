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
        public static readonly Dictionary<int, ServerClient> Clients = new();

        public delegate void PacketHandler(int fromClient, Packet packet);

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

            Plugin.log.LogInfo("Server started successfully!");
            ServerData.isRunning = true;
            _isStopping = false;

            Application.runInBackground = true;

            Client.instance.ConnectToServer("127.0.0.1");
        }

        public static void Stop()
        {
            if (!ServerData.isRunning) return;

            Application.runInBackground = false;
            foreach (var id in Server.Clients.Keys)
            {
                ServerSend.DisconnectClient(id, "Server is shutting down.");
            }

            ServerData.ResetData();
            _isStopping = true;

            _udpListener?.Close();
            _tcpListener?.Stop();

            Clients?.Clear();
            packetHandlers?.Clear();

            ModUI.Instance.window = GUIWindow.Main;
            Plugin.log.LogInfo("Server Closed.");
        }

        private static void TcpConnectCallback(IAsyncResult result)
        {
            if (_isStopping)
                return;

            // this is needed so the server keeps listening for new clients,
            // as otherwise when a user joins it wouldn't accept any new clients.
            var client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

            foreach (var clientID in Clients.Keys.Where(clientID => Clients[clientID].ServerTcp.Socket == null))
            {
                try
                {
                    Clients[clientID].ServerTcp.Connect(client);
                }
                catch (Exception ex)
                {
                    Plugin.log.LogError("SV: TCP Connect Failed: " + ex.Message);
                }

                return;
            }

            Plugin.log.LogInfo($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void UDPReceiveCallback(IAsyncResult result)
        {
            if (_isStopping)
                return;
            try
            {
                var receivedIP = new IPEndPoint(IPAddress.Any, 0);
                var data = _udpListener.EndReceive(result, ref receivedIP);
                _udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4)
                    return;


                using var packet = new Packet(data);
                var clientId = packet.ReadInt();
                
                if (clientId == 0)
                    return;

                if (Clients[clientId].ServerUdp.EndPoint == null)
                {
                    Clients[clientId].ServerUdp.Connect(receivedIP);
                    return;
                }


                if (Clients[clientId].ServerUdp.EndPoint.ToString() == receivedIP.ToString())
                {
                    Clients[clientId].ServerUdp.HandleData(packet);
                }
            }
            catch (Exception ex)
            {
                // TODO: properly handle error
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if (clientEndPoint != null)
                {
                    _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                // TODO: properly handle error
            }
        }

        private static void InitializeServerData()
        {
            for (var i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new ServerClient(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)PacketTypes.Empty, ServerHandle.Empty },
                { (int)PacketTypes.Welcome, ServerHandle.WelcomeReceived },
                { (int)PacketTypes.ReadyState, ServerHandle.ReadyState },
                { (int)PacketTypes.Disconnect, ServerHandle.Disconnect },

                { (int)PacketTypes.PlayerPosition, ServerHandle.PlayerPosition },
                { (int)PacketTypes.PlayerInitialPos, ServerHandle.PlayerInitialPosition },
                { (int)PacketTypes.PlayerRotation, ServerHandle.PlayerRotation },
                { (int)PacketTypes.PlayerSceneChange, ServerHandle.PlayerSceneChange },
            };
        }
    }
}