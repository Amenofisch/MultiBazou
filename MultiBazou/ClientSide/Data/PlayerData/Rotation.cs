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
            if (player.GameObject != null)
            {
                player.GameObject.GetComponent<ModCharacterController>().RotateToRotation(rotation.ToQuaternion());
            }
        }
        
        public static void SendRotation()
        {
            if (GameData.Instance.LocalPlayer != null)
            {
                var rotation = GameData.Instance.LocalPlayerCamera.transform.rotation;
                if (!(Quaternion.Angle(rotation, _lastRotation) > UpdateRate)) return;
                _lastRotation = rotation;
                ClientSend.SendRotation(new QuaternionSerializable(rotation));
            }
        }
    }
}