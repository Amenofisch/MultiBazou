using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using MultiBazou.ClientSide;
using MultiBazou.ClientSide.Data;
using MultiBazou.ServerSide;
using MultiBazou.ServerSide.Data;
using MultiBazou.ServerSide.Handle;
using MultiBazou.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Logger = BepInEx.Logging.Logger;

namespace MultiBazou
{
    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {

        public ConfigEntry<string> ConfigModUiToggle;
        public const int MaxPlayer = 2;
        public const int Port = 7777;
        
        public Client client;
        public ModUI modUI;
        public ContentManager contentManager;
        public GameObject multiplayerGameObject;

        public bool isModInitialized;
        public static Plugin instance;
        public static ManualLogSource log;
        
        void Awake()
        {
            instance = this;
            log = Logger;
            ConfigModUiToggle = Config.Bind(new ConfigDefinition("General", "modUiKey"), "L", new ConfigDescription("This buttons opens the menu for the multiplayer mod."));
        }

        void Start()
        {
            multiplayerGameObject = new GameObject(PluginInfo.Name);
            DontDestroyOnLoad(multiplayerGameObject);
            SceneManager.activeSceneChanged += OnSceneWasLoaded;
            client = multiplayerGameObject.AddComponent<Client>();
            client.Awake();
            
            modUI = multiplayerGameObject.AddComponent<ModUI>();
            modUI.Awake();
            
            contentManager = multiplayerGameObject.AddComponent<ContentManager>();
            
            PreferencesManager.LoadPreferences();
            isModInitialized = true;
            log.LogInfo(PluginInfo.Name + " has been initialized.");
        }

        private void OnSceneWasLoaded(Scene previousScene, Scene newScene)
        {
            if(newScene.name == SceneNames.MainMenu) contentManager.Initialize();
            if(!isModInitialized) {return;}
            
            if (client.isConnected || ServerData.isRunning)
            {
                log.LogInfo("Scene change occured...");
                GameData.dataInitialized = false;
                ModSceneManager.UpdatePlayerScene();
                if(ModSceneManager.IsInMenu() && client.isConnected && !ServerData.isRunning)
                {
                    Client.Instance.Disconnect();
                    Application.runInBackground = false;
                }
                if(ModSceneManager.IsInMenu() && ServerData.isRunning)
                {
                    Server.Stop();
                    Application.runInBackground = false;
                }
                if(newScene.name == SceneNames.Master)
                {
                    StartCoroutine(ClientData.instance.Initialize());

                    foreach (KeyValuePair<int, Player> player in ClientData.instance.Players)
                    {
                        if (ModSceneManager.IsInGame(player.Value))
                        {
                            log.LogInfo($"Player: {player.Value.username} ingame, Spawning...");
                            if (!ClientData.instance.PlayersGameObjects.ContainsKey(player.Value.id))
                            {
                                ClientData.instance.SpawnPlayer(player.Value);
                            }
                        }
                    }
                }
            }
        }
        
        void Update()
        {
            if(!isModInitialized) {return;}
            if (GameData.dataInitialized)
            {
                if (Client.Instance.isConnected)
                {
                    if(ModSceneManager.IsInGame())
                        ClientData.instance.UpdateClient();
                }
            }
            if (Client.Instance.isConnected)
            {
                if (ClientData.instance.NeedToKeepAlive)
                {
                    if (!ClientData.instance.IsKeepingAlive)
                    {
                        StartCoroutine(ClientData.instance.KeepClientAlive());
                        StartCoroutine(ClientData.instance.isServer_alive());
                    }
                }
            }

            if (ServerData.isRunning)
            {
                if (Server.clients.Count == 0)
                {
                    Server.Stop();
                }
            }
            ThreadManager.UpdateThread();
        }

        public void OnApplicationQuit()
        {
            if (ServerData.isRunning)
            {
                foreach (int id in Server.clients.Keys)
                {
                    ServerSend.DisconnectClient(id, "Server is shutting down.");
                }
            }
        }

        private void OnGUI()
        {
            if(!isModInitialized) {return;}
            if (Event.current.Equals(Event.KeyboardEvent(ConfigModUiToggle.Value))) ModUI.Instance.showModUI = !ModUI.Instance.showModUI;
            
            modUI.OnGUI();
        }
    }
}
