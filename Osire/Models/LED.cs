using CommunityToolkit.Mvvm.ComponentModel;
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
        
        public int Temperature { get; set; }
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

        public double RedDayU { get; set; }
        public double RedDayV { get; set; }
        public double RedDayLv { get; set; }
        public double RedNightU { get; set; }
        public double RedNightV { get; set; }
        public double RedNightLv { get; set; }

        public double GreenDayU { get; set; }
        public double GreenDayV { get; set; }
        public double GreenDayLv { get; set; }
        public double GreenNightU { get; set; }
        public double GreenNightV { get; set; }
        public double GreenNightLv { get; set; }

        public double BlueDayU { get; set; }
        public double BlueDayV { get; set; }
        public double BlueDayLv { get; set; }
        public double BlueNightU { get; set; }
        public double BlueNightV { get; set; }
        public double BlueNightLv { get; set; }

        public double[] tempConstants = new double[]
        {   //a         //b
            1.030E-05, -7.397E-03,  //Red|Night|L
            -1.653E-06, 4.029E-04,  //Red|Night|u
            2.567E-07, -6.984E-05,  //Red|Night|v
            -5.864E-07, -2.306E-03, //Green|Night|L
            4.555E-06, 2.081E-03,   //Green|Night|u
            -8.166E-08, -1.921E-05, //Green|Night|v
            -2.679E-06, 7.200E-04,  //Blue|Night|L
            -1.060E-06, -6.739E-04, //Blue|Night|u
            5.774E-06, 2.434E-03,   //Blue|Night|v
    
            1.122E-05, -7.808E-03,  //Red|Day|L
            -1.466E-06, 3.727E-04,  //Red|Day|u
            2.475E-07, -6.384E-05,  //Red|Day|v
            -5.946E-06, -1.813E-03, //Green|Day|L
            5.878E-06, 2.241E-03,   //Green|Day|u
            -1.897E-07, 1.981E-05,  //Green|Day|v
            -2.226E-06, 7.734E-04,  //Blue|Day|L
            -1.036E-06, -6.427E-04, //Blue|Day|u
            6.519E-06, 2.423E-03    //Blue|Day|v
        };
        public double[] calibrationConstatns = new double[]
        { //L   //U,V
          0.44, 0.0025, // Night
          1.25, 0.0025 // Day
        };

        public LED() { }

        public void SetOTP(ref Message msg)
        {
            ChipId = BitConverter.ToUInt16(msg.OTP, 0);
            WaverId = (byte)((msg.OTP[3]) & 0b00001111) ;

            RedNightU = Math.Ceiling( msg.OTP[10] * calibrationConstatns[3] * 100000);
            RedNightV = Math.Ceiling( msg.OTP[11]*calibrationConstatns[3] * 100000);
            RedNightLv = Math.Ceiling( (msg.OTP[12] | ((msg.OTP[13] & 0x0F) << 8)) * calibrationConstatns[2] * 4);

            RedDayU = Math.Ceiling( msg.OTP[14]*calibrationConstatns[3] * 100000);
            RedDayV = Math.Ceiling( msg.OTP[15]*calibrationConstatns[3] * 100000);
            RedDayLv = Math.Ceiling( (msg.OTP[16] | ((msg.OTP[13] & 0xF0) << 4))*calibrationConstatns[2] * 4);

            BlueNightU = Math.Ceiling( msg.OTP[17]*calibrationConstatns[3] * 100000);
            BlueNightV = Math.Ceiling( msg.OTP[18]*calibrationConstatns[3] * 100000);
            BlueNightLv = Math.Ceiling( (msg.OTP[19] | ((msg.OTP[20] & 0x0F) << 8))*calibrationConstatns[2] * 4);

            BlueDayU = Math.Ceiling( msg.OTP[21]*calibrationConstatns[3] * 100000);
            BlueDayV = Math.Ceiling( msg.OTP[22]*calibrationConstatns[3] * 100000);
            BlueDayLv = Math.Ceiling( (msg.OTP[23] | ((msg.OTP[20] & 0xF0) << 4))*calibrationConstatns[2] * 4);

            GreenNightU = Math.Ceiling( msg.OTP[24]*calibrationConstatns[3] * 100000);
            GreenNightV = Math.Ceiling( msg.OTP[25]*calibrationConstatns[3] * 100000);
            GreenNightLv = Math.Ceiling( (msg.OTP[26] | ((msg.OTP[27] & 0x0F) << 8))*calibrationConstatns[2] * 4);

            GreenDayU = Math.Ceiling( msg.OTP[28]*calibrationConstatns[3] * 100000);
            GreenDayV = Math.Ceiling( msg.OTP[29]*calibrationConstatns[3] * 100000);
            GreenDayLv = Math.Ceiling( (msg.OTP[30] | ((msg.OTP[27] & 0xF0) << 4))*calibrationConstatns[2] * 4);

            using (StreamWriter writer = new StreamWriter("C:\\Programmieren\\Osire\\UVL.txt", true))
            {
                writer.WriteLine(ChipId);
                writer.WriteLine("--Red--");
                writer.WriteLine(RedDayLv);
                writer.WriteLine(RedDayU);
                writer.WriteLine(RedDayV);
                writer.WriteLine(RedNightLv);
                writer.WriteLine(RedNightU);
                writer.WriteLine(RedNightV);
                writer.WriteLine("--Green--");
                writer.WriteLine(GreenDayLv);
                writer.WriteLine(GreenDayU);
                writer.WriteLine(GreenDayV);
                writer.WriteLine(GreenNightLv);
                writer.WriteLine(GreenNightU);
                writer.WriteLine(GreenNightV);
                writer.WriteLine("--Blue--");

                writer.WriteLine(BlueDayLv);
                writer.WriteLine(BlueDayU);
                writer.WriteLine(BlueDayV);
                writer.WriteLine(BlueNightLv);
                writer.WriteLine(BlueNightU);
                writer.WriteLine(BlueNightV);
                writer.WriteLine("---------------------");
                writer.WriteLine();
            }
        }

        public void SetPWM(ref Message msg)
        {
            PwmRed = (msg.CurrentRed == true ? "50":"10") + "|" + (msg.PwmRed &= 0x7FFF);
            PwmGreen = (msg.CurrentGreen == true ? "50" : "10") + "|" + (msg.PwmGreen &= 0x7FFF);
            PwmBlue = (msg.CurrentBlue == true ? "50" : "10") + "|" + (msg.PwmBlue &= 0x7FFF);
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
            TimestampLedState = DateTime.Now.ToString("HH:mm:ss");
        }

        public void SetTempSt(ref Message msg)
        {
            SetTemp(ref msg);
            SetStatus(ref msg);
        }

        public void SetTemp(ref Message msg)
        {
            Temperature = msg.Temperature;
            TimestampOtth = DateTime.Now.ToString("HH:mm:ss");
        }

        public void SetOtth(ref Message msg)
        {
            OtHighValue = (byte)(msg.OTTH.ElementAt(0) - 113);
            OtLowValue = (byte)(msg.OTTH.ElementAt(1) - 113);
            OrCycle = (byte)(((msg.OTTH.ElementAt(2)) & 0b00000011)+1);
            TimestampOtth = DateTime.Now.ToString("HH:mm:ss");
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
