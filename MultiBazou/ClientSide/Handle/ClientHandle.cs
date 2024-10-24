using System.Collections;
using System.Collections.Generic;
using MultiBazou.ClientSide.Data;
using MultiBazou.ClientSide.Data.PlayerData;
using MultiBazou.Shared;
using MultiBazou.Shared.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiBazou.ClientSide.Handle
{
    public class ClientHandle : MonoBehaviour
    {
        #region Lobby and connection

        public static void Welcome(Packet packet)
        {
            var msg = packet.ReadString();
            var myId = packet.ReadInt();
            
            Client.instance.Id = myId;

            ClientSend.WelcomeReceived();
            packet.Dispose();
        }

        public static void Disconnect(Packet packet)
        {
            var msg = packet.ReadString();
            var id = packet.ReadInt();

            if (!Client.instance.isConnected) return;

            if (id == Client.instance.Id)
                Plugin.log.LogInfo($"You were disconnected from the server :{msg}");
            else
                Plugin.log.LogInfo($"Message from server : {msg}");

            if (id != Client.instance.Id && id != 1)
            {
                if (ClientData.instance.Players.ContainsKey(id))
                    ClientData.instance.Players[id].Disconnect();
            }
            else
            {
                if (!ModSceneManager.IsInMenu())
                    SceneManager.LoadScene("MainMenu");
                else
                {
                    Client.instance.Disconnect();
                }

                Application.runInBackground = false;
            }

            packet.Dispose();
        }

        public static void ReadyState(Packet packet)
        {
            var ready = packet.ReadBool();
            var id = packet.ReadInt();

            ClientData.instance.Players[id].isReady = ready;
            packet.Dispose();
        }

        public static void UpdatePlayerInDictionary(Packet packet)
        {
            var info = packet.Read<Player>();
            ClientData.instance.Players[info.id] = info;
            
            packet.Dispose();
        }

        public static void UpdatePlayersInDictionary(Packet packet)
        {
            var list = packet.Read<Dictionary<int, Player>>();
            if (list != null)
                ClientData.instance.Players = list;
            packet.Dispose();
        }

        public static void StartGame(Packet packet)
        {
            SavesManager.StartGame();
            ModUI.Instance.showModUI = false;

            packet.Dispose();
        }

        public static void SpawnPlayer(Packet packet)
        {
            var player = packet.Read<Player>();
            var id = packet.ReadInt();

            CoroutineHelper.Instance.StartCoroutine(DelaySpawnPlayer(id, player));

            packet.Dispose();
        }

        private static IEnumerator DelaySpawnPlayer(int id, Player _player)
        {
            while (ModSceneManager.GetCurrentScene() == GameScene.Menu)
            {
                yield return new WaitForEndOfFrame();
            }

            if (_player.GameObject == null)
            {
                ClientData.instance.SetupPlayerGameObject(_player);
            }

            while (!GameData.dataInitialized)
            {
                yield return new WaitForEndOfFrame();
            }

            if (GameData.dataInitialized)
            {
                ClientSend.SendInitialPosition(new Vector3Serializable(GameData.Instance.LocalPlayer.transform.position));
            }
        }

        #endregion

        #region PlayerData

        public static void PlayerInitialPos(Packet packet)
        {
            var id = packet.ReadInt();
            Vector3Serializable position = packet.Read<Vector3Serializable>();
            Movement.SetInitialPosition(id, position);
            packet.Dispose();
        }

        public static void PlayerPosition(Packet packet)
        {
            var id = packet.ReadInt();
            var position = packet.Read<Vector3Serializable>();
            Movement.UpdatePlayerPosition(id, position);
            packet.Dispose();
        }

        public static void PlayerRotation(Packet packet)
        {
            var id = packet.ReadInt();
            var rotation = packet.Read<QuaternionSerializable>();
            Rotation.UpdatePlayerRotation(id, rotation);
            packet.Dispose();
        }

        public static void PlayerSceneChange(Packet packet)
        {
            var id = packet.ReadInt();
            var scene = packet.Read<GameScene>();

            if (!ClientData.instance.Players.TryGetValue(id, out var player))
            {
                packet.Dispose();
                return;
            }

            player.scene = scene;

            // Check if player's scene matches the current scene
            if (player.scene == ModSceneManager.GetCurrentScene())
            {
                // If the player object doesn't exist, spawn the player
                if (player.GameObject == null)
                {
                    ClientData.instance.SetupPlayerGameObject(player);
                }
            }

            // Check if player's scene differs from the current scene
            if (player.scene != ModSceneManager.GetCurrentScene())
            {
                // If the player object exists, destroy it
                if (player.GameObject != null)
                {
                    Destroy(player.GameObject);
                    player.GameObject = null;
                }
            }

            packet.Dispose();
        }

        #endregion
    }
}