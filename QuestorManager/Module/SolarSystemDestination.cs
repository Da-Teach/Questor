// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace QuestorManager.Module
{
    using System;
    using DirectEve;
    using global::QuestorManager.Common;
    using DirectEve = global::QuestorManager.Common.DirectEve;

    public class SolarSystemDestination : TravelerDestination
    {
        private DateTime _nextAction;

        public SolarSystemDestination(long solarSystemId)
        {
            Logging.Log("Traveler.SolarSystemDestination: Destination set to solar system id [" + solarSystemId + "]");
            SolarSystemId = solarSystemId;
        }

        public override bool PerformFinalDestinationTask()
        {
            // The destination is the solar system, not the station in the solar system.
            if (DirectEve.Instance.Session.IsInStation)
            {
                if (_nextAction < DateTime.Now)
                {
                    Logging.Log("Traveler.SolarSystemDestination: Exiting station");

                    DirectEve.Instance.ExecuteCommand(DirectCmd.CmdExitStation);
                    _nextAction = DateTime.Now.AddSeconds(30);
                }

                // We are not there yet
                return false;
            }

            // The task was to get to the solar system, we're threre :)
            Logging.Log("Traveler.SolarSystemDestination: Arrived in system");
            return true;
        }
    }
}