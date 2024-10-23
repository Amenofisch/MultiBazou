using System.Linq;
using MultiBazou.ClientSide;
using MultiBazou.ClientSide.Data;
using MultiBazou.ClientSide.Handle;
using MultiBazou.ServerSide;
using MultiBazou.ServerSide.Data;
using MultiBazou.ServerSide.Handle;
using MultiBazou.Shared.Data;
using UnityEngine;

namespace MultiBazou.Shared
{
    // TODO: clean this mess up and move everything to a seperate classes and directories
    public class ModUI : MonoBehaviour
    {
        public static ModUI Instance;

        public bool showModUI;
        public GUIWindow window;
        private GUIStyle label_S, text_S, button_S, textField_S;
        
        private float buttonHeight = 20f;
        private float maxScrollViewHeight = 121f; 
        private Vector2 scrollPosition = Vector2.zero;
        
        private Rect mRect = new Rect(Screen.width / 2.5f, Screen.height / 3.5f, 315, 280);
        
        private string saveName = "save";
        private int saveIndex;
        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Plugin.log.LogInfo("Instance already exists, destroying object!");
                Destroy(this);
            }
            window = GUIWindow.Main;
        }

        public void OnGUI()
        {
            if (showModUI)
            {
                Initialize();
                if (ModSceneManager.IsInMenu())
                {
                    switch (window)
                    {
                        case GUIWindow.Main:
                            RenderMain();
                            break;
                        case GUIWindow.Host:
                            RenderHost();
                            break;
                        case GUIWindow.Lobby:
                            RenderLobby();
                            break;
                    }
                }
                else
                {
                    RenderLobby();
                }
            }
        }

        private void Initialize()
        {
            InitializeGuiStyles();
        }


        private void InitializeGuiStyles()
        {
            label_S = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            text_S = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };

            button_S = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };

            textField_S = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16
            };
        }
        
        private void RenderMain()
        {
            GUILayout.BeginArea(mRect, GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(PluginInfo.Version, label_S, GUILayout.Width(190), GUILayout.Height(40));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Username:", text_S);
            Client.Instance.username = GUILayout.TextField(Client.Instance.username, textField_S, GUILayout.Width(190));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
    
            GUILayout.Label("IP Address:", text_S);
            Client.Instance.ip = GUILayout.TextField(Client.Instance.ip, textField_S, GUILayout.Width(190));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Join Lobby", button_S, GUILayout.Width(190), GUILayout.Height(30)))
            {
                JoinLobby();
                window = GUIWindow.Lobby;
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Host Game", button_S, GUILayout.Width(190), GUILayout.Height(30)))
            {
                window = GUIWindow.Host;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
    
            GUILayout.EndArea();
            
            PreferencesManager.SavePreferences();
        }

        private void RenderHost()
        {
            GUILayout.BeginArea(mRect, GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label("Hosting", label_S);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Go back", button_S, GUILayout.Width(190), GUILayout.Height(30)))
            {
                window = GUIWindow.Main;
                
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Start Server on current Save", button_S, GUILayout.Width(160), GUILayout.Height(buttonHeight)))
            {
                            
                if(!ServerData.isRunning)
                    Server.Start();
                else
                {
                    Server.Stop();
                    Server.Start();
                }
                window = GUIWindow.Lobby;
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

        }

        private void RenderLobby()
        {
            GUILayout.BeginArea(mRect, GUI.skin.box);
            
            GUILayout.Label("Lobby", label_S);
            
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(70));
            foreach (int i in ClientData.instance.Players.Keys)
            {
                Player player =  ClientData.instance.Players[i];
                if (player != null)
                {
                    GUILayout.Label("   " + player.username, text_S);
                    GUILayout.Space(5);
                }
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            
            GUILayout.BeginVertical(GUILayout.Width(90));
            foreach (int i in ClientData.instance.Players.Keys)
            {
                Player player =  ClientData.instance.Players[i];

                if (player != null)
                {
                    if (player.isReady)
                    {
                        GUILayout.Label("Ready", text_S);
                    }
                    else
                    {
                        GUILayout.Label("Not Ready", text_S);
                    }
                    GUILayout.Space(25);
                }
            }
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            
            GUILayout.BeginVertical(GUILayout.Width(100));
            foreach (int i in ClientData.instance.Players.Keys)
            {
                Player player =  ClientData.instance.Players[i];
                if (player != null)
                {
                    if (player.id == Client.Instance.Id)
                    {
                        if (GUILayout.Button("Ready", button_S)) 
                        {
                            player.isReady = !player.isReady;
                            ClientSend.SendReadyState(player.isReady, i);
                        }
                    }
                    if (Client.Instance.Id == 1 && player.id != 1) 
                    {  
                        if (GUILayout.Button("Kick", button_S)) 
                        {
                            ServerSend.DisconnectClient(player.id, "You've been kicked from server!");
                            Server.clients[player.id].Disconnect(player.id);
                        }
                    }
                }
                GUILayout.Space(40);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
            if (ServerData.isRunning)
            {
                if (ModSceneManager.IsInMenu())
                {
                    if (GUILayout.Button("Start Game", button_S, GUILayout.Width(175), GUILayout.Height(35)))
                    {
                        if (ServerData.Players.Values.Any(player => player != null && !player.isReady))
                        {
                            return;
                        }

                        StartGame();
                        showModUI = false;
                    }
                }

                if(GUILayout.Button("Close Server", button_S, GUILayout.Width(175), GUILayout.Height(35))) 
                {
                    window = GUIWindow.Main;
                    Server.Stop();
                    if (!ModSceneManager.IsInMenu())
                    {
                        // IDEA: maybe send back to main menu?
                    }
                }
            }
            else
            {
                if(GUILayout.Button("Disconnect", button_S, GUILayout.Width(100), GUILayout.Height(35)))
                {
                    window = GUIWindow.Main;
                    Client.Instance.Disconnect();
                }
            }
            
            GUILayout.Space(15);
            GUILayout.EndArea();
            
        }

        private void StartGame()
        {
            SavesManager.StartGame();
            ServerSend.StartGame(new ModGameSaveData());
        }
        
        private void JoinLobby()
        {
            if (!string.IsNullOrEmpty(Client.Instance.username) && !string.IsNullOrEmpty(Client.Instance.ip))
            {
                Client.Instance.ConnectToServer(Client.Instance.ip);
                window = GUIWindow.Lobby;
                Application.runInBackground = true;
            }
        }

        public void ShowUI()
        {
            if (Input.GetKeyDown(Plugin.instance.ConfigModUiToggle.Value))
            {
                if(ModSceneManager.GetCurrentScene() == GameScene.Unknown)
                    return;
                
                showModUI = !showModUI;
            }
        }
    }

    public enum GUIWindow
    {
        Main,
        Host,
        Lobby
    }
}