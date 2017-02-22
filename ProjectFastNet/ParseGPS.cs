using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFastNet
{
    class ParseGPS
    {
        static String[] NMEAstring;
        
        public static void parseNMEAstring(String NMEAin)
        {
            NMEAstring = NMEAin.Split(',');
        }

        //Returns an integer value representative of the type of NMEA command it is
        public static int getCommand()
        {
            String[] commandList = new String[] { "$GPGGA","$GPGLL","$GPGLC","$GPGSA","$GGGSV","$GGRMA","$GGRMB","$GGRMC" };
            
            for (int i=0;i<8;i++)
            {
                if (NMEAstring[0].Equals(commandList[i]))
                {
                    return i;
                }
            }
            return -1;
        }
        //Returns latitute and longitude as an array of 2 floating point numbers
        public static float[] getCoordinates()
        {
            float[] latLog = new float[2];
            latLog[0] = float.Parse(NMEAstring[2 - ParseGPS.getCommand()]);
            latLog[1] = float.Parse(NMEAstring[4 - ParseGPS.getCommand()]);
            return latLog;
        }
        //Input a GGA string and this function will reply with the state of the network fix
        public static bool findSignal()
        {
            if ((ParseGPS.getCommand()==0)&&(Int32.Parse(NMEAstring[6]) != 0)) 
            {
                return true;
            }
            return false;
        }
    }
}
