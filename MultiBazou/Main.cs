using BepInEx;
using Riptide;
using Riptide.Utils;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiBazou
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Main : BaseUnityPlugin
    {
        public static Main instance;

        private readonly GameObject clientGameObject = new GameObject("ClientNetworkManager");
        private GameObject clientNetworkObject;

        private readonly GameObject serverGameObject = new GameObject("ServerNetworkManager");
        private GameObject serverNetworkObject;

        private string ip = "127.0.0.1";
        private string port = "7777";
        private string username = Environment.MachineName;

        private Rect _window = new Rect(200, 200, 240, 240);

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            RiptideLogger.Initialize(UnityEngine.Debug.Log, true);

            clientNetworkObject = Instantiate(clientGameObject, Vector3.zero, Quaternion.identity);
            clientNetworkObject.AddComponent<ClientNetworkManager>();

            serverNetworkObject = Instantiate(serverGameObject, Vector3.zero, Quaternion.identity);
            serverNetworkObject.AddComponent<ServerNetworkManager>();
        }
        
        void FixedUpdate()
        {
            if(SceneManager.GetActiveScene().name == "Master")
            {
                SendPostionData();
            }   
        }

        private void SendPostionData()
        {
            var message = Message.Create(MessageSendMode.unreliable, (ushort)ClientToServerId.playerPosRot);
            message.Add(Gameplay.i.PlayerWalking.transform.position);
            message.Add(Gameplay.i.PlayerWalking.transform.rotation);
            ClientNetworkManager.Singleton.Client.Send(message);
        }

        void Update()
        {
          
            // This is causing "object reference not set to an instance" errors

            /*
            foreach (var player in ClientPlayerManager.List)
            {
                player.Value.LerpPosition();
            }
            */
        }

        private void OnGUI()
        {
            if(SceneManager.GetActiveScene().name == "Master")
            {
                _window = GUILayout.Window(46489, _window, menu, "Multiplayer");   
            }
        }

        private void menu(int id)
        {
            ip = GUILayout.TextField(ip);
            port = GUILayout.TextField(port);
            username = GUILayout.TextField(username);

            ClientNetworkManager.Singleton.ip = ip;
            ClientNetworkManager.Singleton.port = ushort.Parse(port);
            ClientNetworkManager.Singleton.username = username;
            ServerNetworkManager.Singleton.port = ushort.Parse(port);

            if (GUILayout.Button("Connect"))
            {
                if (ClientNetworkManager.Singleton.Connect())
                {
                    Debug.Log("[CLIENT] Connected (custom log)");
                } 
            }

            if (GUILayout.Button("Create Server")) ServerNetworkManager.Singleton.StartServer();
        }

        public GameObject SpawnObject(GameObject obj, bool destroyOnLoad = false)
        {
            var go = Instantiate(obj);
            if (!destroyOnLoad) DontDestroyOnLoad(go);
            return go;
        }

        public void DestroyObject(GameObject obj)
        {
            Destroy(obj);
        }

        public void DestroyObject(Component obj)
        {
            Destroy(obj);
        }
    }
}
