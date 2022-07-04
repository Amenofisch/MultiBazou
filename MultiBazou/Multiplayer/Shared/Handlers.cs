using Riptide;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiBazou
{
    public class ServerHandlers
    {
        #region ServerMessages

        [MessageHandler((ushort)ClientToServerId.playerName)]
        public static void PlayerName(ushort ClientId, Message message)
        {
            UnityEngine.Debug.Log("[SERVER] Received PlayerName with id: " + ClientId);
            ServerPlayerManager.Spawn(ClientId, message.GetString());
        }
        
        [MessageHandler((ushort)ClientToServerId.playerPosRot)]
        public static void PlayerPosRot(ushort ClientId, Message message)
        {
            ServerPlayer player = ServerPlayerManager.List[ClientId];
            Vector3 position = message.GetVector3();
            Quaternion rotation = message.GetQuaternion();
            player.SetPosRot(position, rotation);
            UnityEngine.Debug.Log("[SERVER] Received PlayerPosRot with id: " + ClientId.ToString() + " vector3: " + position.ToString() + " rotation: " + rotation.ToString());
        }
        #endregion
    }

    public class ClientHandlers
    {
        [MessageHandler((ushort)ServerToClientId.spawnPlayer)]
        public static void SpawnPlayer(Message message)
        {
            var id = message.GetUShort();
            Debug.Log("[CLIENT] Received SpawnPlayer with id: " + id.ToString());
            ClientPlayerManager.Spawn(id, message.GetString(), message.GetVector3());
        }

        [MessageHandler((ushort)ServerToClientId.playerPosRot)]
        public static void PlayerPosRot(Message message)
        {
            var playerId = message.GetUShort();
            Debug.Log("[CLIENT] Received PlayerPosRot from id: " + playerId.ToString());
            if (ClientPlayerManager.List.TryGetValue(playerId, out var player))
                player.Move(message.GetVector3(), message.GetQuaternion());
        }
    }
}