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
        public const byte channel = 6;
        public const byte PANID = 0x34;

        public static bool runTranceiver = true;
        public static SerialPort wirelessIn;
        public static SerialPort GPSin;
        public static Thread receiveThread;
        public static Thread transmitThread;
        public static Thread GPSThread;
        public static byte[] rxPacket = new byte[259];
        public static String deviceData;
        public static String[] receivedData = new String[100];
        public static String[] devLookup = new String[100];
        public static bool[] devRefresh = new bool[100];
        public static int[] timeToRefresh = new int[100];
        public static String testStr = "27E9,3747.8690,N,08025.9719,W,23,26,14";
        public static String noSignalStr = "NO GPS DATA";
        public static int devIndex = 0;
        public static ushort devID;

        static void Main(string[] args)
        {
            //Generate the device ID of this end
            Random devIDgen = new Random();
            devID = (ushort)devIDgen.Next(0x0000, 0xFFFF);
            deviceData = String.Format("{0:X}", devID) + ","+noSignalStr;
            //Initial GPS data read test
            Console.WriteLine("Enter the port number for the GPS below");           //Get the port number from the user
            String portNumber = Console.ReadLine();
            GPSin = new SerialPort(portNumber, 4800, Parity.None, 8);               //Initialize the serial port for the GPS 
            GPSin.Open();                                                           //Open the port to communicate with the GPS
            Console.WriteLine("Enter the port number for the ALT5801 Wireless transceiver below");
            portNumber = Console.ReadLine();                                        //Get the Zigbee COM port from the user
            wirelessIn = new SerialPort(portNumber, 115200, Parity.None, 8, StopBits.One);
            wirelessIn.RtsEnable = true;                                            //RTS/DTR must be enabled for communication with the ALT5801
            wirelessIn.DtrEnable = false;
            wirelessIn.Open();                                                      //Open the ALT5801 port

            //Start the recieve thread. This thread will wait for an input from the wireless module and process the input into local variables when received
            receiveThread = new Thread(Program.wirelessReceive);
            transmitThread = new Thread(Program.wirelessTransmit);
            //Start the GPS thread. This will read data from the GPS module and sort it into local variables until needed
            GPSThread = new Thread(Program.GPSupdate);

            receiveThread.Start();
            GPSThread.Start();

            Console.WriteLine("Enter 'COORD' for Coordinator");
            String coordDec = Console.ReadLine();
            if (String.Equals("COORD",coordDec))
            {
                initALT5801(true);
            } else {
                initALT5801(false);
            }

            transmitThread.Start();

            /*while (true)
            {
                String getMessage = Console.ReadLine();

                AltCOM.genCom(wirelessIn, AltCOM.ZB_SEND_DATA(0xFFFD, 2, 0, 1, 1, Encoding.ASCII.GetBytes(getMessage)));
                while (!AltGET.isMessage(rxPacket, mDef.ZB_SEND_DATA_RSP)) ;
                while (!AltGET.isMessage(rxPacket, mDef.ZB_SEND_DATA_CONFIRM)) ;
                Console.WriteLine("Data send Confirmed!");
            }*/

            while (true)                                                              
            {
                Console.Clear();
                Console.WriteLine("Active Device: ");
                Console.WriteLine(deviceData);
                Console.WriteLine();
                for (int i=0;i<devIndex;i++)
                {
                    if(devRefresh[i] == true)
                    {
                        timeToRefresh[i] = 0;
                        devRefresh[i] = false;
                    } else
                    {
                        timeToRefresh[i]++;
                    }
                    if (timeToRefresh[i] < 12)
                    {
                        Console.WriteLine("Mesh Device {0}", i);
                        Console.WriteLine(receivedData[i]);
                        Console.WriteLine();
                    }
                }
                Thread.Sleep(250);
            }
        }

        public static void wirelessTransmit()
        {
            Console.WriteLine("Transmit Thread Started");
            while (Program.runTranceiver)
            {
                Thread.Sleep(800);
                AltCOM.genCom(wirelessIn, AltCOM.ZB_SEND_DATA(0xFFFD, 2, 0, 1, 1, Encoding.UTF8.GetBytes(deviceData)));
                while (!AltGET.isMessage(rxPacket, mDef.ZB_SEND_DATA_RSP)) ;
                while (!AltGET.isMessage(rxPacket, mDef.ZB_SEND_DATA_CONFIRM)) ;
            }
        }
        
        //This function runs in a separate thread and handles all receiving and processisng of the received data
        public static void wirelessReceive()
        {
            Console.WriteLine("Receive Thread Started");
            while (Program.runTranceiver)
            {
                rxPacket = AltGET.getPacket(wirelessIn);

                /*Console.WriteLine("Packet Received");                             //Uncomment this to print all incoming transmissions to the console for debugging
                for (int i=0;i<rxPacket.Length;i++)
                {
                    Console.Write("{0} ", String.Format("{0:X}", rxPacket[i]));
                }
                Console.WriteLine(" ");*/

                if (AltGET.isMessage(rxPacket, 0x4687))
                {
                    String newDat = Encoding.ASCII.GetString(AltGET.getRx(rxPacket));           //Store the packet locally
                    String[] cotSet = newDat.Split(',');
                                                                    //Look up the devID in the index and see if it's already been entered
                    int wasEntered = -1;
                    for (int i=0;i<devIndex;i++)
                    {
                        if (devLookup[i]==cotSet[0])
                        {
                            wasEntered = i;
                        }
                    }
                    if (wasEntered!=-1)                             //If it's already registered, override the current data
                    {
                        receivedData[wasEntered] = newDat;
                        devRefresh[wasEntered] = true;
                    } else
                    {                                               //If not, register a new entry in the table and increase the index
                        devIndex++;
                        devLookup[devIndex] = cotSet[0];
                        receivedData[devIndex] = newDat;
                        devRefresh[devIndex] = true;
                    }
                }
            }
        }
        //This function runs in a separate thread and handles processing and parsing of the GPS data
        public static void GPSupdate()
        {
            Console.WriteLine("GPS Processing Thread Started");
            while (Program.runTranceiver)
            {
                do
                {
                    String GPSinData = GPSin.ReadLine();
                    ParseGPS.parseNMEAstring(GPSinData);
                    if (ParseGPS.getCommand() == 0)
                    {
                        //Console.Clear();
                        if (ParseGPS.findSignal())
                        {
                            float[] latlog = ParseGPS.getCoordinates();
                            char[] Compass = ParseGPS.getCompass();
                            //Console.WriteLine("Latitude - " + latlog[0] + " " + Compass[0] + " Longitude - " + latlog[1] + " " + Compass[1]);
                            List<String> TimeList = ParseGPS.getTime();
                            String[] assemStr = new String[] { "0x", String.Format("{0:X}", devID), ",", ParseGPS.NMEAstring[2], ",", Compass[0].ToString(), ",", ParseGPS.NMEAstring[4], ",", Compass[1].ToString(), ",", TimeList[0], ",", TimeList[1], ",", TimeList[2].Substring(0,3) };
                            deviceData = String.Join("", assemStr);
                            //Console.WriteLine("Time: " + TimeList[0] + ":" + TimeList[1] + ":" + TimeList[2]);
                        }
                        else
                        {
                            deviceData = ("0x"+String.Format("{0:X}", devID)+","+noSignalStr);
                            //Console.WriteLine("No Signal Found");
                        }
                    }
                } while (ParseGPS.getCommand() != 0);
            }
        }

        public static void initALT5801(bool isCoordinator)
        {
            //Run the full initialization procedure for the ALT5801
            AltCOM.genCom(wirelessIn, AltCOM.ZB_WRITE_CFG(0x03, new byte[1] { 3 }));
            while (!AltGET.isMessage(rxPacket, mDef.ZB_WRITE_CFG_RSP)) ;
            Console.WriteLine("Config Reset");
            AltCOM.genCom(wirelessIn, AltCOM.SYS_RESET());
            while (!AltGET.isMessage(rxPacket, mDef.SYS_RESET_IND)) ;
            Console.WriteLine("Device Reset");
            AltCOM.genCom(wirelessIn, AltCOM.ZB_WRITE_CFG(0x83, new byte[2] { 0x12, PANID }));
            while (!AltGET.isMessage(rxPacket, mDef.ZB_WRITE_CFG_RSP)) ;
            Console.WriteLine("Set PANID of the device");
            AltCOM.genCom(wirelessIn, AltCOM.ZB_WRITE_CFG(0x84, new byte[4] { 0, 0, 0, channel }));
            while (!AltGET.isMessage(rxPacket, mDef.ZB_WRITE_CFG_RSP)) ;
            Console.WriteLine("Set the channel of the device to " + channel);
            AltCOM.genCom(wirelessIn, AltCOM.ZB_READ_CFG(0x84));
            while (!AltGET.isMessage(rxPacket, mDef.ZB_READ_CFG_RSP)) ;
            Console.WriteLine("//////CHANGES CONFIRMED//////");
            /*////////////COPY PASTABLE DEBUG BLOCK FOR PRINTING THE SERIAL TERMINAL/////////
            while (true)
            {
                string hexOutput = String.Format("{0:X}", wirelessIn.ReadByte());
                Console.Write("{0} ", hexOutput);
            }
            ///////////////////////////////////////////////////////////////////////////////*/
            if (isCoordinator)
            {
                Console.WriteLine("Configuring Coordinator");
                byte[] cmd_in = new byte[2] { 0x01, 0x00 };
                byte[] cmd_out = { 0x02, 0x00 };
                AltCOM.genCom(wirelessIn, AltCOM.ZB_APP_REGISTER(1, 1, 1, 0, 1, cmd_in, 1, cmd_out));
                AltCOM.genCom(wirelessIn, AltCOM.ZB_START_REQ());
                while (!AltGET.isMessage(rxPacket, mDef.ZB_START_REQUEST_RSP)) ;
                while (!AltGET.isMessage(rxPacket, mDef.ZB_START_CONFIRM)) ;
                Console.WriteLine("Configured Coordinator");
            }
            else
            {
                Console.WriteLine("Configuring Router");

                AltCOM.genCom(wirelessIn, AltCOM.ZB_WRITE_CFG(0x87, new byte[1] { 1 }));
                while (!AltGET.isMessage(rxPacket, mDef.ZB_WRITE_CFG_RSP)) ;
                Console.WriteLine("Configured Device Type");

                AltCOM.genCom(wirelessIn, AltCOM.SYS_RESET());
                while (!AltGET.isMessage(rxPacket, mDef.SYS_RESET_IND)) ;

                AltCOM.genCom(wirelessIn, AltCOM.ZB_READ_CFG(0x87));
                while (!AltGET.isMessage(rxPacket, mDef.ZB_READ_CFG_RSP)) ;

                byte[] cmd_in = new byte[2] { 0x02, 0x00 };
                byte[] cmd_out = { 0x01, 0x00 };
                AltCOM.genCom(wirelessIn, AltCOM.ZB_APP_REGISTER(1, 1, 0, 0, 1, cmd_in, 1, cmd_out));
                while (!AltGET.isMessage(rxPacket, mDef.ZB_REGISTER_RSP)) ;
                AltCOM.genCom(wirelessIn, AltCOM.ZB_START_REQ());
                while (!AltGET.isMessage(rxPacket, mDef.ZB_START_REQUEST_RSP)) ;
                while (!AltGET.isMessage(rxPacket, mDef.ZB_START_CONFIRM)) ;
                Console.WriteLine("Configured Router");
            }
        }
    } 
}
