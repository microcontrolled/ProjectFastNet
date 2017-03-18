using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ProjectFastNet
{
    class AltGET
    {
        //This will wait until a valid packet is received, then it will return the packet as a byte array
        public static byte[] getPacket(SerialPort rxPort)
        {
            while ((byte)rxPort.ReadByte() != 0xFE) ;               //Wait for the data packet to start
           // while (rxPort.BytesToRead == 0) ;                       //Get the length of the packet
            byte packetLength = (byte)rxPort.ReadByte();
            byte[] dataPacket = new byte[packetLength+5];
            dataPacket[0] = 0xFE;
            dataPacket[1] = packetLength;
            for (int i=2;i<(packetLength+5);i++)                   //Snag all the bytes in the packet, store in the packet buffer
            {
                //while (rxPort.BytesToRead == 0) ;
                dataPacket[i] = (byte)rxPort.ReadByte();
            }
            return dataPacket;                                      //Return the unprocessed packet
        }
        
        //If the packet is a return message, parses the message and returns the received string
        public static byte[] getRx(byte[] packet)
        {
            if (isMessage(packet,0x4687))
            {
                byte[] outputField = new byte[packet[1] - 6];
                for (int i=0;i<(packet[1]-6);i++)
                {
                    outputField[i] = packet[i + 10];
                }
                return outputField;
            }
            return new byte[] { };
        }

        //See if the return packet is the response expected
        public static bool isMessage(byte[] packet, ushort message)
        {
            if (packet.Length>3)
            {
                ushort compVal = BitConverter.ToUInt16(new byte[2] { packet[3], packet[2] }, 0);
                //Console.WriteLine("This item is {0}", compVal);
                if (compVal == message) { return true; }
            }
            return false;
        }

        //Compare the FCS value to the measured one and see if the packet is valid
        public static bool isPacketValid(byte[] packet)
        {
            byte[] justGFF = new byte[packet.Length - 2];
            for (int i=1;i<packet.Length-2;i++)             //Isolate the GFF to its own array (not the most efficient way to do this)
            {
                justGFF[i - 1] = packet[i];
            }
            byte outputVal = AltCOM.FCSgenerate(justGFF);     //Calculate the FCS
            if (outputVal==packet[packet.Length-1])
            {
                return true;                                //If the calculated value matches the read value, the packet is valid
            }
            return false;                                   //Otherwise return false
        }
    }
}
