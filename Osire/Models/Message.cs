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
            READSETUP, SETSETUP, READPWM, SETPWM, SETSETUPSR, SETPWMSR, SETOTTHSR, READOTP = 0x58
        }
        public enum MessageTypes
        {
            COMMAND, COMMAND_WITH_RESPONSE, DEMO = 222
        }
        public enum PossibleDemos
{
            STATIC_COLOR, LED_STRIPE, DIMING, PINGPONG
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
        public ushort U { get; set; }
        public ushort V { get; set; }
        public ushort Lv { get; set; }

        public byte ComST { get; set; }
        public byte Status { get; set; }
        public byte LedStatus { get; set; }
        public byte Temperature { get; set; }
        public byte[] OTTH { get; set; }
        public byte Setup { get; set; }
        public byte[] OTP { get; set; }

        public UInt16 Delay { get; set; }
        public OSP_ERROR_CODE Error { get; set; }
        public UInt16 Crc { get; set; }


        public Message()
        {
            CurrentRed = true; CurrentGreen = true; CurrentBlue = true;
            OTTH = new byte[3];
            OTP = new byte[32];
            Setup = 0x32; //Default config
        }


        public byte[] Serialize()
        {
            byte[] tmp;

            MemoryStream stream= new();
            BinaryWriter writer = new(stream);

         

            try
            {
                writer.Write(Preamble);         //[0]
                writer.Write(PSI);              //[1]
                writer.Write((byte)Type);       //[2]
                writer.Write((byte)Command);    //[3]
                writer.Write(Address);          //[4]+[5]
                switch (Command)
                {
                    case PossibleCommands.SETOTTH:
                    case PossibleCommands.SETOTTHSR:
                        writer.Write(OTTH);             //[6]
                        break;
                    case PossibleCommands.SETSETUP:
                    case PossibleCommands.SETSETUPSR:
                        writer.Write(Setup);            //[6]
                        break;
                    case PossibleCommands.SETPWM:
                    case PossibleCommands.SETPWMSR:
                        //Set MSB of the pwm value to slect max current
                        //U |= (ushort)((CurrentRed ? 1 : 0) << 15);
                        //V |= (ushort)((CurrentGreen ? 1 : 0) << 15);
                        //Lv |= (ushort)((CurrentBlue ? 1 : 0) << 15);
                        writer.Write(U);            //[6]+[7]
                        writer.Write(V);            //[8]+[9]
                        writer.Write(Lv);           //[10]+[11]
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

            this.Crc = BitConverter.ToUInt16(data, this.PSI - 2); //[PSI-1][PSI] //-3 -> sizeof(crc) + preamble
            Array.Resize(ref data, data.Length - 2);
            if (this.Crc != CalculateCRC(data))
            {
                this.Error = OSP_ERROR_CODE.OSP_ERROR_CRC;
                return false;
            
            }

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
                    this.Temperature = data[5]; 
                    this.Status = data[6];
                    this.LedCount = BitConverter.ToUInt16(data, 7);     //[7][8]
                    this.Address = this.LedCount;
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
                    this.Status = data[5];
                    break;
                case PossibleCommands.SETOTTHSR:
                case PossibleCommands.SETSETUPSR:
                case PossibleCommands.SETPWMSR:
                case PossibleCommands.READTEMPST:
                    this.Temperature = data[5];
                    this.Status = data[6];
                    break;
                case PossibleCommands.READCOMST:
                    this.ComST = data[5];
                    break;
                case PossibleCommands.READLEDST:
                    this.LedStatus = data[5];
                    break;
                case PossibleCommands.READTEMP:
                    this.Temperature = data[5];
                    break;
                case PossibleCommands.READOTTH:
                    this.OTTH[0] = data[5];
                    this.OTTH[1] = data[6];
                    this.OTTH[2] = data[7];
                    break;
                case PossibleCommands.SETOTTH:
                    break;
                case PossibleCommands.READSETUP:
                    this.Setup = data[5];
                    break;
                case PossibleCommands.SETSETUP:
                    break;
                case PossibleCommands.READPWM:
                    this.PwmRed = BitConverter.ToUInt16(data, 5); //[5][6]
                    this.PwmGreen = BitConverter.ToUInt16(data, 7); //[7][8] 
                    this.PwmBlue = BitConverter.ToUInt16(data, 9); //[9][10]
                    break;
                case PossibleCommands.SETPWM:
                    break;
                case PossibleCommands.READOTP:
                    for (int i = 5; i < data.Length; i++)
                    {
                        this.OTP[i - 5] = data[i];
                    }
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

        public void SetMessageToDefaultConf()
        {
            this.Command = PossibleCommands.SETSETUP;
            this.Type = MessageTypes.COMMAND_WITH_RESPONSE;
            this.PSI = 8;
            this.Address = 0;
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
