using UnityEngine;

namespace MultiBazou.ClientSide.Data
{
    public class ContentManager : MonoBehaviour
    {
        public float GameVersion { get; private set; }
        
        public static ContentManager instance;
        
        public void Initialize()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
            }
            
            GetGameVersion();
        }

        private void GetGameVersion()
        {
            GameVersion = GameManager.CurrentGameVersion;
        }
    }
}