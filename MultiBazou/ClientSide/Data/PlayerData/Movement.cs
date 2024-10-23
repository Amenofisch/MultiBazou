using MultiBazou.ClientSide.Handle;
using MultiBazou.Shared;
using UnityEngine;

namespace MultiBazou.ClientSide.Data.PlayerData
{
    public static class Movement
    {
        private const float UpdateRate = 0.02f;
        private static Vector3 _lastPosition;

        public static void SetInitialPosition(int id, Vector3Serializable position)
        {
            if (!ClientData.instance.Players.TryGetValue(id, out var player)) return;
            if (player.scene != ModSceneManager.GetCurrentScene()) return;
            if (!ClientData.instance.PlayersGameObjects.TryGetValue(id, out var gameObject)) return;
            
            if (gameObject != null)
            {
                gameObject.transform.position = position.toVector3();
            }
        }

        public static void UpdatePlayerPosition(int id, Vector3Serializable position)
        {
            if (!ClientData.instance.Players.TryGetValue(id, out var player)) return;
            if (player.scene != ModSceneManager.GetCurrentScene()) return;
            if (ClientData.instance.PlayersGameObjects.TryGetValue(id, out var gameObject))
            {
                if (gameObject == null) return;
                player.desiredPosition = position;
                            
                gameObject.GetComponent<ModCharacterController>().MoveToPosition(position.toVector3());
            }
            else
            {
                ClientData.instance.SpawnPlayer(player);
            }
        }

        public static void SendPosition()
        {
            if (GameData.instance.LocalPlayer != null)
            {
                var position = GameData.instance.LocalPlayer.transform.position;
                position.y -= 0.72f;
                if (!(Vector3.Distance(position, _lastPosition) > UpdateRate)) return;
                
                _lastPosition = position;
                ClientSend.SendPosition(new Vector3Serializable(position));
            }
            else
            {
                GameData.instance.LocalPlayer = Object.FindObjectOfType<Gameplay>().PlayerWalking.gameObject;
            }
        }
    }
}