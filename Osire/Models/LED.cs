using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osire.Models
{
    internal class LED
    {
        public UInt16 Address { get; set; }
        private UInt16 Temperature { get; set; }
        
        private UInt16 PwmRed { get; set; }
        private UInt16 PwmGreen { get; set; }
        private UInt16 PwmBlue { get; set; }

        //ComStats
        private string Cs_SIO1 { get; set; }
        private string Cs_SIO2 { get; set; }

        //Setup
        private string Setup_pwmFrequency { get; set; }
        private bool Setup_crc { get; set; }
        private float Setup_tempFrequency { get; set; }
        private bool Setup_clk { get; set; }

        //Error handeling
        private string Error_ce { get; set; } //Communication error
        private string Error_los { get; set; } //open/short
        private string Error_ot { get; set; } // over Temperature
        private string Error_uv { get; set; } // under Voltage

        private long LastUpdated { get; set; } // Indicates when the data of this LED was last refreshed

        public LED() { }

        public void setPWM(UInt16 red, UInt16 green, UInt16 blue)
        {
            PwmRed= red;
            PwmGreen= green;
            PwmBlue= blue;
        }

        public void setComStats(string SIO1, String SIO2)
        {
            Cs_SIO1= SIO1;
            Cs_SIO2= SIO2;
        }

        public void setSetup(String pwmF, bool crc, float tempf, bool clk)
        {
            Setup_pwmFrequency= pwmF;
            Setup_crc= crc;
            Setup_tempFrequency= tempf;
            Setup_clk= clk;
        }

        public void setErrorhandeling(string ce, string los, string ot, string uv)
        {
            Error_ce= ce;
            Error_los= los;
            Error_ot= ot;
            Error_uv= uv;
        }
    }
}
