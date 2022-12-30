using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Osire.Models
{
    internal class ComPort
    {
        public string Name { get; set; }
        public string Description { get; set; } 
        public int Port { get; set; }
        

        public static List<string> GetComPorts()
        {
            List<string> portNames = new List<string>();

            try
            {
                portNames = SerialPort.GetPortNames().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while trying to get the list of available COM ports: " + ex.Message);
            }

            foreach (string portName in portNames)
            {
                Console.WriteLine(portName);
            }

            return portNames;
        }

    }
}
