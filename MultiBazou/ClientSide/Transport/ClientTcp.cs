using System;
using System.Net.Sockets;
using MultiBazou.Shared;

namespace MultiBazou.ClientSide.Transport
{
    public class ClientTcp
    {
        public TcpClient Socket = new TcpClient();

        private NetworkStream _stream;
        private Packet _receivedData;
        private byte[] _receiveBuffer;

        public void Connect()
        {
            try
            {
                Socket = new TcpClient
                { 
                    ReceiveBufferSize = Client.dataBufferSize,
                    SendBufferSize = Client.dataBufferSize
                };

                _receiveBuffer = new byte[Client.dataBufferSize];

                Socket.BeginConnect(Client.instance.ip, Client.instance.port, ConnectCallback, Socket);
            }
            catch (Exception ex)
            {
                // TODO: properly handle error
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                Socket.EndConnect(result);

                if (!Socket.Connected)
                {
                    return;
                }
                
                _stream = Socket.GetStream();
                _receivedData = new Packet();

                _stream.BeginRead(_receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                // TODO: properly handle error
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            if(_stream == null) return;
            try
            {
                var byteLength = _stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Disconnect();
                    return;
                }

                var data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);
                
                _receivedData.Reset(HandleData(data));
                Array.Clear(_receiveBuffer, 0, _receiveBuffer.Length);
                _stream.BeginRead(_receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                // TODO: properly handle error
                Disconnect();
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
                        if (Client.packetHandlers.ContainsKey(packetId))
                        {
                            Client.packetHandlers[packetId](packet);
                        }
                    }
                },  null);
                
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

        public void SendData(Packet packet)
        {
            if (Socket != null)
            {
                _stream.BeginWrite(packet.ToArray(), 0,
                    packet.Length(), null, null);

            }
        }

        public void Disconnect()
        {
            Socket?.Close();

            _stream = null;
            _receivedData = null;
            _receiveBuffer = null;
            Socket = null;
        }
    }
}