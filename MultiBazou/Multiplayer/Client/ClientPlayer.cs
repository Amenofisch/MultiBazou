using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiBazou
{
    public class ClientPlayer
    {

        public ushort id;
        public string username;

        public GameObject playerObject;

        public Vector3 wantedPosition = Vector3.zero;
        public float timeSyncing;

        public void Move(Vector3 newPos, Quaternion newRot)
        {
            playerObject.transform.position = wantedPosition;

            wantedPosition = newPos;
            playerObject.transform.rotation = newRot;
            timeSyncing = 0;
        }

        public void LerpPosition()
        {
            timeSyncing += Time.deltaTime / Time.fixedDeltaTime;
            timeSyncing = Mathf.Clamp(timeSyncing, 0, 1);

            Vector3 newPos = Vector3.Lerp(playerObject.transform.position, wantedPosition, timeSyncing);

            playerObject.transform.position = newPos;
        }

        public void RemovePlayer()
        {
            UnityEngine.Debug.Log("Player removed");

            Main.instance.DestroyObject(playerObject);
            ClientPlayerManager.List.Remove(id);
        }
    }
}