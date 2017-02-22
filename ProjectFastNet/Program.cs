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
            Console.WriteLine("Enter the port number for the GPS below");           //Get the port number from the user
            String portNumber = Console.ReadLine();
            SerialPort GPSin = new SerialPort(portNumber, 4800, Parity.None, 8);    //Initialize the serial port for the GPS 
            GPSin.Open();                                                           //Open the port to communicate with the GPS
            while (Console.ReadLine)                           //Read data on enter until the user enters 'q'   
            {
                do {
                    String GPSinData = GPSin.ReadLine();
                    ParseGPS.parseNMEAstring(GPSinData);
                    if (ParseGPS.getCommand() == 0)
                    {
                        Console.Clear();
                        if (ParseGPS.findSignal())
                        {
                            Console.WriteLine("Signal Found");
                            float[] latlog = ParseGPS.getCoordinates();
                            Console.WriteLine("Latitude - ", latlog[0], " Longitude - ", latlog[1]);
                        } else
                        {
                            Console.WriteLine("No Signal Found"); }

                    }
                } while (ParseGPS.getCommand() != 0);
            }
        }
    }
}
