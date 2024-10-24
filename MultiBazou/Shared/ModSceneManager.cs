using MultiBazou.ClientSide;
using MultiBazou.ClientSide.Data;
using MultiBazou.ClientSide.Handle;
using MultiBazou.Shared.Data;
using UnityEngine.SceneManagement;

namespace MultiBazou.Shared
{
    public class ModSceneManager
    {
        public static GameScene GetCurrentScene()
        {
            if (IsInGame())
                return GameScene.InGame;
            if (IsInMenu())
                return GameScene.Menu;
            
            return GameScene.Unknown;
        }

        public static bool IsInGame()
        {
            return SceneManager.GetActiveScene().name == SceneNames.Master;
        }
        
        public static bool IsInGame(Player player = null)
        {
            if (player == null)
            {
                return IsInGame();
            }

            return player.scene == GameScene.InGame;
        }

        public static bool IsInMenu()
        {
            return SceneManager.GetActiveScene().name == SceneNames.MainMenu;
        }
        
        public static void UpdatePlayerScene()
        {
            ClientData.instance.Players.TryGetValue(Client.instance.Id, out var player);
            
            if (player != null)
            {
                player.scene = GetCurrentScene();
                ClientSend.SendSceneChange(GetCurrentScene());
            }
        }
    }
}