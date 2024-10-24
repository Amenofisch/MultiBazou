using CMF;
using UnityEngine;

namespace MultiBazou.ClientSide.Data
{
    public class GameData
    {
        private static GameData _instance;

        public static GameData Instance
        {
            get => _instance ??= new GameData();
            set => _instance = value;
        }
        
        public static bool dataInitialized;
        
        public GameObject LocalPlayer;
        public GameObject LocalPlayerCamera;
        
        public void Initialize()
        {
            while (LocalPlayer == null && LocalPlayerCamera == null)
            {
                LocalPlayer = Object.FindObjectOfType<Gameplay>().PlayerWalking.gameObject;
                
                if (LocalPlayer != null)
                {
                    LocalPlayerCamera = LocalPlayer.GetComponent<AdvancedWalkerController>().playerInteractable.gameObject;
                }
            }
            
            dataInitialized = true;
        }
    }
}