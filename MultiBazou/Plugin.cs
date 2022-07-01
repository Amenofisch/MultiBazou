using BepInEx;
using UnityEngine;
using RiptideNetworking;
using UnityEngine.SceneManagement;
using MultiBazou.Multiplayer;

namespace MultiBazou
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInProcess("Mon Bazou.exe")]
    public class Plugin : BaseUnityPlugin
    {

        private bool _showMpMenu = false;
        private Rect _window = new Rect(200, 200, 240, 240);
        private string _ip = "127.0.0.1";
        private string _port = "7777";
        void Awake()
        {
            
        }

        void Start()
        {
            
        }

        void Update()
        {
            
        }

        void OnGUI()
        {
            if (SceneManager.GetActiveScene().name == "Master")
            {
                if(Event.current.Equals(Event.KeyboardEvent("P")))
                {
                    _showMpMenu = !_showMpMenu;
                }

                if(_showMpMenu)
                {
                    _window = GUILayout.Window(6942069, _window, menu, "Multiplayer");
                }
            }
        }

        void menu(int id)
        {
            if(GUILayout.Button("Host Server"))
            {
                NetworkManager.Singleton.StartHost();
            }
            GUILayout.Label("Enter Server Ip:");
            _ip = GUILayout.TextField(_ip);
            GUILayout.Label("Enter Server Port:");
            _port = GUILayout.TextField(_port);
            if (GUILayout.Button("Connect to Server"))
            {
                if (!string.IsNullOrEmpty(_ip) && !string.IsNullOrEmpty(_port))
                {
                    NetworkManager.Singleton.JoinGame(_ip, _port);
                }
            }
        }

        void FixedUpdate()
        {
            
        }
    }
}
