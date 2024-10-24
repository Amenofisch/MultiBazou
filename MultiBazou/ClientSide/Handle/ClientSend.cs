using System.Net;
using MultiBazou.ClientSide.Data;
using MultiBazou.Shared;
using MultiBazou.Shared.Data;

namespace MultiBazou.ClientSide.Handle
{
    public class ClientSend
    {
        private static void SendTcpData(Packet packet)
        {
            packet.WriteLength();
            Client.instance.Tcp.SendData(packet);
        }
        
        private static void SendUdpData(Packet packet)
        {
            packet.WriteLength();
            Client.instance.UDP.SendData(packet);
        }
        
        #region Lobby and connection
        
        public static void WelcomeReceived()
        {
            using (var packet = new Packet((int)PacketTypes.Welcome))
            {
                packet.Write(Client.instance.Id);
                packet.Write(Client.instance.username);
                packet.Write(PluginInfo.Version);
                packet.Write(ContentManager.instance.GameVersion);
                    
                SendTcpData(packet);
            }
            Client.instance.UDP.Connect(((IPEndPoint)Client.instance.Tcp.Socket.Client.LocalEndPoint).Port);
        }

        public static void SendReadyState(bool b, int number)
        {
            using (var packet = new Packet((int)PacketTypes.ReadyState))
            {
                packet.Write(b);
                packet.Write(number);
                    
                SendTcpData(packet);
            }
        }
            
        public static void Disconnect(int id)
        {
            using (var packet = new Packet((int)PacketTypes.Disconnect))
            {
                packet.Write(id);
                    
                SendTcpData(packet);
            }
        }
        #endregion


        #region PlayerData
        
            public static void SendInitialPosition(Vector3Serializable position)
            {
                using var packet = new Packet((int)PacketTypes.PlayerInitialPos);
                packet.Write(position);
                SendUdpData(packet);
            }
            
            public static void SendPosition(Vector3Serializable position)
            {
                using var packet = new Packet((int)PacketTypes.PlayerPosition);
                packet.Write(position);
                SendUdpData(packet);
            }
            
            public static void SendRotation(QuaternionSerializable rotation)
            {
                using var packet = new Packet((int)PacketTypes.PlayerRotation);
                packet.Write(rotation);
                SendUdpData(packet);
            }
            
            public static void SendSceneChange(GameScene scene)
            {
                using var packet = new Packet((int)PacketTypes.PlayerSceneChange);
                packet.Write(scene);
                SendTcpData(packet);
            }
            
        #endregion
    }
}