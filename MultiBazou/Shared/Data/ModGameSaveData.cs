using System;

namespace MultiBazou.Shared.Data
{
    [Serializable]
    public class ModGameSaveData
    {
        public string SaveName;
        public int TimeStamp;
        public float GameVersion;
        public bool isHardMode;
        public bool isPermaDeath;

        public ModGameSaveData(GameSaveData data)
        {
            SaveName = data.SaveName;
            TimeStamp = data.TimeStamp;
            GameVersion = data.GameVersion;
        }
        
        public ModGameSaveData()
        {
            SaveName = "test";
            TimeStamp = 0;
            GameVersion = 0;
        }

        public GameSaveData ToGame()
        {
            var profileData = new GameSaveData(TimeStamp)
            {
                SaveName = SaveName,
                GameVersion = GameVersion,
                isPermadeath = isPermaDeath,
                isHardMode = isHardMode
            };

            return profileData;
        }
    }
}