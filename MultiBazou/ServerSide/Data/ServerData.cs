using System.Collections.Generic;
using MultiBazou.Shared;

namespace MultiBazou.ServerSide.Data
{
    public static class ServerData
    {
        public static readonly Dictionary<int, Player> Players = new Dictionary<int, Player>(); 
        public static bool isRunning;

        public static void ResetData()
        {
            Players.Clear();
            isRunning = false;
        }
    }
}