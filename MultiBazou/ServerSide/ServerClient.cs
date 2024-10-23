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
        public int id;
        public bool isHosting = false;
        public bool Alive = true;
        
        public ServerTcp tcp;
        public ServerUDP udp;

        public ServerClient(int _clientId)
        {
            id = _clientId;
            tcp = new ServerTcp(id);
            udp = new ServerUDP(id);
        }


        public void SendToLobby(string _playerName)
        {
            ServerData.Players[id] = new Player(id, _playerName, new Vector3(0, 0, 0));
            Plugin.log.LogInfo($"SV: New player ! {_playerName}, ID:{id}");
            
            ServerSend.SendPlayersInfo(ServerData.Players);
        }

        public void Disconnect(int _id)
        {
            Plugin.log.LogInfo($"{tcp.Socket.Client.RemoteEndPoint} has disconnected.");
            if (ServerData.Players.TryGetValue(_id, out var player))
                ServerSend.DisconnectClient(_id, $"{ player.username} as disconnected!");
            if (Server.clients.ContainsKey(_id))
                Server.clients.Remove(_id);
            
            if(ServerData.Players.ContainsKey(_id))
                ServerData.Players.Remove(_id);
            
            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}