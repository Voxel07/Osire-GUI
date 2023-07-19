using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.CompilerServices;

namespace Osire.Models
{
    [Serializable]
    class Message
    {
        public enum ERROR_OSP
        {
            OSP_NO_ERROR = 0x00, /*< no error */
            OSP_ADDRESS_ERROR, /*< invalid device address*/
            OSP_ERROR_INITIALIZATION, /*< error while initializing */
            OSP_ERROR_CRC, /*< incorrect CRC of OSP command */
            OSP_ERROR_UNKNOWN_COMMAND, /*< incorrect or unknown command was send*/
            OSP_ERROR_SPI, /*< SPI interface error */
            OSP_ERROR_PARAMETER, /*< invalid parameter error */
            OSP_ERROR_NOT_IMPLEMENTED, /*< CMD not implemented error */
            OSP_ERROR_TYPE_NOT_KNOWN,
            OSP_ERROR_NOT_DEFINED = 0x0F
        }

        public enum PossibleCommands
        {
            RESET_LED, CLEAR_ERROR, INITBIDIR, INITLOOP, GOSLEEP, GOACTIVE, GODEEPSLEEP,
            READSTATUS = 0x40, READTEMPST = 0x42, READCOMST = 0x44, READLEDST = 0x46, READTEMP = 0x48, READOTTH = 0x4A, SETOTTH,
            READSETUP, SETSETUP, READPWM, SETPWM, SETSETUPSR, SETPWMSR, SETOTTHSR, READOTP = 0x58, GETGAMUT, SETLUV = 0x69, SETLUVSR
        }

        public enum MessageTypes
        {
            COMMAND, COMMAND_WITH_RESPONSE, DEMO = 222
        }

        public enum PossibleDemos
        {
            STATIC_COLOR, LED_STRIPE, DIMING, PINGPONG, TEMPCOMP, IOL
        }

        enum Categories:byte
        {
            NoError, Package, OSP, SPI, IoL, ColorCorrection, Update, NotDefined = 0xF
        }

        enum Error_SPI:byte
        {
            NO_ERROR_SPI,
            SPI_BUSY,
            SPI_SEND_ERROR,
            SPI_RECEIVE_ERROR,
            SPI_ERROR_TIME_OUT,
            SPI_ERROR_CONFIG,
            SPI_ERROR_RECEIVE_TO_MANY_BYTE,
            SPI_ERROR_NOT_DEFINED = 0x0F
        }

        enum Error_Package : byte
        {
            PACKAGE_OK,
            PACKAGE_CRC_ERROR,
            PACKAGE_LENGTH_ERROR,
            PACKAGE_TYPE_ERROR,
            PACKAGE_PSI_TO_SHORT_ERROR,
            PACKAGE_ERROR_NOT_DEFINED = 0x0F

        }

        enum Error_IoL : byte
        {
            IOL_OK,
            IOL_PACKAGE_LENGTH_ERROR,
            IOL_PACKAGE_PARSING_ERROR,
            IOL_PACKAGE_CRC_ERROR,
            IOL_GET_TEMP_ERRPR,
            IOL_LED_STATUS_ERROR,
            IOL_TEMPCOMP_ERROR,
            IOL_CALCPWM_ERROR,
            IOL_SET_PWM_ERROR,
            IOL_ERROR_NOT_DEFINED = 0x0F
        }

        enum Error_ColorCorrection : byte
        {
            CC_NO_ERROR,
            CC_GET_TEMP_ERROR, //Error during temp data fetching
            CC_STATE_ERROR, //Error in LED State
            CC_OTP_ERROR,
            CC_ALGO_ERROR,
            CC_SET_PWM_ERROR,
            CC_ERROR_NOT_DEFINED = 0x0F
        }

        enum ERROR_FLASH: byte
        {
            CMD_FLASHMEMORY_ACKNOWLEDGE,
            CMD_FLASHMEMORY_ERROR_MESSAGE,
            CMD_FLASHMEMORY_READ,
            CMD_FLASHMEMORY_ERASE,
            CMD_FLASHMEMORY_WRITE,
            CMD_FLASHMEMORY_EXECUTE,
            CMD_FLASHMEMORY_NOT_DEFINED = 0x0F
        }

        public PossibleCommands Command { get; set; }

        public MessageTypes Type { get; set; }
        public UInt16 PSI { get; set; } //Payload Size Index

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
        public int Temperature { get; set; }
        public byte[] OTTH { get; set; }
        public byte Setup { get; set; }
        public byte[] OTP { get; set; }
        public byte[] Gamut { get; set; }

        public UInt16 Delay { get; set; }
        public string Error { get; set; }
        public byte rawErrorCode { get; set; }
        public byte ErrorCategory { get; set; }
        public byte ErrorType { get; set; }
        public UInt16 Crc { get; set; }


        public Message()
        {
            CurrentRed = true; CurrentGreen = true; CurrentBlue = true;
            OTTH = new byte[3];
            OTP = new byte[32];
            Gamut = new byte[72];
            Setup = 0x32; //Default config
        }


        public byte[] Serialize()
        {
            byte[] tmp;

            MemoryStream stream= new();
            BinaryWriter writer = new(stream);

            try
            {
                //writer.Write(Preamble);         //[0]
                writer.Write(PSI);              //[0] + [1]
                writer.Write((byte)Type);       //[2]
                writer.Write((byte)Command);    //[3]
                writer.Write(Address);          //[4] + [5]
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
                    case PossibleCommands.SETLUV:
                    case PossibleCommands.SETLUVSR:
                        //Set MSB of the pwm value to slect max current
                        writer.Write(U);            //[6]+[7]
                        writer.Write(V);            //[8]+[9]
                        writer.Write(Lv);           //[10]+[11]
                        break;
                    case PossibleCommands.SETPWM:
                    case PossibleCommands.SETPWMSR:
                            //Set MSB of the pwm value to slect max current
                            //UInt16 max = (ushort)((ushort)(CurrentRed ? 1 : 0) << 15);
                            //PwmRed |= max;
                            //PwmGreen |= (ushort)((CurrentGreen ? 1 : 0) << 15);
                            //PwmBlue |= (ushort)((CurrentBlue ? 1 : 0) << 15);
                           
                            
                            PwmRed = SetCurrent(PwmRed, CurrentRed);
                            PwmBlue = SetCurrent(PwmBlue, CurrentBlue);
                            PwmGreen = SetCurrent(PwmGreen, CurrentGreen);

                            writer.Write(PwmBlue);           //[10]+[11]
                            writer.Write(PwmGreen);            //[8]+[9]
                            writer.Write(PwmRed);            //[6]+[7]
                        break;
                    default:
                        break;
                }

                tmp = stream.ToArray();
                writer.Write(CalculateCRC(tmp)); //Calculate crc without preamble

                //writer.Write(Delay);
                return stream.ToArray();
            }
            catch (Exception)
            {

                throw;
            }
            finally { stream.Dispose(); }
        }

        public ushort SetCurrent(ushort pwm, bool current)
        {
            ushort val = 1;
            if (current)
            {
                return pwm |= 1 << 15;
            }
            else
            {
                return pwm &= (ushort)~(val << 15);
                //pwm &= 0 << 15;
            }
        }

        public bool DeSerialize(byte[] data)
        {
            if (data.Length < 9)
            {
                this.Error = Error_Package.PACKAGE_LENGTH_ERROR.ToString() + data.Length;
                return false;
            }
            this.PSI = BitConverter.ToUInt16(data, 0);            //[0]

            this.Crc = BitConverter.ToUInt16(data, this.PSI - 2); //[PSI-1][PSI] //-3 -> sizeof(crc) + preamble
            Array.Resize(ref data, data.Length - 2);
            ushort calculatedCrc = CalculateCRC(data);

           

            if (this.Crc != calculatedCrc)
            {
                this.Error = ERROR_OSP.OSP_ERROR_CRC.ToString(); 
                return false;
            }

            if (data[2] != 0xFF)
            {
                this.Error = ERROR_OSP.OSP_ERROR_TYPE_NOT_KNOWN.ToString();
                return false;
            }

            this.Command = (PossibleCommands)data[3];       //[1]
            this.Address = BitConverter.ToUInt16(data, 4);  //[4][5]
            this.rawErrorCode = data[6];           //[3]

            switch (this.Command)
            {
                case PossibleCommands.RESET_LED:
                    break;
                case PossibleCommands.CLEAR_ERROR:
                    break;
                case PossibleCommands.INITBIDIR:
                    this.Temperature = data[7] - 113; 
                    this.Status = data[8];
                    this.LedCount = BitConverter.ToUInt16(data, 9);     //[7][8]
                    if (LedCount > 81) return false;
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
                    this.Status = data[7];
                    break;
                case PossibleCommands.SETOTTHSR:
                case PossibleCommands.SETSETUPSR:
                case PossibleCommands.SETPWMSR:
                case PossibleCommands.READTEMPST:
                case PossibleCommands.SETLUVSR:
                    this.Status = data[7];
                    this.Temperature = data[9] - 113;
                    break;
                case PossibleCommands.READCOMST:
                    this.ComST = data[7];
                    break;
                case PossibleCommands.READLEDST:
                    this.LedStatus = data[7];
                    break;
                case PossibleCommands.READTEMP:
                    //this.Temperature = (data[5] & 0b00111111);
                    this.Temperature = data[7] - 113;
                    break;
                case PossibleCommands.READOTTH:
                    this.OTTH[0] = data[7];
                    this.OTTH[1] = data[8];
                    this.OTTH[2] = data[9];
                    break;
                case PossibleCommands.SETOTTH:
                    break;
                case PossibleCommands.READSETUP:
                    this.Setup = data[7];
                    break;
                case PossibleCommands.SETSETUP:
                    break;
                case PossibleCommands.READPWM:
                    this.PwmBlue = BitConverter.ToUInt16(data, 7); //[5][6]
                    this.PwmGreen = BitConverter.ToUInt16(data, 9); //[7][7] 
                    this.PwmRed = BitConverter.ToUInt16(data, 11); //[9][10]
                    break;
                case PossibleCommands.SETPWM:
                    break;
                case PossibleCommands.READOTP:
                    for (int i = 7; i < data.Length; i++)
                    {
                        this.OTP[i - 7] = data[i];
                    }
                    break;
                case PossibleCommands.GETGAMUT:
                    for (int i = 7; i < data.Length; i++)
                    {
                        this.Gamut[i - 7] = data[i];
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
            this.PSI = 8; // PSI (1) + Type (1) + Command (1) + Address (2) +  CRC (2)
            this.Address= 0;
            //this.ExpectResponse = true;
        }

        public void SetMessageToInit()
        {
            this.Command = PossibleCommands.INITBIDIR;
            this.Type = MessageTypes.COMMAND_WITH_RESPONSE;
            this.PSI = 8;  // PSI (1) + Type (1) + Command (1) + Address (2) +  CRC (2)
            this.Address = 1; //Init command starts at first LED
        }

        public void SetMessageToDefaultConf()
        {
            this.Command = PossibleCommands.SETSETUP;
            this.Type = MessageTypes.COMMAND_WITH_RESPONSE;
            this.PSI = 9;
            this.Address = 0;
        }

        ushort CalculateCRC(byte[] data)
        {
	    /* CRC-16_ARC (aka IBM/ANSI)  Polynomial: x^16 + x^15 + x^2 + 1
	     * using the configuration:
	     *    Polynomial   = 0x8005
	     *    ReflectIn    = True
	     *    XorIn        = 0x0000
	     *    ReflectOut   = True
	     *    XorOut       = 0x0000
	     *    Algorithm    = table-driven
	     *
	     * http://www.sunshine2k.de/coding/javascript/crc/crc_js.html */

	            ushort [] crc16_table = new ushort[] {
			    0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241, 0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
			    0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40, 0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
			    0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40, 0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
			    0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641, 0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
			    0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240, 0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
			    0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41, 0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
			    0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41, 0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
			    0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640, 0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
			    0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240, 0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
			    0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41, 0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
			    0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41, 0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
			    0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640, 0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
			    0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241, 0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
			    0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40, 0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
			    0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40, 0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
			    0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641, 0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040
	    };

	    ushort crc = 0; //0xFFFF;

	    // compute crc
        for(int i = 0; i < data.Length; i++)
        {
            crc = (ushort)((crc >> 8) ^ (crc16_table[(crc ^ data[i]) & 0xFF]));
        }

	    return crc;
        }

        public string getCommand()
        {
            if(Enum.IsDefined(typeof(PossibleCommands), this.Command))
            {
                if(this.Type == MessageTypes.DEMO)
                {
                    PossibleDemos dm = (PossibleDemos)Enum.ToObject(typeof(PossibleCommands), this.Command);
                    return dm.ToString();
                }
                else
                {
                    PossibleCommands cmd = (PossibleCommands)Enum.ToObject(typeof(PossibleCommands), this.Command);
                    return cmd.ToString();
                }
            }
            else
            {
                return "Convertion failed";
            }
        }

        public string getErrorCode()
        {
            byte first4Bits = (byte)(rawErrorCode  >> 4);      // Shift right by 4 bits to get the first 4 bits
            byte last4Bits = (byte)(rawErrorCode & 0x0F);     // Use bitwise AND to get the last 4 bits

            Categories category = (Categories)first4Bits;

            switch (category)
            {
                case Categories.Package:

                    if (Enum.IsDefined(typeof(Error_Package), last4Bits))
                    {
                        return Enum.Parse(typeof(Error_Package), last4Bits.ToString()).ToString();
                    }
                    else
                    {
                        return Error_Package.PACKAGE_ERROR_NOT_DEFINED.ToString();
                    }

                case Categories.SPI:

                    if (Enum.IsDefined(typeof(Error_SPI), last4Bits))
                    {
                        return Enum.Parse(typeof(Error_SPI), last4Bits.ToString()).ToString();
                    }
                    else
                    {
                        return Error_SPI.SPI_ERROR_NOT_DEFINED.ToString();
                    }

                case Categories.OSP:

                    if(Enum.IsDefined(typeof(ERROR_OSP), last4Bits))
                    {
                        return Enum.Parse(typeof(ERROR_OSP), last4Bits.ToString()).ToString();
                    }
                    else
                    {
                        return ERROR_OSP.OSP_ERROR_NOT_DEFINED.ToString();
                    }

                case Categories.IoL:

                    if (Enum.IsDefined(typeof(Error_IoL), last4Bits))
                    {
                        return Enum.Parse(typeof(Error_IoL), last4Bits.ToString()).ToString();
                    }
                    else
                    {
                        return Error_IoL.IOL_ERROR_NOT_DEFINED.ToString();
                    }

                case Categories.ColorCorrection:

                    if (Enum.IsDefined(typeof(Error_ColorCorrection), last4Bits))
                    {
                        return Enum.Parse(typeof(Error_ColorCorrection), last4Bits.ToString()).ToString();
                    }
                    else
                    {
                        return Error_ColorCorrection.CC_ERROR_NOT_DEFINED.ToString();
                    }

                case Categories.Update:

                    if (Enum.IsDefined(typeof(ERROR_FLASH), last4Bits))
                    {
                        return Enum.Parse(typeof(ERROR_FLASH), last4Bits.ToString()).ToString();
                    }
                    else
                    {
                        return ERROR_FLASH.CMD_FLASHMEMORY_NOT_DEFINED.ToString();
                    }

                default:
                    Console.WriteLine("Unknown category: {0}", category);
                    return null;
            }
        }
    }
}
