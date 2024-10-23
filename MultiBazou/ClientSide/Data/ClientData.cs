using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Michsky.MUIP;
using MultiBazou.ClientSide;
using MultiBazou.ClientSide.Data.PlayerData;
using MultiBazou.ClientSide.Handle;
using MultiBazou.Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace MultiBazou.ClientSide.Data
{
    public class ClientData
    {
        public static ClientData instance;

        private static GameObject _playerPrefab;
        public Dictionary<int, Player> Players = new Dictionary<int, Player>();
        public readonly Dictionary<int, GameObject> PlayersGameObjects = new Dictionary<int, GameObject>();
        
        public bool IsServerAlive = true;
        public bool NeedToKeepAlive = false;
        public bool IsKeepingAlive;
        
        public bool GameReady;

        public IEnumerator Initialize()
        {
            instance = this;
            
            PlayersGameObjects.Clear();
            
            if (GameData.instance != null) GameData.instance.Initialize();
            else
            {
                GameData data = new GameData(); 
                data.Initialize();
            }

            yield return new WaitForSeconds(2);
            yield return new WaitForEndOfFrame();

            GameReady = true;
            
            Plugin.log.LogInfo("------ Game is Ready! ------");
        }
        
        public void UpdateClient()
        {
            Movement.SendPosition();
            Rotation.SendRotation();
            
            foreach (var player in PlayersGameObjects.Where(player => player.Value != null && player.Key != Client.Instance.Id))
            {
                player.Value.GetComponent<ModCharacterController>().UpdatePlayer();
            }

        }
        public IEnumerator KeepClientAlive()
        {
            IsKeepingAlive = true;
            ClientSend.KeepAlive();
            
            yield return new WaitForSeconds(5);
            IsKeepingAlive = false;
        }

        public IEnumerator isServer_alive()
        {
            if (!Client.Instance.isConnected)
                yield break;

            if (IsServerAlive)
            {
                yield return new WaitForSeconds(30);
                IsServerAlive = false;
            }
            else
            {
                yield return new WaitForSeconds(35);
                if (IsServerAlive) yield break;
                if (Client.Instance.isConnected) yield break;
                
                if (!ModSceneManager.IsInMenu())
                    SceneManager.LoadScene(SceneNames.MainMenu);
                
                Client.Instance.Disconnect();
            }
        }
        public void SpawnPlayer(Player player)
        {
            if (_playerPrefab != null)
            {
                if (!PlayersGameObjects.ContainsKey(player.id))
                {
                    var playerObject = Object.Instantiate(_playerPrefab, player.position.toVector3(),player.rotation.toQuaternion());
                    playerObject.transform.name = player.username;
                    PlayersGameObjects[player.id] = playerObject;
                    Plugin.log.LogInfo($"{player.username} is ingame as an object...");
                }
            }
            else
            {
                PlayerPrefabSetup();
                SpawnPlayer(player);
            }
        }


        private static void PlayerPrefabSetup()
        {
                var player = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                player.AddComponent<ModCharacterController>();
                player.AddComponent<Animator>();
                
                player.transform.localScale = new Vector3(1, 2, 1);
                player.transform.position = new Vector3(0, 0, 0);
                player.transform.rotation = new Quaternion(0, 0, 0, 0);
                    
                _playerPrefab = player; 
                    
                Object.DontDestroyOnLoad(_playerPrefab);
        }
    }
}