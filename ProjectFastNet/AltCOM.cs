using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace ProjectFastNet
{
    class AltCOM
    {

        //Send commands through this command (commandIn) for the full packet
        public static void genCom(SerialPort output, byte[] commandIn)
        {
            byte[] fullStr = new byte[270];
            fullStr[0] = 0xFE;
            for (int i=0;i<commandIn.Length;i++)
            {
                fullStr[i + 1] = commandIn[i];
            }
            fullStr[commandIn.Length + 1] = FCSgenerate(commandIn);
            output.Write(fullStr,0,commandIn.Length+2);
        }

        //Commands in this class output the GFF (General Format Frame) unless otherwise specified
        //Data is NOT transmitted from this class outside of the genCom method, it just formats the strings to be transmitted

        //Register the PC with the ALT5801 device (MUST call to initialize)
        public static byte[] ZB_APP_REGISTER(byte ep, ushort profID, ushort devID, byte devVer, byte inputs, byte[] cmdIn, byte outputs, byte[] cmdOut)
        {
            byte[] outputStr = new byte[(byte)(12+ (2 * inputs) + (2 * outputs))];
            outputStr[0] = (byte)(9 + (2 * inputs) + (2 * outputs));
            outputStr[1] = 0x26;
            outputStr[2] = 0x0A;
            outputStr[3] = ep;
            outputStr[4] = (byte)profID;
            outputStr[5] = (byte)(profID >> 8);
            outputStr[6] = (byte)devID;
            outputStr[7] = (byte)(devID >> 8);
            outputStr[8] = devVer;
            outputStr[9] = 0;
            outputStr[10] = inputs;
            for (int i=0;i<cmdIn.Length;i++)
            {
                outputStr[i + 11] = cmdIn[i];
            }
            outputStr[cmdIn.Length + 11] = outputs;
            for (int i=0;i<cmdOut.Length;i++)
            {
                outputStr[i + 12 + cmdIn.Length] = cmdOut[i];
            }
            return outputStr;
        }

        //Sends the reset signal to the ALT5801
        public static byte[] SYS_RESET()
        {
            byte[] outputStr = new byte[] { 1,0x41,0x00,0x00};
            return outputStr;
        }

        //Write the configuration details to the ALT5801
        public static byte[] ZB_WRITE_CFG(byte cfgID, byte[] value)
        {
            byte[] outputStr = new byte[5 + value.Length];
            outputStr[0] = (byte)(2 + value.Length);
            outputStr[1] = 0x26;
            outputStr[2] = 0x05;
            outputStr[3] = cfgID;
            outputStr[4] = (byte)value.Length;
            for (int i=0;i<(value.Length);i++)
            {
                outputStr[i + 5] = value[i];
            }
            return outputStr;
        }

        public static byte[] ZB_READ_CFG(byte cfgID)
        {
            byte[] outputStr = new byte[] { 1, 0x26, 0x04, cfgID };
            return outputStr;
        }

        //Starts the Zigbee stack in the device
        public static byte[] ZB_START_REQ()
        {
            byte[] outputStr = new byte[] { 0x00, 0x26, 0x00 };
            return outputStr;
        }

        //Send the data to the network
        /* DEST = 0-0xFFF7 - Send to the device with that address
         * DEST = 0xFFFC - Send to all routers and coordinator
         * DEST = 0xFFFD - Send to all devices with receiver turned on
         * DEST = 0xFFFE - Binding address (don't use)
         * DEST = 0xFFFF - Broadcast group of all devices in the network
         * CMD - Command ID to send with the message
         * HANDLE - A handle used to identify the send data request
         * ACK - TRUE if requesting ack from the destination
         * DATA - The data wanting to be sent
         */
        public static byte[] ZB_SEND_DATA(ushort dest, ushort cmd, byte handle, byte ack, byte radius, byte[] data)
        {
            byte[] outputStr = new byte[11 + data.Length];
            outputStr[0] = (byte)(8 + data.Length);
            outputStr[1] = 0x26;
            outputStr[2] = 0x03;
            outputStr[3] = (byte)dest;
            outputStr[4] = (byte)(dest >> 8);
            outputStr[5] = (byte)cmd;
            outputStr[6] = (byte)(cmd >> 8);
            outputStr[7] = handle;
            outputStr[8] = ack;
            outputStr[9] = radius;
            outputStr[10] = (byte)(data.Length);
            for (int i=0;i<data.Length;i++)
            {
                outputStr[11 + i] = data[i];
            }
            return outputStr;
        }

        public static byte[] ZB_GET_INFO(byte param)
        {
            byte[] outputStr = new byte[] { 1, 0x26, 0x06, param };
            return outputStr;
        }

        public static byte[] ZB_PERMIT_JOINING_REQUEST(ushort destination,byte timeout)
        {
            return new byte[] { 0x03, 0x26, 0x08, (byte)destination, (byte)(destination >> 8), timeout };
        }

        //Sets up the radio for testing, 0xFF is the max txPower
        //TESTMODE = 0 - Transmit unmodulated carrier with spcified frequency
        //TESTMODE = 1 - Transmit psudo-random data with specified frequency
        //TESTMODE = 2 - Set to receive mode on specified frequency
        public static byte[] SYS_TEST_RF(byte testMode, ushort frequency, byte txPower)
        {
            byte[] outputStr = new byte[] { 4, 0x41, 0x40, testMode, (byte)frequency, (byte)(frequency >> 8), txPower };
            return outputStr;
        }

        //Generate the FCS byte to confirm the end of the data string
        public static byte FCSgenerate(byte[] input)
        {
            byte result = 0;
            for (int i = 0; i < input.Length; i++)
            {
                result ^= input[i];
            }
            return result;
        }
    }
}
