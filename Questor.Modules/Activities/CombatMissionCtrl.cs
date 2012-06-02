// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using System.Globalization;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.States;
    using global::Questor.Modules.Combat;
    using global::Questor.Modules.Actions;
    using Questor.Modules.Caching;

    //using System.Reflection;

    public class CombatMissionCtrl
    {
        private DateTime? _clearPocketTimeout;
        private static int _currentAction;

        private readonly Dictionary<long, DateTime> _lastWeaponReload = new Dictionary<long, DateTime>();
        private double _lastX;
        private double _lastY;
        private double _lastZ;
        private static List<Actions.Action> _pocketActions;
        private bool _waiting;
        private DateTime _waitingSince;
        private DateTime _moveToNextPocket = DateTime.MaxValue;

        private bool _targetNull = false;

        public long AgentId { get; set; }

        public CombatMissionCtrl()
        {
            _pocketActions = new List<Actions.Action>();
        }

        public string Mission { get; set; }

        private void Nextaction()
        {
            // make sure all approach / orbit / align timers are reset (why cant we wait them out in the next action!?)
            Cache.Instance.NextApproachAction = DateTime.Now;
            Cache.Instance.NextOrbit = DateTime.Now;
            Cache.Instance.NextAlign = DateTime.Now;
            // now that we've completed this action revert OpenWrecks to false
            Cache.Instance.OpenWrecks = false;
            Cache.Instance.MissionLoot = false;
            _currentAction++;
        }

        public static void NavigateIntoRange(EntityCache target)
        {
            if (Cache.Instance.InWarp || Cache.Instance.InStation)
                return;

            if (Settings.Instance.SpeedTank)
            {   //this should be only executed when no specific actions
                if (DateTime.Now > Cache.Instance.NextOrbit)
                {
                    if (target.Distance + (int)Cache.Instance.OrbitDistance < Cache.Instance.MaxRange)
                    {
                        //Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction] ,"StartOrbiting: Target in range");
                        if (!Cache.Instance.IsApproachingOrOrbiting)
                        {
                            Logging.Log("CombatMissionCtrl.NavigateIntoRange", "We are not approaching nor orbiting", Logging.teal);
                            const bool orbitStructure = true;
                            var structure = Cache.Instance.Entities.Where(i => i.GroupId == (int)Group.LargeCollidableStructure || i.Name.Contains("Gate") || i.Name.Contains("Beacon")).OrderBy(t => t.Distance).OrderBy(t => t.Distance).FirstOrDefault();

                            if (orbitStructure && structure != null)
                            {
                                structure.Orbit((int)Cache.Instance.OrbitDistance);
                                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Initiating Orbit [" + structure.Name + "][ID: " + structure.Id + "]", Logging.teal);
                            }
                            else
                            {
                                target.Orbit(Cache.Instance.OrbitDistance);
                                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Initiating Orbit [" + target.Name + "][ID: " + target.Id + "]", Logging.teal);
                            }
                            Cache.Instance.NextOrbit = DateTime.Now.AddSeconds((int)Time.OrbitDelay_seconds);
                            return;
                        }
                    }
                    else
                    {
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Possible out of range. ignoring orbit around structure", Logging.teal);
                        target.Orbit(Cache.Instance.OrbitDistance);
                        Logging.Log("CombatMissionCtrlcode." + _pocketActions[_currentAction], "Initiating Orbit [" + target.Name + "][ID: " + target.Id + "]", Logging.teal);
                        Cache.Instance.NextOrbit = DateTime.Now.AddSeconds((int)Time.OrbitDelay_seconds);
                        return;
                    }
                }
            }
            else //if we aren't speed tanking then check optimalrange setting, if that isn't set use the less of targeting range and weapons range to dictate engagement range
            {
                if (DateTime.Now > Cache.Instance.NextApproachAction)
                {
                    //if optimalrange is set - use it to determine engagement range
                    if (Settings.Instance.OptimalRange != 0)
                    {
                        if (target.Distance > Settings.Instance.OptimalRange + (int)Distance.OptimalRangeCushion && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != target.Id))
                        {
                            target.Approach(Settings.Instance.OptimalRange);
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Using Optimal Range: Approaching target [" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.teal);
                        }
                        //I think when approach distance will be reached ship will be stopped so this is not needed
                        if (target.Distance <= Settings.Instance.OptimalRange && Cache.Instance.Approaching != null)
                        {
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                            Cache.Instance.Approaching = null;
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Using Optimal Range: Stop ship, target at [" + Math.Round(target.Distance / 1000, 0) + "k away] is inside optimal", Logging.teal);
                        }
                    }
                    //if optimalrange is not set use MaxRange (shorter of weapons range and targeting range)
                    else
                    {
                        if (target.Distance > Cache.Instance.MaxRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != target.Id))
                        {
                            target.Approach((int)(Cache.Instance.WeaponRange * 0.8d));
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Using Weapons Range: Approaching target [" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.teal);
                        }
                        //I think when approach distance will be reached ship will be stopped so this is not needed
                        if (target.Distance <= Cache.Instance.MaxRange && Cache.Instance.Approaching != null)
                        {
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                            Cache.Instance.Approaching = null;
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Using Weapons Range: Stop ship, target is in orbit range", Logging.teal);
                        }
                    }
                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                    return;
                }
            }
        }

        //
        // this action still needs some TLC - currently broken (unimplemented)
        //
        private void NavigateToObject(EntityCache target)  //this needs to accept a distance parameter....
        {
            if (Settings.Instance.SpeedTank)
            {   //this should be only executed when no specific actions
                if (DateTime.Now > Cache.Instance.NextOrbit)
                {
                    if (target.Distance + (int)Cache.Instance.OrbitDistance < Cache.Instance.MaxRange)
                    {
                        Logging.Log("CombatMission." + _pocketActions[_currentAction], "StartOrbiting: Target in range", Logging.teal);
                        if (!Cache.Instance.IsApproachingOrOrbiting)
                        {
                            Logging.Log("CombatMissionCtrl.NavigateToObject", "We are not approaching nor orbiting", Logging.teal);
                            const bool orbitStructure = true;
                            var structure = Cache.Instance.Entities.Where(i => i.GroupId == (int)Group.LargeCollidableStructure || i.Name.Contains("Gate") || i.Name.Contains("Beacon")).OrderBy(t => t.Distance).OrderBy(t => t.Distance).FirstOrDefault();

                            if (orbitStructure && structure != null)
                            {
                                structure.Orbit((int)Cache.Instance.OrbitDistance);
                                Logging.Log("CombatMission." + _pocketActions[_currentAction], "Initiating Orbit [" + structure.Name + "][ID: " + structure.Id + "]", Logging.teal);
                            }
                            else
                            {
                                target.Orbit(Cache.Instance.OrbitDistance);
                                Logging.Log("CombatMission." + _pocketActions[_currentAction], "Initiating Orbit [" + target.Name + "][ID: " + target.Id + "]", Logging.teal);
                            }
                            Cache.Instance.NextOrbit = DateTime.Now.AddSeconds((int)Time.OrbitDelay_seconds);
                            return;
                        }
                    }
                    else
                    {
                        Logging.Log("CombatMission." + _pocketActions[_currentAction], "Possible out of range. ignoring orbit around structure", Logging.teal);
                        target.Orbit(Cache.Instance.OrbitDistance);
                        Logging.Log("CombatMissionCtrlcode." + _pocketActions[_currentAction], "Initiating Orbit [" + target.Name + "][ID: " + target.Id + "]", Logging.teal);
                        Cache.Instance.NextOrbit = DateTime.Now.AddSeconds((int)Time.OrbitDelay_seconds);
                        return;
                    }
                }
            }
            else //if we aren't speed tanking then check optimalrange setting, if that isn't set use the less of targeting range and weapons range to dictate engagement range
            {
                if (DateTime.Now > Cache.Instance.NextApproachAction)
                {
                    //if optimalrange is set - use it to determine engagement range
                    //
                    // this assumes that both optimal range and missile boats both want to be within 5k of the object they asked us to navigate to
                    //
                    if (target.Distance > Cache.Instance.MaxRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != target.Id))
                    {
                        target.Approach((int)(Distance.SafeDistancefromStructure));
                        Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                        Logging.Log("CombatMission." + _pocketActions[_currentAction], "Using SafeDistanceFromStructure: Approaching target [" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.teal);
                    }
                    return;
                }
            }
        }

        private void BookmarkPocketForSalvaging()
        {
            // Nothing to loot
            if (Cache.Instance.UnlootedContainers.Count() < Settings.Instance.MinimumWreckCount)
            {
                // If Settings.Instance.LootEverything is false we may leave behind a lot of unlooted containers.
                // This scenario only happens when all wrecks are within tractor range and you have a salvager
                // (typically only with a Golem).  Check to see if there are any cargo containers in space.  Cap
                // boosters may cause an unneeded salvage trip but that is better than leaving millions in loot behind.
                if (DateTime.Now > Cache.Instance.NextBookmarkPocketAttempt)
                {
                    Cache.Instance.NextBookmarkPocketAttempt = DateTime.Now.AddSeconds((int)Time.BookmarkPocketRetryDelay_seconds);
                    if (!Settings.Instance.LootEverything && Cache.Instance.Containers.Count() < Settings.Instance.MinimumWreckCount)
                    {
                        Logging.Log("CombatMissionCtrl", "No bookmark created because the pocket has [" + Cache.Instance.Containers.Count() + "] wrecks/containers and the minimum is [" + Settings.Instance.MinimumWreckCount + "]", Logging.teal);
                    }
                    else if (Settings.Instance.LootEverything)
                    {
                        Logging.Log("CombatMissionCtrl", "No bookmark created because the pocket has [" + Cache.Instance.UnlootedContainers.Count() + "] wrecks/containers and the minimum is [" + Settings.Instance.MinimumWreckCount + "]", Logging.teal);
                    }
                }
            }
            else
            {
                // Do we already have a bookmark?
                List<DirectBookmark> bookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");
                DirectBookmark bookmark = bookmarks.FirstOrDefault(b => Cache.Instance.DistanceFromMe(b.X ?? 0, b.Y ?? 0, b.Z ?? 0) < (int)Distance.OnGridWithMe);
                if (bookmark != null)
                {
                    Logging.Log("CombatMissionCtrl", "Pocket already bookmarked for salvaging [" + bookmark.Title + "]", Logging.teal);
                }
                else
                {
                    // No, create a bookmark
                    string label = string.Format("{0} {1:HHmm}", Settings.Instance.BookmarkPrefix, DateTime.UtcNow);
                    //IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Distance);
                    Logging.Log("CombatMissionCtrl", "Bookmarking pocket for salvaging [" + label + "]", Logging.teal);
                    Cache.Instance.CreateBookmark(label);
                    //Cache.Instance.CreateBookmarkofwreck(containers,label);
                    bookmark = null;
                    bookmarks = null;
                    label = null;
                }
            }
        }

        private void DoneAction()
        {
            // Tell the drones module to retract drones
            Cache.Instance.IsMissionPocketDone = true;
            Cache.Instance.UseDrones = true;

            // We do not switch to "done" status if we still have drones out
            if (Cache.Instance.ActiveDrones.Any())
                return;

            // Add bookmark (before we're done)
            if (Settings.Instance.CreateSalvageBookmarks)
                BookmarkPocketForSalvaging();

            _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.Done;
        }

        private void ActivateAction(Actions.Action action)
        {
            bool optional;
            if (!bool.TryParse(action.GetParameterValue("optional"), out optional))
                optional = false;

            string target = action.GetParameterValue("target");

            // No parameter? Although we shouldn't really allow it, assume its the acceleration gate :)
            if (string.IsNullOrEmpty(target))
                target = "Acceleration Gate";

            IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByName(target).ToList();
            if (!targets.Any())
            {
                if (!_waiting)
                {
                    Logging.Log("CombatMissionCtrl", "Activate: Can't find [" + target + "] to activate! Waiting 30 seconds before giving up", Logging.teal);
                    _waitingSince = DateTime.Now;
                    _waiting = true;
                }
                else if (_waiting)
                {
                    if (DateTime.Now.Subtract(_waitingSince).TotalSeconds > (int)Time.NoGateFoundRetryDelay_seconds)
                    {
                        Logging.Log("CombatMissionCtrl",
                                    "Activate: After 30 seconds of waiting the gate is still not on grid: CombatMissionCtrlState.Error",
                                    Logging.teal);
                        if (optional) //if this action has the optional paramater defined as true then we are done if we cant find the gate
                            DoneAction();
                        else
                            _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.Error;
                    }
                }
                return;
            }

            //if (closest.Distance <= (int)Distance.CloseToGateActivationRange) // if your distance is less than the 'close enough' range, default is 7000 meters
            EntityCache closest = targets.OrderBy(t => t.Distance).First();
            if (closest.Distance < (int)Distance.GateActivationRange + 5000)
            {
                // Tell the drones module to retract drones
                Cache.Instance.IsMissionPocketDone = true;

                // We cant activate if we have drones out
                if (Cache.Instance.ActiveDrones.Any())
                    return;

                //
                // this is a bad idea for a speed tank, we ought to somehow cache the object they are orbiting/approaching, etc
                // this seemingly slowed down the exit from certain missions for me for 2-3min as it had a command to orbit some random object
                // after the "done" command
                //
                if (closest.Distance < -10100)
                {
                    if (DateTime.Now > Cache.Instance.NextOrbit)
                    {
                        closest.Orbit(1000);
                        Logging.Log("CombatMissionCtrl", "Activate: We are too close to [" + closest.Name + "] Initiating orbit", Logging.orange);
                        Cache.Instance.NextOrbit = DateTime.Now.AddSeconds((int)Time.OrbitDelay_seconds);
                    }
                    return;
                }
                //Logging.Log("CombatMissionCtrl","distance " + closest.Distance);
                //if ((closest.Distance <= (int)Distance.TooCloseToStructure) && (DateTime.Now.Subtract(Cache.Instance._lastOrbit).TotalSeconds > 30)) //-10100 meters (inside docking ring) - so close that we may get tangled in the structure on activation - move away
                //{
                //    Logging.Log("CombatMissionCtrl.Activate: Too close to Structure to activate: orbiting");
                //    closest.Orbit((int)Distance.GateActivationRange); // 1000 meters
                //    Cache.Instance._nextOrbit = DateTime.Now.AddSeconds((int)Time.OrbitDelay_seconds);
                //}

                //if (closest.Distance >= (int)Distance.TooCloseToStructure) //If we aren't so close that we may get tangled in the structure, activate it
                if (closest.Distance >= -10100)
                {
                    // Add bookmark (before we activate)
                    if (Settings.Instance.CreateSalvageBookmarks)
                        BookmarkPocketForSalvaging();

                    // Reload weapons and activate gate to move to the next pocket
                    if (DateTime.Now > Cache.Instance.NextReload)
                    {
                        //Logging.Log("CombatMissionCtrl", "Activate: Reload before moving to next pocket", Logging.teal);
                        Combat.ReloadAll();
                        Cache.Instance.NextReload = DateTime.Now.AddSeconds((int)Time.ReloadWeaponDelayBeforeUsable_seconds);
                    }
                    if (DateTime.Now > Cache.Instance.NextActivateAction)
                    {
                        Logging.Log("CombatMissionCtrl", "Activate: [" + closest.Name + "] Move to next pocket after reload command and change state to 'NextPocket'", Logging.green);
                        closest.Activate();

                        // Do not change actions, if NextPocket gets a timeout (>2 mins) then it reverts to the last action
                        Cache.Instance.NextActivateAction = DateTime.Now.AddSeconds(15);
                        _moveToNextPocket = DateTime.Now;
                        _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.NextPocket;
                    }
                    return;
                }
            }
            else if (closest.Distance < (int)Distance.WarptoDistance) //else if (closest.Distance < (int)Distance.WarptoDistance) //if we are inside warpto distance then approach
            {
                // Move to the target
                if (DateTime.Now > Cache.Instance.NextApproachAction && (Cache.Instance.IsOrbiting || Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id))
                {
                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                    Logging.Log("CombatMissionCtrl.Activate", "Approaching target [" + closest.Name + "][ID: " + closest.Id + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.teal);
                    closest.Approach();
                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                }
                else if (Cache.Instance.IsOrbiting || Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)
                {
                    Logging.Log("CombatMissionCtrl", "Activate: Delaying approach for: [" + Math.Round(Cache.Instance.NextApproachAction.Subtract(DateTime.Now).TotalSeconds, 0) + "] seconds", Logging.teal);
                }
                return;
            }
            else if (closest.Distance > (int)Distance.WarptoDistance)//we must be outside warpto distance, but we are likely in a deadspace so align to the target
            {
                // We cant warp if we have drones out - but we are aligning not warping so we do not care
                //if (Cache.Instance.ActiveDrones.Count() > 0)
                //    return;

                if (DateTime.Now > Cache.Instance.NextAlign)
                {
                    // Only happens if we are asked to Activate something that is outside Distance.CloseToGateActivationRange (default is: 6k)
                    Logging.Log("CombatMissionCtrl", "Activate: AlignTo: [" + closest.Name + "] This only happens if we are asked to Activate something that is outside [" + Distance.CloseToGateActivationRange + "]", Logging.teal);
                    closest.AlignTo();
                    Cache.Instance.NextAlign = DateTime.Now.AddMinutes((int)Time.AlignDelay_minutes);
                }
                else
                {
                    Logging.Log("CombatMissionCtrl", "Activate: Unable to align: Next Align in [" + Cache.Instance.NextAlign.Subtract(DateTime.Now).TotalSeconds + "] seconds", Logging.teal);
                }
                return;
            }
            else //how in the world would we ever get here?
            {
                Logging.Log("CombatMissionCtrl", "Activate: Error: [" + closest.Name + "] at [" + closest.Distance + "] is not within jump distance, within warpable distance or outside warpable distance, (!!!), retrying action.", Logging.teal);
                return;
            }
        }

        private void ClearPocketAction(Actions.Action action)
        {
            if (!Cache.Instance.NormalApproch)
                Cache.Instance.NormalApproch = true;

            // Get lowest range
            double range = Cache.Instance.MaxRange;
            int distancetoclear;
            if (!int.TryParse(action.GetParameterValue("distance"), out distancetoclear))
                distancetoclear = (int)range;

            if (distancetoclear != 0 && distancetoclear != -2147483648 && distancetoclear != 2147483647)
            {
                range = Math.Min(Cache.Instance.MaxRange, distancetoclear);
            }

            int priority;
            if (!int.TryParse(action.GetParameterValue("priority"), out priority))
                priority = (int)range;

            //panic handles adding any priority targets and combat will prefer to kill any priority targets

            // Is there a priority target out of range?
            EntityCache target = Cache.Instance.PriorityTargets.OrderBy(t => t.Distance).FirstOrDefault(t => !(Cache.Instance.IgnoreTargets.Contains(t.Name.Trim()) && !Cache.Instance.TargetedBy.Any(w => w.IsWarpScramblingMe || w.IsNeutralizingMe || w.IsWebbingMe)));
            if (target == null)
                _targetNull = true;
            else
                _targetNull = false;
            // Or is there a target out of range that is targeting us?
            target = target ?? Cache.Instance.TargetedBy.Where(t => !t.IsSentry && !t.IsEntityIShouldLeaveAlone && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim())).OrderBy(t => t.Distance).FirstOrDefault();
            // Or is there any target out of range?
            target = target ?? Cache.Instance.Entities.Where(t => !t.IsSentry && !t.IsEntityIShouldLeaveAlone && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim())).OrderBy(t => t.Distance).FirstOrDefault();
            if (Settings.Instance.KillSentries)
            {
                target = target ?? Cache.Instance.Entities.Where(t => !t.IsEntityIShouldLeaveAlone && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim())).OrderBy(t => t.Distance).FirstOrDefault();    
            }
            
            int targetedby = Cache.Instance.TargetedBy.Count(t => !t.IsSentry && !t.IsEntityIShouldLeaveAlone && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim()));

            if (target != null)
            {
                // Reset timeout
                _clearPocketTimeout = null;

                // Lock target if within weapons range
                if (target.Distance < range)
                {
                    //panic handles adding any priority targets and combat will prefer to kill any priority targets
                    if (_targetNull && targetedby == 0 && DateTime.Now > Cache.Instance.NextReload)
                    {
                        Combat.ReloadAll();
                        Cache.Instance.NextReload = DateTime.Now.AddSeconds((int)Time.ReloadWeaponDelayBeforeUsable_seconds);
                        return;
                    }

                    if (Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets > 0)
                    {
                        if (target.IsTarget || target.IsTargeting) //This target is already targeted no need to target it again
                        {
                            return;
                        }
                        else
                        {
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Targeting [" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.teal);
                            target.LockTarget();
                        }
                    }
                    return;
                }
                else //target is not in range...
                {
                    if (DateTime.Now > Cache.Instance.NextReload)
                    {
                        //Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction] ,"ReloadAll: Reload weapons",Logging.teal);
                        Combat.ReloadAll();
                        Cache.Instance.NextReload = DateTime.Now.AddSeconds((int)Time.ReloadWeaponDelayBeforeUsable_seconds);
                        return;
                    }
                }
                NavigateIntoRange(target);
                return;
            }

            // Do we have a timeout?  No, set it to now + 5 seconds
            if (!_clearPocketTimeout.HasValue)
                _clearPocketTimeout = DateTime.Now.AddSeconds(5);

            // Are we in timeout?
            if (DateTime.Now < _clearPocketTimeout.Value)
                return;

            // We have cleared the Pocket, perform the next action \o/ - reset the timers that we had set for actions...
            target = null;
            targetedby = 0;
            priority = 0;
            distancetoclear = 0;
            Nextaction();

            // Reset timeout
            _clearPocketTimeout = null;
        }

        private void ClearWithinWeaponsRangeOnlyAction(Actions.Action action)
        {
            // Get lowest range
            double distancetoconsidertargets = Cache.Instance.MaxRange;

            int distancetoclear;
            if (!int.TryParse(action.GetParameterValue("distance"), out distancetoclear))
                distancetoclear = (int)distancetoconsidertargets;

            if (distancetoclear != 0 && distancetoclear != -2147483648 && distancetoclear != 2147483647)
            {
                distancetoconsidertargets = Math.Min(Cache.Instance.MaxRange, distancetoclear);
            }

            EntityCache target = Cache.Instance.PriorityTargets.OrderBy(t => t.Distance).FirstOrDefault(t => t.Distance < distancetoconsidertargets && !(Cache.Instance.IgnoreTargets.Contains(t.Name.Trim()) && !Cache.Instance.TargetedBy.Any(w => w.IsWarpScramblingMe || w.IsNeutralizingMe || w.IsWebbingMe)));

            // Or is there a target within distancetoconsidertargets that is targeting us?
            target = target ?? Cache.Instance.TargetedBy.Where(t => t.Distance < distancetoconsidertargets && !t.IsEntityIShouldLeaveAlone && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim())).OrderBy(t => t.Distance).FirstOrDefault();
            // Or is there any target within distancetoconsidertargets?
            target = target ?? Cache.Instance.Entities.Where(t => t.Distance < distancetoconsidertargets && !t.IsEntityIShouldLeaveAlone && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim())).OrderBy(t => t.Distance).FirstOrDefault();

            if (target != null)
            {
                // Reset timeout
                _clearPocketTimeout = null;

                // Lock priority target if within weapons range
                if (target.Distance < Cache.Instance.MaxRange)
                {
                    //panic handles adding any priority targets and combat will prefer to kill any priority targets
                    if (Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets > 0)
                    {
                        if (target.IsTarget || target.IsTargeting) //This target is already targeted no need to target it again
                        {
                            return;
                        }
                        else
                        {
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Targeting [" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.teal);
                            target.LockTarget();
                        }
                    }
                    return;
                }
            }

            // Do we have a timeout?  No, set it to now + 5 seconds
            if (!_clearPocketTimeout.HasValue)
                _clearPocketTimeout = DateTime.Now.AddSeconds(5);

            // Are we in timeout?
            if (DateTime.Now < _clearPocketTimeout.Value)
                return;

            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "is complete: no more targets in weapons range", Logging.teal);
            target = null;
            distancetoclear = 0;
            Nextaction();

            // Reset timeout
            _clearPocketTimeout = null;
        }

        private void MoveToBackgroundAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproch)
                Cache.Instance.NormalApproch = false;

            int distancetoapp;
            if (!int.TryParse(action.GetParameterValue("distance"), out distancetoapp))
                distancetoapp = 1000;

            string target = action.GetParameterValue("target");

            // No parameter? Although we shouldn't really allow it, assume its the acceleration gate :)
            if (string.IsNullOrEmpty(target))
                target = "Acceleration Gate";

            IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByName(target).ToList();
            if (!targets.Any())
            {
                // Unlike activate, no target just means next action
                _currentAction++;
                return;
            }

            EntityCache closest = targets.OrderBy(t => t.Distance).First();
            // Move to the target
            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Approaching target [" + closest.Name + "][ID: " + closest.Id + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.teal);
            closest.Approach(distancetoapp);
            Nextaction();
        }

        private void MoveToAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproch)
                Cache.Instance.NormalApproch = false;

            string target = action.GetParameterValue("target");

            // No parameter? Although we shouldn't really allow it, assume its the acceleration gate :)
            if (string.IsNullOrEmpty(target))
                target = "Acceleration Gate";

            int distancetoapp;
            if (!int.TryParse(action.GetParameterValue("distance"), out distancetoapp))
                distancetoapp = 1000;

            bool stopWhenTargeted;
            if (!bool.TryParse(action.GetParameterValue("StopWhenTargeted"), out stopWhenTargeted))
                stopWhenTargeted = false;

            bool stopWhenAggressed;
            if (!bool.TryParse(action.GetParameterValue("StopWhenAggressed"), out stopWhenAggressed))
                stopWhenAggressed = false;

            IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByName(target).ToList();
            if (!targets.Any())
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "no entities found named [" + target + "] proceeding to next action", Logging.teal);
                Nextaction();
                return;
            }

            EntityCache closest = targets.OrderBy(t => t.Distance).First();

            if (stopWhenTargeted)
            {
                IEnumerable<EntityCache> targetedBy = Cache.Instance.TargetedBy;
                if (targetedBy != null && targetedBy.Any())
                {
                    if (Cache.Instance.Approaching != null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                        Cache.Instance.Approaching = null;
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Stop ship, we have been targeted and are [" + distancetoapp + "] from [ID: " +
                                    closest.Name + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.teal);
                    }
                }
            }

            if (stopWhenAggressed)
            {
                IEnumerable<EntityCache> targetedBy = Cache.Instance.TargetedBy;
                if (Cache.Instance.Aggressed.Any(t => !t.IsSentry))
                {
                    if (Cache.Instance.Approaching != null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                        Cache.Instance.Approaching = null;
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Stop ship, we have been targeted and are [" + distancetoapp + "] from [ID: " +
                                    closest.Name + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.teal);
                    }
                }
            }

            if (closest.Distance <= distancetoapp + 5000) // if we are inside the range that we are supposed to approach assume we are done
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "We are [" + Math.Round(closest.Distance, 0) + "] from a [" + target + "] we do not need to go any further", Logging.teal);
                Nextaction();

                if (Cache.Instance.Approaching != null)
                {
                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                    Cache.Instance.Approaching = null;
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Stop ship, we are [" + distancetoapp + "] from [ID: " + closest.Name + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.teal);
                }
                //if (Settings.Instance.SpeedTank)
                //{
                //    //this should at least keep speed tanked ships from going poof if a mission XML uses moveto
                //    closest.Orbit(Cache.Instance.OrbitDistance);
                //    Logging.Log("CombatMissionCtrl","MoveTo: Initiating orbit after reaching target")
                //}
                return;
            }
            else if (closest.Distance < (int)Distance.WarptoDistance) // if we are inside warptorange you need to approach (you cant warp from here)
            {
                // Move to the target
                if (DateTime.Now > Cache.Instance.NextApproachAction && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id))
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Approaching target [" + closest.Name + "][ID: " + closest.Id + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.teal);
                    closest.Approach();
                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                }
                return;
            }
            else // if we are outside warpto distance (presumably inside a deadspace where we cant warp) align to the target
            {
                if (DateTime.Now > Cache.Instance.NextAlign)
                {
                    // Probably never happens
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Aligning to target [" + closest.Name + "][ID: " + closest.Id + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.teal);
                    closest.AlignTo();
                    Cache.Instance.NextAlign = DateTime.Now.AddMinutes((int)Time.AlignDelay_minutes);
                }
                return;
            }
        }

        private void WaitUntilTargeted(Actions.Action action)
        {
            IEnumerable<EntityCache> targetedBy = Cache.Instance.TargetedBy;
            if (targetedBy != null && targetedBy.Any())
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "We have been targeted!", Logging.teal);

                // We have been locked, go go go ;)
                _waiting = false;
                Nextaction();
                return;
            }

            // Default timeout is 30 seconds
            int timeout;
            if (!int.TryParse(action.GetParameterValue("timeout"), out timeout))
                timeout = 30;

            if (_waiting)
            {
                if (DateTime.Now.Subtract(_waitingSince).TotalSeconds < timeout)
                    return;

                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Nothing targeted us within [ " + timeout + "sec]!", Logging.teal);

                // Nothing has targeted us in the specified timeout
                _waiting = false;
                Nextaction();
                return;
            }

            // Start waiting
            _waiting = true;
            _waitingSince = DateTime.Now;
        }

        private void DebuggingWait(Actions.Action action)
        {
            IEnumerable<EntityCache> targetedBy = Cache.Instance.TargetedBy;

            // Default timeout is 1200 seconds
            int timeout;
            if (!int.TryParse(action.GetParameterValue("timeout"), out timeout))
                timeout = 1200;

            if (_waiting)
            {
                if (DateTime.Now.Subtract(_waitingSince).TotalSeconds < timeout)
                    return;

                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Nothing targeted us within [ " + timeout + "sec]!", Logging.teal);

                // Nothing has targeted us in the specified timeout
                _waiting = false;
                Nextaction();
                return;
            }

            // Start waiting
            _waiting = true;
            _waitingSince = DateTime.Now;
        }

        private void AggroOnlyAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproch)
                Cache.Instance.NormalApproch = false;

            bool ignoreAttackers;
            if (!bool.TryParse(action.GetParameterValue("ignoreattackers"), out ignoreAttackers))
                ignoreAttackers = false;

            bool breakOnAttackers;
            if (!bool.TryParse(action.GetParameterValue("breakonattackers"), out breakOnAttackers))
                breakOnAttackers = false;

            bool nottheclosest;
            if (!bool.TryParse(action.GetParameterValue("notclosest"), out nottheclosest))
                nottheclosest = false;

            int numbertoignore;
            if (!int.TryParse(action.GetParameterValue("numbertoignore"), out numbertoignore))
                numbertoignore = 0;

            List<string> targetNames = action.GetParameterValues("target");
            // No parameter? Ignore kill action
            if (targetNames.Count == 0)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "No targets defined!", Logging.teal);
                Nextaction();
                return;
            }

            IEnumerable<EntityCache> targets = Cache.Instance.Entities.Where(e => targetNames.Contains(e.Name)).ToList();
            if (targets.Count() == numbertoignore)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "All targets gone " + targetNames.Aggregate((current, next) => current + "[" + next + "]"), Logging.teal);

                // We killed it/them !?!?!? :)
                Nextaction();
                return;
            }

            if (Cache.Instance.Aggressed.Any(t => !t.IsSentry && targetNames.Contains(t.Name)))
            {
                // We are being attacked, break the kill order
                if (Cache.Instance.RemovePriorityTargets(targets))
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Done with AggroOnly: We have aggro.", Logging.teal);

                foreach (EntityCache target in Cache.Instance.Targets.Where(e => targets.Any(t => t.Id == e.Id)))
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Unlocking [" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away] due to aggro being obtained", Logging.teal);
                    target.UnlockTarget();
                    return;
                }
                Nextaction();
                return;
            }

            if (!ignoreAttackers || breakOnAttackers)
            {
                // Apparently we are busy, wait for combat to clear attackers first
                IEnumerable<EntityCache> targetedBy = Cache.Instance.TargetedBy;
                if (targetedBy != null && targetedBy.Count(t => !t.IsSentry && t.Distance < Cache.Instance.WeaponRange) > 0)
                    return;
            }

            EntityCache closest = targets.OrderBy(t => t.Distance).First();

            if (nottheclosest)
                closest = targets.OrderByDescending(t => t.Distance).First();

            //panic handles adding any priority targets and combat will prefer to kill any priority targets
            if (!Cache.Instance.PriorityTargets.Any(pt => pt.Id == closest.Id))
            {
                //Adds the target we want to kill to the priority list so that combat.cs will kill it (especially if it is an LCO this is important)
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Adding [" + closest.Name + "][ID: " + closest.Id + "] as a priority target", Logging.teal);
                Cache.Instance.AddPriorityTargets(new[] { closest }, Priority.PriorityKillTarget);
            }
        }

        private void KillAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproch)
                Cache.Instance.NormalApproch = false;

            bool ignoreAttackers;
            if (!bool.TryParse(action.GetParameterValue("ignoreattackers"), out ignoreAttackers))
                ignoreAttackers = false;

            bool breakOnAttackers;
            if (!bool.TryParse(action.GetParameterValue("breakonattackers"), out breakOnAttackers))
                breakOnAttackers = false;

            bool nottheclosest;
            if (!bool.TryParse(action.GetParameterValue("notclosest"), out nottheclosest))
                nottheclosest = false;

            int numbertoignore;
            if (!int.TryParse(action.GetParameterValue("numbertoignore"), out numbertoignore))
                numbertoignore = 0;

            List<string> targetNames = action.GetParameterValues("target");
            // No parameter? Ignore kill action
            if (targetNames.Count == 0)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "No targets defined in kill action!", Logging.teal);
                Nextaction();
                return;
            }

            IEnumerable<EntityCache> targets = Cache.Instance.Entities.Where(e => targetNames.Contains(e.Name)).ToList();
            if (targets.Count() == numbertoignore)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "All targets killed " + targetNames.Aggregate((current, next) => current + "[" + next + "]"), Logging.teal);

                // We killed it/them !?!?!? :)
                Nextaction();
                return;
            }

            if (breakOnAttackers && Cache.Instance.TargetedBy.Any(t => !t.IsSentry && t.Distance < Cache.Instance.WeaponRange))
            {
                // We are being attacked, break the kill order
                if (Cache.Instance.RemovePriorityTargets(targets))
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Breaking off kill order, new spawn has arrived!", Logging.teal);

                foreach (EntityCache entity in Cache.Instance.Targets.Where(e => targets.Any(t => t.Id == e.Id)))
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Unlocking [" + entity.Name + "][ID: " + entity.Id + "][" + Math.Round(entity.Distance / 1000, 0) + "k away] due to kill order being put on hold", Logging.teal);
                    entity.UnlockTarget();
                }

                return;
            }

            if (!ignoreAttackers || breakOnAttackers)
            {
                // Apparently we are busy, wait for combat to clear attackers first
                IEnumerable<EntityCache> targetedBy = Cache.Instance.TargetedBy;
                if (targetedBy != null && targetedBy.Count(t => !t.IsSentry && t.Distance < Cache.Instance.WeaponRange) > 0)
                    return;
            }

            EntityCache target = targets.OrderBy(t => t.Distance).First();
            int targetedby = Cache.Instance.TargetedBy.Count(t => !t.IsSentry && !t.IsEntityIShouldLeaveAlone && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim()));
            if (target != null)
            {
                // Reset timeout
                _clearPocketTimeout = null;

                // Are we approaching the active (out of range) target?
                // Wait for it (or others) to get into range

                // Lock priority target if within weapons range

                if (nottheclosest)
                    target = targets.OrderByDescending(t => t.Distance).First();

                if (target.Distance < Cache.Instance.MaxRange)
                {
                    //panic handles adding any priority targets and combat will prefer to kill any priority targets
                    if (!Cache.Instance.PriorityTargets.Any(pt => pt.Id == target.Id))
                    {
                        //Adds the target we want to kill to the priority list so that combat.cs will kill it (especially if it is an LCO this is important)
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Adding [" + target.Name + "][ID: " + target.Id + "] as a priority target", Logging.teal);
                        Cache.Instance.AddPriorityTargets(new[] { target }, Priority.PriorityKillTarget);
                    }
                    if (_targetNull && targetedby == 0 && DateTime.Now > Cache.Instance.NextReload)
                    {
                        //Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction] ,"Reload if [" + _targetNull + "] && [" + targetedby + "] == 0 AND [" + Math.Round(target.Distance, 0) + "] < [" + Cache.Instance.MaxRange + "]", Logging.teal);
                        Combat.ReloadAll();
                        Cache.Instance.NextReload = DateTime.Now.AddSeconds((int)Time.ReloadWeaponDelayBeforeUsable_seconds);
                        return;
                    }

                    if (Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets > 0)
                    {
                        if (!(target.IsTarget || target.IsTargeting)) //This target is not targeted and need to target it
                        {
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Targeting [" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.teal);
                            target.LockTarget();
                            return;
                        }
                    }
                }
                else
                {
                    if (DateTime.Now > Cache.Instance.NextReload)
                    {
                        //Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction] ,"ReloadAll: Reload weapons", Logging.teal);
                        Combat.ReloadAll();
                        Cache.Instance.NextReload = DateTime.Now.AddSeconds((int)Time.ReloadWeaponDelayBeforeUsable_seconds);
                        return;
                    }
                }
                NavigateIntoRange(target);
                return;
            }
        }

        private void KillOnceAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproch)
                Cache.Instance.NormalApproch = false;

            bool nottheclosest;
            if (!bool.TryParse(action.GetParameterValue("notclosest"), out nottheclosest))
                nottheclosest = false;

            int numbertoignore;
            if (!int.TryParse(action.GetParameterValue("numbertoignore"), out numbertoignore))
                numbertoignore = 0;

            List<string> targetNames = action.GetParameterValues("target");
            // No parameter? Ignore kill action
            if (targetNames.Count == 0)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "No targets defined in kill action!", Logging.orange);
                Nextaction();
                return;
            }

            IEnumerable<EntityCache> targets = Cache.Instance.Entities.Where(e => targetNames.Contains(e.Name)).ToList();
            if (targets.Count() == numbertoignore)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "All targets killed " + targetNames.Aggregate((current, next) => current + "[" + next + "]"), Logging.teal);

                // We killed it/them !?!?!? :)
                Nextaction();
                return;
            }

            EntityCache target = targets.OrderBy(t => t.Distance).First();
            int targetedby = Cache.Instance.TargetedBy.Count(t => !t.IsSentry && !t.IsEntityIShouldLeaveAlone && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim()));
            if (target != null)
            {
                // Reset timeout
                _clearPocketTimeout = null;

                // Are we approaching the active (out of range) target?
                // Wait for it (or others) to get into range

                // Lock priority target if within weapons range

                if (nottheclosest)
                    target = targets.OrderByDescending(t => t.Distance).First();

                if (target.Distance < Cache.Instance.MaxRange)
                {
                    //panic handles adding any priority targets and combat will prefer to kill any priority targets
                    if (!Cache.Instance.PriorityTargets.Any(pt => pt.Id == target.Id))
                    {
                        //Adds the target we want to kill to the priority list so that combat.cs will kill it (especially if it is an LCO this is important)
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Adding [" + target.Name + "][ID: " + target.Id + "] as a priority target", Logging.teal);
                        Cache.Instance.AddPriorityTargets(new[] { target }, Priority.PriorityKillTarget);
                    }
                    if (Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets > 0)
                    {
                        if (!(target.IsTarget || target.IsTargeting)) //This target is not targeted and need to target it
                        {
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Targeting [" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.teal);
                            target.LockTarget();
                            // the target has been added to the priority targets list and has been targeted.
                            // this should ensure that the combat module (and/or the next action) kills the target.
                            Nextaction();
                            return;
                        }
                    }
                }
                NavigateIntoRange(target);
                return;
            }
        }

        private void UseDrones(Actions.Action action)
        {
            bool usedrones;
            if (!bool.TryParse(action.GetParameterValue("use"), out usedrones))
                usedrones = true;

            if (!usedrones)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Disable launch of drones", Logging.teal);
                Cache.Instance.UseDrones = false;
            }
            else
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Enable launch of drones", Logging.teal);
                Cache.Instance.UseDrones = true;
            }
            Nextaction();
            return;
        }

        private void KillClosestByNameAction(Actions.Action action)
        {
            bool nottheclosest;
            if (!bool.TryParse(action.GetParameterValue("notclosest"), out nottheclosest))
                nottheclosest = false;

            if (Cache.Instance.NormalApproch)
                Cache.Instance.NormalApproch = false;

            List<string> targetNames = action.GetParameterValues("target");
            // No parameter? Ignore kill action
            if (targetNames.Count == 0)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "No targets defined!", Logging.teal);
                Nextaction();
                return;
            }

            //IEnumerable<EntityCache> targets = Cache.Instance.Entities.Where(e => targetNames.Contains(e.Name));
            EntityCache target = Cache.Instance.Entities.Where(e => targetNames.Contains(e.Name)).OrderBy(t => t.Distance).First();
            if (nottheclosest)
                target = Cache.Instance.Entities.Where(e => targetNames.Contains(e.Name)).OrderByDescending(t => t.Distance).First();

            if (target != null)
            {
                if (target.Distance < Cache.Instance.MaxRange)
                {
                    if (!Cache.Instance.PriorityTargets.Any(pt => pt.Id == target.Id))
                    {
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Adding [" + target.Name + "][ID: " + target.Id + "] as a priority target", Logging.teal);
                        Cache.Instance.AddPriorityTargets(new[] { target }, Priority.PriorityKillTarget);
                    }

                    if (Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets > 0)
                    {
                        if (!(target.IsTarget || target.IsTargeting))
                        //This target is not targeted and need to target it
                        {
                            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Targeting [" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.teal);
                            target.LockTarget();
                            // the target has been added to the priority targets list and has been targeted.
                            // this should ensure that the combat module (and/or the next action) kills the target.
                            Nextaction();
                            return;
                        }
                    }
                }
                NavigateIntoRange(target);
            }
            else
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "All targets killed, not valid anymore ", Logging.teal);

                // We killed it/them !?!?!? :)
                Nextaction();
                return;
            }
        }

        private void KillClosestAction(Actions.Action action)
        {
            if (Cache.Instance.NormalApproch)
                Cache.Instance.NormalApproch = false;

            bool nottheclosest;
            if (!bool.TryParse(action.GetParameterValue("notclosest"), out nottheclosest))
                nottheclosest = false;

            //IEnumerable<EntityCache> targets = Cache.Instance.Entities.Where(e => targetNames.Contains(e.Name));
            EntityCache target = Cache.Instance.Entities.OrderBy(t => t.Distance).First();
            if (nottheclosest)
                target = Cache.Instance.Entities.OrderByDescending(t => t.Distance).First();
            if (!target.IsValid)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "All targets killed, not valid anymore ", Logging.teal);

                // We killed it/them !?!?!? :)
                Nextaction();
                return;
            }

            if (target.Distance < Cache.Instance.MaxRange)
            {
                if (!Cache.Instance.PriorityTargets.Any(pt => pt.Id == target.Id))
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Adding [" + target.Name + "][ID: " + target.Id + "] as a priority target", Logging.teal);
                    Cache.Instance.AddPriorityTargets(new[] { target }, Priority.PriorityKillTarget);
                }

                if (Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets > 0)
                {
                    if (!(target.IsTarget || target.IsTargeting))
                    //This target is not targeted and need to target it
                    {
                        Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Targeting [" + target.Name + "][ID: " + target.Id + "][" + Math.Round(target.Distance / 1000, 0) + "k away]", Logging.teal);
                        target.LockTarget();
                        // the target has been added to the priority targets list and has been targeted.
                        // this should ensure that the combat module (and/or the next action) kills the target.
                        Nextaction();
                        return;
                    }
                }
            }
            NavigateIntoRange(target);
        }

        //
        // this action still needs some TLC - currently broken (unimplemented)
        //
        private void PutItemAction(Actions.Action action)
        {
            //
            // example syntax:
            // <action name="PutItem">
            //    <parameter name="Item" value="Fajah Ateshi" />
            //    <parameter name="Container" value="Rogue Drone" />
            // </action>
            //
            bool nottheclosest;
            if (!bool.TryParse(action.GetParameterValue("notclosest"), out nottheclosest))
                nottheclosest = false;

            int numbertoignore;
            if (!int.TryParse(action.GetParameterValue("numbertoignore"), out numbertoignore))
                numbertoignore = 0;

            string container = action.GetParameterValue("container");
            // No parameter? Although we shouldn't really allow it, assume its one of the few missions that needs the put action
            if (string.IsNullOrEmpty(container))
                container = "Rogue Drone"; //http://eve-survival.org/wikka.php?wakka=Anomaly4

            Cache.Instance.MissionLoot = true;
            List<string> items = action.GetParameterValues("item");
            List<string> targetNames = action.GetParameterValues("target");
            // if we aren't generally looting we need to re-enable the opening of wrecks to
            // find this LootItems we are looking for
            Cache.Instance.OpenWrecks = true;

            int quantity;
            if (!int.TryParse(action.GetParameterValue("quantity"), out quantity))
                quantity = 1;
            //
            // we need to make sure we are in scoop range before calling this...
            //
            //if (!Cache.Instance.OpenContainerInSpace("PutItemAction", container)) return;

            bool done = items.Count == 0;
            if (!done)
            {
                //DirectContainer cargo = Cache.Instance.DirectEve.;
                // We assume that the ship's cargo will be opened somewhere else
                //if (cargo.Window.IsReady)
                //    done |= cargo.Items.Any(i => (items.Contains(i.TypeName) && (i.Quantity >= quantity)));
            }
            if (done)
            {
                Logging.Log("CombatMission." + _pocketActions[_currentAction], "We are done looting", Logging.teal);
                Nextaction();
                return;
            }

            IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Distance);
            //IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Id);
            //IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderByDescending(e => e.Id);
            if (!containers.Any())
            {
                Logging.Log("CombatMission." + _pocketActions[_currentAction], "We are done looting", Logging.teal);
                containers = null;
                Nextaction();
                return;
            }

            EntityCache closest = containers.LastOrDefault(c => targetNames.Contains(c.Name)) ?? containers.LastOrDefault();
            if (closest != null && (closest.Distance > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)))
            {
                if (DateTime.Now > Cache.Instance.NextApproachAction && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id))
                {
                    Logging.Log("CombatMission." + _pocketActions[_currentAction], "Approaching target [" + closest.Name + "][ID: " + closest.Id + "] which is at [" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.teal);
                    closest.Approach();
                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                }
            }
        }


        private void DropItem(Actions.Action action)
        {
            Cache.Instance.DropMode = true;
            var items = action.GetParameterValues("item");
            var target = action.GetParameterValue("target");

            int quantity;
            if (!int.TryParse(action.GetParameterValue("quantity"), out quantity))
                quantity = 1;

            var done = items.Count == 0;

            IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByName(target);
            if (targets == null || targets.Count() == 0)
            {
                Logging.Log("MissionController.DropItem","No target name: " + targets, Logging.orange);
                // now that we've completed this action revert OpenWrecks to false
                Cache.Instance.DropMode = false;
                Nextaction();
                return;
            }

            var closest = targets.OrderBy(t => t.Distance).First();
            if (closest.Distance > (int)Distance.SafeScoopRange)
            {
                if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)
                {
                    if (DateTime.Now > Cache.Instance.NextApproachAction)
                    {
                        Logging.Log("MissionController.DropItem","Approaching target [" + closest.Name + "][ID: " + closest.Id + "] which is at [" + Math.Round(closest.Distance / 1000, 0) + "k away]",Logging.white);
                        closest.Approach(1000);
                        Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                    }
                }
            }
            else
            {
                if (!done)
                {
                    if (DateTime.Now > Cache.Instance.NextOpenContainerInSpaceAction)
                    {
                        var cargo = Cache.Instance.DirectEve.GetShipsCargo();

                        if (closest.CargoWindow == null)
                        {
                            Logging.Log("MissionController.DropItem","Open Cargo",Logging.white);
                            closest.OpenCargo();
                            Cache.Instance.NextOpenContainerInSpaceAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(4,6));
                            return;
                        }

                        // Get the container that is associated with the cargo container
                        var container = Cache.Instance.DirectEve.GetContainer(closest.Id);

                        var ItemsToMove = cargo.Items.FirstOrDefault(i => i.TypeName.ToLower() == items.FirstOrDefault().ToLower());
                        if (ItemsToMove != null)
                        {
                            Logging.Log("MissionController.DropItem","Moving Items: " + items.FirstOrDefault() + " from cargo ship to " + container.TypeName,Logging.white);
                            container.Add(ItemsToMove, quantity);

                            done = container.Items.Any(i => i.TypeName.ToLower() == items.FirstOrDefault().ToLower() && (i.Quantity >= quantity));
                            Cache.Instance.NextOpenContainerInSpaceAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(4, 6));
                        }
                        else
                        {
                            Logging.Log("MissionController.DropItem","Error not found Items",Logging.white);
                            Cache.Instance.DropMode = false;
                            Nextaction();
                            return;
                        }
                    }
                }
                else
                {
                    Logging.Log("MissionController.DropItem","We are done",Logging.white);
                    // now that we've completed this action revert OpenWrecks to false
                    Cache.Instance.DropMode = false;
                    Nextaction();
                    return;
                }
            }
        }



        private void LootItemAction(Actions.Action action)
        {
            Cache.Instance.MissionLoot = true;
            List<string> items = action.GetParameterValues("item");
            List<string> targetNames = action.GetParameterValues("target");
            // if we aren't generally looting we need to re-enable the opening of wrecks to
            // find this LootItems we are looking for
            Cache.Instance.OpenWrecks = true;

            int quantity;
            if (!int.TryParse(action.GetParameterValue("quantity"), out quantity))
                quantity = 1;

            bool done = items.Count == 0;
            if (!done)
            {
                DirectContainer cargo = Cache.Instance.DirectEve.GetShipsCargo();
                // We assume that the ship's cargo will be opened somewhere else
                if (cargo.Window.IsReady)
                    done |= cargo.Items.Any(i => (items.Contains(i.TypeName) && (i.Quantity >= quantity)));
            }
            if (done)
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "We are done looting", Logging.teal);
                // now that we've completed this action revert OpenWrecks to false
                Cache.Instance.OpenWrecks = false;
                Cache.Instance.MissionLoot = false;
                _currentAction++;
                return;
            }

            IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Distance);
            //IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Id);
            //IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderByDescending(e => e.Id);
            if (!containers.Any())
            {
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "We are done looting", Logging.teal);

                _currentAction++;
                return;
            }

            EntityCache container = containers.FirstOrDefault(c => targetNames.Contains(c.Name)) ?? containers.FirstOrDefault();
            if (container != null && (container.Distance > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != container.Id)))
            {
                if (DateTime.Now > Cache.Instance.NextApproachAction && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != container.Id))
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Approaching target [" + container.Name + "][ID: " + container.Id + "] which is at [" + Math.Round(container.Distance / 1000, 0) + "k away]", Logging.teal);
                    container.Approach();
                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                }
            }
        }

        //
        // this action still needs some TLC - currently broken (unimplemented)
        //
        private void SalvageAction(Actions.Action action)
        {
            Cache.Instance.MissionLoot = true;
            List<string> items = action.GetParameterValues("item");
            List<string> targetNames = action.GetParameterValues("target");
            // if we aren't generally looting we need to re-enable the opening of wrecks to
            // find this LootItems we are looking for
            Cache.Instance.OpenWrecks = true;

            //
            // when the salvage action is 'done' we will be able to open the "target"
            //
            bool done = items.Count == 0;
            if (!done)
            {
                DirectContainer cargo = Cache.Instance.DirectEve.GetShipsCargo();
                // We assume that the ship's cargo will be opened somewhere else
                if (cargo.Window.IsReady)
                    done |= cargo.Items.Any(i => (items.Contains(i.TypeName)));
            }
            if (done)
            {
                Logging.Log("CombatMission." + _pocketActions[_currentAction], "We are done looting", Logging.teal);
                // now that we've completed this action revert OpenWrecks to false
                Cache.Instance.OpenWrecks = false;
                Cache.Instance.MissionLoot = false;
                _currentAction++;
                return;
            }

            IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Distance);
            if (!containers.Any())
            {
                Logging.Log("CombatMission." + _pocketActions[_currentAction], "We are done looting", Logging.teal);

                _currentAction++;
                return;
            }

            EntityCache closest = containers.LastOrDefault(c => targetNames.Contains(c.Name)) ?? containers.LastOrDefault();
            if (closest != null && (closest.Distance > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)))
            {
                if (DateTime.Now > Cache.Instance.NextApproachAction && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id))
                {
                    Logging.Log("CombatMission." + _pocketActions[_currentAction], "Approaching target [" + closest.Name + "][ID: " + closest.Id + "] which is at [" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.teal);
                    closest.Approach();
                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                }
            }
        }

        private void LootAction(Actions.Action action)
        {
            List<string> items = action.GetParameterValues("item");
            List<string> targetNames = action.GetParameterValues("target");
            // if we aren't generally looting we need to re-enable the opening of wrecks to
            // find this LootItems we are looking for
            Cache.Instance.OpenWrecks = true;
            if (!Settings.Instance.LootEverything)
            {
                bool done = items.Count == 0;
                if (!done)
                {
                    DirectContainer cargo = Cache.Instance.DirectEve.GetShipsCargo();
                    // We assume that the ship's cargo will be opened somewhere else
                    if (cargo.Window.IsReady)
                        done |= cargo.Items.Any(i => items.Contains(i.TypeName));
                }
                if (done)
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "LootEverything:  We are done looting", Logging.teal);
                    // now that we are done with this action revert OpenWrecks to false
                    Cache.Instance.OpenWrecks = false;

                    _currentAction++;
                    return;
                }
            }
            // unlock targets count
            Cache.Instance.MissionLoot = true;

            IOrderedEnumerable<EntityCache> containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderByDescending(e => e.Id);
            if (!containers.Any())
            {
                // lock targets count
                Cache.Instance.MissionLoot = false;
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "We are done looting", Logging.teal);
                // now that we are done with this action revert OpenWrecks to false
                Cache.Instance.OpenWrecks = false;

                _currentAction++;
                return;
            }

            EntityCache container = containers.FirstOrDefault(c => targetNames.Contains(c.Name)) ?? containers.LastOrDefault();
            if (container != null && (container.Distance > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != container.Id)))
            {
                if (DateTime.Now > Cache.Instance.NextApproachAction)
                {
                    Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Approaching target [" + container.Name + "][ID: " + container.Id + "][" + Math.Round(container.Distance / 1000, 0) + "k away]", Logging.teal);
                    container.Approach();
                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                }
            }
        }

        private void IgnoreAction(Actions.Action action)
        {
            bool clear;
            if (!bool.TryParse(action.GetParameterValue("clear"), out clear))
                clear = false;

            List<string> removehighestbty = action.GetParameterValues("RemoveHighestBty");
            List<string> addhighestbty = action.GetParameterValues("AddHighestBty");

            List<string> add = action.GetParameterValues("add");
            List<string> remove = action.GetParameterValues("remove");

            string targetNames = action.GetParameterValue("target");

            int distancetoapp;
            if (!int.TryParse(action.GetParameterValue("distance"), out distancetoapp))
                distancetoapp = 1000;

            //IEnumerable<EntityCache> targets = Cache.Instance.Entities.Where(e => targetNames.Contains(e.Name));
            // EntityCache target = targets.OrderBy(t => t.Distance).First();

            //IEnumerable<EntityCache> targetsinrange = Cache.Instance.Entities.Where(b => Cache.Instance.DistanceFromEntity(b.X ?? 0, b.Y ?? 0, b.Z ?? 0,target) < distancetoapp);
            //IEnumerable<EntityCache> targetsoutofrange = Cache.Instance.Entities.Where(b => Cache.Instance.DistanceFromEntity(b.X ?? 0, b.Y ?? 0, b.Z ?? 0, target) < distancetoapp);

            if (clear)
                Cache.Instance.IgnoreTargets.Clear();
            else
            {
                add.ForEach(a => Cache.Instance.IgnoreTargets.Add(a.Trim()));
                remove.ForEach(a => Cache.Instance.IgnoreTargets.Remove(a.Trim()));
            }
            Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Updated ignore list", Logging.teal);
            if (Cache.Instance.IgnoreTargets.Any())
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Currently ignoring: " + Cache.Instance.IgnoreTargets.Aggregate((current, next) => current + "[" + next + "]"), Logging.teal);
            else
                Logging.Log("CombatMissionCtrl." + _pocketActions[_currentAction], "Your ignore list is empty", Logging.teal);
            _currentAction++;
        }

        private void PerformAction(Actions.Action action)
        {
            switch (action.State)
            {
                case ActionState.Activate:
                    ActivateAction(action);
                    break;

                case ActionState.ClearPocket:
                    ClearPocketAction(action);
                    break;

                case ActionState.SalvageBookmark:
                    BookmarkPocketForSalvaging();

                    _currentAction++;
                    break;

                case ActionState.Done:
                    DoneAction();
                    break;

                case ActionState.Kill:
                    KillAction(action);
                    break;

                case ActionState.KillOnce:
                    KillOnceAction(action);
                    break;

                case ActionState.UseDrones:
                    UseDrones(action);
                    break;

                case ActionState.AggroOnly:
                    AggroOnlyAction(action);
                    break;

                case ActionState.KillClosestByName:
                    KillClosestByNameAction(action);
                    break;

                case ActionState.KillClosest:
                    KillClosestAction(action);
                    break;

                case ActionState.MoveTo:
                    MoveToAction(action);
                    break;

                case ActionState.MoveToBackground:
                    MoveToBackgroundAction(action);
                    break;

                case ActionState.ClearWithinWeaponsRangeOnly:
                    ClearWithinWeaponsRangeOnlyAction(action);
                    break;

                //case ActionState.Salvage:
                //    SalvageAction(action);
                //    break;

                //case ActionState.Analyze:
                //    AnalyzeAction(action);
                //    break;

                case ActionState.Loot:
                    LootAction(action);
                    break;

                case ActionState.LootItem:
                    LootItemAction(action);
                    break;

                //case ActionState.PutItem:
                //    PutItemAction(action);
                //    break;

                case ActionState.Ignore:
                    IgnoreAction(action);
                    break;

                case ActionState.WaitUntilTargeted:
                    WaitUntilTargeted(action);
                    break;

                case ActionState.DebuggingWait:
                    DebuggingWait(action);
                    break;
            }
        }

        public void ProcessState()
        {
            // There is really no combat in stations (yet)
            if (Cache.Instance.InStation)
                return;

            // if we are not in space yet, wait...
            if (!Cache.Instance.InSpace)
                return;

            // What? No ship entity?
            if (Cache.Instance.DirectEve.ActiveShip.Entity == null)
                return;

            // There is no combat when cloaked
            if (Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked)
                return;

            switch (_States.CurrentCombatMissionCtrlState)
            {
                case CombatMissionCtrlState.Idle:
                    break;
                case CombatMissionCtrlState.Done:
                    Statistics.WritePocketStatistics();

                    if (!Cache.Instance.NormalApproch)
                        Cache.Instance.NormalApproch = true;

                    Cache.Instance.IgnoreTargets.Clear();
                    break;
                case CombatMissionCtrlState.Error:
                    break;

                case CombatMissionCtrlState.Start:
                    Cache.Instance.PocketNumber = 0;
                    // Update statistic values
                    Cache.Instance.WealthatStartofPocket = Cache.Instance.DirectEve.Me.Wealth;
                    Statistics.Instance.StartedPocket = DateTime.Now;

                    // Reload the items needed for this mission from the XML file
                    Cache.Instance.RefreshMissionItems(AgentId);

                    // Update x/y/z so that NextPocket wont think we are there yet because its checking (very) old x/y/z cords
                    _lastX = Cache.Instance.DirectEve.ActiveShip.Entity.X;
                    _lastY = Cache.Instance.DirectEve.ActiveShip.Entity.Y;
                    _lastZ = Cache.Instance.DirectEve.ActiveShip.Entity.Z;

                    _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.LoadPocket;
                    break;

                case CombatMissionCtrlState.LoadPocket:
                    _pocketActions.Clear();
                    _pocketActions.AddRange(Cache.Instance.LoadMissionActions(AgentId, Cache.Instance.PocketNumber, true));

                    //
                    // LogStatistics();
                    //
                    if (_pocketActions.Count == 0)
                    {
                        // No Pocket action, load default actions
                        Logging.Log("CombatMissionCtrl", "No mission actions specified, loading default actions", Logging.orange);

                        // Wait for 30 seconds to be targeted
                        _pocketActions.Add(new Actions.Action { State = ActionState.WaitUntilTargeted });
                        _pocketActions[0].AddParameter("timeout", "15");

                        // Clear the Pocket
                        _pocketActions.Add(new Actions.Action { State = ActionState.ClearPocket });

                        // Is there a gate?
                        IEnumerable<EntityCache> gates = Cache.Instance.EntitiesByName("Acceleration Gate");
                        if (gates != null && gates.Any())
                        {
                            // Activate it (Activate action also moves to the gate)
                            _pocketActions.Add(new Actions.Action { State = ActionState.Activate });
                            _pocketActions[_pocketActions.Count - 1].AddParameter("target", "Acceleration Gate");
                        }
                        else // No, were done
                            _pocketActions.Add(new Actions.Action { State = ActionState.Done });

                        // TODO: Check mission HTML to see if we need to pickup any items
                        // Not a priority, apparently retrieving HTML causes a lot of crashes
                    }

                    Logging.Log("-", "-----------------------------------------------------------------", Logging.teal);
                    Logging.Log("-", "-----------------------------------------------------------------", Logging.teal);
                    Logging.Log("CombatMissionCtrl", "Mission Timer Currently At: [" + Math.Round(DateTime.Now.Subtract(Statistics.Instance.StartedMission).TotalMinutes, 0) + "]", Logging.teal);
                    if (Cache.Instance.OrbitDistance != Settings.Instance.OrbitDistance) //this should be done elsewhere
                    {
                        if (Cache.Instance.OrbitDistance == 0)
                        {
                            Cache.Instance.OrbitDistance = Settings.Instance.OrbitDistance;
                            Logging.Log("CombatMissionCtrl", "Using default orbit distance: " + Cache.Instance.OrbitDistance + " (as the custom one was 0)", Logging.teal);
                        }
                        else
                            Logging.Log("CombatMissionCtrl", "Using custom orbit distance: " + Cache.Instance.OrbitDistance, Logging.teal);
                    }
                    if (Cache.Instance.OrbitDistance != 0)
                        Logging.Log("CombatMissionCtrl", "Orbit Distance is set to: " + (Cache.Instance.OrbitDistance / 1000).ToString(CultureInfo.InvariantCulture) + "k", Logging.teal);
                    //if (Cache.Instance.OptimalRange != 0)
                    //    Logging.Log("Optimal Range is set to: " + (Cache.Instance.OrbitDistance / 1000).ToString(CultureInfo.InvariantCulture) + "k");
                    Logging.Log("CombatMissionCtrl", "Max Range is currently: " + (Cache.Instance.MaxRange / 1000).ToString(CultureInfo.InvariantCulture) + "k", Logging.teal);
                    Logging.Log("-", "-----------------------------------------------------------------", Logging.teal);
                    Logging.Log("-", "-----------------------------------------------------------------", Logging.teal);
                    Logging.Log("CombatMissionCtrl", "Pocket [" + Cache.Instance.PocketNumber + "] loaded, executing the following actions", Logging.orange);
                    var pocketactioncount = 1;
                    foreach (Actions.Action a in _pocketActions)
                    {
                        Logging.Log("CombatMissionCtrl", "Action [ " + pocketactioncount + " ] " + a, Logging.teal);
                        pocketactioncount++;
                    }
                    Logging.Log("-", "-----------------------------------------------------------------", Logging.teal);
                    Logging.Log("-", "-----------------------------------------------------------------", Logging.teal);

                    // Reset pocket information
                    _currentAction = 0;
                    Cache.Instance.IsMissionPocketDone = false;
                    Cache.Instance.IgnoreTargets.Clear();
                    Statistics.PocketObjectStatistics(Cache.Instance.Objects.ToList());
                    _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.ExecutePocketActions;
                    break;

                case CombatMissionCtrlState.ExecutePocketActions:
                    if (_currentAction >= _pocketActions.Count)
                    {
                        // No more actions, but we're not done?!?!?!
                        Logging.Log("CombatMissionCtrl", "We're out of actions but did not process a 'Done' or 'Activate' action", Logging.red);

                        _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.Error;
                        break;
                    }

                    Actions.Action action = _pocketActions[_currentAction];
                    if (action.ToString() != Cache.Instance.CurrentPocketAction)
                    {
                        Cache.Instance.CurrentPocketAction = action.ToString();
                    }
                    int currentAction = _currentAction;
                    PerformAction(action);

                    if (currentAction != _currentAction)
                    {
                        Logging.Log("CombatMissionCtrl", "Finished Action." + action, Logging.yellow);

                        if (_currentAction < _pocketActions.Count)
                        {
                            action = _pocketActions[_currentAction];
                            Logging.Log("CombatMissionCtrl", "Starting Action." + action, Logging.yellow);
                        }
                    }

                    if (Settings.Instance.DebugStates)
                        Logging.Log("CombatMissionCtrl", "Action.State = " + action, Logging.teal);
                    break;

                case CombatMissionCtrlState.NextPocket:
                    double distance = Cache.Instance.DistanceFromMe(_lastX, _lastY, _lastZ);
                    if (distance > (int)Distance.NextPocketDistance)
                    {
                        Logging.Log("CombatMissionCtrl", "We've moved to the next Pocket [" + Math.Round(distance / 1000, 0) + "k away]", Logging.green);

                        // If we moved more then 100km, assume next Pocket
                        Cache.Instance.PocketNumber++;
                        _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.LoadPocket;
                        Statistics.WritePocketStatistics();
                    }
                    else if (DateTime.Now.Subtract(_moveToNextPocket).TotalMinutes > 2)
                    {
                        Logging.Log("CombatMissionCtrl", "We've timed out, retry last action", Logging.orange);

                        // We have reached a timeout, revert to ExecutePocketActions (e.g. most likely Activate)
                        _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.ExecutePocketActions;
                    }
                    break;
            }

            double newX = Cache.Instance.DirectEve.ActiveShip.Entity.X;
            double newY = Cache.Instance.DirectEve.ActiveShip.Entity.Y;
            double newZ = Cache.Instance.DirectEve.ActiveShip.Entity.Z;

            // For some reason x/y/z returned 0 sometimes
            if (newX != 0 && newY != 0 && newZ != 0)
            {
                // Save X/Y/Z so that NextPocket can check if we actually went to the next Pocket :)
                _lastX = newX;
                _lastY = newY;
                _lastZ = newZ;
            }
        }
    }
}