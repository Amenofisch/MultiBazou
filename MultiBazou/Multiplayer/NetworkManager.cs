using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using UnityEngine;

namespace MultiBazou.Multiplayer
{
    internal enum MessageId : ushort
    {
        spawnPlayer = 1,
        playerMovement
    }

    public class NetworkManager : MonoBehaviour
    {
        private static NetworkManager _singleton;
        public static NetworkManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying object!");
                    Destroy(value);
                }
            }
        }

        private ushort port = 7777;
        private ushort maxPlayers = 4;
        [Header("Prefabs")]
        private GameObject playerPrefab = Singleton<Gameplay>.i.PlayerWalking.gameObject;
        private GameObject localPlayerPrefab = Singleton<Gameplay>.i.PlayerWalking.gameObject;

        public GameObject PlayerPrefab => playerPrefab;
        public GameObject LocalPlayerPrefab => localPlayerPrefab;

        internal Server Server { get; private set; }
        internal Client Client { get; private set; }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

            Server = new Server { AllowAutoMessageRelay = true };

            Client = new Client();
            Client.Connected += DidConnect;
            Client.ConnectionFailed += FailedToConnect;
            Client.ClientConnected += PlayerJoined;
            Client.ClientDisconnected += PlayerLeft;
            Client.Disconnected += DidDisconnect;
        }

        private void DidDisconnect(object sender, EventArgs e)
        {
            foreach (Player player in Player.List.Values)
                Destroy(player.gameObject);

            UIManager.Singleton.BackToMain();
        }

        private void FixedUpdate()
        {
            if (Server.IsRunning)
                Server.Tick();

            Client.Tick();
        }

        private void OnApplicationQuit()
        {
            Server.Stop();
            Client.Disconnect();
        }

        internal void StartHost()
        {
            Server.Start(port, maxPlayers);
            Client.Connect($"127.0.0.1:{port}");
        }

        internal void JoinGame(string ipString, string port)
        {
            Client.Connect($"{ipString}:{port}");
        }

        internal void LeaveGame()
        {
            Server.Stop();
            Client.Disconnect();
        }

        private void DidConnect(object sender, EventArgs e)
        {
            Player.Spawn(Client.Id, UIManager.Singleton.Username, Vector3.zero, true);
        }

        private void FailedToConnect(object sender, EventArgs e)
        {
            UIManager.Singleton.BackToMain();
        }

        private void PlayerJoined(object sender, ClientConnectedEventArgs e)
        {
            Player.List[Client.Id].SendSpawn(e.Id);
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            Destroy(Player.List[e.Id].gameObject);
        }

    }
}