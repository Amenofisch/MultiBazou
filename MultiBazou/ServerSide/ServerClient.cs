using System.Collections;
using MultiBazou.ServerSide.Data;
using MultiBazou.ServerSide.Handle;
using MultiBazou.ServerSide.Transport;
using MultiBazou.Shared;
using UnityEngine;

namespace MultiBazou.ServerSide
{
    public class ServerClient
    {
        public int ID;
        
        public ServerTcp ServerTcp;
        public ServerUDP ServerUdp;

        public ServerClient(int clientId)
        {
            ID = clientId;
            ServerTcp = new ServerTcp(ID);
            ServerUdp = new ServerUDP(ID);
        }
        
        public void UpdatePlayerDictionary(string playerName)
        {
            ServerData.Players[ID] = new Player(ID, playerName, new Vector3(0, 0, 0));
            ServerSend.SendPlayersUpdateInDictionary(ServerData.Players);
        }

        public void Disconnect(int id)
        {
            Plugin.log.LogInfo($"{ServerTcp.Socket.Client.RemoteEndPoint} has disconnected.");
            if (ServerData.Players.TryGetValue(id, out var player))
                ServerSend.DisconnectClient(id, $"{player.username} has disconnected!");
            if (Server.Clients.ContainsKey(id))
                Server.Clients.Remove(id);
            
            if(ServerData.Players.ContainsKey(id))
                ServerData.Players.Remove(id);
            
            ServerTcp.Disconnect();
            ServerUdp.Disconnect();
        }
    }
}