// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Combat
{
    using System;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;

    /// <summary>
    ///   The drones class will manage any and all drone related combat
    /// </summary>
    /// <remarks>
    ///   Drones will always work their way from lowest value target to highest value target and will only attack entities (not structures)
    /// </remarks>
    public class Drones
    {
        private double _armorPctTotal;
        private int _lastDroneCount;
        private DateTime _lastEngageCommand;
        private DateTime _lastRecallCommand;

        private int _recallCount;
        private DateTime _lastLaunch;
        private DateTime _lastRecall;

        private long _lastTarget;
        private DateTime _launchTimeout;
        private int _launchTries;
        private double _shieldPctTotal;
        private double _structurePctTotal;
        public bool Recall; //false
        public bool WarpScrambled; //false
        private DateTime _lastDronesProcessState;
        private DateTime _nextWrapScrambledWarning = DateTime.MinValue;

        private void GetDamagedDrones()
        {
            Cache.Instance.DamagedDrones = Cache.Instance.ActiveDrones.Where(d => d.ArmorPct < .8);
        }

        private double GetShieldPctTotal()
        {
            if (!Cache.Instance.ActiveDrones.Any())
                return 0;

            return Cache.Instance.ActiveDrones.Sum(d => d.ShieldPct);
        }

        private double GetArmorPctTotal()
        {
            if (!Cache.Instance.ActiveDrones.Any())
                return 0;

            return Cache.Instance.ActiveDrones.Sum(d => d.ArmorPct);
        }

        private double GetStructurePctTotal()
        {
            if (!Cache.Instance.ActiveDrones.Any())
                return 0;

            return Cache.Instance.ActiveDrones.Sum(d => d.StructurePct);
        }

        /// <summary>
        ///   Return the best possible target
        /// </summary>
        /// <remarks>
        ///   Note this GetTarget works differently then the one from Combat
        /// </remarks>
        /// <returns></returns>
        private EntityCache GetTarget()
        {
            // Find the first active weapon's target
            TargetingCache.CurrentDronesTarget = Cache.Instance.EntityById(_lastTarget);
            if (Cache.Instance.DronesKillHighValueTargets)
            {
                // Return best possible high value target
                return Cache.Instance.GetBestTarget(TargetingCache.CurrentDronesTarget, Settings.Instance.DroneControlRange, false, "Drones");
            }
            else
            {
                // Return best possible low value target
                return Cache.Instance.GetBestTarget(TargetingCache.CurrentDronesTarget, Settings.Instance.DroneControlRange, true, "Drones");
            }
        }

        /// <summary>
        ///   Engage the target
        /// </summary>
        private void EngageTarget()
        {
            EntityCache target = GetTarget();

            // Nothing to engage yet, probably retargeting
            if (target == null)
                return;

            if (target.IsBadIdea)
                return;

            // Is our current target still the same and is the last Engage command no longer then 15s ago?
            if (_lastTarget == target.Id && DateTime.Now.Subtract(_lastEngageCommand).TotalSeconds < 15)
            {
                return;
            }

            // Are we still actively shooting at the target?
            bool mustEngage = false;
            foreach (EntityCache drone in Cache.Instance.ActiveDrones)
                mustEngage |= drone.FollowId != target.Id;
            if (!mustEngage)
                return;

            // Is the last target our current active target?
            if (target.IsActiveTarget)
            {
                // Save target id (so we do not constantly switch)
                _lastTarget = target.Id;

                // Engage target
                Logging.Log("Drones", "Engaging [ " + Cache.Instance.ActiveDrones.Count() + " ] drones on [" + target.Name + "][ID: " + target.Id + "]" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.magenta);
                Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdDronesEngage);
                _lastEngageCommand = DateTime.Now;
            }
            else // Make the target active
            {
                target.MakeActiveTarget();
                Logging.Log("Drones", "[" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away] is now the target for drones", Logging.magenta);
            }
        }

        public void ProcessState()
        {
            if (DateTime.Now < _lastDronesProcessState.AddMilliseconds(100)) //if it has not been 100ms since the last time we ran this ProcessState return. We can't do anything that close together anyway
                return;

            _lastDronesProcessState = DateTime.Now;

            if (Cache.Instance.InStation ||                             // There is really no combat in stations (yet)
                !Cache.Instance.InSpace ||                             // if we are not in space yet, wait...
                Cache.Instance.DirectEve.ActiveShip.Entity == null ||   // What? No ship entity?
                Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked || // There is no combat when cloaked
                !Settings.Instance.UseDrones                            //if UseDrones is false
                )
            {
                _States.CurrentDroneState = DroneState.Idle;
                return;
            }

            if (!Cache.Instance.ActiveDrones.Any() && Cache.Instance.InWarp)
            {
                _States.CurrentDroneState = DroneState.Idle;
                return;
            }

            switch (_States.CurrentDroneState)
            {
                case DroneState.WaitingForTargets:
                    // Are we in the right state ?
                    if (Cache.Instance.ActiveDrones.Any())
                    {
                        // Apparently not, we have drones out, go into fight mode
                        _States.CurrentDroneState = DroneState.Fighting;
                        break;
                    }

                    // Should we launch drones?
                    bool launch = true;
                    // Always launch if we're scrambled
                    if (!Cache.Instance.PriorityTargets.Any(pt => pt.IsWarpScramblingMe))
                    {
                        launch &= Cache.Instance.UseDrones;
                        // Are we done with this mission pocket?
                        launch &= !Cache.Instance.IsMissionPocketDone;

                        // If above minimums
                        launch &= Cache.Instance.DirectEve.ActiveShip.ShieldPercentage >= Settings.Instance.DroneMinimumShieldPct;
                        launch &= Cache.Instance.DirectEve.ActiveShip.ArmorPercentage >= Settings.Instance.DroneMinimumArmorPct;
                        launch &= Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage >= Settings.Instance.DroneMinimumCapacitorPct;

                        // yes if there are targets to kill
                        launch &= Cache.Instance.TargetedBy.Count(e => !e.IsSentry && e.CategoryId == (int)CategoryID.Entity && e.IsNpc && !e.IsContainer && e.GroupId != (int)Group.LargeCollidableStructure && e.Distance < Settings.Instance.DroneControlRange) > 0;

                        if (_States.CurrentQuestorState != QuestorState.CombatMissionsBehavior)
                        {
                            launch &= Cache.Instance.Entities.Count(e => !e.IsSentry && !e.IsBadIdea && e.CategoryId == (int)CategoryID.Entity && e.IsNpc && !e.IsContainer && e.GroupId != (int)Group.LargeCollidableStructure && e.Distance < Settings.Instance.DroneControlRange) > 0;
                        }
                        // If drones get aggro'd within 30 seconds, then wait (5 * _recallCount + 5) seconds since the last recall
                        if (_lastLaunch < _lastRecall && _lastRecall.Subtract(_lastLaunch).TotalSeconds < 30)
                        {
                            if (_lastRecall.AddSeconds(5 * _recallCount + 5) < DateTime.Now)
                            {
                                // Increase recall count and allow the launch
                                _recallCount++;

                                // Never let _recallCount go above 5
                                if (_recallCount > 5)
                                    _recallCount = 5;
                            }
                            else
                            {
                                // Do not launch the drones until the delay has passed
                                launch = false;
                            }
                        }
                        else // Drones have been out for more then 30s
                            _recallCount = 0;
                    }

                    if (launch)
                    {
                        // Reset launch tries
                        _launchTries = 0;
                        _lastLaunch = DateTime.Now;
                        _States.CurrentDroneState = DroneState.Launch;
                    }
                    break;

                case DroneState.Launch:
                    // Launch all drones
                    Recall = false;
                    _launchTimeout = DateTime.Now;
                    Cache.Instance.DirectEve.ActiveShip.LaunchAllDrones();
                    _States.CurrentDroneState = DroneState.Launching;
                    break;

                case DroneState.Launching:
                    // We haven't launched anything yet, keep waiting
                    if (!Cache.Instance.ActiveDrones.Any())
                    {
                        if (DateTime.Now.Subtract(_launchTimeout).TotalSeconds > 10)
                        {
                            // Relaunch if tries < 10
                            if (_launchTries < 10)
                            {
                                _launchTries++;
                                _States.CurrentDroneState = DroneState.Launch;
                                break;
                            }
                            else
                                _States.CurrentDroneState = DroneState.OutOfDrones;
                        }
                        break;
                    }

                    // Are we done launching?
                    if (_lastDroneCount == Cache.Instance.ActiveDrones.Count())
                        _States.CurrentDroneState = DroneState.Fighting;
                    break;

                case DroneState.OutOfDrones:
                    //if (DateTime.Now.Subtract(_launchTimeout).TotalSeconds > 1000)
                    //{
                    //    State = DroneState.WaitingForTargets;
                    //}
                    TargetingCache.CurrentDronesTarget = null;
                    break;

                case DroneState.Fighting:
                    // Should we recall our drones? This is a possible list of reasons why we should

                    if (!Cache.Instance.ActiveDrones.Any())
                    {
                        Logging.Log("Drones", "Apparently we have lost all our drones", Logging.orange);
                        Recall = true;
                    }
                    else
                    {
                        if (Cache.Instance.PriorityTargets.Any(pt => pt.IsWarpScramblingMe))
                        {
                            EntityCache warpscrambledby = Cache.Instance.PriorityTargets.FirstOrDefault(pt => pt.IsWarpScramblingMe);
                            if (warpscrambledby != null && _nextWrapScrambledWarning > DateTime.Now)
                            {
                                _nextWrapScrambledWarning = DateTime.Now.AddSeconds(20);
                                Logging.Log("Drones", "We are scrambled by: [" + Logging.white + warpscrambledby.Name + Logging.orange + "][" + Logging.white + Math.Round(warpscrambledby.Distance, 0) + Logging.orange + "][" + Logging.white + warpscrambledby.Id + Logging.orange + "]",
                                            Logging.orange);
                                Recall = false;
                                WarpScrambled = true;
                            }
                        }
                        else
                        {
                            //Logging.Log("Drones: We are not warp scrambled at the moment...");
                            WarpScrambled = false;
                        }
                    }

                    if (!Recall)
                    {
                        // Are we done (for now) ?
                        if (
                            Cache.Instance.TargetedBy.Count(
                                e => !e.IsSentry && e.IsNpc && e.Distance < Settings.Instance.DroneControlRange) == 0)
                        {
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because no NPC is targeting us within dronerange", Logging.magenta);
                            Recall = true;
                        }

                        if (!Recall & (Cache.Instance.IsMissionPocketDone) && !WarpScrambled)
                        {
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because we are done with this pocket.", Logging.magenta);
                            Recall = true;
                        }
                        else if (!Recall & (_shieldPctTotal > GetShieldPctTotal()))
                        {
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because drones have lost some shields! [Old: " +
                                        _shieldPctTotal.ToString("N2") + "][New: " + GetShieldPctTotal().ToString("N2") +
                                        "]", Logging.magenta);
                            Recall = true;
                        }
                        else if (!Recall & (_armorPctTotal > GetArmorPctTotal()))
                        {
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because drones have lost some armor! [Old:" +
                                        _armorPctTotal.ToString("N2") + "][New: " + GetArmorPctTotal().ToString("N2") +
                                        "]", Logging.magenta);
                            Recall = true;
                        }
                        else if (!Recall & (_structurePctTotal > GetStructurePctTotal()))
                        {
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because drones have lost some structure! [Old:" +
                                        _structurePctTotal.ToString("N2") + "][New: " +
                                        GetStructurePctTotal().ToString("N2") + "]", Logging.magenta);
                            Recall = true;
                        }
                        else if (!Recall & (Cache.Instance.ActiveDrones.Count() < _lastDroneCount))
                        {
                            // Did we lose a drone? (this should be covered by total's as well though)
                            Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones because we have lost a drone! [Old:" + _lastDroneCount +
                                        "][New: " + Cache.Instance.ActiveDrones.Count() + "]", Logging.orange);
                            Recall = true;
                        }
                        else if (!Recall)
                        {
                            // Default to long range recall
                            int lowShieldWarning = Settings.Instance.LongRangeDroneRecallShieldPct;
                            int lowArmorWarning = Settings.Instance.LongRangeDroneRecallArmorPct;
                            int lowCapWarning = Settings.Instance.LongRangeDroneRecallCapacitorPct;

                            if (Cache.Instance.ActiveDrones.Average(d => d.Distance) <
                                (Settings.Instance.DroneControlRange / 2d))
                            {
                                lowShieldWarning = Settings.Instance.DroneRecallShieldPct;
                                lowArmorWarning = Settings.Instance.DroneRecallArmorPct;
                                lowCapWarning = Settings.Instance.DroneRecallCapacitorPct;
                            }

                            if (Cache.Instance.DirectEve.ActiveShip.ShieldPercentage < lowShieldWarning && !WarpScrambled)
                            {
                                Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to shield [" +
                                            Math.Round(Cache.Instance.DirectEve.ActiveShip.ShieldPercentage,0) + "%] below [" +
                                            lowShieldWarning + "%] minimum", Logging.orange);
                                Recall = true;
                            }
                            else if (Cache.Instance.DirectEve.ActiveShip.ArmorPercentage < lowArmorWarning && !WarpScrambled)
                            {
                                Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to armor [" +
                                            Math.Round(Cache.Instance.DirectEve.ActiveShip.ArmorPercentage,0) + "%] below [" +
                                            lowArmorWarning + "%] minimum", Logging.orange);
                                Recall = true;
                            }
                            else if (Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage < lowCapWarning && !WarpScrambled)
                            {
                                Logging.Log("Drones", "Recalling [ " + Cache.Instance.ActiveDrones.Count() + " ] drones due to capacitor [" +
                                            Math.Round(Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage,0) + "%] below [" +
                                            lowCapWarning + "%] minimum", Logging.orange);
                                Recall = true;
                            }
                        }
                    }

                    // Recall or engage
                    if (Recall)
                    {
                        Statistics.Instance.DroneRecalls++;
                        _States.CurrentDroneState = DroneState.Recalling;
                    }
                    else
                    {
                        EngageTarget();

                        // We lost a drone and did not recall, assume panicking and launch (if any) additional drones
                        if (Cache.Instance.ActiveDrones.Count() < _lastDroneCount)
                            _States.CurrentDroneState = DroneState.Launch;
                    }
                    break;

                case DroneState.Recalling:
                    // Are we done?
                    if (!Cache.Instance.ActiveDrones.Any())
                    {
                        _lastRecall = DateTime.Now;
                        Recall = false;
                        TargetingCache.CurrentDronesTarget = null;
                        _States.CurrentDroneState = DroneState.WaitingForTargets;
                        break;
                    }

                    // Give recall command every 15 seconds
                    if (DateTime.Now.Subtract(_lastRecallCommand).TotalSeconds > 15)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdDronesReturnToBay);
                        _lastRecallCommand = DateTime.Now;
                    }
                    break;
                case DroneState.Idle:
                    //
                    // below is the reasons we will start the combat state(s) - if the below is not met do nothing
                    //
                    if (Cache.Instance.InSpace &&
                        Cache.Instance.DirectEve.ActiveShip.Entity != null &&
                        !Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked &&
                        Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != Settings.Instance.CombatShipName &&
                        Settings.Instance.UseDrones &&
                        !Cache.Instance.InWarp)
                    {
                        _States.CurrentDroneState = DroneState.WaitingForTargets;
                        return;
                    }
                    TargetingCache.CurrentDronesTarget = null;
                    break;
            }
            // Update health values
            _shieldPctTotal = GetShieldPctTotal();
            _armorPctTotal = GetArmorPctTotal();
            _structurePctTotal = GetStructurePctTotal();
            _lastDroneCount = Cache.Instance.ActiveDrones.Count();
            GetDamagedDrones();
        }
    }
}