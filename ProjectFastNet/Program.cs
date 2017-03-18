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
        public static byte[] rxPacket = new byte[259];

        public const byte channel = 6;
        public const byte PANID = 0x34;

        static void Main(string[] args)
        {
            //Initial GPS data read test
            Console.WriteLine("Enter the port number for the GPS below");           //Get the port number from the user
            String portNumber = Console.ReadLine();
            SerialPort GPSin = new SerialPort(portNumber, 4800, Parity.None, 8);    //Initialize the serial port for the GPS 
            //GPSin.Open();                                                           //Open the port to communicate with the GPS
            Console.WriteLine("Enter the port number for the ALT5801 Wireless transceiver below");
            portNumber = Console.ReadLine();                                        //Get the Zigbee COM port from the user
            wirelessIn = new SerialPort(portNumber, 115200, Parity.None, 8, StopBits.One);
            wirelessIn.RtsEnable = true;                                            //RTS/DTR must be enabled for communication with the ALT5801
            wirelessIn.DtrEnable = false;
            wirelessIn.Open();                                                      //Open the ALT5801 port

            //Start the recieve thread. This thread will wait for an input from the wireless module and process the input into local variables when received
            receiveThread = new Thread(Program.wirelessReceive);
            //Start the transmit thread. This thread will transmit the data packet announcing the node to the entire network
            //transmitThread = new Thread(Program.wirelessTransmit);

            receiveThread.Start();
            //transmitThread.Start();

            //AltCOM.genCom(wirelessIn, AltCOM.ZB_READ_CFG(0x87));
            initALT5801(false);     

            //AltCOM.genCom(wirelessIn, AltCOM.ZB_PERMIT_JOINING_REQUEST(0xFFFC, 0xFF));
            //while (!AltGET.isMessage(rxPacket, 0x6608)) ;
            //Console.WriteLine("Permissions Set");

            //AltCOM.genCom(wirelessIn, AltCOM.ZB_READ_CFG(0x87));

            while (true)
            {
                String getMessage = Console.ReadLine();

                AltCOM.genCom(wirelessIn, AltCOM.ZB_SEND_DATA(0xFFFD, 2, 0, 1, 1, Encoding.ASCII.GetBytes(getMessage)));
                while (!AltGET.isMessage(rxPacket, mDef.ZB_SEND_DATA_RSP)) ;
                while (!AltGET.isMessage(rxPacket, mDef.ZB_SEND_DATA_CONFIRM)) ;
                Console.WriteLine("Data send Confirmed!");
            }

            while (false)                                                            //Read data on enter until the user enters 'q'   
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
                rxPacket = AltGET.getPacket(wirelessIn);
                /*Console.WriteLine("Packet Received");                             //Uncomment this to print all incoming transmissions to the console for debugging
                for (int i=0;i<rxPacket.Length;i++)
                {
                    Console.Write("{0} ", String.Format("{0:X}", rxPacket[i]));
                }
                Console.WriteLine(" ");*/
                if (AltGET.isMessage(rxPacket, 0x4687))
                {
                    Console.WriteLine(AltGET.getRx(rxPacket));
                }
            }
        }

        public static void wirelessTransmit()
        {
            Console.WriteLine("Transmit Thread Started");
            while (Program.runTranceiver)
            {
                System.Threading.Thread.Sleep(500);                             //Pause the thread for half a second
                
            }
        }

        public static void initALT5801(bool isCoordinator)
        {
            //Run the full initialization procedure for the ALT5801
            AltCOM.genCom(wirelessIn, AltCOM.ZB_WRITE_CFG(0x03, new byte[1] { 3 }));
            while (!AltGET.isMessage(rxPacket, mDef.ZB_WRITE_CFG_RSP)) ;
            Console.WriteLine("Config Reset");
            //if (!isCoordinator)
            //{
                
            //}
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
