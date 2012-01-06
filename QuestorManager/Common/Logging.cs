// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace QuestorManager.Common
{
    using System;
    using InnerSpaceAPI;

    public static class Logging
    {
        /// <summary>
        ///   Log a line to the console
        /// </summary>
        /// <param name = "line"></param>
        public static void Log(string line)
        {
            InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line));
        }

        /// <summary>
        ///   Log a line to the console
        /// </summary>
        /// <param name = "line"></param>
        public static void Log(string format, params object[] parms)
        {
            var line = string.Format(format, parms);
            Log(line);
        }
    }
}