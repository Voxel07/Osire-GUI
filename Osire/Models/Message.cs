using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Osire.Models
{
    [Serializable]
    class Message
    {
        private ulong Timestamp { get; set; }
        private string Command { get; set; }

        private UInt16 PSI { get; set; } //Payload Size Index

        private UInt16 Address { get; set; } //LED Addresse 0 (Broadcast) ... 1024 
        
        //Possible payloads
        private UInt16 PwmRed { get; set; }
        private UInt16 PwmGreen { get; set; }
        private UInt16 PwmBlue { get; set; }

        private UInt16 Delay { get; set; }


        public Message() { }


        public byte[] serialize(SerializationInfo info, StreamingContext context)
        {
            //byte[] tmp;

            MemoryStream stream= new();
            BinaryWriter writer = new(stream);

            try
            {
                writer.Write(Command);
                writer.Write(PSI);
                writer.Write(Address);
                writer.Write(PwmRed);
                writer.Write(PwmGreen);
                writer.Write(PwmBlue);
                writer.Write(Delay);
                return stream.ToArray();
            }
            catch (Exception)
            {

                throw;
            }
            finally { stream.Dispose(); }
        }


        public static ushort CalculateCRC(byte[] data)
        {
            const uint POLYNOMOAL = 0x04C11DB7;
            int DATA_BITS = data.Length;
            uint crc = 0;

            foreach (byte b in data)
            {
                crc <<= 8;
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & (1 << (DATA_BITS - 1))) != 0)
                    {
                        crc = (crc << 1) ^ POLYNOMOAL;
                    }
                    else
                    {
                        crc <<= 1;
                    }

                }
            }
            return (ushort)crc;
        }
    }
}
