using System.Runtime.CompilerServices;
using MultiBazou.ClientSide.Handle;
using MultiBazou.Shared;
using UnityEngine;

namespace MultiBazou.ClientSide.Data.PlayerData
{
    public static class Movement
    {
        private const float UpdateRate = 0.05f;
        private static Vector3 _lastPosition;

        public static void SetInitialPosition(int id, Vector3Serializable position)
        {
            if (!ClientData.instance.Players.TryGetValue(id, out var player))
            {
                Plugin.log.LogDebug($"[ClientSide/Data/PlayerData/Movement/SetInitialPosition]: Couldn't find player with id: {id}");
                return;
            }
            
            if (player.scene != ModSceneManager.GetCurrentScene()) return;

            if (player.GameObject != null)
            {
                player.GameObject.GetComponent<ModCharacterController>().SetPosition(position.ToVector3());
                Plugin.log.LogDebug($"[ClientSide/Data/PlayerData/Movement/SetInitialPosition]: Init position for {player.id}/{player.username} at {position.ToVector3().ToString()}");
            }
        } 

        public static void UpdatePlayerPosition(int id, Vector3Serializable position)
        {
            if (!ClientData.instance.Players.TryGetValue(id, out var player))
            {
                Plugin.log.LogDebug($"[ClientSide/Data/PlayerData/Movement/UpdatePlayerPosition]: Can't find {id} in players list with position: {position.ToVector3().ToString()}");
                return;
            }
            if (player.scene != ModSceneManager.GetCurrentScene()) return;
            
            if (player.GameObject != null)
            {
                player.GameObject.GetComponent<ModCharacterController>().MoveToPosition(position.ToVector3());
                //Plugin.log.LogDebug($"[ClientSide/Data/PlayerData/Movement/UpdatePlayerPosition]: {player.id}/{player.username} now at {position.ToVector3().ToString()}");
            }
        }

        public static void SendPosition()
        {
            if (GameData.Instance.LocalPlayer != null)
            {
                var position = GameData.Instance.LocalPlayer.transform.position;
                var isDistanceHighEnoughToUpdate = Vector3.Distance(position, _lastPosition) > UpdateRate;
                if (!isDistanceHighEnoughToUpdate) return;

                _lastPosition = position;
                ClientSend.SendPosition(new Vector3Serializable(position));
            }
        }
    }
}