using System.Collections;
using CMF;
using UnityEngine;

namespace MultiBazou.ClientSide.Data
{
    public class GameData
    {
        public static GameData instance;
        public static bool dataInitialized;
        
        public GameObject LocalPlayer;
        
        public void Initialize()
        {
            instance = this;
            
            LocalPlayer = Object.FindObjectOfType<Gameplay>().PlayerWalking.gameObject;
            
            dataInitialized = true;
        }
    }
}