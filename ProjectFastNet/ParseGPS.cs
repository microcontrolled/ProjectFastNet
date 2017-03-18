using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFastNet
{
    class ParseGPS
    {
        public static String[] NMEAstring;
        
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
        //Returns a list of strings for the coordinates, sorting the angle from the bearing
        public static List<String> getCoordStr()
        {
            List<String> latlogStr = new List<String>();
            String lat = NMEAstring[2 - ParseGPS.getCommand()];
            String log = NMEAstring[4 - ParseGPS.getCommand()];
            latlogStr.Add(lat.Substring(0, 2));
            latlogStr.Add(lat.Substring(2, 5));
            latlogStr.Add(log.Substring(0, 3));
            latlogStr.Add(log.Substring(3, 5));
            return latlogStr;
        }

         public static List<String> getTime()
        {
            List<String> TimeList = new List<String>();
            String Time = NMEAstring[1 - ParseGPS.getCommand()];
            String hours = Time.Substring(0, 2);
            String mins = Time.Substring(2, 2);
            String secs = Time.Substring(4, 5);
            TimeList.Add(hours);
            TimeList.Add(mins);
            TimeList.Add(secs);
            return TimeList;
        }

        public static char[] getCompass()
        {
            char[] Compass = new char[2];
            Compass[0] = char.Parse(NMEAstring[3 - ParseGPS.getCommand()]);
            Compass[1] = char.Parse(NMEAstring[5 - ParseGPS.getCommand()]);
            return Compass;
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
