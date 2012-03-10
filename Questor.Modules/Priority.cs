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
    public enum Priority
    {
        WarpScrambler = Settings.Instance.PriorityLevelOfWarpScrambling,
        Webbing = Settings.Instance.PriorityLevelOfWebbing,
        TargetPainting = Settings.Instance.PriorityLevelOfTargetPainting,
        Neutralizing = Settings.Instance.PriorityLevelOfNeutralizing,
        Jamming = Settings.Instance.PriorityLevelOfJamming,
        Dampening = Settings.Instance.PriorityLevelOfTrackingDistrupting,
        PriorityKillTarget = Settings.Instance.PriorityLevelOfPriorityKillTarget                             
    }
}