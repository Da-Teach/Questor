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
    using global::QuestorManager.Extensions;
    using DirectEve = global::QuestorManager.Common.DirectEve;

    public class StationDestination : TravelerDestination
    {
        private DateTime _nextAction;

        public StationDestination(long stationId)
        {
            var station = DirectEve.Instance.Navigation.GetLocation(stationId);
            if (station == null || !station.ItemId.HasValue || !station.SolarSystemId.HasValue)
            {
                Logging.Log("Traveler.StationDestination: Invalid station id [" + stationId + "]");

                SolarSystemId = DirectEve.Instance.Session.SolarSystemId ?? -1;
                StationId = -1;
                StationName = "";
                return;
            }

            Logging.Log("Traveler.StationDestination: Destination set to [" + station.Name + "]");

            StationId = stationId;
            StationName = station.Name;
            SolarSystemId = station.SolarSystemId.Value;
        }

        public StationDestination(long solarSystemId, long stationId, string stationName)
        {
            Logging.Log("Traveler.StationDestination: Destination set to [" + stationName + "]");

            SolarSystemId = solarSystemId;
            StationId = stationId;
            StationName = stationName;
        }

        public long StationId { get; set; }
        public string StationName { get; set; }

        public override bool PerformFinalDestinationTask()
        {
            return PerformFinalDestinationTask(StationId, StationName, ref _nextAction);
        }

        internal static bool PerformFinalDestinationTask(long stationId, string stationName, ref DateTime nextAction)
        {
            if (DirectEve.Instance.Session.IsInStation && DirectEve.Instance.Session.StationId == stationId)
            {
                Logging.Log("Traveler.StationDestination: Arrived in station");
                return true;
            }

            if (DirectEve.Instance.Session.IsInStation)
            {
                // We are in a station, but not the correct station!
                if (nextAction < DateTime.Now)
                {
                    Logging.Log("Traveler.StationDestination: We're docked in the wrong station, undocking");

                    DirectEve.Instance.ExecuteCommand(DirectCmd.CmdExitStation);
                    nextAction = DateTime.Now.AddSeconds(30);
                }

                // We are not there yet
                return false;
            }

            if (!DirectEve.Instance.Session.IsInSpace)
            {
                // We are not in station and not in space?  Wait for a bit
                return false;
            }

            if (nextAction > DateTime.Now)
                return false;

            var entity = DirectEve.Instance.GetEntityByName(stationName);
            if (entity == null)
            {
                // We are there but no station? Wait a bit
                return false;
            }

            if (entity.Distance < 2500)
            {
                Logging.Log("Traveler.StationDestination: Dock at [" + entity.Name + "]");
                entity.Dock();
            }
            else if (entity.Distance < 150000)
                entity.Approach();
            else
            {
                Logging.Log("Traveler.StationDestination: Warp to and dock at [" + entity.Name + "]");
                entity.WarpToAndDock();
            }

            nextAction = DateTime.Now.AddSeconds(30);
            return false;
        }
    }
}