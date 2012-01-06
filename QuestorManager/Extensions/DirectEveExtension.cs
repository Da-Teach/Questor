// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace QuestorManager.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;

    public static class DirectEveExtension
    {
        /// <summary>
        ///   return a bookmark by it's id
        /// </summary>
        /// <param name = "directEve"></param>
        /// <param name = "bookmarkId"></param>
        /// <returns></returns>
        /// <remarks>
        ///   Return's null if no bookmark was found
        /// </remarks>
        public static DirectBookmark GetBookmarkById(this DirectEve directEve, long bookmarkId)
        {
            return directEve.Bookmarks.FirstOrDefault(b => b.BookmarkId == bookmarkId);
        }

        /// <summary>
        ///   Calculate distance from me
        /// </summary>
        /// <param name = "directEve"></param>
        /// <param name = "x"></param>
        /// <param name = "y"></param>
        /// <param name = "z"></param>
        /// <returns></returns>
        public static double DistanceFromMe(this DirectEve directEve, double x, double y, double z)
        {
            if (directEve.ActiveShip.Entity == null)
                return -1;

            var curX = directEve.ActiveShip.Entity.X;
            var curY = directEve.ActiveShip.Entity.Y;
            var curZ = directEve.ActiveShip.Entity.Z;

            return Math.Sqrt((curX - x)*(curX - x) + (curY - y)*(curY - y) + (curZ - z)*(curZ - z));
        }

        /// <summary>
        ///   Get the first entity with a certain name
        /// </summary>
        /// <param name = "directEve"></param>
        /// <param name = "name"></param>
        /// <returns></returns>
        public static DirectEntity GetEntityByName(this DirectEve directEve, string name)
        {
            return directEve.Entities.FirstOrDefault(e => string.Compare(e.Name, name, true) == 0);
        }

        /// <summary>
        ///   Get all entities with the name
        /// </summary>
        /// <param name = "directEve"></param>
        /// <param name = "name"></param>
        /// <returns></returns>
        public static IEnumerable<DirectEntity> GetEntitiesByName(this DirectEve directEve, string name)
        {
            return directEve.Entities.Where(e => string.Compare(e.Name, name, true) == 0);
        }

        /// <summary>
        ///   Returns the entity that we're approaching
        /// </summary>
        /// <param name = "directEve"></param>
        /// <returns></returns>
        public static DirectEntity GetApproachingEntity(this DirectEve directEve)
        {
            if (directEve.ActiveShip.Entity == null)
                return null;

            return directEve.GetEntityById(directEve.ActiveShip.Entity.FollowId);
        }
    }
}