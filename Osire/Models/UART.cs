using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osire.Models
{
    class UART
    {
        //public byte[] Data { get; set;}
        public SerialPort serialPort { get; set; }
        public bool Waiting { get; set; }

        public static ManualResetEvent _resetEvent = new ManualResetEvent(false);
       
        public UART()
        {
            serialPort = new SerialPort("COM28", 115200, Parity.None, 8, StopBits.One);
        }

        public void SendMessage(byte[] data)
        {
            if(!serialPort.IsOpen)
            {
                serialPort.Open();
            }
            serialPort.Write(data, 0, data.Length);
            //serialPort.Close();
        }

        public async Task<byte[]> ReceiveMessage()
        {
            int length = 0;
            //serialPort.Open();
           while(true) 
            {
                if(serialPort.BytesToRead > 0)
                {
                    if (serialPort.ReadByte() == 219) //219 = 0b11011011 = Preamble
                    {
                        length = serialPort.ReadByte(); //First bit after Preamble is package length
                        if (length - 1 != serialPort.BytesToRead ) return Array.Empty<byte>(); //Check if PSI is the same as bytes received

                        byte[] data = new byte[length];

                        for (int i = 1; i < length; i++)
                        {
                            data[i] = (byte)serialPort.ReadByte();
                        }
                        data[0] = (byte)length; //put PSI 
                        Waiting = false;
                        return data;
                    }
                }
               await Task.Delay(100);
           }
        }
    }
}
