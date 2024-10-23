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
            Client.Instance.tcp.SendData(packet);
        }
        
        private static void SendUdpData(Packet packet)
        {
            packet.WriteLength();
            Client.Instance.udp.SendData(packet);
        }
        
        #region Lobby and connection
        
        public static void WelcomeReceived()
        {
            using (var packet = new Packet((int)PacketTypes.welcome))
            {
                packet.Write(Client.Instance.Id);
                packet.Write(Client.Instance.username);
                packet.Write(PluginInfo.Version);
                packet.Write(ContentManager.instance.GameVersion);
                    
                SendTcpData(packet);
            }
            Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.Socket.Client.LocalEndPoint).Port);
            KeepAlive();
        }
            
        public static void KeepAlive()
        {
            using (var packet = new Packet((int)PacketTypes.keepAlive))
            {
                SendTcpData(packet);
            }
        }

        public static void SendReadyState(bool b, int number)
        {
            using (var packet = new Packet((int)PacketTypes.readyState))
            {
                packet.Write(b);
                packet.Write(number);
                    
                SendTcpData(packet);
            }
        }
            
        public static void Disconnect(int id)
        {
            using (var packet = new Packet((int)PacketTypes.disconnect))
            {
                packet.Write(id);
                    
                SendTcpData(packet);
            }
        }
        #endregion


        #region PlayerData
        
            public static void SendInitialPosition(Vector3Serializable position)
            {
                using (var packet = new Packet((int)PacketTypes.playerInitialPos))
                {
                    packet.Write(position);
                        
                    SendUdpData(packet);
                }
            }
            public static void SendPosition(Vector3Serializable position)
            {
                using (var packet = new Packet((int)PacketTypes.playerPosition))
                {
                    packet.Write(position);
                    
                    SendUdpData(packet);
                }
            }
            public static void SendRotation(QuaternionSerializable rotation)
            {
                using (var packet = new Packet((int)PacketTypes.playerRotation))
                {
                    packet.Write(rotation);
                    
                    SendUdpData(packet);
                }
            }
            
            public static void SendSceneChange(GameScene scene)
            {
                using (var packet = new Packet((int)PacketTypes.playerSceneChange))
                {
                    packet.Write(scene);

                    SendTcpData(packet);
                }
            }
            
        #endregion
    }
}