// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

using Questor.Modules.Caching;
using Questor.Modules.Lookup;

namespace Questor.Modules.Logging
{
    using System;
    using InnerSpaceAPI;
    using System.IO;

    public static class Logging
    {
        //list of colors
        public const string green = "\ag";    //traveler mission control
        public const string yellow = "\ay";
        public const string blue = "\ab";     //DO NOT USE - blends into default lavish GUIs background.
        public const string red = "\ar";      //error panic
        public const string orange = "\ao";   //error can fix
        public const string purple = "\ap";   //combat
        public const string magenta = "\am";  //drones
        public const string teal = "\at";     //log debug
        public const string white = "\aw";    //questor

        //public  void Log(string line)
        //public static void Log(string module, string line, string color = Logging.white)
        public static void Log(string module, string line, string color)
        {
            string colorLogLine = line;
            //colorLogLine contains color and is for the InnerSpace console
            InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, Logging.orange + "[" + Logging.yellow + module + Logging.orange +  "] " + color + colorLogLine));                            //Innerspace Console Log
            
            string plainLogLine = FilterColorsFromLogs(line);
            //plainLogLine contains plain text and is for the log file and the GUI console (why cant the GUI be made to use color too?)
            Cache.Instance.ExtConsole += string.Format("{0:HH:mm:ss} {1}", DateTime.Now, "[" + module + "] " + plainLogLine + "\r\n");               //Questor GUI Console Log
            Cache.Instance.ConsoleLog += string.Format("{0:HH:mm:ss} {1}", DateTime.Now, "[" + module + "] " + plainLogLine + "\r\n");               //In memory Console Log
            if (Settings.Instance.SaveConsoleLog)
            {
                if (!Cache.Instance.ConsoleLogOpened)
                {
                    if (Settings.Instance.ConsoleLogPath != null && Settings.Instance.ConsoleLogFile != null)
                    {
                        module = "Logging";
                        line = "Writing to Daily Console Log ";
                        InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, Logging.orange + "[" + Logging.yellow + module + Logging.orange + "] " + color + colorLogLine));                            //Innerspace Console Log
                        Cache.Instance.ExtConsole += string.Format("{0:HH:mm:ss} {1}", DateTime.Now, plainLogLine + "\r\n");

                        if (!string.IsNullOrEmpty(Settings.Instance.ConsoleLogFile))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(Settings.Instance.ConsoleLogFile));
                            if (Directory.Exists(Path.GetDirectoryName(Settings.Instance.ConsoleLogFile)))
                            {
                                Cache.Instance.ConsoleLog += string.Format("{0:HH:mm:ss} {1}", DateTime.Now,
                                                                           "[" + module + "]" + plainLogLine + "\r\n");
                                Cache.Instance.ConsoleLogOpened = true;
                            }
                            else
                            {
                                InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now,
                                                              "Logging: Unable to find (or create): " +
                                                              Settings.Instance.ConsoleLogPath));
                            }
                            line = "";
                        }
                        else
                        {
                            line = "Logging: Unable to write log to file yet as: ConsoleLogFile is not yet defined";
                            InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, colorLogLine));
                            Cache.Instance.ExtConsole += string.Format("{0:HH:mm:ss} {1}", DateTime.Now, "[" + module + "] " + plainLogLine + "\r\n");
                        }
                    }
                }
                if (Cache.Instance.ConsoleLogOpened)
                {
                    if (Settings.Instance.ConsoleLogFile != null)
                        File.AppendAllText(Settings.Instance.ConsoleLogFile, Cache.Instance.ConsoleLog);               //Write In Memory Console log to File
                    Cache.Instance.ConsoleLog = null;
                }
            }
        }

        public static string FilterColorsFromLogs(string line)
        {
            if (line == null)
                return string.Empty;

            line = line.Replace("\ag", "");
            line = line.Replace("\ay", "");
            line = line.Replace("\ab", "");
            line = line.Replace("\ar", "");
            line = line.Replace("\ao", "");
            line = line.Replace("\ap", "");
            line = line.Replace("\am", "");
            line = line.Replace("\at", "");
            line = line.Replace("\aw", "");
            while (line.IndexOf("  ", System.StringComparison.Ordinal) >= 0)
                line = line.Replace("  ", " ");
            return line.Trim();
        }
    }
}