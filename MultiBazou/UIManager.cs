using MultiBazou.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MultiBazou
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _singleton;
        public static UIManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    Debug.Log($"{nameof(UIManager)} instance already exists, destroying object!");
                    Destroy(value);
                }
            }
        }

        internal string Username => ""; // This has to be set from a UI Element, although not added yet

        private void Awake()
        {
            Singleton = this;
        }

        public void HostClicked()   // BUTTON IS MISSING
        {
            NetworkManager.Singleton.StartHost();
        }

        public void JoinClicked()   // BUTTON IS MISSING
        {
            NetworkManager.Singleton.JoinGame("");  // Get IP from UI Element
        }

        public void LeaveClicked()
        {
            NetworkManager.Singleton.LeaveGame();
            BackToMain();
        }

        internal void BackToMain()
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}