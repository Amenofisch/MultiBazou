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
        public static Client instance;
        public static int dataBufferSize = 4096;

        public string username = "player";
        public string ip = "127.0.0.1";
        public int port;
        public int Id;
        public ClientTcp Tcp;
        public ClientUDP UDP;

        public bool isConnected;
        public static bool forceDisconnected;

        public delegate void PacketHandler(Packet packet);

        public static Dictionary<int, PacketHandler> packetHandlers;
        
        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
            }
            
            Tcp = new ClientTcp();
            UDP = new ClientUDP();
            InitializeClientData();
        }

        public void ClientOnApplicationQuit()
        {
           // ClientSend.Disconnect(Instance.Id); TODO:FIX
        }
        
        public void ConnectToServer(string ipAddress)
        {
            instance.ip = ipAddress;
            instance.port = Plugin.Port;

            var data = new ClientData();
            ClientData.instance = data;

            try
            {
                isConnected = true;
                Tcp.Connect();
            }
            catch (Exception ex)
            {
                Plugin.log.LogInfo($"Failed to connect to server. Error: {ex}");
                instance.Disconnect();
            }

            if (!isConnected)
            {
                // TODO: properly handle error
                packetHandlers.Clear();
            }
        }

        private static void InitializeClientData()
        {
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)PacketTypes.Welcome, ClientHandle.Welcome },
                { (int)PacketTypes.Disconnect, ClientHandle.Disconnect },
                { (int)PacketTypes.ReadyState, ClientHandle.ReadyState },
                { (int)PacketTypes.UpdatePlayerInDictionary, ClientHandle.UpdatePlayerInDictionary },
                { (int)PacketTypes.UpdatePlayersInDictionary, ClientHandle.UpdatePlayersInDictionary },
                { (int)PacketTypes.StartGame, ClientHandle.StartGame },
                { (int)PacketTypes.SpawnPlayer, ClientHandle.SpawnPlayer },
                
                { (int)PacketTypes.PlayerPosition, ClientHandle.PlayerPosition },
                { (int)PacketTypes.PlayerInitialPos, ClientHandle.PlayerInitialPos },
                { (int)PacketTypes.PlayerRotation, ClientHandle.PlayerRotation},
                { (int)PacketTypes.PlayerSceneChange, ClientHandle.PlayerSceneChange},
            };
        }
        
        public void Disconnect()
        {
            if (!isConnected) return;
            Application.runInBackground = false;
            isConnected = false;
            Tcp.Disconnect();
            UDP.Disconnect();

            Tcp.Socket?.Close();
            UDP.Socket?.Close();

            ClientData.instance.GameReady = false;
            ClientData.instance = null;
            GameData.Instance = null;

            ModUI.Instance.window = GUIWindow.Main;
                
            Plugin.log.LogInfo("Disconnected from server.");
        }
    }
}