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
                EndPoint = new IPEndPoint(IPAddress.Parse(Client.instance.ip), Client.instance.port);
                Socket.Connect(EndPoint);

                Socket.BeginReceive(ReceiveCallback, null);
                
                using(var packet = new Packet())
                {
                    SendData(packet);
                }
            }
            catch (Exception e)
            {
                // TODO: properly handle error
                Plugin.log.LogError(e.Message);
            }
        }

        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertInt(Client.instance.Id);
                Socket?.BeginSend(packet.ToArray(), packet.Length(), null, null);
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
                    var receivedIP = new IPEndPoint(IPAddress.Any, 0);
                    var data = Socket.EndReceive(result, ref receivedIP);

                    if (data.Length < 4)
                    {
                        return;
                    }
                    
                    HandleData(data);
                    Socket.BeginReceive(ReceiveCallback, null);
                }
                catch (SocketException ex)
                {
                    // TODO: properly handle error
                }
            }
        }

        private static void HandleData(byte[] data)
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