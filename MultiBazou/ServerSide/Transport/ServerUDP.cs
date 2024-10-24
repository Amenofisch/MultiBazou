using System;
using System.Net;
using MultiBazou.Shared;

namespace MultiBazou.ServerSide.Transport
{
    public class ServerUDP(int id)
    {
        public IPEndPoint EndPoint;

        public bool IsConnected()
        {
            return EndPoint != null;
        }

        public void Connect(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public void SendData(Packet packet)
        {
            Server.SendUDPData(EndPoint, packet);
        }

        public void HandleData(Packet packetData)
        {
            var packetLength = packetData.ReadInt();
            var packetBytes = packetData.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread<Exception>(ex =>
            {
                using var packet = new Packet(packetBytes);
                var packetId = packet.ReadInt();
                
                Server.packetHandlers[packetId](id, packet);
            }, null);
        }

        public void Disconnect()
        {
            EndPoint = null;
        }
    }
}