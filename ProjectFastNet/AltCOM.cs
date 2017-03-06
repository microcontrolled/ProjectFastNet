using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFastNet
{
    class AltCOM
    {
        //Commands in this class output the GFF (General Format Frame) unless otherwise specified
        //Data is NOT transmitted from this class, it just formats the strings to be transmitted

        //Register the PC with the ALT5801 device (MUST call to initialize)
        public static String ZB_APP_REGISTER(byte ep, uint profID, uint devID, byte devVer, String cmdIn, String cmdOut)
        {
            byte[] outputStr = new byte[] { (byte)(9 + (2 * cmdIn.Length) + (2 * cmdIn.Length)), 0x26, 0x0A, ep, (byte)profID, (byte)(profID >> 8), (byte)devID, (byte)(devID >> 8), devVer, 0, (byte)(2 * cmdIn.Length) };
            return String.Concat(Encoding.ASCII.GetString(outputStr), cmdIn) +(byte)(2*cmdOut.Length)+ cmdOut;
        }

        //Sends the reset signal to the ALT5801
        public static String SYS_RESET()
        {
            byte[] outputStr = new byte[] { 1,0x41,0x00,0x00};
            return Encoding.ASCII.GetString(outputStr);
        }

        //Write the configuration details to the ALT5801
        public static String ZB_WRITE_CFG(byte cfgID, String value)
        {
            byte[] outputStr = new byte[] { (byte)(2 + value.Length), 0x26, 0x05, cfgID, (byte)value.Length };
            return String.Concat(Encoding.ASCII.GetString(outputStr), value);
        }

        public static String ZB_READ_CFG(byte cfgID)
        {
            byte[] outputStr = new byte[] { 1, 0x26, 0x04, cfgID };
            return Encoding.ASCII.GetString(outputStr);
        }

        //Starts the Zigbee stack in the device
        public static String ZB_START_REQ()
        {
            byte[] outputStr = new byte[] { 0x00, 0x26, 0x00 };
            return Encoding.ASCII.GetString(outputStr);
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
        public static String ZB_SEND_DATA(uint dest, uint cmd, byte handle, byte ack, byte radius, String data)
        {
            byte[] outputStr = new byte[] { (byte)(8 + data.Length), 0x26, 0x03, (byte)dest, (byte)(dest >> 8), (byte)cmd, (byte)(cmd >> 8), handle, ack, radius, (byte)(data.Length) };
            return String.Concat(Encoding.ASCII.GetString(outputStr), data);
        }

        public static String ZB_GET_INFO(byte param)
        {
            byte[] outputStr = new byte[] { 1, 0x26, 0x06, param };
            return Encoding.ASCII.GetString(outputStr);
        }

        //Sets up the radio for testing, 0xFF is the max txPower
        //TESTMODE = 0 - Transmit unmodulated carrier with spcified frequency
        //TESTMODE = 1 - Transmit psudo-random data with specified frequency
        //TESTMODE = 2 - Set to receive mode on specified frequency
        public static String SYS_TEST_RF(byte testMode, uint frequency, byte txPower)
        {
            byte[] outputStr = new byte[] { 4, 0x41, 0x40, testMode, (byte)frequency, (byte)(frequency >> 8), txPower };
            return Encoding.ASCII.GetString(outputStr);
        }

        //Generate the FCS byte to confirm the end of the data string
        public static char FCSgenerate(String input)
        {
            char result = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                result = (char)(result ^ input[i]);
            }
            return result;
        }
    }
}
