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
        public Socket socket;
        public bool Waiting { get; set; }


        public UDP(string Ip)
        {
            client = new UdpClient();
            this.Ip = Ip;
            remotEndPoint = new IPEndPoint(IPAddress.Parse(Ip), PortSend);
            localEndPoint = new IPEndPoint(IPAddress.Any, PortReceive);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public void SendMessage(byte[] data)
        {
            if (!socket.IsBound)
            {
                socket.Bind(localEndPoint);
            }
            //else
            //{
            //    socket.Disconnect(true);
            //    socket.Bind(localEndPoint);
            //}
            client.Send(data, data.Length, remotEndPoint);
        }

        public async Task <byte[]> ReceiveMessage()
        {
            byte[] data = new byte[64]; //39 is the max answer size (OTP)
            
            await socket.ReceiveAsync(data, 0); //Wait for answer
            
            Waiting = false; //New command can be send now
            Array.Resize(ref data, (data[1]+1)); //Cut data to size
            return data.Skip(1).ToArray(); //remove preamble 
        }

        public void Dispose()
        {
            socket?.Dispose();
        }
    }
}
