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
    using global::QuestorManager.Domains;
    using global::QuestorManager.Extensions;
    using DirectEve = global::QuestorManager.Common.DirectEve;

    public class BookmarkDestination : TravelerDestination
    {
        private DateTime _nextAction;

        public BookmarkDestination(DirectBookmark bookmark)
        {
            if (bookmark == null)
            {
                Logging.Log("Traveler.BookmarkDestination: Invalid bookmark destination!");

                SolarSystemId = DirectEve.Instance.Session.SolarSystemId ?? -1;
                BookmarkId = -1;
                return;
            }

            Logging.Log("Traveler.BookmarkDestination: Destination set to bookmark [" + bookmark.Title + "]");
            var location = GetBookmarkLocation(bookmark);
            if (location == null)
            {
                Logging.Log("Traveler.BookmarkDestination: Invalid bookmark destination!");

                SolarSystemId = DirectEve.Instance.Session.SolarSystemId ?? -1;
                BookmarkId = -1;
                return;
            }

            BookmarkId = bookmark.BookmarkId ?? -1;
            SolarSystemId = location.SolarSystemId ?? DirectEve.Instance.Session.SolarSystemId ?? -1;
        }

        public BookmarkDestination(long bookmarkId)
            : this(DirectEve.Instance.GetBookmarkById(bookmarkId))
        {
        }

        public long BookmarkId { get; set; }

        private static DirectLocation GetBookmarkLocation(DirectBookmark bookmark)
        {
            var location = DirectEve.Instance.Navigation.GetLocation(bookmark.ItemId ?? -1);
            if (!location.IsValid)
                location = DirectEve.Instance.Navigation.GetLocation(bookmark.LocationId ?? -1);
            if (!location.IsValid)
                return null;

            return location;
        }

        public override bool PerformFinalDestinationTask()
        {
            var bookmark = DirectEve.Instance.GetBookmarkById(BookmarkId);
            return PerformFinalDestinationTask(bookmark, 150000, ref _nextAction);
        }

        internal static bool PerformFinalDestinationTask(DirectBookmark bookmark, int warpDistance, ref DateTime nextAction)
        {
            // The bookmark no longer exists, assume we are there
            if (bookmark == null)
                return true;

            var location = GetBookmarkLocation(bookmark);
            if (DirectEve.Instance.Session.IsInStation)
            {
                // We have arived
                if (location != null && location.ItemId == DirectEve.Instance.Session.StationId)
                    return true;

                // We are apparently in a station that is incorrect
                Logging.Log("Traveler.BookmarkDestination: We're docked in the wrong station, undocking");

                DirectEve.Instance.ExecuteCommand(DirectCmd.CmdExitStation);
                nextAction = DateTime.Now.AddSeconds(30);
                return false;
            }

            // Is this a station bookmark?
            if (bookmark.Entity != null && bookmark.Entity.GroupId == (int) Group.Station)
            {
                var arrived = StationDestination.PerformFinalDestinationTask(bookmark.Entity.Id, bookmark.Entity.Name, ref nextAction);
                if (arrived)
                    Logging.Log("Traveler.BookmarkDestination: Arrived at bookmark [" + bookmark.Title + "]");
                return arrived;
            }

            // Its not a station bookmark, make sure we are in space
            if (DirectEve.Instance.Session.IsInStation)
            {
                // We are in a station, but not the correct station!
                if (nextAction < DateTime.Now)
                {
                    Logging.Log("Traveler.BookmarkDestination: We're docked but our destination is in space, undocking");

                    DirectEve.Instance.ExecuteCommand(DirectCmd.CmdExitStation);
                    nextAction = DateTime.Now.AddSeconds(30);
                }

                // We are not there yet
                return false;
            }

            if (!DirectEve.Instance.Session.IsInSpace)
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

            var distance = DirectEve.Instance.DistanceFromMe(bookmark.X ?? 0, bookmark.Y ?? 0, bookmark.Z ?? 0);
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
}