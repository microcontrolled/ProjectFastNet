using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ProjectFastNet
{
    class Program
    {
        static void Main(string[] args)
        {
            //Initial GPS data read test
            Console.WriteLine("Enter the port number below");                       //Get the port number from the user
            String portNumber = Console.ReadLine();
            SerialPort GPSin = new SerialPort(portNumber, 4800, Parity.None, 8);    //Initialize the serial port for the GPS 
            GPSin.Open();                                                           //Open the port to communicate with the GPS
            for (int i = 0; i < 20; i++)                                            //Read 20 strings of data    
            {
                String GPSinData = GPSin.ReadLine();
                Console.WriteLine(GPSinData);
            }

            String conIN = Console.ReadLine();                                      //Wait for user input before closing the window
        }
    }
}
