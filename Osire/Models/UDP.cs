using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


namespace Osire.Models
{
    class UDP
    {
        UdpClient client;
        IPEndPoint remotEndPoint;
        IPEndPoint localEndPoint;
        private string Ip { get; set; }
        private readonly int PortSend = 64000;
        private readonly int PortReceive = 64003;

        public UDP(string Ip)
        {
            client = new UdpClient();
            this.Ip = Ip;
            remotEndPoint = new IPEndPoint(IPAddress.Parse(Ip), PortSend);
            localEndPoint = new IPEndPoint(IPAddress.Any, PortReceive);
        }

        public void SendMessage(byte[] data)
        {

            client.Send(data, data.Length, remotEndPoint);
        }

        public async Task<byte[]> ReceiveMessage()
        {
            UdpReceiveResult result = await client.ReceiveAsync();
            return result.Buffer;
        }
    }
}
