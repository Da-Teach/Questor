// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Actions
{
    using System.Collections.Generic;
    using DirectEve;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;

    public class DebugModule
    {
        public static HashSet<int> Salvagers = new HashSet<int> { 25861, 26983, 30836 };
        public static HashSet<int> TractorBeams = new HashSet<int> { 24348, 24620, 24622, 24644 };

        public int MaximumWreckTargets { get; set; }

        public int ReserveCargoCapacity { get; set; }

        public List<Ammo> Ammo { get; set; }

        private void Debug_Windows()
        {
            // We are should read windows in station and out...
            //if (!Cache.Instance.InSpace)
            //    return;

            var windows = new List<DirectWindow>();

            foreach (DirectWindow window in windows)
            {
                Logging.Log("DebugModule", "Debug_Windows: [" + window.Name + "]", Logging.white);

                //if (window.Name.Contains(wreck.Name))
                //{
                //    Logging.Log("Salvage: Cargo Container [" + wreck.Name + "][ID: " + wreck.Id + "] on the ignore list, ignoring.");
                //    wreck.UnlockTarget();
                //    continue;
                //}

                //if (hasSalvagers && wreck.GroupId != (int)Group.CargoContainer)
                //    continue;
            }
        }

        public void ProcessState()
        {
            // Nothing to salvage in stations
            //if (Cache.Instance.InStation)
            //    return;

            //DirectContainer cargo = Cache.Instance.DirectEve.GetShipsCargo();
            switch (_States.CurrentDebugModuleState)
            {
                case DebugModuleState.DebugWindows:
                    Debug_Windows();

                    // Next state
                    _States.CurrentDebugModuleState = DebugModuleState.Done;
                    break;

                //case DebugModuleState.LootHostileWrecks:
                //    LootHostileWrecks();
                //
                //    //State = DebugModuleState.SalvageHostileWrecks;
                //    break;
                //

                case DebugModuleState.Done:
                    // Wait indefinately...
                    break;

                case DebugModuleState.Error:
                    // Wait indefinately...
                    break;

                default:
                    // Unknown state, goto first state
                    _States.CurrentDebugModuleState = DebugModuleState.DebugWindows;
                    break;
            }
        }
    }
}