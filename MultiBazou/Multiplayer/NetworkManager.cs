using System;
using System.Linq;
using Riptide;
using Riptide.Transports.Rudp;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace MultiBazou
{
    public enum ServerToClientId : ushort
    {
        playerName = 1,
        spawnPlayer = 9,
        playerPosRot = 3 
    }

    public enum ClientToServerId : ushort
    {
        playerName = 1,
        playerPosRot = 3
    }
    
    public class ClientNetworkManager : MonoBehaviour
    {
        private static ClientNetworkManager _singleton;
        
        public static ClientNetworkManager Singleton
        {
            get => _singleton;
            private set
            {
                if(_singleton == null)
                {
                    _singleton = value;
                } else if (_singleton != value)
                {
                    UnityEngine.Debug.Log("ClientNetworkManager instance already exists, destroying...");
                    Destroy(value);
                }
            }
            
        }

        public string ip;
        public ushort port;
        public string username;

        public Client Client { get; set; }

        public void Awake()
        {
            Singleton = this;
        }

        public void Start()
        {
            Client = new Client(1000, 1000, 5, "CLIENT");

            Client.Connected += DidConnect;
            Client.ConnectionFailed += FailedToConnect;
            Client.ClientDisconnected += PlayerLeft;
            Client.Disconnected += DidDisconnect;
        }

        public void FixedUpdate()
        {
            Client.Tick();
        }

        public void OnApplicationQuit()
        {
            Client.Disconnect();

            Client.Connected -= DidConnect;
            Client.ConnectionFailed -= FailedToConnect;
            Client.ClientDisconnected -= PlayerLeft;
            Client.Disconnected -= DidDisconnect;
        }

        public bool Connect()
        {
            return Client.Connect(ip + ":" + port);
        }

        private void DidConnect(object sender, EventArgs e)
        {
            UnityEngine.Debug.Log("Connected, sending spawn message...");
            Message username = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.playerName);
            username.Add(this.username);
            Client.Send(username);
            UnityEngine.Debug.Log("Spawn message sent..");
        }

        private void FailedToConnect(object sender, EventArgs e)
        {
            UnityEngine.Debug.Log("Failed to connect!");
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            UnityEngine.Debug.Log("Player left: " + e.Id);
            ClientPlayerManager.List[e.Id].RemovePlayer();
        }

        private void DidDisconnect(object sender, EventArgs e)
        {
            UnityEngine.Debug.Log("Disconnected");
            ClientPlayerManager.List.Clear();
        }
    }
    public class ServerNetworkManager : MonoBehaviour
    {
        private static ServerNetworkManager _singleton;

        public static ServerNetworkManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    UnityEngine.Debug.Log("ServerNetworkManager instance already exists, destroying...");
                    Destroy(value);
                }
            }
        }

        public ushort port;

        public Server Server { get; private set; }

        public void Awake()
        {
            Singleton = this;
        }

        public void Start()
        {
            Server = new Server(1000, 1000, "SERVER");
            Server.AllowAutoMessageRelay = true;
            Server.ClientConnected += NewPlayerConnected;
            Server.ClientDisconnected += PlayerLeft;
        }

        public void FixedUpdate()
        {
            Server.Tick();
        }

        public void OnApplicationQuit()
        {
            Server.Stop();

            Server.ClientConnected -= NewPlayerConnected;
            Server.ClientDisconnected -= PlayerLeft;
        }

        public void StartServer()
        {
            Server.Start(port, 64);
        }

        private void NewPlayerConnected(object sender, ServerClientConnectedEventArgs e)
        {
            UnityEngine.Debug.Log("New player connected: " + e.Client.Id);
            foreach (var player in ServerPlayerManager.List.Values.Where(player => player.id != e.Client.Id))
            {
                player.SendSpawn(e);
            }
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            UnityEngine.Debug.Log("Player left: " + e.Id);
            ServerPlayerManager.List.Remove(e.Id);
        }
    }
}
