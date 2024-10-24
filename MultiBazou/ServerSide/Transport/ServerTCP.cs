using System;
using System.Net.Sockets;
using MultiBazou.ServerSide.Handle;
using MultiBazou.Shared;
using UnityEngine;


namespace MultiBazou.ServerSide.Transport
{
    public class ServerTcp
        {
            private const int DataBufferSize = 4096;

            public TcpClient Socket;

            private readonly int _id;
            private NetworkStream _stream;
            private Packet _receivedData;
            private byte[] _receiveBuffer;

            public ServerTcp(int id)
            {
                _id = id;
            }

            public void Connect(TcpClient socket)
            {
                Socket = socket;
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;

                _stream = Socket.GetStream();
                _stream.ReadTimeout = 200;
                
                _receivedData = new Packet();

                _receiveBuffer = new byte[DataBufferSize];

                _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                
                ServerSend.Welcome(_id, "Welcome to the server!");
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (Socket != null)
                    {
                        _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    // TODO: properly handle error
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    var byteLength = _stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        if (Server.Clients.TryGetValue(_id, out var client))
                        {
                            client.Disconnect(_id);
                        }
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(_receiveBuffer, data, byteLength);

                    _receivedData.Reset(HandleData(data));
                    Array.Clear(_receiveBuffer, 0, _receiveBuffer.Length);
                    _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                }
                catch (Exception ex)
                {
                    if(ex.InnerException is SocketException sockEx && sockEx.ErrorCode != 10054) {
                        Plugin.log.LogInfo($"Error receiving TCP data: {ex}"); 
                    }
                    if (Server.Clients.TryGetValue(_id, out var client))
                    {
                        client.Disconnect(_id);
                    }
                }
            }

            private bool HandleData(byte[] data)
            {
                var packetLength = 0;
                
                _receivedData.SetBytes(data);
                if (_receivedData.UnreadLength() >= 4)
                {
                    packetLength = _receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
                {
                    var packetBytes = _receivedData.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread<Exception>(ex =>
                    {
                        using (var packet = new Packet(packetBytes))
                        {
                            var packetId = packet.ReadInt();
                            if(Server.packetHandlers.ContainsKey(packetId))
                                Server.packetHandlers[packetId](_id, packet);
                        }
                    }, null);
                    packetLength = 0;
                    
                    if (_receivedData.UnreadLength() < 4) continue;
                    packetLength = _receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                return packetLength <= 1;
            }

            public void Disconnect()
            {
                Socket.Close();
                _stream.Close();
                _stream = null;
                _receivedData = null;
                _receiveBuffer = null;
                Socket = null;
            }
        }
}