// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.BackgroundTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;

    public class Salvage
    {
        public static HashSet<int> Salvagers = new HashSet<int> { 25861, 26983, 30836 };
        public static HashSet<int> TractorBeams = new HashSet<int> { 24348, 24620, 24622, 24644, 4250 };

        private DateTime _lastJettison = DateTime.MinValue;
        private DateTime _nextSalvageAction = DateTime.MinValue;
        private DateTime _lastSalvageProcessState;

        /// <summary>
        ///   Keep a list of times that we have tried to open a container (do not try to open the same container twice within 10 seconds)
        /// </summary>
        public static Dictionary<long, DateTime> OpenedContainers;

        public Salvage()
        {
            OpenedContainers = new Dictionary<long, DateTime>();
        }

        public int MaximumWreckTargets { get; set; }

        public bool LootEverything { get; set; }

        public int ReserveCargoCapacity { get; set; }

        public List<Ammo> Ammo { get; set; }

        public static void MoveIntoRangeOfWrecks() // DO NOT USE THIS ANYWHERE EXCEPT A PURPOSEFUL SALVAGE BEHAVIOR! - if you use this while in combat it will make you go poof quickly.
        {
            EntityCache closestWreck = Cache.Instance.UnlootedContainers.First();
            if (Math.Round(closestWreck.Distance, 0) > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closestWreck.Id))
            {
                if (closestWreck.Distance > (int)Distance.WarptoDistance)
                {
                    if (DateTime.Now > Cache.Instance.NextWarpTo)
                    {
                        Logging.Log("Salvage.NavigateIntorangeOfWrecks", "Warping to [" + closestWreck.Name + "] which is [" + Math.Round(closestWreck.Distance / 1000, 0) + "k away]", Logging.white);
                        closestWreck.WarpTo();
                        Cache.Instance.NextWarpTo = DateTime.Now.AddSeconds((int)Time.WarptoDelay_seconds);
                    }
                }
                else
                {
                    if (Cache.Instance.NextApproachAction < DateTime.Now && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closestWreck.Id))
                    {
                        Logging.Log("Salvage.NavigateIntorangeOfWrecks", "Approaching [" + closestWreck.Name + "] which is [" + Math.Round(closestWreck.Distance / 1000, 0) + "k away]", Logging.white);
                        closestWreck.Approach();
                        Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                    }
                }
            }
            else if (closestWreck.Distance <= (int)Distance.SafeScoopRange && Cache.Instance.Approaching != null)
            {
                Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                Logging.Log("Salvage.NavigateIntorangeOfWrecks", "Stop ship, ClosestWreck [" + Math.Round(closestWreck.Distance, 0) + "] is in scooprange + [" + (int)Distance.SafeScoopRange + "] and we were approaching", Logging.white);
            }
        }
        /// <summary>
        ///   Activates tractorbeam on targeted wrecks
        /// </summary>
        private void ActivateTractorBeams()
        {
            if (Cache.Instance.NextSalvageAction > DateTime.Now) return;
            List<ModuleCache> tractorBeams = Cache.Instance.Modules.Where(m => TractorBeams.Contains(m.TypeId)).ToList();
            if (tractorBeams.Count == 0)
                return;

            double tractorBeamRange = tractorBeams.Min(t => t.OptimalRange);
            List<EntityCache> wrecks = Cache.Instance.Targets.Where(t => (t.GroupId == (int)Group.Wreck || t.GroupId == (int)Group.CargoContainer) && t.Distance < tractorBeamRange).ToList();

            int tractorsProcessedThisTick = 0;

            for (int i = tractorBeams.Count - 1; i >= 0; i--)
            {
                ModuleCache tractorBeam = tractorBeams[i];
                if (!tractorBeam.IsActive && !tractorBeam.IsDeactivating || tractorBeam.InLimboState)
                    continue;

                EntityCache wreck = wrecks.FirstOrDefault(w => w.Id == tractorBeam.TargetId);
                // If the wreck no longer exists, or its within loot range then disable the tractor beam
                // If the wreck no longer exist, beam should be deactivated automatically. Without our interaction.
                if (tractorBeam.IsActive && (wreck == null || wreck.Distance <= (int)Distance.SafeScoopRange))
                {
                    tractorBeam.Click();
                    tractorsProcessedThisTick++;
                    Cache.Instance.NextSalvageAction = DateTime.Now.AddMilliseconds((int)Time.SalvageDelayBetweenActions_milliseconds);
                    if (tractorsProcessedThisTick < 2)
                        continue;
                    else
                    {
                        tractorsProcessedThisTick = 0;
                        return;
                    }
                }
                // Remove the tractor beam as a possible beam to activate
                tractorBeams.RemoveAt(i);
                wrecks.RemoveAll(w => w.Id == tractorBeam.TargetId);
            }

            foreach (EntityCache wreck in wrecks)
            {
                // This velocity check solves some bugs where velocity showed up as 150000000m/s
                if ((int)wreck.Velocity != 0 && wreck.Velocity < 10000)
                    continue;

                // Is this wreck within range?
                if (wreck.Distance < (int)Distance.SafeScoopRange)
                    continue;

                if (tractorBeams.Count == 0)
                    return;

                ModuleCache tractorBeam = tractorBeams[0];
                tractorBeams.RemoveAt(0);
                tractorBeam.Activate(wreck.Id);

                Logging.Log("Salvage", "Activating tractorbeam [" + tractorBeam.ItemId + "] on [" + wreck.Name + "]["+ Math.Round(wreck.Distance/1000,0) +"k][ID: " + wreck.Id + "]", Logging.white);
                Cache.Instance.NextSalvageAction = DateTime.Now.AddMilliseconds((int)Time.SalvageDelayBetweenActions_milliseconds);
                continue;
            }
        }

        /// <summary>
        ///   Activate salvagers on targeted wreck
        /// </summary>
        private void ActivateSalvagers()
        {
            if (Cache.Instance.NextSalvageAction > DateTime.Now) return;
            List<ModuleCache> salvagers = Cache.Instance.Modules.Where(m => Salvagers.Contains(m.TypeId)).ToList();
            if (salvagers.Count == 0)
                return;

            double salvagerRange = salvagers.Min(s => s.OptimalRange);
            List<EntityCache> wrecks = Cache.Instance.Targets.Where(t => t.GroupId == (int)Group.Wreck && t.Distance < salvagerRange && !Settings.Instance.WreckBlackList.Any(a => a == t.TypeId)).ToList();
            if (Cache.Instance.SalvageAll)
                wrecks = Cache.Instance.Targets.Where(t => t.GroupId == (int)Group.Wreck && t.Distance < salvagerRange).ToList();

            if (wrecks.Count == 0)
                return;
            int salvagersProcessedThisTick = 0;
            foreach (ModuleCache salvager in salvagers)
            {
                if (salvager.IsActive || salvager.InLimboState)
                    continue;

                // Spread the salvagers around
                EntityCache wreck = wrecks.OrderBy(w => salvagers.Count(s => s.LastTargetId == w.Id)).First();
                if (wreck == null)
                    return;

                Logging.Log("Salvage", "Activating salvager [" + salvager.ItemId + "] on [" + wreck.Name + "][ID: " + wreck.Id + "]", Logging.white);
                salvager.Activate(wreck.Id);
                salvagersProcessedThisTick++;
                Cache.Instance.NextSalvageAction = DateTime.Now.AddMilliseconds((int)Time.SalvageDelayBetweenActions_milliseconds);
                if (salvagersProcessedThisTick < 2)
                    continue;
                else
                {
                    salvagersProcessedThisTick = 0;
                    return;
                }
            }
        }

        /// <summary>
        ///   Target wrecks within range
        /// </summary>
        private void TargetWrecks()
        {
            if (DateTime.Now < Cache.Instance.NextTargetAction) return;

            // We are jammed, we do not need to log (Combat does this already)
            if (Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets == 0)
                return;

            //List<ModuleCache> salvagers = Cache.Instance.Modules.Where(m => Salvagers.Contains(m.TypeId)).ToList();
            List<ModuleCache> tractorBeams = Cache.Instance.Modules.Where(m => TractorBeams.Contains(m.TypeId)).ToList();

            //if (salvagers.Count == 0 && tractorBeams.Count == 0)
            //    return;
            var targets = new List<EntityCache>();
            targets.AddRange(Cache.Instance.Targets);
            targets.AddRange(Cache.Instance.Targeting);

            bool hasSalvagers = Cache.Instance.Modules.Any(m => Salvagers.Contains(m.TypeId));
            List<EntityCache> wreckTargets = targets.Where(t => (t.GroupId == (int)Group.Wreck || t.GroupId == (int)Group.CargoContainer) && t.CategoryId == (int)CategoryID.Celestial).ToList();

            // Check for cargo containers
            foreach (EntityCache wreck in wreckTargets)
            {
                if (Cache.Instance.IgnoreTargets.Contains(wreck.Name))
                {
                    Logging.Log("Salvage", "Cargo Container [" + wreck.Name + "][" + Math.Round(wreck.Distance / 1000, 0) + "k][ID: " + wreck.Id + "] on the ignore list, ignoring.", Logging.white);
                    wreck.UnlockTarget();
                    Cache.Instance.NextTargetAction = DateTime.Now.AddMilliseconds((int)Time.TargetDelay_milliseconds);
                    continue;
                }

                if (!Cache.Instance.SalvageAll)
                {
                    if (Settings.Instance.WreckBlackList.Any(a => a == wreck.TypeId) && (wreck.Distance < (int)Distance.SafeScoopRange || wreck.IsWreckEmpty))
                    {
                        Logging.Log("Salvage", "Cargo Container [" + wreck.Name + "][" + Math.Round(wreck.Distance / 1000, 0) + "k][ID: " + wreck.Id + "] within loot range,wreck is empty, or wreck is on our blacklist, unlocking container.", Logging.white);
                        wreck.UnlockTarget();
                        Cache.Instance.NextTargetAction = DateTime.Now.AddMilliseconds((int)Time.TargetDelay_milliseconds);
                        continue;
                    }
                }

                if (hasSalvagers && wreck.GroupId != (int)Group.CargoContainer)
                    continue;

                // Unlock if within loot range
                if (wreck.Distance < (int)Distance.SafeScoopRange)
                {
                    Logging.Log("Salvage", "Cargo Container [" + wreck.Name + "][" + Math.Round(wreck.Distance / 1000, 0) + "k][ID: " + wreck.Id + "] within loot range, unlocking container.", Logging.white);
                    wreck.UnlockTarget();
                    Cache.Instance.NextTargetAction = DateTime.Now.AddMilliseconds((int)Time.TargetDelay_milliseconds);
                    continue;
                }
            }

            if (Cache.Instance.MissionLoot)
            {
                if (wreckTargets.Count >= Math.Min(Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets, Cache.Instance.DirectEve.Me.MaxLockedTargets))
                    return;
            }
            else if (wreckTargets.Count >= MaximumWreckTargets)
                return;

            double tractorBeamRange = 0d;
            if (tractorBeams.Count > 0)
                tractorBeamRange = tractorBeams.Min(t => t.OptimalRange);

            int wrecksProcessedThisTick = 0;
            IEnumerable<EntityCache> wrecks = Cache.Instance.UnlootedContainers;
            foreach (EntityCache wreck in wrecks.Where(w => !Cache.Instance.IgnoreTargets.Contains(w.Name.Trim())))
            {
                // Its already a target, ignore it
                if (wreck.IsTarget || wreck.IsTargeting)
                    continue;

                if (wreck.Distance > tractorBeamRange)
                    continue;

                if (!wreck.HaveLootRights)
                    continue;

                // No need to tractor a non-wreck within loot range
                if (wreck.GroupId != (int)Group.Wreck && wreck.Distance < (int)Distance.SafeScoopRange)
                    continue;

                if (!Cache.Instance.SalvageAll)
                {
                    if (Settings.Instance.WreckBlackList.Any(a => a == wreck.TypeId) && (wreck.IsWreckEmpty || wreck.Distance < (int)Distance.SafeScoopRange))
                        continue;
                }

                if (wreck.GroupId != (int)Group.Wreck && wreck.GroupId != (int)Group.CargoContainer)
                    continue;

                if (!hasSalvagers)
                {
                    // Ignore already looted wreck
                    if (Cache.Instance.LootedContainers.Contains(wreck.Id))
                        continue;

                    // Ignore empty wrecks
                    if (wreck.GroupId == (int)Group.Wreck && wreck.IsWreckEmpty)
                        continue;

                    // Ignore wrecks already in loot range
                    if (wreck.Distance < (int)Distance.SafeScoopRange)
                        continue;
                }

                Logging.Log("Salvage", "Locking [" + wreck.Name + "][" + Math.Round(wreck.Distance / 1000, 0) + "k][ID:" + wreck.Id + "][" + Math.Round(wreck.Distance / 1000, 0) + "k away]", Logging.white);

                wreck.LockTarget();
                wreckTargets.Add(wreck);
                wrecksProcessedThisTick++;
                //_nextTargetAction = DateTime.Now.AddMilliseconds((int)Time.TargetDelay_miliseconds);
                if (Cache.Instance.MissionLoot)
                {
                    if (wreckTargets.Count >= Math.Min(Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets, Cache.Instance.DirectEve.Me.MaxLockedTargets))
                        return;
                }
                else
                    if (wreckTargets.Count >= MaximumWreckTargets)
                        return;
                //return;
            }
        }

        /// <summary>
        ///   Loot any wrecks & cargo containers close by
        /// </summary>
        private void LootWrecks()
        {
            if (Cache.Instance.NextLootAction > DateTime.Now) return;

            if (!Cache.Instance.OpenCargoHold("Salvage")) return;

            List<ItemCache> shipsCargo = Cache.Instance.CargoHold.Items.Select(i => new ItemCache(i)).ToList();
            double freeCargoCapacity = Cache.Instance.CargoHold.Capacity - Cache.Instance.CargoHold.UsedCapacity;
            
            DirectContainerWindow lootWindows = Cache.Instance.DirectEve.Windows.OfType<DirectContainerWindow>().FirstOrDefault(w => w.Type == "form.Inventory");
            List<long> ContainersID = lootWindows.GetIdsFromTree();

            foreach (long ContainerID in ContainersID) //ItemWreck and ItemFloatingCargo
            {
                lootWindows.SelectTreeEntryByID(ContainerID);

                // Get the container entity
                    EntityCache containerEntity = Cache.Instance.EntityById(ContainerID);

                // Get the container that is associated with the cargo container
                    DirectContainer container = Cache.Instance.DirectEve.GetContainer(ContainerID);

                // List its items
                IEnumerable<ItemCache> items = container.Items.Select(i => new ItemCache(i)).ToList();

                // Build a list of items to loot
                var lootItems = new List<ItemCache>();

                if (containerEntity != null && containerEntity.IsValid)
                {
                    // log wreck contents to file
                    if (!Statistics.WreckStatistics(items, containerEntity)) break;
                }

                // Does it no longer exist or is it out of transfer range or its looted
                if (containerEntity == null || containerEntity.Distance > (int)Distance.SafeScoopRange || Cache.Instance.LootedContainers.Contains(containerEntity.Id))
                {
                    lootWindows.CloseTreeEntry(ContainerID);
                    return;
                }

                //if (freeCargoCapacity < 1000) //this should allow BSs to dump scrapmetal but haulers and noctus' to hold onto it
                //{
                //	// Dump scrap metal if we have any
                //	if (containerEntity.Name == "Cargo Container" && shipsCargo.Any(i => i.IsScrapMetal))
                //	{
                //		foreach (var item in shipsCargo.Where(i => i.IsScrapMetal))
                //		{
                //			container.Add(item.DirectItem);
                //			freeCargoCapacity += item.TotalVolume;
                //		}
                //
                //		shipsCargo.RemoveAll(i => i.IsScrapMetal);
                //	}
                //}
                // Walk through the list of items ordered by highest value item first
                foreach (ItemCache item in items.OrderByDescending(i => i.IskPerM3))
                {
                    if (freeCargoCapacity < 1000) //this should allow BSs to not pickup large low value items but haulers and noctus' to scoop everything
                    {
                        // We never want to pick up a cap booster
                        if (item.GroupID == (int)Group.CapacitorGroupCharge)
                            continue;
                    }
                    // We pick up loot depending on isk per m3
                    bool isMissionItem = Cache.Instance.MissionItems.Contains((item.Name ?? string.Empty).ToLower());

                    // Never pick up contraband (unless its the mission item)
                    if (!isMissionItem && item.IsContraband)
                        continue;

                    // Do we want to loot other items?
                    if (!isMissionItem && !LootEverything)
                        continue;

                    // Do not pick up items that cannot enter in a freighter container (unless its the mission item)
                    // Note: some mission items that are alive have been allowed to be
                    //       scooped because unloadlootstate.MoveCommonMissionCompletionitems
                    //       will move them into the hangar floor not the loot location
                    if (!isMissionItem && item.IsAliveandWontFitInContainers)
                        continue;

                    // We are at our max, either make room or skip the item
                    if ((freeCargoCapacity - item.TotalVolume) <= (isMissionItem ? 0 : ReserveCargoCapacity))
                    {
                        // We can't drop items in this container anyway, well get it after its salvaged
                        if (!isMissionItem && containerEntity.GroupId != (int)Group.CargoContainer)
                            continue;

                        // Make a list of items which are worth less
                        List<ItemCache> worthLess;
                        if (isMissionItem)
                            worthLess = shipsCargo;
                        else if (item.IskPerM3.HasValue)
                            worthLess = shipsCargo.Where(sc => sc.IskPerM3.HasValue && sc.IskPerM3 < item.IskPerM3).ToList();
                        else
                            worthLess = shipsCargo.Where(sc => sc.IskPerM3.HasValue).ToList();

                        // Remove mission item from this list
                        worthLess.RemoveAll(wl => Cache.Instance.MissionItems.Contains((wl.Name ?? string.Empty).ToLower()));
                        worthLess.RemoveAll(wl => (wl.Name ?? string.Empty).ToLower() == Cache.Instance.BringMissionItem.ToLower());

                        // Consider dropping ammo if it concerns the mission item!
                        if (!isMissionItem)
                            worthLess.RemoveAll(wl => Ammo.Any(a => a.TypeId == wl.TypeId));

                        // Nothing is worth less then the current item
                        if (!worthLess.Any())
                            continue;

                        // Not enough space even if we dumped the crap
                        if ((freeCargoCapacity + worthLess.Sum(wl => wl.TotalVolume)) < item.TotalVolume)
                        {
                            if (isMissionItem)
                                Logging.Log("Salvage", "Not enough space for mission item! Need [" + item.TotalVolume + "] maximum available [" + (freeCargoCapacity + worthLess.Sum(wl => wl.TotalVolume)) + "]", Logging.white);

                            continue;
                        }

                        // Start clearing out items that are worth less
                        var moveTheseItems = new List<DirectItem>();
                        foreach (ItemCache wl in worthLess.OrderBy(wl => wl.IskPerM3.HasValue ? wl.IskPerM3.Value : double.MaxValue).ThenByDescending(wl => wl.TotalVolume))
                        {
                            // Mark this item as moved
                            moveTheseItems.Add(wl.DirectItem);

                            // Subtract (now) free volume
                            freeCargoCapacity += wl.TotalVolume;

                            // We freed up enough space?
                            if ((freeCargoCapacity - item.TotalVolume) >= ReserveCargoCapacity)
                                break;
                        }

                        if (moveTheseItems.Count > 0)
                        {
                            // If this is not a cargo container, then jettison loot
                            if (containerEntity.GroupId != (int)Group.CargoContainer || isMissionItem)
                            {
                                if (DateTime.Now.Subtract(Cache.Instance.LastJettison).TotalSeconds < (int)Time.DelayBetweenJetcans_seconds)
                                    return;

                                Logging.Log("Salvage", "Jettisoning [" + moveTheseItems.Count + "] items to make room for the more valuable loot", Logging.white);

                                // Note: This could (in theory) fuck up with the bot jettison an item and
                                // then picking it up again :/ (granted it should never happen unless
                                // mission item volume > reserved volume
                                Cache.Instance.CargoHold.Jettison(moveTheseItems.Select(i => i.ItemId));
                                Cache.Instance.LastJettison = DateTime.Now;
                                return;
                            }

                            // Move items to the cargo container
                            container.Add(moveTheseItems);
                            Cache.Instance.NextLootAction = DateTime.Now.AddMilliseconds((int)Time.LootingDelay_milliseconds);

                            // Remove it from the ships cargo list
                            shipsCargo.RemoveAll(i => moveTheseItems.Any(wl => wl.ItemId == i.Id));
                            Logging.Log("Salvage", "Moving [" + moveTheseItems.Count + "] items into the cargo container to make room for the more valuable loot", Logging.white);
                            return;
                        }
                    }

                    // Update free space
                    freeCargoCapacity -= item.TotalVolume;
                    lootItems.Add(item);
                }

                // Mark container as looted
                Cache.Instance.LootedContainers.Add(containerEntity.Id);

                // Loot actual items
                if (lootItems.Count != 0)
                {
                    Logging.Log("Salvage", "Looting container [" + containerEntity.Name + "][" + Math.Round(containerEntity.Distance / 1000, 0) + "k][ID: " + containerEntity.Id + "], [" + lootItems.Count + "] valuable items", Logging.white);
                    Cache.Instance.CargoHold.Add(lootItems.Select(i => i.DirectItem));
                }
                else
                    Logging.Log("Salvage", "Container [" + containerEntity.Name + "][" + Math.Round(containerEntity.Distance / 1000, 0) + "k][ID: " + containerEntity.Id + "] contained no valuable items", Logging.white);
            }

            // Open a container in range
            foreach (EntityCache containerEntity in Cache.Instance.Containers.Where(e => e.Distance <= (int)Distance.SafeScoopRange))
            {
                // Empty wreck, ignore
                if (containerEntity.GroupId == (int)Group.Wreck && containerEntity.IsWreckEmpty)
                    continue;

                // We looted this container
                if (Cache.Instance.LootedContainers.Contains(containerEntity.Id))
                {
                    if (Settings.Instance.DebugLootWrecks) Logging.Log("Salvage.LootWrecks", "We have already looted [" + containerEntity.Id + "]", Logging.white);
                    continue;
                }

                // Ignore open request within 10 seconds
                if (OpenedContainers.ContainsKey(containerEntity.Id) && DateTime.Now.Subtract(OpenedContainers[containerEntity.Id]).TotalSeconds < 10)
                    continue;

                // Don't even try to open a wreck if you are speed tanking and you aren't processing a loot action
                if (Settings.Instance.SpeedTank && Cache.Instance.OpenWrecks == false)
                {
                    if (Settings.Instance.DebugLootWrecks) Logging.Log("Salvage.LootWrecks", "SpeedTank is true and OpenWrecks is false [" + containerEntity.Id + "]", Logging.white);
                    continue;
                }

                // Don't even try to open a wreck if you are specified LootEverything as false and you aren't processing a loot action
                //      this is currently commented out as it would keep golems and other non-speed tanked ships from looting the field as they cleared
                //      missions, but NOT stick around after killing things to clear it ALL. Looteverything==false does NOT mean loot nothing
                //if (Settings.Instance.LootEverything == false && Cache.Instance.OpenWrecks == false)
                //    continue;

                // Open the container
                Logging.Log("Salvage", "Opening container [" + containerEntity.Name + "][" + Math.Round(containerEntity.Distance / 1000, 0) + "k][ID: " + containerEntity.Id + "]", Logging.white);
                Cache.Instance.ContainerInSpace = Cache.Instance.DirectEve.GetContainer(containerEntity.Id);
                if (Cache.Instance.ContainerInSpace == null)
                    continue;
                    
                if (Cache.Instance.ContainerInSpace.Window == null)
                {
                    containerEntity.OpenCargo();    
                    Cache.Instance.NextLootAction = DateTime.Now.AddMilliseconds((int)Time.LootingDelay_milliseconds);
                    return;
                }

                if (!Cache.Instance.ContainerInSpace.Window.IsReady)
                {
                    if (Settings.Instance.DebugLootWrecks) Logging.Log("Salvage", "LootWrecks: Cache.Instance.ContainerInSpace.Window is not ready", Logging.white);
                    return;
                }

                if (Cache.Instance.ContainerInSpace.Window.IsReady)
                {
                    if (Settings.Instance.DebugLootWrecks) Logging.Log("Salvage", "LootWrecks: Cache.Instance.ContainerInSpace.Window is ready", Logging.white);
                    //if (!Cache.Instance.ContainerInSpace.Window.Name.ToLower().Contains("secondary".ToLower()))
                    //{
                    //    if (Settings.Instance.DebugLootWrecks) Logging.Log("Salvage", "LootWrecks: Cache.Instance.ContainerInSpace.Window is not yet a secondarywindow", Logging.white);
                    //    Cache.Instance.ContainerInSpace.Window.OpenAsSecondary();
                    //    return;
                    //}
                    OpenedContainers[containerEntity.Id] = DateTime.Now;
                    Cache.Instance.NextLootAction = DateTime.Now.AddMilliseconds((int)Time.LootingDelay_milliseconds);
                    return;
                }
                return;
            }
        }

        public void ProcessState()
        {
            if (DateTime.Now < _lastSalvageProcessState.AddMilliseconds(100)) //if it has not been 100ms since the last time we ran this ProcessState return. We can't do anything that close together anyway
                return;

            _lastSalvageProcessState = DateTime.Now;

            // Nothing to salvage in stations
            if (Cache.Instance.InStation)
            {
                _States.CurrentSalvageState = SalvageState.Idle;
                return;
            }

            if (!Cache.Instance.InSpace)
            {
                _States.CurrentSalvageState = SalvageState.Idle;
                return;
            }

            // What? No ship entity?
            if (Cache.Instance.DirectEve.ActiveShip.Entity == null)
            {
                _States.CurrentSalvageState = SalvageState.Idle;
                return;
            }

            // When in warp there's nothing we can do, so ignore everything
            if (Cache.Instance.InWarp)
            {
                _States.CurrentSalvageState = SalvageState.Idle;
                return;
            }

            // There is no salving when cloaked -
            // why not? seems like we might be able to ninja-salvage with a covert-ops hauler with some additional coding (someday?)
            if (Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked)
            {
                _States.CurrentSalvageState = SalvageState.Idle;
                return;
            }

            DirectContainer cargo = Cache.Instance.DirectEve.GetShipsCargo();
            switch (_States.CurrentSalvageState)
            {
                case SalvageState.TargetWrecks:
                    TargetWrecks();

                    // Next state
                    _States.CurrentSalvageState = SalvageState.LootWrecks;
                    break;

                case SalvageState.LootWrecks:
                    LootWrecks();

                    _States.CurrentSalvageState = SalvageState.SalvageWrecks;
                    break;

                case SalvageState.SalvageWrecks:
                    ActivateTractorBeams();
                    ActivateSalvagers();

                    // Default action
                    _States.CurrentSalvageState = SalvageState.TargetWrecks;
                    if (cargo.Window.IsReady && cargo.Items.Any() && Cache.Instance.NextSalvageAction < DateTime.Now)
                    {
                        // Check if there are actually duplicates
                        bool duplicates = cargo.Items.Where(i => i.Quantity > 0).GroupBy(i => i.TypeId).Any(t => t.Count() > 1);
                        if (duplicates)
                        {
                            _States.CurrentSalvageState = SalvageState.StackItems;
                            Cache.Instance.NextSalvageAction = DateTime.Now.AddSeconds((int)Time.SalvageStackItems_seconds);
                        }
                    }
                    break;

                case SalvageState.StackItems:
                    Logging.Log("Salvage", "Stacking items", Logging.white);

                    if (cargo.Window.IsReady)
                        cargo.StackAll();

                    Cache.Instance.NextSalvageAction = DateTime.Now.AddSeconds((int)Time.SalvageStackItemsDelayBeforeResuming_seconds);
                    _States.CurrentSalvageState = SalvageState.WaitForStacking;
                    break;

                case SalvageState.WaitForStacking:
                    // Wait 5 seconds after stacking
                    if (Cache.Instance.NextSalvageAction > DateTime.Now)
                        break;

                    if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        Logging.Log("Salvage", "Done stacking", Logging.white);
                        _States.CurrentSalvageState = SalvageState.TargetWrecks;
                        break;
                    }

                    if (DateTime.Now.Subtract(Cache.Instance.NextSalvageAction).TotalSeconds > 120)
                    {
                        Logging.Log("Salvage", "Stacking items timed out, clearing item locks", Logging.white);
                        Cache.Instance.DirectEve.UnlockItems();

                        Logging.Log("Salvage", "Done stacking", Logging.white);
                        _States.CurrentSalvageState = SalvageState.TargetWrecks;
                        break;
                    }
                    break;

                case SalvageState.Idle:
                    if (Cache.Instance.InSpace &&
                        Cache.Instance.DirectEve.ActiveShip.Entity != null &&
                        !Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked &&
                        (Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != Settings.Instance.CombatShipName ||
                        Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != Settings.Instance.SalvageShipName) &&
                        !Cache.Instance.InWarp)
                    {
                        _States.CurrentSalvageState = SalvageState.TargetWrecks;
                        return;
                    }
                    break;

                default:
                    // Unknown state, goto first state
                    _States.CurrentSalvageState = SalvageState.TargetWrecks;
                    break;
            }
        }
    }
}