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
                return GameScene.Ingame;
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

            return player.scene == GameScene.Ingame;
        }

        public static bool IsInMenu()
        {
            return SceneManager.GetActiveScene().name == SceneNames.MainMenu;
        }
        
        public static void UpdatePlayerScene()
        {
            Plugin.log.LogInfo("Updating player scene data for " + Client.Instance.Id);
            ClientData.instance.Players[Client.Instance.Id].scene = GetCurrentScene();
            ClientSend.SendSceneChange(GetCurrentScene());
        }
    }
}