using MultiBazou.ClientSide.Data;
using MultiBazou.ServerSide.Data;
using MultiBazou.Shared;
using MultiBazou.Shared.Data;

namespace MultiBazou.ServerSide.Handle
{
    public class ServerHandle
    {
        #region Lobby and Connection
        
            public static void Empty(int fromClient, Packet packet)
            {
                Plugin.log.LogDebug($"[ServerSide/Handle/ServerHandle/Empty]: Received an empty packet from {fromClient}, something is wrong...");
            }
            
            public static void WelcomeReceived(int fromClient, Packet packet)
            {
                var clientId = packet.ReadInt();
                var username = packet.ReadString();
                var modVersion = packet.ReadString();
                var gameVersion = packet.ReadFloat();

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (gameVersion != ContentManager.instance.GameVersion)
                {
                    Plugin.log.LogInfo($"[ServerSide/Handle/ServerHandle/WelcomeReceived]: Player {clientId}/{username} tried to connect with incorrect game version.");
                    ServerSend.DisconnectClient(fromClient, $"Game is not on same version as Server ! ({ContentManager.instance.GameVersion})");
                    return;
                }

                if (modVersion != PluginInfo.Version)
                {
                    Plugin.log.LogInfo($"[ServerSide/Handle/ServerHandle/WelcomeReceived]: Player {clientId}/{username} tried to connect with incorrect mod version.");
                    ServerSend.DisconnectClient(fromClient, $"Mod is not on same version as Server ! ({PluginInfo.Version}))");
                    return;
                }
                
                if (!ClientData.instance.GameReady)
                {
                    Plugin.log.LogInfo($"[ServerSide/Handle/ServerHandle/WelcomeReceived]: {Server.Clients[fromClient].ServerTcp.Socket.Client.RemoteEndPoint} connected succesfully and is now {username}.");

                    if (fromClient != clientId)
                    {
                        Plugin.log.LogInfo($"[ServerSide/Handle/ServerHandle/WelcomeReceived]: Player \"{username}\" (ID:{fromClient}) has assumed the wrong client ID ({clientId})!");
                    }
                    
                    Server.Clients[fromClient].UpdatePlayerDictionary(username);
                }
                else
                {
                    ServerSend.DisconnectClient(fromClient, "You can't connect to an unready game!");
                }
            }
            
            public static void Disconnect(int fromClient, Packet packet)
            {
                var id = packet.ReadInt();
                
                Server.Clients[fromClient].Disconnect(id);
                Plugin.log.LogInfo($"[ServerSide/Handle/ServerHandle/Disconnect]: {Server.Clients[fromClient].ServerTcp.Socket.Client.RemoteEndPoint}/{id} has disconnected.");
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

                ServerData.Players[fromClient].scene = scene;
                ServerSend.SendPlayerSceneChange(fromClient, scene);
            }

        #endregion
    }
}