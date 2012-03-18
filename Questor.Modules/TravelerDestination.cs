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
        public DirectBookmark UndockBookmark { get; set; }

        /// <summary>
        ///   This function returns true if we are at the final destination and false if the task is not yet complete
        /// </summary>
        /// <returns></returns>
        public abstract bool PerformFinalDestinationTask();
    }

    public class SolarSystemDestination : TravelerDestination
    {
        private DateTime _nextSolarSystemAction;

        public SolarSystemDestination(long solarSystemId)
        {
            Logging.Log("TravelerDestination.SolarSystemDestination: Destination set to solar system id [" + solarSystemId + "]");
            SolarSystemId = solarSystemId;
        }

        public override bool PerformFinalDestinationTask()
        {
            // The destination is the solar system, not the station in the solar system.
            if (Cache.Instance.InStation && !Cache.Instance.InSpace)
            {
                if (_nextSolarSystemAction < DateTime.Now)
                {
                    Logging.Log("TravelerDestination.SolarSystemDestination: Exiting station");

                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdExitStation);
                    _nextSolarSystemAction = DateTime.Now.AddSeconds((int)Time.TravelerExitStationAmIInSpaceYet_seconds);
                }

                // We are not there yet
                return false;
            }

            // The task was to get to the solar system, we're there :)
            Logging.Log("TravelerDestination.SolarSystemDestination: Arrived in system");
            return true;
        }
    }

    public class StationDestination : TravelerDestination
    {
        private DateTime _nextStationAction;

        public StationDestination(long stationId)
        {
            var station = Cache.Instance.DirectEve.Navigation.GetLocation(stationId);
            if (station == null || !station.ItemId.HasValue || !station.SolarSystemId.HasValue)
            {
                Logging.Log("TravelerDestination.StationDestination: Invalid station id [" + stationId + "]");

                SolarSystemId = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
                StationId = -1;
                StationName = "";
                return;
            }

            Logging.Log("TravelerDestination.StationDestination: Destination set to [" + station.Name + "]");

            StationId = stationId;
            StationName = station.Name;
            SolarSystemId = station.SolarSystemId.Value;
        }

        public StationDestination(long solarSystemId, long stationId, string stationName)
        {
            Logging.Log("TravelerDestination.StationDestination: Destination set to [" + stationName + "]");

            SolarSystemId = solarSystemId;
            StationId = stationId;
            StationName = stationName;
        }

        public long StationId { get; set; }
        public string StationName { get; set; }

        public override bool PerformFinalDestinationTask()
        {
            var localundockBookmark = UndockBookmark;
            var arrived = PerformFinalDestinationTask(StationId, StationName, ref _nextStationAction, ref localundockBookmark);
            UndockBookmark = localundockBookmark;
            return arrived;
        }

        internal static bool PerformFinalDestinationTask(long stationId, string stationName, ref DateTime nextAction, ref DirectBookmark localundockBookmark)
        {
            if (Cache.Instance.InStation && Cache.Instance.DirectEve.Session.StationId == stationId)
            {
                Logging.Log("TravelerDestination.StationDestination: Arrived in station");
                return true;
            }

            if (Cache.Instance.InStation)
            {
                // We are in a station, but not the correct station!
                if (nextAction < DateTime.Now)
                {
                    Logging.Log("TravelerDestination.StationDestination: We're docked in the wrong station, undocking from [" + Cache.Instance.DirectEve.GetLocationName(Cache.Instance.DirectEve.Session.StationId ?? 0) + "]");

                    if (!string.IsNullOrEmpty(Settings.Instance.UndockPrefix))
                    {
                        var bookmarks = Cache.Instance.DirectEve.Bookmarks.Where(b => b.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId).Where(b => b.Title.Contains(Cache.Instance.DirectEve.GetLocationName(Cache.Instance.DirectEve.Session.StationId ?? 0)) && b.Title.Contains(Settings.Instance.UndockPrefix));
                        if (bookmarks != null && bookmarks.Count() > 0)
                        {
                            localundockBookmark = bookmarks.FirstOrDefault();
                            if (localundockBookmark.X == null || localundockBookmark.Y == null || localundockBookmark.Z == null)
                            {
                                Logging.Log("TravelerDestination.StationDestination: undock bookmark [" + localundockBookmark.Title + "] is unusable: it has no coords");
                                localundockBookmark = null;
                            }
                            else Logging.Log("TravelerDestination.StationDestination: undock bookmark [" + localundockBookmark.Title + "] is usable: it has coords");
                        }
                        else Logging.Log("TravelerDestination.StationDestination: undock bookmark does not exist: " + Cache.Instance.DirectEve.GetLocationName((long)Cache.Instance.DirectEve.Session.StationId) + " and " + Settings.Instance.UndockPrefix + " did not both exist in a bookmark");
                    }
                    else Logging.Log("TravelerDestination.StationDestination: UndockPrefix is not configured");
                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdExitStation);
                    nextAction = DateTime.Now.AddSeconds((int)Time.TravelerExitStationAmIInSpaceYet_seconds);
                    //nextAction = DateTime.Now.AddSeconds(Settings.Instance.UndockDelay);
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

            if (localundockBookmark != null)
            {
                if (Cache.Instance.DistanceFromMe(localundockBookmark.X ?? 0, localundockBookmark.Y ?? 0, localundockBookmark.Z ?? 0) < (int)Distance.WarptoDistance)
                {
                    Logging.Log("TravelerDestination.BookmarkDestination: Arrived at undock bookmark [" + localundockBookmark.Title + "]");
                    localundockBookmark = null;
                }
                else
                {
                    Logging.Log("TravelerDestination.BookmarkDestination: Warping to undock bookmark [" + localundockBookmark.Title + "]");
                    localundockBookmark.WarpTo();
                    nextAction = DateTime.Now.AddSeconds(10);
                    //nextAction = DateTime.Now.AddSeconds(Settings.Instance.UndockDelay);
                    return false;
                }
            }
            else Logging.Log("TravelerDestination.BookmarkDestination: undock bookmark missing: " + Cache.Instance.DirectEve.GetLocationName((long)Cache.Instance.DirectEve.Session.StationId) + " and " + Settings.Instance.UndockPrefix + " did not both exist in a bookmark");

            var entity = Cache.Instance.EntitiesByName(stationName).FirstOrDefault();
            if (entity == null)
            {
                // We are there but no station? Wait a bit
                return false;
            }

            if (entity.Distance < (int)Distance.DockingRange)
            {
                Logging.Log("TravelerDestination.StationDestination: Dock at [" + entity.Name + "]");
                entity.Dock();
            }
            else if (entity.Distance < (int)Distance.WarptoDistance)
                entity.Approach();
            else
            {
                Logging.Log("TravelerDestination.StationDestination: Warp to and dock at [" + entity.Name + "]");
                entity.WarpTo();
            }

            nextAction = DateTime.Now.AddSeconds(20);
            return false;
        }
    }

    public class BookmarkDestination : TravelerDestination
    {
        private DateTime _nextBookmarkAction;

        public BookmarkDestination(DirectBookmark bookmark)
        {
            if (bookmark == null)
            {
                Logging.Log("TravelerDestination.BookmarkDestination: Invalid bookmark destination!");

                SolarSystemId = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
                BookmarkId = -1;
                return;
            }

            Logging.Log("TravelerDestination.BookmarkDestination: Destination set to bookmark [" + bookmark.Title + "]");
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
            var undockBookmark = UndockBookmark;
            var arrived = PerformFinalDestinationTask(bookmark, 150000, ref _nextBookmarkAction, ref undockBookmark);
            UndockBookmark = undockBookmark;
            return arrived;
        }

        internal static bool PerformFinalDestinationTask(DirectBookmark bookmark, int warpDistance, ref DateTime nextAction, ref DirectBookmark undockBookmark)
        {
            // The bookmark no longer exists, assume we aren't there
            if (bookmark == null)
                return false;

            if (Cache.Instance.DirectEve.Session.IsInStation)
            {
                // We have arived
                if (bookmark.ItemId.HasValue && bookmark.ItemId == Cache.Instance.DirectEve.Session.StationId)
                    return true;

                // We are apparently in a station that is incorrect
                Logging.Log("TravelerDestination.BookmarkDestination: We're docked in the wrong station, undocking");
                if (!string.IsNullOrEmpty(Settings.Instance.UndockPrefix))
                {
                    var bookmarks = Cache.Instance.DirectEve.Bookmarks.Where(b => b.Title.Contains(Cache.Instance.DirectEve.GetLocationName(Cache.Instance.DirectEve.Session.StationId ?? 0)) && b.Title.Contains(Settings.Instance.UndockPrefix));
                    if (bookmarks != null && bookmarks.Count() > 0)
                    {
                        undockBookmark = bookmarks.FirstOrDefault();
                        if (undockBookmark.X == null || undockBookmark.Y == null || undockBookmark.Z == null)
                        {
                            Logging.Log("TravelerDestination.StationDestination: undock bookmark [" + undockBookmark.Title + "] is unusable: it has no coords");
                            undockBookmark = null;
                        }
                        else Logging.Log("TravelerDestination.StationDestination: undock bookmark [" + undockBookmark.Title + "] is usable: it has coords");
                    }
                    else Logging.Log("TravelerDestination.StationDestination: undock bookmark does not exist");
                }
                else Logging.Log("TravelerDestination.StationDestination: UndockPrefix is not configured");
                Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdExitStation);
                nextAction = DateTime.Now.AddSeconds((int)Time.TravelerExitStationAmIInSpaceYet_seconds);
                return false;
            }

            // Is this a station bookmark?
            if (bookmark.Entity != null && bookmark.Entity.GroupId == (int)Group.Station)
            {
                var arrived = StationDestination.PerformFinalDestinationTask(bookmark.Entity.Id, bookmark.Entity.Name, ref nextAction, ref undockBookmark);
                if (arrived)
                    Logging.Log("TravelerDestination.BookmarkDestination: Arrived at bookmark [" + bookmark.Title + "]");
                return arrived;
            }

            // Its not a station bookmark, make sure we are in space
            if (Cache.Instance.DirectEve.Session.IsInStation)
            {
                // We are in a station, but not the correct station!
                if (nextAction < DateTime.Now)
                {
                    Logging.Log("TravelerDestination.BookmarkDestination: We're docked but our destination is in space, undocking");

                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdExitStation);
                    nextAction = DateTime.Now.AddSeconds((int)Time.TravelerExitStationAmIInSpaceYet_seconds);
                }

                // We are not there yet
                return false;
            }

            if (!Cache.Instance.InSpace)
            {
                // We are not in space and not in a station, wait a bit
                return false;
            }

            if (undockBookmark != null)
            {
                if (Cache.Instance.DistanceFromMe(undockBookmark.X ?? 0, undockBookmark.Y ?? 0, undockBookmark.Z ?? 0) < (int)Distance.WarptoDistance)
                {
                    Logging.Log("TravelerDestination.BookmarkDestination: Arrived at undock bookmark [" + undockBookmark.Title + "]");
                    undockBookmark = null;
                }
                else
                {
                    Logging.Log("TravelerDestination.BookmarkDestination: Warping to undock bookmark [" + undockBookmark.Title + "]");
                    undockBookmark.WarpTo();
                    nextAction = DateTime.Now.AddSeconds((int)Time.TravelerInWarpedNextCommandDelay_seconds);
                    //nextAction = DateTime.Now.AddSeconds(Settings.Instance.UndockDelay);
                    return false;
                }
            }

            // This bookmark has no x / y / z, assume we are there.
            if (bookmark.X == -1 || bookmark.Y == -1 || bookmark.Z == -1)
            {
                Logging.Log("TravelerDestination.BookmarkDestination: Arrived at the bookmark [" + bookmark.Title + "][No XYZ]");
                return true;
            }

            var distance = Cache.Instance.DistanceFromMe(bookmark.X ?? 0, bookmark.Y ?? 0, bookmark.Z ?? 0);
            if (distance < warpDistance)
            {
                Logging.Log("TravelerDestination.BookmarkDestination: Arrived at the bookmark [" + bookmark.Title + "]");
                return true;
            }

            if (nextAction > DateTime.Now)
                return false;

            Logging.Log("TravelerDestination.BookmarkDestination: Warping to bookmark [" + bookmark.Title + "]");
            Cache.Instance.DoNotBreakInvul = false;
            bookmark.WarpTo();
            nextAction = DateTime.Now.AddSeconds((int)Time.TravelerInWarpedNextCommandDelay_seconds);
            return false;
        }
    }

    public class MissionBookmarkDestination : TravelerDestination
    {
        private DateTime _nextMissionBookmarkAction;

        public MissionBookmarkDestination(DirectAgentMissionBookmark bookmark)
        {
            if (bookmark == null)
            {

                AgentId = -1;
                Title = null;
                SolarSystemId = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
                //Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdLogOff);
                //Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdQuitGame);
                
                Cache.Instance.CloseQuestorCMDLogoff = false;
                Cache.Instance.CloseQuestorCMDExitGame = true;
                Cache.Instance.ReasonToStopQuestor = "TravelerDestination.MissionBookmarkDestination: Invalid mission bookmark! - Lag?! Closing EVE";
                Logging.Log(Cache.Instance.ReasonToStopQuestor);
                Cache.Instance.SessionState = "Quitting";
            }

            Logging.Log("TravelerDestination.MissionBookmarkDestination: Destination set to mission bookmark [" + bookmark.Title + "]");
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
            var undockBookmark = UndockBookmark;
            var arrived = BookmarkDestination.PerformFinalDestinationTask(GetMissionBookmark(AgentId, Title), (int)Distance.MissionWarpLimit, ref _nextMissionBookmarkAction, ref undockBookmark);
            UndockBookmark = undockBookmark;
            return arrived;// Mission bookmarks have a 1.000.000 distance warp-to limit (changed it to 150.000.000 as there are some bugged missions around)  
        }
    }
}