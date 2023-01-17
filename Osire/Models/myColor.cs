using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osire.Models
{
    internal class myColor
    {
        public ushort U { get; set; }
        public ushort V { get; set; }
        public ushort Lv { get; set; }

        public myColor(ushort U, ushort V, ushort Lv) 
        {
            this.U = U;
            this.V = V;
            this.Lv= Lv;
        }

    }
}
