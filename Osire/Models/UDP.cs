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
            socket.Bind(localEndPoint);
        }

        public void SendMessage(byte[] data)
        {        
            client.Send(data, data.Length, remotEndPoint);
        }

        public void BindSocket()
        {
            if (!socket.IsBound)
            {
                socket.Bind(localEndPoint);
            }
            else
            {
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            }
        }

        public async Task <byte[]> ReceiveMessage(CancellationToken cT)
        {
            byte[] data = new byte[64]; //39 is the max answer size (OTP)
            
            try
            {
                await socket.ReceiveAsync(data, 0, cT); //Wait for answer
            }
            catch(OperationCanceledException e)
            {
                Console.WriteLine(e);
                return Array.Empty<byte>();
            }
           
            Array.Resize(ref data, (data[1]+1)); //Cut data to size
            return data.Skip(1).ToArray(); //remove preamble 
        }

        public async Task<byte[]> Receive(CancellationToken cT)
        {
            byte[] data = new byte[64]; //39 is the max answer size (OTP)

            try
            {
                await socket.ReceiveAsync(data, 0, cT); //Wait for answer
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e);
                return Array.Empty<byte>();
            }

            Array.Resize(ref data, (data[0])); //Cut data to size
            return data; //remove preamble 
        }

        public void Dispose()
        {
            socket?.Dispose();
        }
    }
}
