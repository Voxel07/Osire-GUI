using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osire.Models
{
    internal class Leuchte
    {
        public UInt16 LedCount;
        public List<LED> LEDs { get; set; }
        public UDP connection;

        public Leuchte(UInt16 LedCount) 
        {
            this.LedCount = LedCount;
            LEDs = Enumerable.Repeat(new LED(), LedCount).ToList();
        }

        public void updateLED(UInt16 address)
        {
            //
            //LEDs.ElementAt(address).setSetup;

        }
    }
}
