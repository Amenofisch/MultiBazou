using MultiBazou.ClientSide.Handle;
using MultiBazou.Shared;
using UnityEngine;

namespace MultiBazou.ClientSide.Data.PlayerData
{
    public static class Rotation
    {
        private const float UpdateRate = 0.02f;
        private static Quaternion _lastRotation;

        public static void UpdatePlayerRotation(int id, QuaternionSerializable rotation)
        {
            if (!ClientData.instance.Players.TryGetValue(id, out var player)) return;
            if (player.scene != ModSceneManager.GetCurrentScene()) return;
            if (ClientData.instance.PlayersGameObjects.TryGetValue(id, out var gameObject))
            {
                if (gameObject != null)
                {
                            
                    gameObject.GetComponent<ModCharacterController>().RotateToRotation(rotation.toQuaternion());
                    /*_gameObject.transform.rotation = 
                                Quaternion.Lerp(_gameObject.transform.rotation,_rotation.toQuaternion(), 10f * Time.deltaTime);*/
                }
            }
            else
            {
                ClientData.instance.SpawnPlayer(player);
            }
        }
        
        public static void SendRotation()
        {
            if (GameData.instance.LocalPlayer != null)
            {
                Quaternion rotation = GameData.instance.LocalPlayer.transform.rotation;
                if (!(Quaternion.Angle(rotation, _lastRotation) > UpdateRate)) return;
                _lastRotation = rotation;
                ClientSend.SendRotation(new QuaternionSerializable(rotation));
            }
            else
            {
                GameData.instance.LocalPlayer = Object.FindObjectOfType<Gameplay>().PlayerWalking.gameObject;
            }
        }
    }
}