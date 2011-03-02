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
    using System.Linq;
    using DirectEve;

    public abstract class TravelerDestination
    {
        public long SolarSystemId { get; set; }

        /// <summary>
        ///   This function returns true if we are at the final destination and false if the task is not yet complete
        /// </summary>
        /// <returns></returns>
        public abstract bool PerformFinalDestinationTask();
    }

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
            if (Cache.Instance.InStation && !Cache.Instance.InSpace)
            {
                if (_nextAction < DateTime.Now)
                {
                    Logging.Log("Traveler.SolarSystemDestination: Exiting station");

                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdExitStation);
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

    public class StationDestination : TravelerDestination
    {
        private DateTime _nextAction;

        public StationDestination(long stationId)
        {
            var station = Cache.Instance.DirectEve.Navigation.GetLocation(stationId);
            if (station == null || !station.ItemId.HasValue || !station.SolarSystemId.HasValue)
            {
                Logging.Log("Traveler.StationDestination: Invalid station id [" + stationId + "]");

                SolarSystemId = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
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
            if (Cache.Instance.InStation && Cache.Instance.DirectEve.Session.StationId == stationId)
            {
                Logging.Log("Traveler.StationDestination: Arrived in station");
                return true;
            }

            if (Cache.Instance.InStation)
            {
                // We are in a station, but not the correct station!
                if (nextAction < DateTime.Now)
                {
                    Logging.Log("Traveler.StationDestination: We're docked in the wrong station, undocking");

                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdExitStation);
                    nextAction = DateTime.Now.AddSeconds(30);
                }

                // We are not there yet
                return false;
            }

            if (!Cache.Instance.InSpace)
            {
                // We are not in station and not in space?  Wait for a bit
                return false;
            }

            if (nextAction > DateTime.Now)
                return false;

            var entity = Cache.Instance.EntitiesByName(stationName).FirstOrDefault();
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

    public class BookmarkDestination : TravelerDestination
    {
        private DateTime _nextAction;

        public BookmarkDestination(DirectBookmark bookmark)
        {
            if (bookmark == null)
            {
                Logging.Log("Traveler.BookmarkDestination: Invalid bookmark destination!");

                SolarSystemId = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
                BookmarkId = -1;
                return;
            }

            Logging.Log("Traveler.BookmarkDestination: Destination set to bookmark [" + bookmark.Title + "]");
            BookmarkId = bookmark.BookmarkId ?? -1;
            SolarSystemId = bookmark.LocationId ?? -1;
        }

        public BookmarkDestination(long bookmarkId)
            : this(Cache.Instance.BookmarkById(bookmarkId))
        {
        }

        public long BookmarkId { get; set; }

        public override bool PerformFinalDestinationTask()
        {
            var bookmark = Cache.Instance.BookmarkById(BookmarkId);
            return PerformFinalDestinationTask(bookmark, 150000, ref _nextAction);
        }

        internal static bool PerformFinalDestinationTask(DirectBookmark bookmark, int warpDistance, ref DateTime nextAction)
        {
            // The bookmark no longer exists, assume we are there
            if (bookmark == null)
                return true;

            var invType = Cache.Instance.InvTypesById[bookmark.TypeId ?? -1];
            if (invType.GroupId == (int) Group.Station) // Let StationDestination handle it :)
            {
                var arrived = StationDestination.PerformFinalDestinationTask(bookmark.ItemId ?? -1, bookmark.Entity.Name, ref nextAction);
                if (arrived)
                    Logging.Log("Traveler.BookmarkDestination: Arrived at bookmark [" + bookmark.Title + "]");
                return arrived;
            }

            // Its not a station bookmark, make sure we are in space
            if (!Cache.Instance.InSpace && Cache.Instance.InStation)
            {
                // We are in a station, but not the correct station!
                if (nextAction < DateTime.Now)
                {
                    Logging.Log("Traveler.BookmarkDestination: We're docked but our destination is in space, undocking");

                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdExitStation);
                    nextAction = DateTime.Now.AddSeconds(30);
                }

                // We are not there yet
                return false;
            }

            if (!Cache.Instance.InSpace)
            {
                // We are not in space and not in a station, wait a bit
                return false;
            }

            // This bookmark has no x / y / z, assume we are there.
            if (bookmark.X == -1 || bookmark.Y == -1 || bookmark.Z == -1)
            {
                Logging.Log("Traveler.BookmarkDestination: Arrived at the bookmark [" + bookmark.Title + "][No XYZ]");
                return true;
            }

            var distance = Cache.Instance.DistanceFromMe(bookmark.X ?? 0, bookmark.Y ?? 0, bookmark.Z ?? 0);
            if (distance < warpDistance)
            {
                Logging.Log("Traveler.BookmarkDestination: Arrived at the bookmark [" + bookmark.Title + "]");
                return true;
            }

            if (nextAction > DateTime.Now)
                return false;

            Logging.Log("Traveler.BookmarkDestination: Warping to bookmark [" + bookmark.Title + "]");
            bookmark.WarpTo();
            nextAction = DateTime.Now.AddSeconds(30);
            return false;
        }
    }

    public class MissionBookmarkDestination : TravelerDestination
    {
        private DateTime _nextAction;

        public MissionBookmarkDestination(DirectAgentMissionBookmark bookmark)
        {
            if (bookmark == null)
            {
                Logging.Log("Traveler.MissionBookmarkDestination: Invalid mission bookmark!");

                AgentId = -1;
                Title = null;
                SolarSystemId = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
                return;
            }

            Logging.Log("Traveler.MissionBookmarkDestination: Destination set to mission bookmark [" + bookmark.Title + "]");
            AgentId = bookmark.AgentId ?? -1;
            Title = bookmark.Title;
            SolarSystemId = bookmark.SolarSystemId ?? -1;
        }

        public MissionBookmarkDestination(int agentId, string title)
            : this(GetMissionBookmark(agentId, title))
        {
        }

        public long AgentId { get; set; }
        public string Title { get; set; }

        private static DirectAgentMissionBookmark GetMissionBookmark(long agentId, string title)
        {
            var mission = Cache.Instance.GetAgentMission(agentId);
            if (mission == null)
                return null;

            return mission.Bookmarks.FirstOrDefault(b => b.Title == title);
        }

        public override bool PerformFinalDestinationTask()
        {
            // Mission bookmarks have a 1.000.000 distance warp-to limit (changed it to 150.000.000 as there are some bugged missions around)
            return BookmarkDestination.PerformFinalDestinationTask(GetMissionBookmark(AgentId, Title), 150000000, ref _nextAction);
        }
    }
}