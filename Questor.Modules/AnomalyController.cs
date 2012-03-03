//NOT FINISH DON'T USE
namespace Questor.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;

    public class AnomalyController
    {
        private DateTime? _clearPocketTimeout;
        private int _currentAction;
        private DateTime _lastActivateAction;
        private readonly Dictionary<long, DateTime> _lastWeaponReload = new Dictionary<long, DateTime>();
        private double _lastX;
        private double _lastY;
        private double _lastZ;
        private int _pocket;
        private List<Action> _pocketActions;
        private bool _waiting;
        private DateTime _waitingSince;
        private DateTime _lastAlign;
    
        public long AgentId { get; set; }

        public AnomalyController()
        {
            _pocketActions = new List<Action>();
        }

        public AnomalyControllerState State { get; set; }

        private void ReloadAll()
        {
            var weapons = Cache.Instance.Weapons;
            var cargo = Cache.Instance.DirectEve.GetShipsCargo();
            var correctAmmo1 = Settings.Instance.Ammo.Where(a => a.DamageType == Cache.Instance.DamageType);
            correctAmmo1 = correctAmmo1.Where(a => cargo.Items.Any(i => i.TypeId == a.TypeId));

            if (correctAmmo1.Count() == 0)
                return;

            var ammo = correctAmmo1.Where(a => a.Range > 1).OrderBy(a => a.Range).FirstOrDefault();
            var charge = cargo.Items.FirstOrDefault(i => i.TypeId == ammo.TypeId);

            if (ammo == null)
                return;

            foreach (var weapon in weapons)
            {

                if (weapon.CurrentCharges >= weapon.MaxCharges)
                    return;

                if (weapon.IsReloadingAmmo || weapon.IsDeactivating || weapon.IsChangingAmmo)
                    return;

                if (_lastWeaponReload.ContainsKey(weapon.ItemId) && DateTime.Now < _lastWeaponReload[weapon.ItemId].AddSeconds(22))
                    return;

                _lastWeaponReload[weapon.ItemId] = DateTime.Now;

                if (weapon.Charge.TypeId == charge.TypeId)
                {
                    Logging.Log("AnomalyController: Reloading All [" + weapon.ItemId + "] with [" + charge.TypeName + "][" + charge.TypeId + "]");

                    weapon.ReloadAmmo(charge);
                }

            }
            return;
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
                if(!Settings.Instance.LootEverything && Cache.Instance.Containers.Count() < Settings.Instance.MinimumWreckCount)
                {
                    Logging.Log("AnomalyController: No bookmark created because the pocket has [" + Cache.Instance.Containers.Count() + "] wrecks/containers and the minimum is [" + Settings.Instance.MinimumWreckCount + "]");
                    return;
                }
                else if(Settings.Instance.LootEverything)
                {
                    Logging.Log("AnomalyController: No bookmark created because the pocket has [" + Cache.Instance.UnlootedContainers.Count() + "] wrecks/containers and the minimum is [" + Settings.Instance.MinimumWreckCount + "]");
                    return;
                }
            }

            // Do we already have a bookmark?
            var bookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");
            var bookmark = bookmarks.FirstOrDefault(b => Cache.Instance.DistanceFromMe(b.X ?? 0, b.Y ?? 0, b.Z ?? 0) < (int)Distance.BookmarksOnGridWithMe);
            if (bookmark != null)
            {
                Logging.Log("AnomalyController: Pocket already bookmarked for salvaging [" + bookmark.Title + "]");
                return;
            }

            // No, create a bookmark
            var label = string.Format("{0} {1:HHmm}", Settings.Instance.BookmarkPrefix, DateTime.UtcNow);
            Logging.Log("AnomalyController: Bookmarking pocket for salvaging [" + label + "]");
            Cache.Instance.CreateBookmark(label);
        }

        private void ActivateAction(Action action)
        {
            var target = action.GetParameterValue("target");

            // No parameter? Although we shouldnt really allow it, assume its the acceleration gate :)
            if (string.IsNullOrEmpty(target))
                target = "Acceleration Gate";

            var targets = Cache.Instance.EntitiesByName(target);
            if (targets == null || targets.Count() == 0)
            {
                Logging.Log("AnomalyController.Activate: Can't find [" + target + "] to activate! Stopping Questor!");
                State = AnomalyControllerState.Error;
                return;
            }

            var closest = targets.OrderBy(t => t.Distance).First();
            if (closest.Distance < (int)Distance.GateActivationRange)
            {
                // Tell the drones module to retract drones
                Cache.Instance.IsMissionPocketDone = true;

                // We cant activate if we have drones out
                if (Cache.Instance.ActiveDrones.Count() > 0)
                    return;

                if (closest.Distance < (int)Distance.WayTooClose)
                {
                    closest.Orbit((int)Distance.GateActivationRange);
                }
                Logging.Log(" dist " + closest.Distance);
                if (closest.Distance >= (int)Distance.WayTooClose)
                {
                    // Add bookmark (before we activate)
                    if (Settings.Instance.CreateSalvageBookmarks)
                        BookmarkPocketForSalvaging();

                    // Reload weapons and activate gate to move to the next pocket
                    ReloadAll();
                    closest.Activate();

                    // Do not change actions, if NextPocket gets a timeout (>2 mins) then it reverts to the last action
                    Logging.Log("AnomalyController.Activate: Activate [" + closest.Name + "] and change state to 'NextPocket'");

                    _lastActivateAction = DateTime.Now;
                    State = AnomalyControllerState.NextPocket;
                }
            }
            else if (closest.Distance < (int)Distance.WarptoDistance)
            {
                // Move to the target
                if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)
                {
                    Logging.Log("AnomalyController.Activate: Approaching target [" + closest.Name + "][" + closest.Id + "]");
                    closest.Approach();
                }
            }
            else
            {
                // We cant warp if we have drones out
                if (Cache.Instance.ActiveDrones.Count() > 0)
                    return;
                    
                if (DateTime.Now.Subtract(_lastAlign ).TotalMinutes > 2)
                {
                // Probably never happens
                closest.AlignTo();
                _lastAlign = DateTime.Now;
                }
            }
        }

        private void ClearPocketAction(Action action)
        {
            var activeTargets = new List<EntityCache>();
            activeTargets.AddRange(Cache.Instance.Targets);
            activeTargets.AddRange(Cache.Instance.Targeting);

            // Get lowest range
            var range = Math.Min(Cache.Instance.WeaponRange, Cache.Instance.DirectEve.ActiveShip.MaxTargetRange);

            // We are obviously still killing stuff that's in range
            if (activeTargets.Count(t => t.Distance < range && t.IsNpc && t.CategoryId == (int) CategoryID.Entity) > 0)
            {
                // Reset timeout
                _clearPocketTimeout = null;
                /*if (Cache.Instance.Approaching != null && !Settings.Instance.SpeedTank)
                 {
                     Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                     Cache.Instance.Approaching = null;
                 }*/
                return;
            }

            // Is there a priority target out of range?
            var target = Cache.Instance.PriorityTargets.OrderBy(t => t.Distance).Where(t => (!Cache.Instance.IgnoreTargets.Contains(t.Name.Trim()) || Cache.Instance.TargetedBy.Any(w => w.IsWarpScramblingMe))).FirstOrDefault();
            // Or is there a target out of range that is targeting us?
            target = target ?? Cache.Instance.TargetedBy.Where(t => !t.IsSentry && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim()) || Cache.Instance.TargetedBy.Any(w => w.IsWarpScramblingMe)).OrderBy(t => t.Distance).FirstOrDefault();
            // Or is there any target out of range?
            target = target ?? Cache.Instance.Entities.Where(t => !t.IsSentry && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim()) || Cache.Instance.TargetedBy.Any(w => w.IsWarpScramblingMe)).OrderBy(t => t.Distance).FirstOrDefault();

            if (target != null)
            {
                // Reset timeout
                _clearPocketTimeout = null;
                // Lock priority target if within weapons range
                if (target.Distance < range)
                {
                    if (Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets > 0)
                    {
                        Logging.Log("AnomalyController.ClearPocket: Targeting [" + target.Name + "][" + target.Id + "] - Distance [" + target.Distance + "]");
                        target.LockTarget();
                    }
                    return;
                }

                // Are we approaching the active (out of range) target?
                // Wait for it (or others) to get into range
                if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != target.Id)
                {
                    Logging.Log("AnomalyController.ClearPocket: Approaching target [" + target.Name + "][" + target.Id + "]");

                    if (Settings.Instance.SpeedTank)
                        target.Orbit(Cache.Instance.OrbitDistance);
                    else 
                    {
                        if (target.Distance > Cache.Instance.OrbitDistance + (int)Distance.OrbitDistanceCushion)
                            target.Approach(Cache.Instance.OrbitDistance);
                        else
                        {
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                            Cache.Instance.Approaching = null;
                        }     
                    }
                }

                return;
            }

            // Do we have a timeout?  No, set it to now + 5 seconds
            if (!_clearPocketTimeout.HasValue)
                _clearPocketTimeout = DateTime.Now.AddSeconds(5);

            // Are we in timeout?
            if (DateTime.Now < _clearPocketTimeout.Value)
                return;

            // We have cleared the Pocket, perform the next action \o/
            _currentAction++;

            // Reset timeout
            _clearPocketTimeout = null;
        }

        private void MoveToAction(Action action)
        {
            var target = action.GetParameterValue("target");

            // No parameter? Although we shouldnt really allow it, assume its the acceleration gate :)
            if (string.IsNullOrEmpty(target))
                target = "Acceleration Gate";

            var targets = Cache.Instance.EntitiesByName(target);
            if (targets == null || targets.Count() == 0)
            {
                // Unlike activate, no target just means next action
                _currentAction++;
                return;
            }

            var closest = targets.OrderBy(t => t.Distance).First();
            if (closest.Distance < (int)Distance.GateActivationRange)
            {
                // We are close enough to whatever we needed to move to
                _currentAction++;

                if (Cache.Instance.Approaching != null)
                {
                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                    Cache.Instance.Approaching = null;
                }
            }
            else if (closest.Distance < (int)Distance.WarptoDistance)
            {
                // Move to the target
                if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)
                {
                    Logging.Log("AnomalyController.MoveTo: Approaching target [" + closest.Name + "][" + closest.Id + "]");
                    closest.Approach();
                }
            }
            else
            {
                // We cant warp if we have drones out
                if (Cache.Instance.ActiveDrones.Count() > 0)
                    return;

                if (DateTime.Now.Subtract(_lastAlign ).TotalMinutes > 2)
                {
                // Probably never happens
                closest.AlignTo();
                _lastAlign = DateTime.Now;
                }
            }
        }

        private void WaitUntilTargeted(Action action)
        {
            var targetedBy = Cache.Instance.TargetedBy;
            if (targetedBy != null && targetedBy.Count() > 0)
            {
                Logging.Log("AnomalyController.WaitUntilTargeted: We have been targeted!");

                // We have been locked, go go go ;)
                _waiting = false;
                _currentAction++;
                return;
            }

            // Default timeout is 30 seconds
            int timeout;
            if (!int.TryParse(action.GetParameterValue("timeout"), out timeout))
                timeout = 30; // Probably don't need to do this

            if (_waiting)
            {
                if (DateTime.Now.Subtract(_waitingSince).TotalSeconds < timeout)
                    return;

                Logging.Log("AnomalyController.WaitUntilTargeted: Nothing targeted us within the timeout!");

                // Nothing has targeted us in the specified timeout
                _waiting = false;
                _currentAction++;
                return;
            }

            // Start waiting
            _waiting = true;
            _waitingSince = DateTime.Now;
        }

        private void KillAction(Action action)
        {
            bool ignoreAttackers;
            if (!bool.TryParse(action.GetParameterValue("ignoreattackers"), out ignoreAttackers))
                ignoreAttackers = false;

            bool breakOnAttackers;
            if (!bool.TryParse(action.GetParameterValue("breakonattackers"), out breakOnAttackers))
                breakOnAttackers = false;

            var targetNames = action.GetParameterValues("target");
            // No parameter? Ignore kill action
            if (targetNames.Count == 0)
            {
                Logging.Log("AnomalyController.Kill: No targets defined!");

                _currentAction++;
                return;
            }

            var targets = Cache.Instance.Entities.Where(e => targetNames.Contains(e.Name));
            if (targets.Count() == 0)
            {
                Logging.Log("AnomalyController.Kill: All targets killed " + targetNames.Aggregate((current, next) => current + "[" + next + "]"));

                // We killed it/them !?!?!? :)
                _currentAction++;
                return;
            }

            if (breakOnAttackers && Cache.Instance.TargetedBy.Any(t => !t.IsSentry && t.Distance < Cache.Instance.WeaponRange))
            {
                // We are being attacked, break the kill order
                if (Cache.Instance.RemovePriorityTargets(targets))
                    Logging.Log("AnomalyController.Kill: Breaking off kill order, new spawn has arived!");

                foreach (var target in Cache.Instance.Targets.Where(e => targets.Any(t => t.Id == e.Id)))
                {
                    Logging.Log("AnomalyController.Kill: Unlocking [" + target.Name + "][" + target.Id + "] due to kill order being put on hold");
                    target.UnlockTarget();
                }

                return;
            }

            if (!ignoreAttackers || breakOnAttackers)
            {
                // Apparently we are busy, wait for combat to clear attackers first
                var targetedBy = Cache.Instance.TargetedBy;
                if (targetedBy != null && targetedBy.Count(t => !t.IsSentry && t.Distance < Cache.Instance.WeaponRange) > 0)
                    return;
            }

            var closest = targets.OrderBy(t => t.Distance).First();
            if (closest.Distance < Cache.Instance.WeaponRange)
            {
                if (!Cache.Instance.PriorityTargets.Any(pt => pt.Id == closest.Id))
                {
                    Logging.Log("AnomalyController.Kill: Adding [" + closest.Name + "][" + closest.Id + "] as a priority target");
                    Cache.Instance.AddPriorityTargets(new[] {closest}, Priority.PriorityKillTarget);
                }
            }
            else
            {
                if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)
                {
                    Logging.Log("AnomalyController.Kill: Approaching target [" + closest.Name + "][" + closest.Id + "]");

                    if (Settings.Instance.SpeedTank)
                        closest.Orbit(Cache.Instance.OrbitDistance);
                    else
                    {
                        if (closest.Distance > Cache.Instance.OrbitDistance + (int)Distance.OrbitDistanceCushion)
                            closest.Approach(Cache.Instance.OrbitDistance);
                        else
                        {
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                            Cache.Instance.Approaching = null;
                        }
                    }
                }
            }
        }

        private void LootItemAction(Action action)
        {
            var items = action.GetParameterValues("item");
            var targetNames = action.GetParameterValues("target");

            var done = items.Count == 0;
            if (!done)
            {
                var cargo = Cache.Instance.DirectEve.GetShipsCargo();
                // We assume that the ship's cargo will be opened somewhere else
                if (cargo.IsReady)
                    done |= cargo.Items.Any(i => items.Contains(i.TypeName));
            }
            if (done)
            {
                Logging.Log("AnomalyController.LootItem: We are done looting");

                _currentAction++;
                return;
            }

            var containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Distance);
            if (containers.Count() == 0)
            {
                Logging.Log("AnomalyController.LootItem: We are done looting");

                _currentAction++;
                return;
            }

            var closest = containers.FirstOrDefault(c => targetNames.Contains(c.Name)) ?? containers.First();
            if (closest.Distance > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id))
            {
                Logging.Log("AnomalyController.LootItem: Approaching target [" + closest.Name + "][" + closest.Id + "]");
                closest.Approach();
            }
        }

        private void LootAction(Action action)
        {
            var items = action.GetParameterValues("item");
            var targetNames = action.GetParameterValues("target");

            if (!Settings.Instance.LootEverything)
            {
                var done = items.Count == 0;
                if (!done)
                {
                    var cargo = Cache.Instance.DirectEve.GetShipsCargo();
                    // We assume that the ship's cargo will be opened somewhere else
                    if (cargo.IsReady)
                        done |= cargo.Items.Any(i => items.Contains(i.TypeName));
                }
                if (done)
                {
                    Logging.Log("AnomalyController.Loot: We are done looting");

                    _currentAction++;
                    return;
                }
            }
            // unlock targets count
            Cache.Instance.MissionLoot = true;

            var containers = Cache.Instance.Containers.Where(e => !Cache.Instance.LootedContainers.Contains(e.Id)).OrderBy(e => e.Distance);
            if (containers.Count() == 0)
            {
                // lock targets count
                Cache.Instance.MissionLoot = false;
                Logging.Log("AnomalyController.Loot: We are done looting");
                _currentAction++;
                return;
            }

            var closest = containers.FirstOrDefault(c => targetNames.Contains(c.Name)) ?? containers.First();
            if (closest.Distance > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id))
            {
                Logging.Log("AnomalyController.Loot: Approaching target [" + closest.Name + "][" + closest.Id + "]");
                closest.Approach();
            }
        }

        private void IgnoreAction(Action action)
        {
            var add = action.GetParameterValues("add");
            var remove = action.GetParameterValues("remove");

            add.ForEach(a => Cache.Instance.IgnoreTargets.Add(a.Trim()));
            remove.ForEach(a => Cache.Instance.IgnoreTargets.Remove(a.Trim()));

            Logging.Log("AnomalyController.Ignore: Updated ignore list");
            if (Cache.Instance.IgnoreTargets.Any())
                Logging.Log("AnomalyController.Ignore: Currently ignoring: " + Cache.Instance.IgnoreTargets.Aggregate((current, next) => current + "[" + next + "]"));
            else
                Logging.Log("AnomalyController.Ignore: Your ignore list is empty");
            _currentAction++;
        }

        private void PerformAction(Action action)
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
                    // Tell the drones module to retract drones
                    Cache.Instance.IsMissionPocketDone = true;

                    // We do not switch to "done" status if we still have drones out
                    if (Cache.Instance.ActiveDrones.Count() > 0)
                        return;

                    // Add bookmark (before we're done)
                    if(Settings.Instance.CreateSalvageBookmarks)
                        BookmarkPocketForSalvaging();

                    // Reload weapons
                    ReloadAll();

                    State = AnomalyControllerState.Done;
                    break;

                case ActionState.Kill:
                    KillAction(action);
                    break;

                case ActionState.MoveTo:
                    MoveToAction(action);
                    break;

                case ActionState.Loot:
                    LootAction(action);
                    break;

                case ActionState.LootItem:
                    LootItemAction(action);
                    break;

                case ActionState.Ignore:
                    IgnoreAction(action);
                    break;

                case ActionState.WaitUntilTargeted:
                    WaitUntilTargeted(action);
                    break;
            }
        }

        public void ProcessState()
        {
            // What? No ship entity?
            if (Cache.Instance.DirectEve.ActiveShip.Entity == null)
                return;

            switch (State)
            {
                case AnomalyControllerState.Idle:
                case AnomalyControllerState.Done:
                case AnomalyControllerState.Error:
                    break;

                case AnomalyControllerState.Start:
                    _pocket = 0;

                    // Update x/y/z so that NextPocket wont think we are there yet because its checking (very) old x/y/z cords
                    _lastX = Cache.Instance.DirectEve.ActiveShip.Entity.X;
                    _lastY = Cache.Instance.DirectEve.ActiveShip.Entity.Y;
                    _lastZ = Cache.Instance.DirectEve.ActiveShip.Entity.Z;

                    State = AnomalyControllerState.LoadPocket;
                    break;

                case AnomalyControllerState.LoadPocket:
                    _pocketActions.Clear();
                    _pocketActions.AddRange(Cache.Instance.LoadMissionActions(0, _pocket, false));

                    if (_pocketActions.Count == 0)
                    {
                        // No Pocket action, load default actions
                        Logging.Log("AnomalyController: No mission actions specified, loading default actions");

                        // Wait for 30 seconds to be targeted
                        _pocketActions.Add(new Action {State = ActionState.WaitUntilTargeted});
                        _pocketActions[0].AddParameter("timeout", "15");

                        // Clear the Pocket
                        _pocketActions.Add(new Action {State = ActionState.ClearPocket});

                        // Is there a gate?
                        var gates = Cache.Instance.EntitiesByName("Acceleration Gate");
                        if (gates != null && gates.Count() > 0)
                        {
                            // Activate it (Activate action also moves to the gate)
                            _pocketActions.Add(new Action {State = ActionState.Activate});
                            _pocketActions[_pocketActions.Count - 1].AddParameter("target", "Acceleration Gate");
                        }
                        else // No, were done
                            _pocketActions.Add(new Action {State = ActionState.Done});

                        // TODO: Check mission HTML to see if we need to pickup any items
                        // Not a priority, apparently retrieving HTML causes a lot of crashes
                    }

                    Logging.Log("AnomalyController: Pocket loaded, executing the following actions");
                    foreach (var a in _pocketActions)
                        Logging.Log("AnomalyController: Action." + a);
					
					if (Cache.Instance.OrbitDistance != Settings.Instance.OrbitDistance)
						Logging.Log("AnomalyController: Using custom orbit distance: " + Cache.Instance.OrbitDistance);
						
                    // Reset pocket information
                    _currentAction = 0;
                    Cache.Instance.IsMissionPocketDone = false;
                    Cache.Instance.IgnoreTargets.Clear();

                    State = AnomalyControllerState.ExecutePocketActions;
                    break;

                case AnomalyControllerState.ExecutePocketActions:
                    if (_currentAction >= _pocketActions.Count)
                    {
                        // No more actions, but we're not done?!?!?!
                        Logging.Log("AnomalyController: We're out of actions but did not process a 'Done' or 'Activate' action");

                        State = AnomalyControllerState.Error;
                        break;
                    }

                    var action = _pocketActions[_currentAction];
                    var currentAction = _currentAction;
                    PerformAction(action);

                    if (currentAction != _currentAction)
                    {
                        Logging.Log("AnomalyController: Finished Action." + action);

                        if (_currentAction < _pocketActions.Count)
                        {
                            action = _pocketActions[_currentAction];
                            Logging.Log("AnomalyController: Starting Action." + action);
                        }
                    }

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Action.State = " + action);
                    break;

                case AnomalyControllerState.NextPocket:
                    var distance = Cache.Instance.DistanceFromMe(_lastX, _lastY, _lastZ);
                    if (distance > (int)Distance.NextPocketDistance)
                    {
                        Logging.Log("AnomalyController: We've moved to the next Pocket [" + distance + "]");

                        // If we moved more then 100km, assume next Pocket
                        _pocket++;
                        State = AnomalyControllerState.LoadPocket;
                    }
                    else if (DateTime.Now.Subtract(_lastActivateAction).TotalMinutes > 2)
                    {
                        Logging.Log("AnomalyController: We've timed out, retry last action");

                        // We have reached a timeout, revert to ExecutePocketActions (e.g. most likely Activate)
                        State = AnomalyControllerState.ExecutePocketActions;
                    }
                    break;
            }

            var newX = Cache.Instance.DirectEve.ActiveShip.Entity.X;
            var newY = Cache.Instance.DirectEve.ActiveShip.Entity.Y;
            var newZ = Cache.Instance.DirectEve.ActiveShip.Entity.Z;

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