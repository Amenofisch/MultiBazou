using RiptideNetworking;
using UnityEngine;

namespace MultiBazou
{
    public static class MessageExtensions
    {
        /// <summary>Adds a Vector3 to the message.</summary>
        /// <param name="value">The Vector3 to add.</param>
        public static void Add(this Message message, Vector3 value)
        {
            message.Add(value.x);
            message.Add(value.y);
            message.Add(value.z);
        }

        /// <summary>Adds a Quaternion to the message.</summary>
        /// <param name="value">The Quaternion to add.</param>
        public static void Add(this Message message, Quaternion value)
        {
            message.Add(value.x);
            message.Add(value.y);
            message.Add(value.z);
            message.Add(value.w);
        }

        /// <summary>Reads a Vector3 from the message.</summary>
        public static Vector3 GetVector3(this Message message)
        {
            return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
        }

        /// <summary>Reads a Quaternion from the message.</summary>
        public static Quaternion GetQuaternion(this Message message)
        {
            return new Quaternion(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
        }
    }
}

