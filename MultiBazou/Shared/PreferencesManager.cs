using System;
using System.Collections.Generic;
using System.IO;
using MultiBazou.ClientSide;
using Newtonsoft.Json;
using UnityEngine;

namespace MultiBazou.Shared
{
    public static class PreferencesManager // TODO:Clean This!
    {
        private const string ModFolderPath = @"Mods\MultiBazou\";
        private const string PreferencesFilePath = ModFolderPath + "preferences.ini";
        private const string SaveFolderPath = ModFolderPath + "saves";
        private const string DefaultIPAdress = "127.0.0.1";
        private const string DefaultUsername = "player";
        
    public static void LoadPreferences()
    {
        if (File.Exists(PreferencesFilePath))
        {
            string serializedPreferences = File.ReadAllText(PreferencesFilePath);

            if (serializedPreferences.Length > 0)
            {
                Preferences preferences = JsonConvert.DeserializeObject<Preferences>(serializedPreferences);

                if (preferences != null)
                {
                    Client.Instance.ip = preferences.IpAddress;
                    Client.Instance.username = preferences.Username;
                }
            }

            Plugin.log.LogInfo("Loaded Preferences Successfully!");
        }
        else
        {
            Plugin.log.LogInfo("No saved preferences, using default...");
            Client.Instance.ip = DefaultIPAdress;
            Client.Instance.username = DefaultUsername;
        }
    }

    public static void SavePreferences()
    {
        Preferences preferences = new Preferences
        {
            IpAddress = Client.Instance.ip,
            Username = Client.Instance.username
        };

        string serializedPreferences = JsonConvert.SerializeObject(preferences);
        if (!Directory.Exists(ModFolderPath))
        {
            Directory.CreateDirectory(ModFolderPath);
        }
        
        File.WriteAllText(PreferencesFilePath, serializedPreferences);
    }
}

    public class Preferences
    {
        public string IpAddress { get; set; }
        public string Username { get; set; }
    }
}