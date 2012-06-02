// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Lookup
{
    using System.Xml.Linq;
    using System.Globalization;
    using System;
    using global::Questor.Modules.Logging;

    public class CharSchedule
    {
        public CharSchedule(XElement element)
        {
            //var timeformat = new CultureInfo("en-US");
            User = (string)element.Attribute("user");
            PW = (string)element.Attribute("pw");
            Name = (string)element.Attribute("name");

            StopTimeSpecified = false;
            StartTimeSpecified = false;
            var start = (string)element.Attribute("start");
            var stop = (string)element.Attribute("stop");
            DateTime startTime;
            DateTime stopTime;
            if (start != null)
            {
                if (!DateTime.TryParseExact(start, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                {
                    Logging.Log("CharSchedule", Name + ": Couldn't parse starttime.", Logging.red);
                    startTime = DateTime.Now.AddSeconds(20);
                }
                else
                    StartTimeSpecified = true;
            }
            else
            {
                Logging.Log("CharSchedule", "No start time specified. Starting now.", Logging.orange);
                startTime = DateTime.Now.AddSeconds(20);
            }
            Start = startTime;

            if (stop != null)
            {
                if (!DateTime.TryParseExact(stop, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out stopTime))
                {
                    Logging.Log("CharSchedule", Name + ": Couldn't parse stoptime.", Logging.red);
                    stopTime = DateTime.Now.AddHours(24);
                }
                else
                    StopTimeSpecified = true;
            }
            else
            {
                Logging.Log("CharSchedule", "No stop time specified.", Logging.red);
                stopTime = DateTime.Now.AddHours(24);
            }
            Stop = stopTime;

            if ((string)element.Attribute("runtime") != null)
            {
                RunTime = (double)element.Attribute("runtime");
                StopTimeSpecified = true;
            }
            else
                RunTime = -1;
        }

        public string User { get; private set; }

        public string PW { get; private set; }

        public string Name { get; private set; }

        public DateTime Start { get; set; }

        public DateTime Stop { get; set; }

        public double RunTime { get; set; }

        public bool StopTimeSpecified { get; set; }

        public bool StartTimeSpecified { get; set; }
    }
}