using System;
using System.Net;
using System.Net.Sockets;
using MultiBazou.Shared;
using UnityEngine;

namespace MultiBazou.ClientSide.Transport
{
    public class ClientUDP
    {
        public UdpClient Socket;
        public IPEndPoint EndPoint;

        public void Connect(int localPort)
        {
            Socket = new UdpClient();
            try
            {
                Plugin.log.LogInfo("Trying to connect...");
                EndPoint = new IPEndPoint(IPAddress.Parse(Client.Instance.ip), Client.Instance.port);
                Socket.Connect(EndPoint);

                Socket.BeginReceive(ReceiveCallback, null);
                
                using(Packet packet = new Packet())
                {
                    SendData(packet);
                }
            }
            catch (Exception e)
            {
                // TODO: properly handle error
                Plugin.log.LogInfo("Error while connecting via UDP...");
                Plugin.log.LogError(e.Message);
            }
        }

        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertInt(Client.Instance.Id);
                if (Socket != null)
                {
                    Socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (SocketException ex)
            {
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            if (Socket != null)
            {
                try
                {
                    IPEndPoint receivedIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = Socket.EndReceive(result, ref receivedIP);

                    if (data.Length < 4)
                    {
                        Plugin.log.LogInfo("UDP Data invalid");
                        return;
                    }
                    
                    HandleData(data);
                    Socket.BeginReceive(ReceiveCallback, null);
                }
                catch (SocketException ex)
                {
                    // TODO: properly handle error
                    Plugin.log.LogInfo("error while receiving data from server via UDP: " + ex);
                }
            }
        }

        private void HandleData(byte[] data)
        {
            using (var packet = new Packet(data))
            {
                var packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread<Exception>(ex =>
            {
                using (var packet = new Packet(data))
                {
                    var packetId = packet.ReadInt();

                    if (Client.packetHandlers.TryGetValue(packetId, out var handler))
                    {
                        handler(packet);
                    }
                    else
                    {
                        // TODO: properly handle error
                        Plugin.log.LogInfo($"Packet with id {packetId} not found in packetHandlers dictionary.");
                    }
                }
            }, null);
        }

        public void Disconnect()
        {
            if (Socket != null)
            {
                Socket.Close();
                Socket = null;
            }
            EndPoint = null;
        }
    }
}