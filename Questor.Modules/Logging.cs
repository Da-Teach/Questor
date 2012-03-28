// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace Questor.Modules
{
    using System;
    using InnerSpaceAPI;
    using System.IO;

    public static class Logging
    {
        public static void Log(string line)
        {
            InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line));
            Cache.Instance.ExtConsole += string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line + "\r\n");
            Cache.Instance.ConsoleLog += string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line + "\r\n");
            if (Settings.Instance.SaveConsoleLog)
            {
                if (!Cache.Instance.ConsoleLogOpened)
                {
                    if (Settings.Instance.ConsoleLogPath != null && Settings.Instance.ConsoleLogFile != null)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(Settings.Instance.ConsoleLogFile));
                        if (Directory.Exists(Path.GetDirectoryName(Settings.Instance.ConsoleLogFile)))
                        {
                            line = "Questor: Writing to Daily Console Log ";
                            InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line));
                            Cache.Instance.ExtConsole += string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line + "\r\n");
                            Cache.Instance.ConsoleLog += string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line + "\r\n");
                            Cache.Instance.ConsoleLogOpened = true;
                            line = "";
                        }
                        else
                        {
                            InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, "Logging: Unable to find (or create): " + Settings.Instance.ConsoleLogPath));
                        }

                    }
                }
                if (Cache.Instance.ConsoleLogOpened)
                {
                    File.AppendAllText(Settings.Instance.ConsoleLogFile, Cache.Instance.ConsoleLog);
                    Cache.Instance.ConsoleLog = null;
                }
            }                
        }
    }
}