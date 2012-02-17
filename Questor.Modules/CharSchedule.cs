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
    using System.Xml.Linq;
    using System.Globalization;
    using Questor.Modules;
    using System;
    //using System.Windows.Forms;

    public class CharSchedule
    {
        public CharSchedule(XElement element)
        {
            CultureInfo enUS = new CultureInfo("en-US"); 
            User = (string)element.Attribute("user");
            PW = (string)element.Attribute("pw");
            Name = (string)element.Attribute("name");

            stopTimeSpecified = false;
            startTimeSpecified = false;
            string _start = (string)element.Attribute("start");
            string _stop = (string)element.Attribute("stop");
            DateTime _startTime = new DateTime();
            DateTime _stopTime = new DateTime();
            if (_start != null)
            {
                if (!DateTime.TryParseExact(_start, "HH:mm", enUS, DateTimeStyles.None, out _startTime))
                {
                    Logging.Log("[CharSchedule] " + Name + ": Couldn't parse starttime.");
                    _startTime = DateTime.Now.AddSeconds(20);
                }
                else
                    startTimeSpecified = true;
            }
            else
            {
                Logging.Log("[CharSchedule] No start time specified. Starting now.");
                _startTime = DateTime.Now.AddSeconds(20);
            }
            Start = _startTime;

            if (_stop != null)
            {
                if (!DateTime.TryParseExact(_stop, "HH:mm", enUS, DateTimeStyles.None, out _stopTime))
                {
                    Logging.Log("[CharSchedule] " + Name + ": Couldn't parse stoptime.");
                    _stopTime = DateTime.Now.AddHours(24);
                }
                else
                    stopTimeSpecified = true;
            }
            else
            {
                Logging.Log("[CharSchedule] No stop time specified.");
                _stopTime = DateTime.Now.AddHours(24);
            }
            Stop = _stopTime;

            if ((string)element.Attribute("runtime") != null)
            {
                RunTime = (double)element.Attribute("runtime");
                stopTimeSpecified = true;
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
        public bool stopTimeSpecified { get; set; }
        public bool startTimeSpecified { get; set; }
    }
}