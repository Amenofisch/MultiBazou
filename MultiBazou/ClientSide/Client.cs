using System;
using System.Collections.Generic;
using MultiBazou.ClientSide.Data;
using MultiBazou.ClientSide.Handle;
using MultiBazou.ClientSide.Transport;
using MultiBazou.Shared;
using UnityEngine;

namespace MultiBazou.ClientSide
{
    public class Client: MonoBehaviour
    {
        public static Client Instance;
        public static int dataBufferSize = 4096;

        public string username = "player";
        public string ip = "127.0.0.1";
        public int port;
        public int Id;
        public ClientTcp tcp;
        public ClientUDP udp;

        public bool isConnected;
        public static bool forceDisconnected;

        public delegate void PacketHandler(Packet packet);

        public static Dictionary<int, PacketHandler> packetHandlers;
        
        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Plugin.log.LogInfo( this.ToString() + " instance already exists, destroying...");
                Destroy(this);
            }
            tcp = new ClientTcp();
            udp = new ClientUDP();
        }

        public void ClientOnApplicationQuit()
        {
           // ClientSend.Disconnect(Instance.Id); TODO:FIX
        }
        
        public void ConnectToServer(string ipAddress)
        {
            InitializeClientData();
            Instance.ip = ipAddress;
            Instance.port = Plugin.Port;

            ClientData data = new ClientData();
            ClientData.instance = data;

            try
            {
                isConnected = true;
                tcp.Connect();
            }
            catch (Exception ex)
            {
                Plugin.log.LogInfo($"Failed to connect to server. Error: {ex}");
                Instance.Disconnect();
            }

            if (!isConnected)
            {
                // TODO: properly handle error
                packetHandlers.Clear();
            }
        }

        private void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)PacketTypes.welcome, ClientHandle.Welcome },
                { (int)PacketTypes.keepAlive, ClientHandle.KeepAlive},
                { (int)PacketTypes.keepAliveConfirmed, ClientHandle.KeepAliveConfirmation},
                { (int)PacketTypes.disconnect, ClientHandle.Disconnect },
                { (int)PacketTypes.readyState, ClientHandle.ReadyState },
                { (int)PacketTypes.playerInfo, ClientHandle.PlayerInfo },
                { (int)PacketTypes.playersInfo, ClientHandle.PlayersInfo },
                { (int)PacketTypes.startGame, ClientHandle.StartGame },
                { (int)PacketTypes.spawnPlayer, ClientHandle.SpawnPlayer },
                
                { (int)PacketTypes.playerPosition, ClientHandle.PlayerPosition },
                { (int)PacketTypes.playerInitialPos, ClientHandle.PlayerInitialPos },
                { (int)PacketTypes.playerRotation, ClientHandle.PlayerRotation},
                { (int)PacketTypes.playerSceneChange, ClientHandle.PlayerSceneChange},
            };
        }
        
        public void Disconnect()
        {
            if (isConnected)
            {
                Application.runInBackground = false;
                isConnected = false;
                tcp.Disconnect();
                udp.Disconnect();
                
                if(tcp.Socket != null)
                    tcp.Socket.Close();
                if (udp.Socket != null)
                    udp.Socket.Close();

                ClientData.instance.GameReady = false;
                ClientData.instance = null;
                GameData.instance = null;

                ModUI.Instance.window = GUIWindow.Main;
                
                Plugin.log.LogInfo("CL : Disconnected from server.");
            }
        }
    }
}