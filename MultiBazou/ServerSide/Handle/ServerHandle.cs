using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MultiBazou.ClientSide.Data;
using MultiBazou.ServerSide.Data;
using MultiBazou.Shared;
using MultiBazou.Shared.Data;
using UnityEngine;

namespace MultiBazou.ServerSide.Handle
{
    public class ServerHandle
    {
        #region Lobby and Connection
        
            public static void Empty(int fromClient, Packet packet)
            {
            }
            
            public static void WelcomeReceived(int fromClient, Packet packet)
            {
                int clientId = packet.ReadInt();
                string username = packet.ReadString();
                string modVersion = packet.ReadString();
                float gameVersion = packet.ReadFloat();

                if (gameVersion != ContentManager.instance.GameVersion)
                {
                    ServerSend.DisconnectClient(fromClient, $"Game is not on same version as Server ! ({ContentManager.instance.GameVersion})");
                    return;
                }

                if (modVersion != PluginInfo.Version)
                {
                    ServerSend.DisconnectClient(fromClient, $"Mod is not on same version as Server ! ({PluginInfo.Version}))");
                    return;
                }
                
                if (!ClientData.instance.GameReady)
                {
                    Plugin.log.LogInfo($" SV: {Server.clients[fromClient].tcp.Socket.Client.RemoteEndPoint} connected succesfully and is now {username}.");

                    if (fromClient != clientId)
                    {
                        Plugin.log.LogInfo($" SV: Player \"{username}\" (ID:{fromClient}) has assumed the wrong client ID ({clientId})!");
                    }
                    Server.clients[fromClient].SendToLobby(username);
                }
                else
                {
                    ServerSend.DisconnectClient(fromClient, "Player couldn't connect an unready game!");
                }

            }
            
            public static void KeepAlive(int fromclient, Packet packet)
            {
                ServerSend.KeepAlive(fromclient);
                Server.clients[fromclient].Alive = true;
                ServerSend.SendKeepAliveConfirmation(fromclient);
                
                Server.lastClientActivity[fromclient] = DateTime.Now;
            }
            
            public static void Disconnect(int fromClient, Packet packet)
            {
                int id = packet.ReadInt();
                
                Server.clients[fromClient].Disconnect(id);
                Plugin.log.LogInfo($" SV: {Server.clients[fromClient].tcp.Socket.Client.RemoteEndPoint} " + $"has disconnected.");
            }
            
            public static void ReadyState(int fromClient, Packet packet)
            {
                var ready = packet.ReadBool();
                var id = packet.ReadInt();

                ServerData.Players[id].isReady = ready;

                ServerSend.SendReadyState(fromClient,ready, id);
            }
            
        #endregion

        #region PlayerData
        
            public static void PlayerInitialPosition(int fromClient, Packet packet)
            {
                var position = packet.Read<Vector3Serializable>();
                ServerData.Players[fromClient].position = position;
                    
                ServerSend.SendInitialPosition(fromClient, position);
            }

            public static void PlayerPosition(int fromClient, Packet packet)
            {
                var position = packet.Read<Vector3Serializable>();
                ServerData.Players[fromClient].position = position;
                
                ServerSend.SendPosition(fromClient, position);
            }
                
            public static void PlayerRotation(int fromClient, Packet packet)
            {
                var rotation = packet.Read<QuaternionSerializable>();
                ServerData.Players[fromClient].rotation = rotation;
                    
                ServerSend.SendRotation(fromClient, rotation);
            }
                
            public static void PlayerSceneChange(int fromClient, Packet packet)
            {
                var scene = packet.Read<GameScene>();
                Plugin.log.LogInfo($"SV:  { ServerData.Players[fromClient].username} changed scene! : " + scene.ToString());

                ServerData.Players[fromClient].scene = scene;
                ServerSend.SendPlayerSceneChange(fromClient, scene);
            }

        #endregion
    }
}