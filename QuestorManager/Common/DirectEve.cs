// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace QuestorManager.Common
{
    public static class DirectEve
    {
        private static global::DirectEve.DirectEve _instance;

        /// <summary>
        ///   An instance to DirectEve which is globally available to all modules
        /// </summary>
        public static global::DirectEve.DirectEve Instance
        {
            get { return _instance ?? (_instance = new global::DirectEve.DirectEve()); }
        }
    }
}