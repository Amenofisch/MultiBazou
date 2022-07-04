using Riptide;
using UnityEngine;
using Riptide.Utils;

namespace MultiBazou
{
    public class ServerPlayer
    {
        public Vector3 position;
        public Quaternion rotation;

        public ushort id { get; set; }
        public string username { get; set; }
        
        public void SendSpawn(ServerClientConnectedEventArgs toClient)
        {
            ServerNetworkManager.Singleton.Server.Send(
                GetSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.spawnPlayer)), toClient.Client.Id);
        }

        public void SendSpawn()
        {
            ServerNetworkManager.Singleton.Server.SendToAll(
                GetSpawnData(Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.spawnPlayer)));
        }

        public void SendPosRot(Vector3 position, Quaternion rotation, ushort id)
        {
            Message message = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClientId.playerPosRot);
            message.Add(id);
            message.Add(position);
            message.Add(rotation);
            ServerNetworkManager.Singleton.Server.SendToAll(message);
        }

        public void SetPosRot(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
            SendPosRot(position, rotation, id);
        }

        public Message GetSpawnData(Message message)
        {
            message.Add(id);
            message.Add(username);
            message.Add(position);
            return message;
        }
    }
}