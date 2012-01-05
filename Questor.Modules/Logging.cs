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

    public static class Logging
    {
        public static void Log(string line)
        {
            InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line));
            Cache.Instance.ExtConsole += string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line + "\r\n");
        }
    }
}