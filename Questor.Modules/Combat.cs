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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;

    /// <summary>
    ///   The combat class will target and kill any NPC that is targeting the questor.
    ///   It will also kill any NPC that is targeted but not agressing the questor.
    /// </summary>
    public class Combat
    {
        private readonly Dictionary<long, DateTime> _lastModuleActivation = new Dictionary<long, DateTime>();
        private readonly Dictionary<long, DateTime> _lastWeaponReload = new Dictionary<long, DateTime>();
        private bool _isJammed;
        public CombatState State { get; set; }

        private int MaxCharges { get; set; }

        /// <summary>
        ///   Reload correct (tm) ammo for the NPC
        /// </summary>
        /// <param name = "weapon"></param>
        /// <param name = "entity"></param>
        /// <returns>True if the (enough/correct) ammo is loaded, false if wrong/not enough ammo is loaded</returns>
        public bool ReloadNormalAmmo(ModuleCache weapon, EntityCache entity)
        {
            var cargo = Cache.Instance.DirectEve.GetShipsCargo();

            // Get ammo based on damage type
            var correctAmmo = Settings.Instance.Ammo.Where(a => a.DamageType == Cache.Instance.DamageType);

            // Check if we still have that ammo in our cargo
            correctAmmo = correctAmmo.Where(a => cargo.Items.Any(i => i.TypeId == a.TypeId && i.Quantity >= Settings.Instance.MinimumAmmoCharges));

            //check if mission specific ammo is defined
            if (Cache.Instance.missionAmmo.Count() != 0)
            {
                correctAmmo = Cache.Instance.missionAmmo.Where(a => a.DamageType == Cache.Instance.DamageType);
            }

            // Check if we still have that ammo in our cargo
            correctAmmo = correctAmmo.Where(a => cargo.Items.Any(i => i.TypeId == a.TypeId && i.Quantity >= Settings.Instance.MinimumAmmoCharges));
            if (Cache.Instance.missionAmmo.Count() != 0)
            {
                correctAmmo = Cache.Instance.missionAmmo;
            }

            // We are out of ammo! :(
            if (correctAmmo.Count() == 0)
            {
                State = CombatState.OutOfAmmo;
                return false;
            }

            // Get the best possible ammo
            var ammo = correctAmmo.Where(a => a.Range > entity.Distance).OrderBy(a => a.Range).FirstOrDefault();

            // We do not have any ammo left that can hit targets at that range!
            if (ammo == null)
                return false;

            // We have enough ammo loaded
            if (weapon.Charge != null && weapon.Charge.TypeId == ammo.TypeId && weapon.CurrentCharges >= Settings.Instance.MinimumAmmoCharges)
                return true;

            // Retry later, assume its ok now
            if (weapon.MatchingAmmo.Count() == 0)
                return true;

            var charge = cargo.Items.FirstOrDefault(i => i.TypeId == ammo.TypeId && i.Quantity >= Settings.Instance.MinimumAmmoCharges);
            // This should have shown up as "out of ammo"
            if (charge == null)
                return false;

            // We are reloading, wait at least 11 seconds
            if (_lastWeaponReload.ContainsKey(weapon.ItemId) && DateTime.Now < _lastWeaponReload[weapon.ItemId].AddSeconds(22))
                return false;
            _lastWeaponReload[weapon.ItemId] = DateTime.Now;

            // Reload or change ammo
            if (weapon.Charge != null && weapon.Charge.TypeId == charge.TypeId)
            {
                Logging.Log("Combat: Reloading [" + weapon.ItemId + "] with [" + charge.TypeName + "][" + charge.TypeId + "]");
                weapon.ReloadAmmo(charge);
            }
            else
            {
                Logging.Log("Combat: Changing [" + weapon.ItemId + "] with [" + charge.TypeName + "][" + charge.TypeId + "]");
                weapon.ChangeAmmo(charge);
            }

            // Return false as we are reloading ammo
            return false;
        }

        public bool ReloadEnergyWeaponAmmo(ModuleCache weapon, EntityCache entity)
        {
            var cargo = Cache.Instance.DirectEve.GetShipsCargo();

            // Get ammo based on damage type
            var correctAmmo = Settings.Instance.Ammo.Where(a => a.DamageType == Cache.Instance.DamageType);

            // Check if we still have that ammo in our cargo
            correctAmmo = correctAmmo.Where(a => cargo.Items.Any(i => i.TypeId == a.TypeId));

            // We are out of ammo! :(
            if (correctAmmo.Count() == 0)
            {
                State = CombatState.OutOfAmmo;
                return false;
            }

            // Get the best possible ammo
            var ammo = correctAmmo.Where(a => a.Range > entity.Distance).OrderBy(a => a.Range).FirstOrDefault();

            // We do not have any ammo left that can hit targets at that range!
            if (ammo == null)
                return false;

            var charge = cargo.Items.OrderBy(i => i.Quantity).FirstOrDefault(i => i.TypeId == ammo.TypeId);
            // We do not have any ammo left that can hit targets at that range!
            if (charge == null)
                return false;

            // We have enough ammo loaded
            if (weapon.Charge != null && weapon.Charge.TypeId == ammo.TypeId)
                return true;

            // We are reloading, wait at least 5 seconds
            if (_lastWeaponReload.ContainsKey(weapon.ItemId) && DateTime.Now < _lastWeaponReload[weapon.ItemId].AddSeconds(5))
                return false;
            _lastWeaponReload[weapon.ItemId] = DateTime.Now;

            // Reload or change ammo
            if (weapon.Charge != null && weapon.Charge.TypeId == charge.TypeId)
            {
                Logging.Log("Combat: Reloading [" + weapon.ItemId + "] with [" + charge.TypeName + "][" + charge.TypeId + "]");
                weapon.ReloadAmmo(charge);
            }
            else
            {
                Logging.Log("Combat: Changing [" + weapon.ItemId + "] with [" + charge.TypeName + "][" + charge.TypeId + "]");
                weapon.ChangeAmmo(charge);
            }

            // Return false as we are reloading ammo
            return false;
        }

        /// <summary>
        ///   Reload correct (tm) ammo for the NPC
        /// </summary>
        /// <param name = "weapon"></param>
        /// <param name = "entity"></param>
        /// <returns>True if the (enough/correct) ammo is loaded, false if wrong/not enough ammo is loaded</returns>
        public bool ReloadAmmo(ModuleCache weapon, EntityCache entity)
        {
            // We need the cargo bay open for both reload actions
            var cargo = Cache.Instance.DirectEve.GetShipsCargo();
            if (cargo.Window == null)
            {
                Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                return false;
            }

            if (!cargo.IsReady)
                return false;

            return weapon.IsEnergyWeapon ? ReloadEnergyWeaponAmmo(weapon, entity) : ReloadNormalAmmo(weapon, entity);
        }

        /// <summary>
        ///   Returns true if it can activate the weapon on the target
        /// </summary>
        /// <remarks>
        ///   The idea behind this function is that a target that explodes isnt being fired on within 5 seconds
        /// </remarks>
        /// <param name = "module"></param>
        /// <param name = "entity"></param>
        /// <param name = "isWeapon"></param>
        /// <returns></returns>
        public bool CanActivate(ModuleCache module, EntityCache entity, bool isWeapon)
        {
            // We have changed target, allow activation
            if (entity.Id != module.LastTargetId)
                return true;

            // We have reloaded, allow activation
            if (isWeapon && module.CurrentCharges == MaxCharges)
                return true;

            // We havent reloaded, insert a wait-time
            if (_lastModuleActivation.ContainsKey(module.ItemId))
            {
                if (DateTime.Now.Subtract(_lastModuleActivation[module.ItemId]).TotalSeconds < 3)
                    return false;

                _lastModuleActivation.Remove(module.ItemId);
                return true;
            }

            _lastModuleActivation.Add(module.ItemId, DateTime.Now);
            return false;
        }

        /// <summary>
        ///   Returns the target we need to activate everything on
        /// </summary>
        /// <returns></returns>
        private EntityCache GetTarget()
        {
            // Find the first active weapon's target
            EntityCache weaponTarget = null;
            foreach (var weapon in Cache.Instance.Weapons.Where(m => m.IsActive))
            {
                // Find the target associated with the weapon
                weaponTarget = Cache.Instance.EntityById(weapon.TargetId);
                if (weaponTarget != null)
                    break;
            }

            // Return best possible target
            return Cache.Instance.GetBestTarget(weaponTarget, Cache.Instance.WeaponRange, false);
        }

        /// <summary>
        ///   Activate weapons
        /// </summary>
        private void ActivateWeapons(EntityCache target)
        {
            // Enable speed tank
            if (Cache.Instance.Approaching == null && Settings.Instance.SpeedTank)
                target.Orbit(Cache.Instance.OrbitDistance);

            // Get the weapons
            var weapons = Cache.Instance.Weapons;

            // TODO: Add check to see if there is better ammo to use! :)
            // Get distance of the target and compare that with the ammo currently loaded
            foreach (var weapon in weapons)
            {
                if (!weapon.IsActive)
                    continue;

                // No ammo loaded
                if (weapon.Charge == null)
                    continue;

                var ammo = Settings.Instance.Ammo.FirstOrDefault(a => a.TypeId == weapon.Charge.TypeId);

                //use mission specific ammo
                if (Cache.Instance.missionAmmo.Count() != 0)
                {
                    ammo = Cache.Instance.missionAmmo.FirstOrDefault(a => a.TypeId == weapon.Charge.TypeId);
                }

                // How can this happen? Someone manually loaded ammo
                if (ammo == null)
                    continue;

                // Target is in range
                if (target.Distance <= ammo.Range)
                    continue;

                // Target is out of range, stop firing
                weapon.Deactivate();
            }

            // Hax for max charges returning incorrect value
            if (!weapons.Any(w => w.IsEnergyWeapon))
            {
                MaxCharges = Math.Max(MaxCharges, weapons.Max(l => l.MaxCharges));
                MaxCharges = Math.Max(MaxCharges, weapons.Max(l => l.CurrentCharges));
            }

            // Activate the weapons (it not yet activated)))
            foreach (var weapon in weapons)
            {
                // Are we on the right target?
                if (weapon.IsActive)
                {
                    if (weapon.TargetId != target.Id)
                        weapon.Deactivate();

                    continue;
                }

                // Are we reloading, deactivating or changing ammo?
                if (weapon.IsReloadingAmmo || weapon.IsDeactivating || weapon.IsChangingAmmo)
                    continue;

                // No, check ammo type and if thats correct, activate weapon
                if (ReloadAmmo(weapon, target) && CanActivate(weapon, target, true))
                {
                    Logging.Log("Combat: Activating weapon [" + weapon.ItemId + "] on [" + target.Name + "][" + target.Id + "]");
                    weapon.Activate(target.Id);
                }
            }
        }

        /// <summary>
        ///   Activate target painters
        /// </summary>
        public void ActivateTargetPainters(EntityCache target)
        {
            var targetPainters = Cache.Instance.Modules.Where(m => m.GroupId == (int) Group.TargetPainter).ToList();

            // Find the first active weapon
            // Assist this weapon
            foreach (var painter in targetPainters)
            {
                // Are we on the right target?
                if (painter.IsActive)
                {
                    if (painter.TargetId != target.Id)
                        painter.Deactivate();

                    continue;
                }

                // Are we deactivating?
                if (painter.IsDeactivating)
                    continue;

                if (CanActivate(painter, target, false))
                {
                    Logging.Log("Combat: Activating painter [" + painter.ItemId + "] on [" + target.Name + "][" + target.Id + "]");
                    painter.Activate(target.Id);
                }
            }
        }


        /// <summary>
        ///   Activate Nos
        /// </summary>
        public void ActivateNos(EntityCache target)
        {
            var noses = Cache.Instance.Modules.Where(m => m.GroupId == (int)Group.nos).ToList();
            //Logging.Log("Combat: we have " + noses.Count.ToString() + " Nos modules");
            // Find the first active weapon
            // Assist this weapon
            foreach (var nos in noses)
            {
                // Are we on the right target?
                if (nos.IsActive)
                {
                    if (nos.TargetId != target.Id)
                        nos.Deactivate();

                    continue;
                }

                // Are we deactivating?
                if (nos.IsDeactivating)
                    continue;
                //Logging.Log("Combat: Distances Target[ " + target.Distance + " Optimal[" + nos.OptimalRange.ToString()+"]");
                // Target is out of Nos range
                if (target.Distance >= Settings.Instance.NosDistance)
                    continue;

                if (CanActivate(nos, target, false))
                {
                    Logging.Log("Combat: Nos  [" + nos.ItemId + "] on [" + target.Name + "][" + target.Id + "]");
                    nos.Activate(target.Id);
                }
                else
                {
                    Logging.Log("Combat: Cannot Activate Nos [" + nos.ItemId + "] on [" + target.Name + "][" + target.Id + "]");
                }
            }
        }

        /// <summary>
        ///   Activate StasisWeb
        /// </summary>
        public void ActivateStasisWeb(EntityCache target)
        {
            var webs = Cache.Instance.Modules.Where(m => m.GroupId == (int) Group.StasisWeb).ToList();

            // Find the first active weapon
            // Assist this weapon
            foreach (var web in webs)
            {
                // Are we on the right target?
                if (web.IsActive)
                {
                    if (web.TargetId != target.Id)
                        web.Deactivate();

                    continue;
                }

                // Are we deactivating?
                if (web.IsDeactivating)
                    continue;

                // Target is out of web range
                if (target.Distance >= web.OptimalRange)
                    continue;

                if (CanActivate(web, target, false))
                {
                    Logging.Log("Combat: Activating stasis web [" + web.ItemId + "] on [" + target.Name + "][" + target.Id + "]");
                    web.Activate(target.Id);
                }
            }
        }

        /// <summary>
        ///   Target combatants
        /// </summary>
        /// <remarks>
        ///   This only targets ships that are targeting you
        /// </remarks>
        private void TargetCombatants()
        {
            // We are jammed, forget targeting anything...
            if (Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets == 0)
            {
                if (!_isJammed)
                    Logging.Log("Combat: We are jammed and can't target anything");

                _isJammed = true;
                return;
            }

            if (_isJammed)
            {
                // Clear targeting list as it doesnt apply
                Cache.Instance.TargetingIDs.Clear();
                Logging.Log("Combat: We are no longer jammed, retargeting");
            }
            _isJammed = false;

            // Whats the range that we can target at
            var maxRange = Math.Min(Cache.Instance.DirectEve.ActiveShip.MaxTargetRange, Cache.Instance.WeaponRange);

            // Get a list of combat targets (combine targets + targeting)
            var targets = new List<EntityCache>();
            targets.AddRange(Cache.Instance.Targets);
            targets.AddRange(Cache.Instance.Targeting);
            var combatTargets = targets.Where(e => e.CategoryId == (int)CategoryID.Entity && e.IsNpc && !e.IsContainer && e.GroupId != (int)Group.LargeCollidableStructure).ToList();

            // Remove any target that is too far out of range (Weapon Range * 1.5)
            for (var i = combatTargets.Count - 1; i >= 0; i --)
            {
                var target = combatTargets[i];
                if (target.Distance > maxRange*1.5d)
                {
                    Logging.Log("Combat: Target [" + target.Name + "][" + target.Id + "] out of range");
                }
                else if (Cache.Instance.IgnoreTargets.Contains(target.Name.Trim()))
                {
                    Logging.Log("Combat: Target [" + target.Name + "][" + target.Id + "] on ignore list");
                }
                else continue;

                target.UnlockTarget();
                combatTargets.RemoveAt(i);
            }

            // Get a list of current high and low value targets
            var highValueTargets = combatTargets.Where(t => t.TargetValue.HasValue || Cache.Instance.PriorityTargets.Any(pt => pt.Id == t.Id)).ToList();
            var lowValueTargets = combatTargets.Where(t => !t.TargetValue.HasValue && !Cache.Instance.PriorityTargets.Any(pt => pt.Id == t.Id)).ToList();

            // Build a list of things targeting me
            var targetingMe = Cache.Instance.TargetedBy.Where(t => t.IsNpc && t.CategoryId == (int) CategoryID.Entity && !t.IsContainer && t.Distance < maxRange && !targets.Any(c => c.Id == t.Id) && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim())).ToList();
            var highValueTargetingMe = targetingMe.Where(t => t.TargetValue.HasValue).OrderByDescending(t => t.TargetValue.Value).ThenBy(t => t.Distance).ToList();
            var lowValueTargetingMe = targetingMe.Where(t => !t.TargetValue.HasValue).OrderBy(t => t.Distance).ToList();

            // Get the number of maximum targets, if there are no low or high value targets left, use the combined total of targets
            var maxHighValueTarget = (lowValueTargetingMe.Count + lowValueTargets.Count) == 0 ? Settings.Instance.MaximumLowValueTargets + Settings.Instance.MaximumHighValueTargets : Settings.Instance.MaximumHighValueTargets;
            var maxLowValueTarget = (highValueTargetingMe.Count + highValueTargets.Count) == 0 ? Settings.Instance.MaximumLowValueTargets + Settings.Instance.MaximumHighValueTargets : Settings.Instance.MaximumLowValueTargets;

            // Do we have too many high (none-priority) value targets targeted?
            while (highValueTargets.Where(t => !Cache.Instance.PriorityTargets.Any(pt => pt.Id == t.Id)).Count() > Math.Max(maxHighValueTarget - Cache.Instance.PriorityTargets.Count(), 0))
            {
                // Unlock any target
                var target = highValueTargets.OrderByDescending(t => t.Distance).Where(t => !Cache.Instance.PriorityTargets.Any(pt => pt.Id == t.Id)).FirstOrDefault();
                if (target == null)
                    break;

                target.UnlockTarget();
                highValueTargets.Remove(target);
            }

            // Do we have too many low value targets targeted?
            while (lowValueTargets.Count > maxLowValueTarget)
            {
                // Unlock any target
                var target = lowValueTargets.OrderByDescending(t => t.Distance).First();
                target.UnlockTarget();
                lowValueTargets.Remove(target);
            }

            // Do we have enough targeted?
            if ((highValueTargets.Count >= maxHighValueTarget && lowValueTargets.Count >= maxLowValueTarget) ||
                ((highValueTargets.Count + lowValueTargets.Count) >= (maxHighValueTarget + maxLowValueTarget)))
                return;

            // Do we have any priority targets?
            var priority = Cache.Instance.PriorityTargets.Where(t => t.Distance < maxRange && !targets.Any(c => c.Id == t.Id) && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim()));
            foreach (var entity in priority)
            {
                // Have we reached the limit of high value targets?
                if (highValueTargets.Count >= maxHighValueTarget)
                    break;

                Logging.Log("Combat: Targeting priority target [" + entity.Name + "][" + entity.Id + "]{" + highValueTargets.Count + "}");
                entity.LockTarget();
                highValueTargets.Add(entity);
            }

            foreach (var entity in highValueTargetingMe)
            {
                // Have we reached the limit of high value targets?
                if (highValueTargets.Count >= maxHighValueTarget)
                    break;

                Logging.Log("Combat: Targeting high value target [" + entity.Name + "][" + entity.Id + "]{" + highValueTargets.Count + "}");
                entity.LockTarget();
                highValueTargets.Add(entity);
            }

            foreach (var entity in lowValueTargetingMe)
            {
                // Have we reached the limit of low value targets?
                if (lowValueTargets.Count >= maxLowValueTarget)
                    break;

                Logging.Log("Combat: Targeting low value target [" + entity.Name + "][" + entity.Id + "]{" + lowValueTargets.Count + "}");
                entity.LockTarget();
                lowValueTargets.Add(entity);
            }
        }

        public void ProcessState()
        {
            // There is really no combat in stations (yet)
            if (Cache.Instance.InStation)
                return;

            // What? No ship entity?
            if (Cache.Instance.DirectEve.ActiveShip.Entity == null)
                return;

            // There is no combat when cloaked
            if (Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked)
                return;

            if (!Cache.Instance.Weapons.Any())
            {
                Logging.Log("Combat: No weapons with GroupId [" + Settings.Instance.WeaponGroupId + "] found!");
                State = CombatState.OutOfAmmo;
            }

            switch (State)
            {
                case CombatState.CheckTargets:
                    // Next state
                    State = CombatState.KillTargets;

                    TargetCombatants();
                    break;

                case CombatState.KillTargets:
                    // Next state
                    State = CombatState.CheckTargets;

                    var target = GetTarget();
                    if (target != null)
                    {
                        ActivateWeapons(target);
                        ActivateTargetPainters(target);
                        ActivateStasisWeb(target);
                        ActivateNos(target);
                    }
                    break;

                case CombatState.OutOfAmmo:
                    break;

                default:
                    // Next state
                    State = CombatState.CheckTargets;
                    break;
            }
        }
    }
}