using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MultiBazou.ClientSide;
using MultiBazou.ServerSide.Data;
using MultiBazou.Shared;
using MultiBazou.Shared.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiBazou.ServerSide.Handle
{
    public class ServerSend
    {
        #region Functions
        private static void SendTcpData(int toClient, Packet packet)
        {
            packet.WriteLength();
            if(Server.Clients.TryGetValue(toClient, out var client))
                client.ServerTcp.SendData(packet);
        }

        private static void SendUDPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            if(Server.Clients.TryGetValue(toClient, out var client))
                client.ServerUdp.SendData(packet);
        }

        private static void SendTcpDataToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.Clients.Values)
            {
                client.ServerTcp.SendData(packet);
            }
        }

        private static void SendTcpDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();

            foreach (var client in Server.Clients.Values.Where(client => client.ID != exceptClient))
            {
                client.ServerTcp.SendData(packet);
            }
        }

        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.Clients.Values)
            {
                client.ServerUdp.SendData(packet);
            }
        }

        private static void SendUDPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.Clients.Values.Where(client => client.ID != exceptClient))
            {
                client.ServerUdp.SendData(packet);
            }
        }
        #endregion
        
        #region Lobby
        
            public static void Welcome(int toClient=-1, string msg="welcome to the server!")
            {
                using var packet = new Packet((int)PacketTypes.Welcome);
                packet.Write(msg);
                packet.Write(toClient);

                SendTcpData(toClient, packet);
            }

            public static void DisconnectClient(int id, string msg)
            {
                using var packet = new Packet((int)PacketTypes.Disconnect);
                packet.Write(msg);
                packet.Write(id);

                SendTcpDataToAll(packet);
            }
            
            public static void SendReadyState(int fromClient, bool ready, int id)
            {
                using var packet = new Packet((int)PacketTypes.ReadyState);
                packet.Write(ready);
                packet.Write(id);

                SendTcpDataToAll(fromClient, packet);
            }
            
            public static void SendPlayerUpdateInDictionary(Player info)
            {
                using var packet = new Packet((int)PacketTypes.UpdatePlayerInDictionary);
                packet.Write(info);

                SendTcpDataToAll(packet);
            }

            public static void SendPlayersUpdateInDictionary(Dictionary<int, Player> info)
            {
                using var packet = new Packet((int)PacketTypes.UpdatePlayersInDictionary);
                packet.Write(info);

                SendTcpDataToAll(packet);
            }

            public static void StartGame(ModGameSaveData gameSaveData)
            {
                using (var packet = new Packet((int)PacketTypes.StartGame))
                {
                    if(gameSaveData != null)
                        packet.Write(gameSaveData);
                    SendTcpDataToAll(Client.instance.Id, packet);
                }

                CoroutineHelper.Instance.StartCoroutine(DelaySpawnPlayer());
            }

            private static void SpawnPlayer(int id,Player player) 
            {
                using (var packet = new Packet((int)PacketTypes.SpawnPlayer))
                {
                    packet.Write(player);
                    packet.Write(id);

                    SendTcpDataToAll(packet);
                }
            }

            private static IEnumerator DelaySpawnPlayer()
            {
                while (SceneManager.GetActiveScene().name != SceneNames.Master)
                    yield return new WaitForEndOfFrame();
                
                yield return new WaitForSeconds(1.5f);
                
                foreach (var client in Server.Clients.Values)
                {
                    if (ServerData.Players.TryGetValue(client.ID, out var player))
                    { 
                        SpawnPlayer(client.ID, player);
                    }
                }
            }
        #endregion

        #region PlayerData
        
            public static void SendInitialPosition(int fromClient, Vector3Serializable position)
            {
                using (var packet = new Packet((int)PacketTypes.PlayerInitialPos))
                {
                    packet.Write(fromClient);
                    packet.Write(position);
                    
                    SendUDPDataToAll(fromClient, packet);
                }
            }
        
            public static void SendPosition(int fromClient, Vector3Serializable position)
            {
                using (var packet = new Packet((int)PacketTypes.PlayerPosition))
                {
                    packet.Write(fromClient);
                    packet.Write(position);
                    
                    SendUDPDataToAll(fromClient, packet);
                }
            }
                
            public static void SendRotation(int fromClient, QuaternionSerializable rotation)
            {
                using (var packet = new Packet((int)PacketTypes.PlayerRotation))
                {
                    packet.Write(fromClient);
                    packet.Write(rotation);

                    SendUDPDataToAll(fromClient, packet);
                }
            }
                
            public static void SendPlayerSceneChange(int fromClient, GameScene scene)
            {
                using (var packet = new Packet((int)PacketTypes.PlayerSceneChange))
                {
                    packet.Write(fromClient);
                    packet.Write(scene);

                    SendTcpDataToAll(fromClient, packet);
                }
            }
        #endregion
    }
}