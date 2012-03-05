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
    public enum Distance
    {
        ScoopRange = 2490,
        SafeScoopRange = ScoopRange - 700,
        TooCloseToStructure = 3000,
        SafeDistancefromStructure = 5000,
        WarptoDistance = 152000,
        NextPocketDistance = 100000, // If we moved more then 100km, assume next Pocket
        GateActivationRange = 1000,
        CloseToGateActivationRange = GateActivationRange + 5000,
        WayTooClose = -10100, // This is uaually used to determine how far inside the 'docking ring' of an acceleration gate we are. 
        OrbitDistanceCushion = 5000, // This is used to determine when to stop orbitingor approaching, if not speed tanking (orbit distance + orbitdistancecushion)
        InsideThisRangeIsLIkelyToBeMostlyFrigates = 9000, // 9k - overall this assumption works, use with caution
        DecloakRange = 1900,
        SafeToCloakDistance = 2300,
        DockingRange = 2400,
        MissionWarpLimit = 150000000, // Mission bookmarks have a 1.000.000 distance warp-to limit (changed it to 150.000.000 as there are some bugged missions around)  
        PanicDistanceToConsiderSafelyWarpedOff = 500000,
        WeCanWarpToStarFromHere = 500000000,
        BookmarksOnGridWithMe = 250000 //250k by default - all bookmarks inside this range will be deleted (used by after mission salvaging)
    }
}