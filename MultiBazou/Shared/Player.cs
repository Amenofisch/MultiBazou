using System;
using System.Collections.Generic;
using MultiBazou.ClientSide.Data;
using MultiBazou.Shared.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiBazou.Shared
{
    [Serializable]
    public class Player(int id, string username, Vector3 spawnPosition)
    {
        public int id = id;
        public string username = username;
        public bool isReady;

        public Vector3Serializable position = new(spawnPosition);
        public QuaternionSerializable rotation = new(Quaternion.identity);
        
        [NonSerialized]
        public GameObject GameObject;

        public GameScene scene = ModSceneManager.GetCurrentScene();

        public void Disconnect()
        {
            foreach (var player in ClientData.instance.Players)
            {
                Object.Destroy(player.Value.GameObject);
                player.Value.GameObject = null;
            }
            
            if (ClientData.instance.Players == null) return;
            
            if(ClientData.instance.Players.ContainsKey(id))
                ClientData.instance.Players.Remove(id);
        }
    }

    [Serializable]
    public class Vector3Serializable(float x, float y, float z)
    {
        public float x = x;
        public float y = y;
        public float z = z;

        public static Vector3Serializable Subtract(Vector3Serializable a, Vector3Serializable b)
        {
            return new Vector3Serializable(
                a.x - b.x, 
                a.y - b.y,
                a.z - b.z
            );
        }

        public Vector3Serializable(Vector3 vector3) : this(vector3.x, vector3.y, vector3.z)
        {
        }
        
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static Vector3Serializable Add(Vector3Serializable a, Vector3Serializable b)
        {
            return new Vector3Serializable(
                a.x + b.x, 
                a.y + b.y,
                a.z + b.z
            );
        }
    }
    [Serializable]
    public class QuaternionSerializable(float x, float y, float z, float w)
    {
        public float x = x;
        public float y = y;
        public float z = z;
        public float w = w;

        public QuaternionSerializable(Quaternion quaternion) : this(quaternion.x, quaternion.y, quaternion.z, quaternion.w)
        {
        }
        
        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
        
    }
}