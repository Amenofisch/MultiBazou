using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MultiBazou.ClientSide;
using MultiBazou.ClientSide.Data;
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
            if(Server.clients.ContainsKey(toClient))
                Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendUDPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            if(Server.clients.ContainsKey(toClient))
                Server.clients[toClient].udp.SendData(packet);
        }

        private static void SendTcpDataToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.clients.Values)
            {
                client.tcp.SendData(packet);
            }
        }

        private static void SendTcpDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();

            foreach (var client in Server.clients.Values.Where(client => client.id != exceptClient))
            {
                client.tcp.SendData(packet);
            }
        }

        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.clients.Values)
            {
                client.udp.SendData(packet);
            }
        }

        private static void SendUDPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            foreach (var client in Server.clients.Values.Where(client => client.id != exceptClient))
            {
                client.udp.SendData(packet);
            }
        }
        #endregion
        
        #region Lobby
        
            public static void Welcome(int toClient=-1, string msg="welcome to the server!")
            {
                using (var packet = new Packet((int)PacketTypes.welcome))
                {
                    packet.Write(msg);
                    packet.Write(toClient);

                    SendTcpData(toClient, packet);
                }
            }

            public static void DisconnectClient(int id, string msg)
            {
                using (var packet = new Packet((int)PacketTypes.disconnect))
                {
                    packet.Write(msg);
                    packet.Write(id);

                    SendTcpDataToAll(packet);
                }
            }
            
            public static void SendReadyState(int fromClient, bool ready, int id)
            {
                using (var packet = new Packet((int)PacketTypes.readyState))
                {
                    packet.Write(ready);
                    packet.Write(id);

                    SendTcpDataToAll(fromClient, packet);
                }
            }
            
            public static void SendPlayerInfo(Player info) 
            {
                using (Packet packet = new Packet((int)PacketTypes.playerInfo))
                {
                    packet.Write(info);

                    SendTcpDataToAll(packet);
                }
            }

            public static void SendPlayersInfo(Dictionary<int, Player> info) 
            {
                using (var packet = new Packet((int)PacketTypes.playersInfo))
                {
                    packet.Write(info);

                    SendTcpDataToAll(packet);
                }
            }

            public static void StartGame(ModGameSaveData gameSaveData)
            {
                using (var packet = new Packet((int)PacketTypes.startGame))
                {
                    if(gameSaveData != null)
                        packet.Write(gameSaveData);
                    SendTcpDataToAll(Client.Instance.Id, packet);
                }
                
                // TODO: use a coroutine for this
                DelaySpawnPlayer();
            }

            private static void SpawnPlayer(int id,Player player) 
            {
                using (var packet = new Packet((int)PacketTypes.spawnPlayer))
                {
                    packet.Write(player);
                    packet.Write(id);

                    SendTcpDataToAll(packet);
                }
            }

            private static IEnumerator DelaySpawnPlayer()
            {
                Plugin.log.LogInfo("Waiting for Scene Change");
                while (SceneManager.GetActiveScene().name != SceneNames.Master)
                    yield return new WaitForEndOfFrame();
                
                yield return new WaitForSeconds(1.5f);
                Plugin.log.LogInfo("Sending Spawn Player Packets");
                
                foreach (var client in Server.clients.Values)
                {
                    if (ServerData.Players.TryGetValue(client.id, out var player))
                    { 
                        SpawnPlayer(client.id, player);
                    }
                }
            }
            
            public static void KeepAlive(int fromclient)
            {
                using (var packet = new Packet((int)PacketTypes.keepAlive))
                {
                    SendTcpData(fromclient, packet);
                }
            }
        #endregion

        #region PlayerData
        
            public static void SendInitialPosition(int fromClient, Vector3Serializable position)
            {
                using (var packet = new Packet((int)PacketTypes.playerInitialPos))
                {
                    packet.Write(fromClient);
                    packet.Write(position);
                    
                    SendUDPDataToAll(fromClient, packet);
                }
            }
        
            public static void SendPosition(int fromClient, Vector3Serializable position)
            {
                using (var packet = new Packet((int)PacketTypes.playerPosition))
                {
                    packet.Write(fromClient);
                    packet.Write(position);
                    
                    SendUDPDataToAll(fromClient, packet);
                }
            }
                
            public static void SendRotation(int fromClient, QuaternionSerializable rotation)
            {
                using (var packet = new Packet((int)PacketTypes.playerRotation))
                {
                    packet.Write(fromClient);
                    packet.Write(rotation);

                    SendUDPDataToAll(fromClient, packet);
                }
            }
                
            public static void SendPlayerSceneChange(int fromClient, GameScene scene)
            {
                using (var packet = new Packet((int)PacketTypes.playerSceneChange))
                {
                    packet.Write(fromClient);
                    packet.Write(scene);

                    SendTcpDataToAll(fromClient, packet);
                }
            }
        #endregion

        public static void SendKeepAliveConfirmation(int fromClient)
        {
            using (var packet = new Packet((int)PacketTypes.keepAliveConfirmed))
            {
                SendTcpData(fromClient, packet);
            }
        }
    }
}