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
        public enum OSP_ERROR_CODE
        {
            OSP_NO_ERROR = 0x00, /*< no error */
            OSP_ADDRESS_ERROR, /*< invalid device address*/
            OSP_ERROR_INITIALIZATION, /*< error while initializing */
            OSP_ERROR_CRC, /*< incorrect CRC of OSP command */
            OSP_ERROR_UNKNOWN_COMMAND, /*< incorrect or unknown command was send*/
            OSP_ERROR_SPI, /*< SPI interface error */
            OSP_ERROR_PARAMETER, /*< invalid parameter error */
            OSP_ERROR_NOT_IMPLEMENTED /*< CMD not implemented error */
        };
        public enum PossibleCommands
        {
            RESET_LED, CLEAR_ERROR, INITBIDIR, INITLOOP, GOSLEEP, GOACTIVE, GODEEPSLEEP,
            READSTATUS = 0x40, READTEMPST = 0x42, READCOMST = 0x44, READLEDST = 0x46, READTEMP = 0x48, READOTTH = 0x4A, SETOTTH,
            READSETUP, SETSETUP, READPWM, SETPWM, READOTP = 0x58
        }
        public enum MessageTypes
        {
            COMMAND, COMMAND_WITH_RESPONSE, DEMO = 222
        }

        public PossibleCommands Command { get; set; }

 
        private readonly byte Preamble = 0b11011011;
        public MessageTypes Type { get; set; }
        public byte PSI { get; set; } //Payload Size Index

        public UInt16 Address { get; set; } //LED Addresse 0 (Broadcast) ... 1024 

        //Possible payloads
        public bool CurrentRed { get; set; }
        public bool CurrentGreen { get; set; }
        public bool CurrentBlue { get; set; }
        public UInt16 PwmRed { get; set; }
        public UInt16 PwmGreen { get; set; }
        public UInt16 PwmBlue { get; set; }
        public UInt16 LedCount { get; set; }

        public byte ComST { get; set; }
        public byte Status { get; set; }
        public byte LedStatus { get; set; }
        public byte Temperature { get; set; }
        public byte[] OTTH { get; set; }
        public byte Setup { get; set; }

        public UInt16 Delay { get; set; }
        public OSP_ERROR_CODE Error { get; set; }
        public UInt16 Crc { get; set; }


        public Message()
        {
            CurrentRed = true; CurrentGreen = true; CurrentBlue = true;
        }


        public byte[] Serialize()
        {
            byte[] tmp;

            MemoryStream stream= new();
            BinaryWriter writer = new(stream);

            //Set MSB of the pwm value to slect max current
            PwmRed |= (ushort)((CurrentRed ? 1 : 0) << 15);
            PwmGreen |= (ushort)((CurrentGreen ? 1 : 0) << 15);
            PwmBlue |= (ushort)((CurrentBlue ? 1 : 0) << 15);

            try
            {
                writer.Write(Preamble);         //[0]
                writer.Write(PSI);              //[1]
                writer.Write((byte)Type);       //[2]
                writer.Write((byte)Command);    //[3]
                writer.Write(Address);          //[4]+[5]
                switch (Command)
                {
                    case PossibleCommands.RESET_LED:
                        break;
                    case PossibleCommands.CLEAR_ERROR:
                        break;
                    case PossibleCommands.INITBIDIR:
                        break;
                    case PossibleCommands.GOSLEEP:
                        break;
                    case PossibleCommands.GOACTIVE:
                        break;
                    case PossibleCommands.GODEEPSLEEP:
                        break;
                    case PossibleCommands.READSTATUS:
                        break;
                    case PossibleCommands.READTEMPST:
                        break;
                    case PossibleCommands.READCOMST:
                        break;
                    case PossibleCommands.READLEDST:
                        break;
                    case PossibleCommands.READOTTH:
                        break;
                    case PossibleCommands.SETOTTH:
                        break;
                    case PossibleCommands.READSETUP:
                        break;
                    case PossibleCommands.SETSETUP:
                        break;
                    case PossibleCommands.READPWM:
                        break;
                    case PossibleCommands.SETPWM:
                        writer.Write(PwmRed);           //[6]+[7]
                        writer.Write(PwmGreen);         //[8]+[9]
                        writer.Write(PwmBlue);          //[10]+[11]
                        break;
                    case PossibleCommands.READOTP:
                        break;
                    default:
                        break;
                }

                tmp = stream.ToArray();
                writer.Write(CalculateCRC(tmp.Skip(1).ToArray())); //Calculate crc without preamble

                //writer.Write(Delay);
                return stream.ToArray();
            }
            catch (Exception)
            {

                throw;
            }
            finally { stream.Dispose(); }
        }

        public bool DeSerialize(byte[] data)
        {
            this.PSI = data[0];                             //[0]

            this.Crc = BitConverter.ToUInt16(data, this.PSI - 2); //[PSI-1][PSI]
            Array.Resize(ref data, data.Length - 2);
            if (this.Crc != CalculateCRC(data)) return false;

            this.Command = (PossibleCommands)data[1];       //[1]
            this.Error = (OSP_ERROR_CODE)data[2];           //[2]
            this.Address = BitConverter.ToUInt16(data, 3);  //[3][4]

            switch (this.Command)
            {
                case PossibleCommands.RESET_LED:
                    break;
                case PossibleCommands.CLEAR_ERROR:
                    break;
                case PossibleCommands.INITBIDIR:
                    this.LedCount = BitConverter.ToUInt16(data, 5);     //[5][6]
                    break;
                case PossibleCommands.INITLOOP:
                    break;
                case PossibleCommands.GOSLEEP:
                    break;
                case PossibleCommands.GOACTIVE:
                    break;
                case PossibleCommands.GODEEPSLEEP:
                    break;
                case PossibleCommands.READSTATUS:
                    break;
                case PossibleCommands.READTEMPST:
                    break;
                case PossibleCommands.READCOMST:
                    this.ComST = data[5];
                    break;
                case PossibleCommands.READLEDST:
                    break;
                case PossibleCommands.READTEMP:
                    break;
                case PossibleCommands.READOTTH:
                    break;
                case PossibleCommands.SETOTTH:
                    break;
                case PossibleCommands.READSETUP:
                    this.Setup = data[5];
                    break;
                case PossibleCommands.SETSETUP:
                    break;
                case PossibleCommands.READPWM:
                    this.PwmRed = BitConverter.ToUInt16(data, 5);
                    this.PwmGreen = BitConverter.ToUInt16(data, 7);
                    this.PwmBlue = BitConverter.ToUInt16(data, 9);
                    break;
                case PossibleCommands.SETPWM:
                    break;
                case PossibleCommands.READOTP:
                    break;
                default:
                    break;
            }
            return true;
        }

        public void SetMessageToReset()
        {
            this.Command = PossibleCommands.RESET_LED;
            this.Type = MessageTypes.COMMAND_WITH_RESPONSE;
            this.PSI = 7; // PSI (1) + Type (1) + Command (1) + Address (2) +  CRC (2)
            this.Address= 0;
            //this.ExpectResponse = true;
        }

        public void SetMessageToInit()
        {
            this.Command = PossibleCommands.INITBIDIR;
            this.Type = MessageTypes.COMMAND_WITH_RESPONSE;
            this.PSI = 7;  // PSI (1) + Type (1) + Command (1) + Address (2) +  CRC (2)
            this.Address = 1; //Init command starts at first LED
        }

        public void setCommand(PossibleCommands cmd)
        {
            this.Command = cmd;

            switch (this.Command)
            {
                case PossibleCommands.INITBIDIR:
                    PSI = 2;
                    break;
                case PossibleCommands.GODEEPSLEEP:
                    PSI = 0;
                    break;
                case PossibleCommands.SETPWM:
                    PSI = 0;
                    break;
                case PossibleCommands.READPWM:
                    PSI = 6;
                    break;
                default:
                    break;
            }
        }

        public static ushort CalculateCRC(byte[] data)
        {
            ushort[] table = new ushort[256];
            ushort poly = 0x8005;
            ushort value;
            ushort temp;
            for (ushort i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x0001) != 0)
                    {
                        value = (ushort)((value >> 1) ^ poly);
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }

            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; ++i)
            {
                byte index = (byte)(crc ^ data[i]);
                crc = (ushort)((crc >> 8) ^ table[index]);
            }
            return crc;
        }      

        public string getCommand()
        {
            if(Enum.IsDefined(typeof(PossibleCommands), this.Command))
            {
                PossibleCommands cmd = (PossibleCommands)Enum.ToObject(typeof(PossibleCommands), this.Command);
                return cmd.ToString();
            }
            else
            {
                return "Convertion failed";
            }
        }
        public string getErrorCode()
        {
            if (Enum.IsDefined(typeof(OSP_ERROR_CODE), this.Error))
            {
                OSP_ERROR_CODE err = (OSP_ERROR_CODE)Enum.ToObject(typeof(OSP_ERROR_CODE), this.Error);
                return err.ToString();
            }
            else
            {
                return "Convertion failed";
            }
        }
    }
}
