﻿using System;
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
            byte[] dataPacket = new byte[259];
            while (dataPacket[0]!=0xFE)                            //Wait for the data packet to start
            {
                while (rxPort.BytesToRead == 0);
                dataPacket[0] = (byte)rxPort.ReadByte();
            }
            while (rxPort.BytesToRead == 0) ;               //Get the length of the packet
            dataPacket[1] = (byte)rxPort.ReadByte();
            for (int i=2;i<(dataPacket[1]+3);i++)                  //Snag all the bytes in the packet, store in the packet buffer
            {
                while (rxPort.BytesToRead == 0) ;
                dataPacket[i] = (byte)rxPort.ReadByte();
            }
            return dataPacket;                              //Return the unprocessed packet
        }
        
        //If the packet is a return message, parses the message and returns the received string
        public static String getRx(byte[] packet)
        {
            if (isMessage(packet,0x4687))
            {
                byte[] outputField = new byte[packet[1] - 8];
                for (int i=0;i<(packet[1]-9);i++)
                {
                    outputField[i] = packet[i + 6];
                }
                return Encoding.ASCII.GetString(outputField);
            }
            return "";
        }

        //See if the return packet is the response expected
        public static bool isMessage(byte[] packet, ushort message)
        {
            if (packet.Length>3)
            {
                ushort compVal = BitConverter.ToUInt16(new byte[2] { packet[2], packet[3] }, 0);
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