using Riptide;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiBazou
{
    public class ServerHandlers
    {
        #region ServerMessages

        [MessageHandler((ushort)ClientToServerId.playerName)]
        public static void PlayerName(Riptide.ServerClientConnectedEventArgs fromClient, Message message)
        {
            ServerPlayerManager.Spawn(fromClient.Client.Id, message.GetString(), fromClient);
        }

        [MessageHandler((ushort)ClientToServerId.playerPosRot)]
        public static void PlayerPosRot(ServerClientConnectedEventArgs fromClient, Message message)
        {
            ServerPlayer player = ServerPlayerManager.List[fromClient.Client.Id];
            Vector3 position = message.GetVector3();
            Quaternion rotation = message.GetQuaternion();
            player.SetPosRot(position, rotation);
        }
        #endregion
    }

    public class ClientHandlers
    {
        [MessageHandler((ushort)ServerToClientId.spawnPlayer)]
        public static void SpawnPlayer(Message message)
        {
            ClientPlayerManager.Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
        }

        [MessageHandler((ushort)ServerToClientId.playerPosRot)]
        public static void PlayerPosRot(Message message)
        {
            var playerId = message.GetUShort();
            if (ClientPlayerManager.List.TryGetValue(playerId, out var player))
                player.Move(message.GetVector3(), message.GetQuaternion());
        }
    }
}