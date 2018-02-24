using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ScanerSimulator
{
    public class TCPState
    {
        public string ID { get; set; }
        public TcpClient Client { get; }
        public NetworkStream Stream { get; set; }
        public byte[] Buffer { get; set; }
        public int BufferSize { get; internal set; }

        public TCPState(TcpClient client)
        {
            this.Client = client;
            this.ID = this.Client.Client.RemoteEndPoint.ToString();
            this.Stream = client.GetStream();
            this.Buffer = new byte[client.ReceiveBufferSize];
            this.BufferSize = this.Buffer.Length;
        }
    }
}
