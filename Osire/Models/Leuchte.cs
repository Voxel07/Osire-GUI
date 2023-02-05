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
        public ushort SelectedLed { get; set; }
        //Configuration
        public bool IpSet { get; set; }
        public bool LedCountSet { get; set; }
        public bool LedsWereReset { get; set; }
        public bool LedsWereInitialized { get; set; }
        public bool LedsAreActive { get; set; }
        public Leuchte() 
        {
            IpSet = false;
            LedCountSet = false;
           //this.LedCount = LedCount;
           // LEDs = Enumerable.Repeat(new LED(), LedCount).ToList();
        }
        public void SetIp(string ip)
        {
            if(connection != null)
            {
                connection.Dispose();
            }
            connection = new UDP(ip);

        }

        public void SetLeds(ushort cnt)
        {
            this.LEDs = new List<LED>(cnt);
            for (int i = 0; i < cnt; i++)
            {
                LEDs.Add(new LED()); 
            }
            this.LedCount= cnt;
        }
        
        public void InitLeds()
        {
            for (int i = 0; i < this.LedCount; i++)
            {
                LEDs[i].Address= (ushort)(i+1);
            }
        }

        public void updateLED(UInt16 address)
        {
            //
            //LEDs.ElementAt(address).setSetup;

        }
    }
}
