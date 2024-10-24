using UnityEngine;

namespace MultiBazou.Shared
{
    // IDEA: implement proper loading of the game (when player joining then make his world empty and respawn all entities that are in hosts world)
    public static class SavesManager
    {
        public static void StartGame()
        {
            Application.runInBackground = true;
            Singleton<MainMenu_Manager>.i.StartGame();
        }
    }
}
