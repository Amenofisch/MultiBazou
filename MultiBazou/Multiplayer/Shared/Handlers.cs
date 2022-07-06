using Riptide;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiBazou
{
    public class Handlers
    {
        public static void LogMessage(string v)
        {
            UnityEngine.Debug.Log(v);
        }
    }

    public class ServerHandlers
    {
        #region ServerMessages

        [MessageHandler((ushort)ClientToServerId.playerName)]
        public static void PlayerName(ushort ClientId, Message message)
        {
            var username = message.GetString();
            Handlers.LogMessage("[ClientToServerId (SERVER)] Received Spawn request with id: " + ClientId.ToString() + " username: " + username);
            ServerPlayerManager.Spawn(ClientId, username);
        }

        [MessageHandler((ushort)ClientToServerId.playerPosRot)]
        public static void PlayerPosRot(ushort ClientId, Message message)
        {
            ServerPlayer player = ServerPlayerManager.List[ClientId];
            Vector3 position = message.GetVector3();
            Quaternion rotation = message.GetQuaternion();
            player.SetPosRot(position, rotation);
            Handlers.LogMessage("[ClientToServerId (SERVER)] Received PlayerPosRot with id: " + ClientId.ToString() + " position: " + position.ToString() + " rotation: " + rotation.ToString());
        }
        #endregion
    }

    public class ClientHandlers
    {
        [MessageHandler((ushort)ServerToClientId.spawnPlayer)]
        public static void SpawnPlayer(Message message)
        {
            var id = message.GetUShort();
            ClientPlayerManager.Spawn(id, message.GetString(), message.GetVector3());
            Handlers.LogMessage("[ServerToClientId (CLIENT)] Received SpawnPlayer with id: " + id.ToString());
        }

        [MessageHandler((ushort)ServerToClientId.playerPosRot)]
        public static void PlayerPosRot(Message message)
        {
            var playerId = message.GetUShort();
            Handlers.LogMessage("[ServerToClientId (CLIENT)] Received PlayerPosRot from id: " + playerId.ToString());
            if (ClientPlayerManager.List.TryGetValue(playerId, out var player))
                player.Move(message.GetVector3(), message.GetQuaternion());
        }
    }
}