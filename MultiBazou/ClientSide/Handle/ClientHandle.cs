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
                string msg = packet.ReadString();
                int myId = packet.ReadInt();

                Plugin.log.LogInfo($"Welcome Message from Server: {msg}");
                Client.Instance.Id = myId;

                ClientSend.WelcomeReceived();
                packet.Dispose();
            }
        
            public static void KeepAlive(Packet packet)
            {
                ClientData.instance.IsServerAlive = true;
                ClientData.instance.NeedToKeepAlive = true;
            }
            
            public static void KeepAliveConfirmation(Packet packet)
            {
                ClientData.instance.IsServerAlive = true;
            }
            

            public static void Disconnect(Packet packet)
            {
                string msg = packet.ReadString();
                int id = packet.ReadInt();
                
                if(!Client.Instance.isConnected) return;
                
                if(id == Client.Instance.Id)
                    Plugin.log.LogInfo($"You were disconnected from the server :{msg}");
                else
                    Plugin.log.LogInfo($"Message from server : {msg}");
                
                if (id != Client.Instance.Id && id != 1)
                {
                    if(ClientData.instance.Players.ContainsKey(id))
                        ClientData.instance.Players[id].Disconnect();
                }
                else
                {
                    if (!ModSceneManager.IsInMenu())
                        SceneManager.LoadScene("MainMenu");
                    else
                    {
                        Client.Instance.Disconnect();
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
            
            public static void PlayerInfo(Packet packet)
            {
                var info = packet.Read<Player>();
                ClientData.instance.Players[info.id] = info;
                
                Plugin.log.LogInfo($"Received {info.username}, {info.id} info from server.");
                packet.Dispose();
            }
            public static void PlayersInfo(Packet packet)
            {
                Dictionary<int, Player> info = packet.Read<Dictionary<int, Player>>();
                if(info != null)
                    ClientData.instance.Players = info;
                else
                    Plugin.log.LogInfo("Received player info is null!");
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
                ClientData.instance.Players[id] = player;

                // TODO: find a way to call this via a coroutine
                DelaySpawnPlayer(id, player);
                
                packet.Dispose();
            }

            private static IEnumerator DelaySpawnPlayer(int _id, Player _player)
            {
                while (ModSceneManager.GetCurrentScene() != GameScene.Menu)
                {
                    yield return new WaitForEndOfFrame();
                }
                
                if (ClientData.instance.Players.TryGetValue(_id, out var player))
                {
                    if(!ClientData.instance.PlayersGameObjects.ContainsKey(player.id))
                        ClientData.instance.SpawnPlayer(_player);
                }
                
                if(GameData.dataInitialized)
                    ClientSend.SendInitialPosition(new Vector3Serializable(GameData.instance.LocalPlayer.transform.position));
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
                int id = packet.ReadInt();
                Vector3Serializable position = packet.Read<Vector3Serializable>();
                Movement.UpdatePlayerPosition(id, position);
                packet.Dispose();
            }
                
            public static void PlayerRotation(Packet packet)
            {
                int id = packet.ReadInt();
                QuaternionSerializable rotation = packet.Read<QuaternionSerializable>();
                Rotation.UpdatePlayerRotation(id, rotation);
                packet.Dispose();
            }
                
            public static void PlayerSceneChange(Packet packet)
            {
                var id = packet.ReadInt();
                var scene = packet.Read<GameScene>();
                    
                Plugin.log.LogInfo("Received Scene Update :" + scene);
                if (ClientData.instance.Players.TryGetValue(id, out var player))
                {
                    player.scene = scene;
                    if (player.scene != ModSceneManager.GetCurrentScene())
                    {
                       if (ClientData.instance.PlayersGameObjects.TryGetValue(id, out var instance))
                       {
                           if (instance != null)
                           {
                               Destroy(instance);
                               ClientData.instance.PlayersGameObjects.Remove(id);
                           }
                       }
                    }
                    else
                    {
                        if (ClientData.instance.PlayersGameObjects.TryGetValue(id, out var instance))
                        {
                            if (instance == null)
                            {
                                ClientData.instance.SpawnPlayer(player);
                            }
                        }
                        else
                        {
                            ClientData.instance.SpawnPlayer(player);
                        }
                    }
                }
                packet.Dispose();
            }

        #endregion
    }
}