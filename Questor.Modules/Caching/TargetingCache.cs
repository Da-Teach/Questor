using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Questor.Modules.Caching
{
    public class TargetingCache
    {
        public static EntityCache CurrentDronesTarget { get; set; }

        public static EntityCache CurrentWeaponsTarget { get; set; }

        public static int CurrentTargetShieldPct { get; set; }

        public static int CurrentTargetArmorPct { get; set; }

        public static int CurrentTargetStructurePct { get; set; }

        public static double CurrentTargetID { get; set; }

        public static IEnumerable<EntityCache> EntitiesWarpDisruptingMe { get; set; }

        public static string EntitiesWarpDisruptingMe_text { get; set; }

        public static IEnumerable<EntityCache> EntitiesJammingMe { get; set; }

        public static string EntitiesJammingMe_text { get; set; }

        public static IEnumerable<EntityCache> EntitiesWebbingMe { get; set; }

        public static string EntitiesWebbingMe_text { get; set; }

        public static IEnumerable<EntityCache> EntitiesNeutralizingMe { get; set; }

        public static string EntitiesNeutralizingMe_text { get; set; }

        public static IEnumerable<EntityCache> EntitiesTrackingDisruptingMe { get; set; }

        public static string EntitiesTrackingDisruptingMe_text { get; set; }

        public static IEnumerable<EntityCache> EntitiesDampeningMe { get; set; }

        public static string EntitiesDampeningMe_text { get; set; }

        public static IEnumerable<EntityCache> EntitiesTargetPatingingMe { get; set; }

        public static string EntitiesTargetPaintingMe_text { get; set; }

        public TargetingCache()
        {
        }
    }
}