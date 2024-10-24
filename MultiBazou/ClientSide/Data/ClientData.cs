using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MultiBazou.ClientSide.Data.PlayerData;
using MultiBazou.Shared;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiBazou.ClientSide.Data
{
    public class ClientData
    {
        public static ClientData instance;

        private static GameObject _playerPrefab;
        public Dictionary<int, Player> Players = new();

        public bool GameReady;

        public IEnumerator Initialize()
        {
            instance = this;

            foreach (var player in Players)
            {
                player.Value.GameObject = null;
            }

            GameData.Instance.Initialize();

            yield return new WaitForSeconds(2);
            yield return new WaitForEndOfFrame();

            GameReady = true;
        }

        public void UpdateClient()
        {
            Movement.SendPosition();
            Rotation.SendRotation();

            foreach (var player in Players.Where(player =>
                         player.Value.GameObject != null && player.Key != Client.instance.Id))
            {
                player.Value.GameObject.GetComponent<ModCharacterController>().UpdatePlayer();
            }
        }

        public void SetupPlayerGameObject(Player player)
        {
            if (_playerPrefab != null)
            {
                var playerObject = Object.Instantiate(_playerPrefab, player.position.ToVector3(), player.rotation.ToQuaternion());
                playerObject.transform.name = player.username;

                player.GameObject = playerObject;
            }
            else
            {
                PlayerPrefabSetup();
                SetupPlayerGameObject(player);
            }
        }


        private static void PlayerPrefabSetup()
        {
            Plugin.log.LogDebug("[ClientSide/Data/ClientData/PlayerPrefabSetup]: Setting up Player Prefab...");
            var player = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            player.AddComponent<Animator>();
            player.AddComponent<ModCharacterController>();

            player.transform.localScale = new Vector3(1, 1, 1);
            player.transform.position = new Vector3(0, 0, 0);
            player.transform.rotation = Quaternion.identity;

            var goggles = GameObject.CreatePrimitive(PrimitiveType.Cube);

            goggles.transform.localScale = new Vector3(0.4f, 0.1f, 0.1f); // Horizontal and narrow
            goggles.transform.position =
                player.transform.position + new Vector3(0, 0.9f, 0.5f); // Slightly above the center of the cylinder
            goggles.transform.rotation = Quaternion.identity;

            goggles.GetComponent<Renderer>().material.color = Color.black;

            goggles.transform.SetParent(player.transform);

            _playerPrefab = player;
            Object.DontDestroyOnLoad(_playerPrefab);

            Plugin.log.LogDebug("[ClientSide/Data/ClientData/PlayerPrefabSetup]: Done setting up Player Prefab.");
        }
    }
}