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
    public abstract class TravelerDestination
    {
        public long SolarSystemId { get; set; }

        /// <summary>
        ///   This function returns true if we are at the final destination and false if the task is not yet complete
        /// </summary>
        /// <returns></returns>
        public abstract bool PerformFinalDestinationTask();
    }
}