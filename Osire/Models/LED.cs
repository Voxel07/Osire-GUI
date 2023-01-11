using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osire.Models
{
    class LED
    {
        public UInt16 Address { get; set; }
        
        public byte Temperature { get; set; }
        public string TimeStampTemp { get; set; }
        //PWM Values
        public string PwmRed { get; set; }
        public string PwmGreen { get; set; }
        public string PwmBlue { get; set; }
        public string TimeStampPwm { get; set; }

        //ComStats
        public string Cs_SIO1 { get; set; }
        public string Cs_SIO2 { get; set; }
        public string TimeStampComstats { get; set; }

        //Setup
        public string PWM_F { get; set; }
        public string CLK_INV { get; set; }
        public string CRC_EN { get; set; }
        public string TEMPCLK { get; set; } 
            //Error handeling
        public string CE_FSAVE { get; set; } //Communication error
        public string LOS_FSAVE { get; set; } //open/short
        public string OT_FSAVE { get; set; } // over Temperature
        public string UV_FSAVE { get; set; } // under Voltage
        public string TimestampSetup { get; set; }

        //Status
        public string State { get; set; }
        public string OtpCRC { get; set; }
        public string Com { get; set; }
        public string Error_ce { get; set; } //Communication error
        public string Error_los { get; set; } //open/short
        public string Error_ot { get; set; } // over Temperature
        public string Error_uv { get; set; } // under Voltage
        public string TimestampStatus { get; set; }

        //LedState
        public string RO { get; set; }
        public string GO { get; set; }
        public string BO { get; set; }
        public string RS { get; set; }
        public string GS { get; set; }
        public string BS { get; set; }
        public string TimestampLedState { get; set; }

        //OTTH
        public byte OtLowValue { get; set; }
        public byte OtHighValue { get; set; }
        public byte OrCycle { get; set; }
        public string TimestampOtth { get; set; }

        //OTP
        public ushort ChipId { get; set; }
        public byte WaverId { get; set; }

        public byte RedDayU { get; set; }
        public byte RedDayV { get; set; }
        public ushort RedDayLv { get; set; }
        public byte RedNightU { get; set; }
        public byte RedNightV { get; set; }
        public ushort RedNightLv { get; set; }

        public byte GreenDayU { get; set; }
        public byte GreenDayV { get; set; }
        public ushort GreenDayLv { get; set; }
        public byte GreenNightU { get; set; }
        public byte GreenNightV { get; set; }
        public ushort GreenNightLv { get; set; }

        public byte BlueDayU { get; set; }
        public byte BlueDayV { get; set; }
        public ushort BlueDayLv { get; set; }
        public byte BlueNightU { get; set; }
        public byte BlueNightV { get; set; }
        public ushort BlueNightLv { get; set; }






        private long LastUpdated { get; set; } // Indicates when the data of this LED was last refreshed

        public LED() { }

        public void SetOTP(ref Message msg)
        {
            ChipId = BitConverter.ToUInt16(msg.OTP, 0);
            WaverId = (byte)((msg.OTP[3]) & 0b00001111) ;
            
            RedNightU = msg.OTP[10];
            RedNightV = msg.OTP[11];
            RedNightLv = (ushort)(msg.OTP[12] | ((msg.OTP[13] & 0x0F) << 8));
            RedDayU = msg.OTP[14];
            RedDayV = msg.OTP[15];
            RedDayLv = (ushort)(msg.OTP[16] | ((msg.OTP[13] & 0xF0) << 8));

            BlueNightU = msg.OTP[17];
            BlueNightV = msg.OTP[18];
            BlueNightLv = (ushort)(msg.OTP[19] | ((msg.OTP[20] & 0x0F) << 8));
            BlueDayU = msg.OTP[21];
            BlueDayV = msg.OTP[22];
            BlueDayLv = (ushort)(msg.OTP[23] | ((msg.OTP[20] & 0xF0) << 8));

            GreenNightU = msg.OTP[24];
            GreenNightV = msg.OTP[25];
            GreenNightLv = (ushort)(msg.OTP[26] | ((msg.OTP[27] & 0x0F) << 8));
            GreenDayU = msg.OTP[28];
            GreenDayV = msg.OTP[29];
            GreenDayLv = (ushort)(msg.OTP[30] | ((msg.OTP[26] & 0xF0) << 8));

        }

        public void SetPWM(ref Message msg)
        {
            PwmRed = msg.CurrentRed + "|" + (msg.PwmRed |= (ushort)((0) << 15));
            PwmGreen = msg.CurrentGreen + "|" + (msg.PwmGreen |= (ushort)((0) << 15));
            PwmBlue = msg.CurrentBlue + "|" + (msg.PwmBlue |= (ushort)((0) << 15));
            TimeStampPwm = DateTime.Now.ToString("HH:mm:ss");
        }

        public void SetComStats(ref Message msg)
        {
            Cs_SIO1 = (byte)((msg.ComST) & 0b00000011) switch
            {
                0 => "LVDS",
                1 => "EOL",
                2 => "MCU",
                3 => "CAN",
                _ => "default",
            };
            Cs_SIO2 = (byte)(((msg.ComST) & 0b00001100) >> 2) switch
            {
                0 => "LVDS",
                1 => "EOL",
                2 => "MCU",
                3 => "CAN",
                _ => "default",
            };
            TimeStampComstats = DateTime.Now.ToString("HH:mm:ss");
        }

        public void SetLedStatus(ref Message msg)
        {
            BS = (byte)((msg.Status) & 0b00000001) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            GS = (byte)(((msg.Status) & 0b00000010) >> 1) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            RS = (byte)(((msg.Status) & 0b00000100) >> 2) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            BO = (byte)(((msg.Status) & 0b00010000) >> 4) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            GO = (byte)(((msg.Status) & 0b00100000) >> 5) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            RO = (byte)(((msg.Status) & 0b01000000) >> 6) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            TimestampStatus = DateTime.Now.ToString("HH:mm:ss");
        }

        public void SetTempSt(ref Message msg)
        {
            SetTemp(ref msg);
            SetStatus(ref msg);
        }

        public void SetTemp(ref Message msg)
        {
            Temperature = (byte)(msg.Temperature - 113);
            TimeStampTemp = DateTime.Now.ToString("HH:mm:ss");

        }

        public void SetOtth(ref Message msg)
        {
            OtLowValue = (byte)(msg.OTTH.ElementAt(0) - 113);
            OtHighValue = (byte)(msg.OTTH.ElementAt(1) - 113);
            State = (byte)((msg.OTTH.ElementAt(2)) & 0b00000011) switch
            {
                0 => "1 CYCLE",
                1 => "2 CYCLE",
                2 => "3 CYCLE",
                3 => "4 CYCLE",
                _ => "default",
            };
            TimeStampTemp = DateTime.Now.ToString("HH:mm:ss");
        }

        public void SetSetup(ref Message msg)
        {
            PWM_F = (byte)(((msg.Setup) & 0b10000000) >> 7) switch
            {
                0 => "586 Hz / 15 bit",
                1 => "1172 Hz / 14 bit",
                _ => "default",
            };
            CLK_INV = (byte)(((msg.Setup) & 0b01000000) >> 6) switch
            {
                0 => "HIGH",
                1 => "LOW",
                _ => "default",
            };
            CRC_EN = (byte)(((msg.Setup) & 0b00100000) >> 5) switch
             {
                 0 => "DISABLED",
                 1 => "ENABLED",
                 _ => "default",
             };
            TEMPCLK = (byte)(((msg.Setup) & 0b00010000) >> 4) switch
            {
                0 => "19.2 kHz",
                1 => "2.4 kHz",
                _ => "default",
            };
            CE_FSAVE = (byte)(((msg.Setup) & 0b0001000) >> 3) switch
            {
                0 => "RAISE",
                1 => "RAISE & SLEEP",
                _ => "default",
            };
            LOS_FSAVE = (byte)(((msg.Setup) & 0b00000100) >> 2) switch
            {
                0 => "RAISE",
                1 => "RAISE & SLEEP",
                _ => "default",
            };
            OT_FSAVE = (byte)(((msg.Setup) & 0b00000010) >> 1) switch
            {
                0 => "RAISE",
                1 => "RAISE & SLEEP",
                _ => "default",
            };
            UV_FSAVE = (byte)((msg.Setup) & 0b00000001) switch
            {
                0 => "RAISE",
                1 => "RAISE & SLEEP",
                _ => "default",
            };
            TimestampSetup = DateTime.Now.ToString("HH:mm:ss");
        }

        public void SetStatus(ref Message msg)
        {
            State = (byte)(((msg.Status) & 0b11000000) >> 6) switch
            {
                0 => "UNINITIALIZED",
                1 => "SLEEP",
                2 => "ACTIVE",
                3 => "DEEPSLEEP",
                _ => "default",
            };
            OtpCRC = (byte)(((msg.Status) & 0b00100000) >> 5) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            Com = (byte)(((msg.Status) & 0b00001000) >> 4) switch
            {
                0 => "BIDIRECTIONAL",
                1 => "LOOP-BACK",
                _ => "default",
            };
            Error_ce = (byte)(((msg.Status) & 0b00001000) >> 3) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            Error_los = (byte)(((msg.Status) & 0b00000100) >> 2) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            Error_ot = (byte)(((msg.Status) & 0b00000010) >> 1) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            Error_uv = (byte)((msg.Status) & 0b00000001) switch
            {
                0 => "CLEAR",
                1 => "ERROR",
                _ => "default",
            };
            TimestampStatus = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
