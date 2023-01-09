using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osire.Models
{
    internal class Leuchte
    {

        public UInt16 LedCount { get; set; }
        public List<LED> LEDs { get; set; }
        public UDP connection;
        public UART connection2 = new UART();

        //Configuration
        public bool IpSet { get; set; }
        public bool LedCountSet { get; set; }
        public bool LedsWereReset { get; set; }
        public bool LedsWereInitialized { get; set; }

        public Leuchte() 
        {
            IpSet = false;
            LedCountSet = false;
           //this.LedCount = LedCount;
           // LEDs = Enumerable.Repeat(new LED(), LedCount).ToList();
        }
        public void SetIp(string ip)
        {
            connection = new UDP(ip);
        }

        public void SetLeds(ushort cnt)
        {
            this.LEDs = Enumerable.Repeat(new LED(), cnt).ToList();
            this.LedCount= cnt;
        }
        
        public bool InitLeds(ushort cnt)
        {
            if(this.LedCount < cnt)
            {
                return false;
            }
            for (int i = 0; i < cnt; i++)
            {
                LEDs[i].Address= (ushort)(i+1);
            }
            return true;
        }

        public void updateLED(UInt16 address)
        {
            //
            //LEDs.ElementAt(address).setSetup;

        }
    }
}
