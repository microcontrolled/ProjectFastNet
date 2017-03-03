using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Ports;

namespace ProjectFastNet
{
    class Program
    {
        public static bool runTranceiver = true;
        public static SerialPort wirelessIn;
        public static Thread receiveThread;
        public static Thread transmitThread;

        static void Main(string[] args)
        {
            //Initial GPS data read test
            Console.WriteLine("Enter the port number for the GPS below");           //Get the port number from the user
            String portNumber = Console.ReadLine();
            SerialPort GPSin = new SerialPort(portNumber, 4800, Parity.None, 8);    //Initialize the serial port for the GPS 
            //GPSin.Open();                                                           //Open the port to communicate with the GPS
            Console.WriteLine("Enter the port number for the ALT5801 Wireless transceiver below");
            portNumber = Console.ReadLine();                                        //Get the Zigbee COM port from the user
            wirelessIn = new SerialPort(portNumber, 115200, Parity.None, 8);
            wirelessIn.Open();                                                      //Open the Zigbee port

            //Start the recieve thread. This thread will wait for an input from the wireless module and process the input into local variables when received
            //receiveThread = new Thread(Program.wirelessReceive);
            //Start the transmit thread. This thread will transmit the data packet announcing the node to the entire network
            //transmitThread = new Thread(Program.wirelessTransmit);

            //receiveThread.Start();
            //transmitThread.Start();

            //wirelessIn.WriteLine(0xFE+transmit1+FCSgenerate(transmit1));
            Console.Write(wirelessIn.ReadLine());
            Thread.Sleep(2000);
            while (true)                                                            //Read data on enter until the user enters 'q'   
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
                            char[] Compass = ParseGPS.getCompass();
                            Console.WriteLine("Latitude - "+ latlog[0]+" "+Compass[0]+ " Longitude - "+latlog[1]+" "+Compass[1]);
                            List<String> TimeList = ParseGPS.getTime();
                            Console.WriteLine("Time: "+TimeList[0] + ":" + TimeList[1] + ":" + TimeList[2]);
                        } else
                        {
                            Console.WriteLine("No Signal Found"); }
                    }
                } while (ParseGPS.getCommand() != 0);
            }
        }

        public static void wirelessReceive()
        {
            Console.WriteLine("Receive Thread Started");
            while (Program.runTranceiver)
            {
                String wirelessInput = wirelessIn.ReadLine();                   //Wait for receive from the wireless module
                Console.WriteLine(wirelessInput);                               //Write the output to the console
            }
        }

        public static void wirelessTransmit()
        {
            Console.WriteLine("Transmit Thread Started");
            while (Program.runTranceiver)
            {
                System.Threading.Thread.Sleep(500);                             //Pause the thread for half a second

                wirelessIn.WriteLine("$THIS_IS_A_TEST");                        //Output the callsign
            }
        }
    } 
}
