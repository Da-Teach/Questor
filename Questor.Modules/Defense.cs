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
    using System.Linq;
    using System.Diagnostics;

    public class Defense
    {
        private void ActivateOnce()
        {
            foreach (var module in Cache.Instance.Modules)
            {
                if (!module.IsActivatable)
                    continue;

                var activate = false;
                activate |= module.GroupId == (int)Group.ShieldHardeners;
                activate |= module.GroupId == (int)Group.DamageControl;
                activate |= module.GroupId == (int)Group.ArmorHardeners;
                activate |= module.GroupId == (int)Group.SensorBooster;
                activate |= module.GroupId == (int)Group.TrackingComputer;
                activate |= module.GroupId == (int)Group.ECCM;
                activate |= module.GroupId == (int)Group.CloakingDevice;

                if (!activate)
                    continue;

                if (module.IsActive | module.IsGoingOnline | module.IsDeactivating)
                    continue;

                if (module.GroupId == (int)Group.CloakingDevice)
                {
                    //Logging.Log("This module has a typeID of: " + module.TypeId + " !!");
                    if (module.TypeId != 11578)  //11578 Covert Ops Cloaking Device - if you don't have a covert ops cloak try the next module
                    {
                        continue;
                    }
                    var StuffThatMayDecloakMe = Cache.Instance.Entities.Where(t => t.Name != Cache.Instance.DirectEve.Me.Name || t.IsBadIdea || t.IsContainer || t.IsNpc || t.IsPlayer).OrderBy(t => t.Distance).FirstOrDefault();
                    if (StuffThatMayDecloakMe != null || StuffThatMayDecloakMe.Distance <= (int)Distance.SafeToCloakDistance) //if their is anything within 2300m do not attempt to cloak
                    {
                        if (StuffThatMayDecloakMe.Distance != 0)
                        {
                            //Logging.Log(StuffThatMayDecloakMe.Name + " is very close at: " + StuffThatMayDecloakMe.Distance + " meters");
                            continue;
                        }
                    }
                }
                //
                // at this point the module should be active but isn't: activate it, set the delay and return. The process will resume on the next tick
                //
                module.Click();
            }
        }

        private void ActivateRepairModules()
        {
            var watch = new Stopwatch();
            foreach (var module in Cache.Instance.Modules)
            {
                if (module.InLimboState)
                    continue;

                double perc;
                if (module.GroupId == (int)Group.ShieldBoosters)
                {
                    perc = Cache.Instance.DirectEve.ActiveShip.ShieldPercentage;
                }
                else if (module.GroupId == (int)Group.ArmorRepairer)
                {
                    perc = Cache.Instance.DirectEve.ActiveShip.ArmorPercentage;
                }
                else
                    continue;

                var inCombat = Cache.Instance.TargetedBy.Count() > 0;
                if (!module.IsActive && ((inCombat && perc < Settings.Instance.ActivateRepairModules) || (!inCombat && perc < Settings.Instance.DeactivateRepairModules && Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage > Settings.Instance.SafeCapacitorPct)))
                {
                    if (Cache.Instance.DirectEve.ActiveShip.ShieldPercentage < Cache.Instance.lowest_shield_percentage_this_pocket)
                    {
                        Cache.Instance.lowest_shield_percentage_this_pocket = Cache.Instance.DirectEve.ActiveShip.ShieldPercentage;
                        Cache.Instance.lowest_shield_percentage_this_mission = Cache.Instance.DirectEve.ActiveShip.ShieldPercentage;
                        Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                    }
                    if (Cache.Instance.DirectEve.ActiveShip.ArmorPercentage < Cache.Instance.lowest_armor_percentage_this_pocket)
                    {
                        Cache.Instance.lowest_armor_percentage_this_pocket = Cache.Instance.DirectEve.ActiveShip.ArmorPercentage;
                        Cache.Instance.lowest_armor_percentage_this_mission = Cache.Instance.DirectEve.ActiveShip.ArmorPercentage;
                        Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                    }
                    if (Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage < Cache.Instance.lowest_capacitor_percentage_this_pocket)
                    {
                        Cache.Instance.lowest_capacitor_percentage_this_pocket = Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage;
                        Cache.Instance.lowest_capacitor_percentage_this_mission = Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage;
                        Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                    }
                    //More human behaviour
                    //System.Threading.Thread.Sleep(333);
                    module.Click();
                    Cache.Instance.StartedBoosting = DateTime.Now;

                    //Logging.Log("LowestShieldPercentage(pocket) [ " + Cache.Instance.lowest_shield_percentage_this_pocket + " ] ");
                    //Logging.Log("LowestArmorPercentage(pocket) [ " + Cache.Instance.lowest_armor_percentage_this_pocket + " ] ");
                    //Logging.Log("LowestCapacitorPercentage(pocket) [ " + Cache.Instance.lowest_capacitor_percentage_this_pocket + " ] ");
                    //Logging.Log("LowestShieldPercentage(mission) [ " + Cache.Instance.lowest_shield_percentage_this_mission + " ] ");
                    //Logging.Log("LowestArmorPercentage(mission) [ " + Cache.Instance.lowest_armor_percentage_this_mission + " ] ");
                    //Logging.Log("LowestCapacitorPercentage(mission) [ " + Cache.Instance.lowest_capacitor_percentage_this_mission + " ] ");
                }
                else if (module.IsActive && perc >= Settings.Instance.DeactivateRepairModules)
                {
                    //More human behaviour
                    //System.Threading.Thread.Sleep(333);
                    module.Click();
                    Cache.Instance._lastModuleActivation = DateTime.Now;
                    Cache.Instance.repair_cycle_time_this_pocket = Cache.Instance.repair_cycle_time_this_pocket + ((int)DateTime.Now.Subtract(Cache.Instance.StartedBoosting).TotalSeconds);
                    Cache.Instance.repair_cycle_time_this_mission = Cache.Instance.repair_cycle_time_this_mission + ((int)DateTime.Now.Subtract(Cache.Instance.StartedBoosting).TotalSeconds);
                    Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                    //Cache.Instance.repair_cycle_time_this_pocket = Cache.Instance.repair_cycle_time_this_pocket + ((int)watch.Elapsed);
                    //Cache.Instance.repair_cycle_time_this_mission = Cache.Instance.repair_cycle_time_this_mission + watch.Elapsed.TotalMinutes;
                }
            }
        }

        private void ActivateAfterburner()
        {
            foreach (var module in Cache.Instance.Modules)
            {
                if (module.GroupId != (int)Group.Afterburner)
                    continue;

                if (module.InLimboState)
                    continue;

                // Should we activate the module
                var activate = Cache.Instance.Approaching != null;
                activate &= !module.IsActive;
                activate &= !module.IsDeactivating;

                // Should we deactivate the module?
                var deactivate = Cache.Instance.Approaching == null;
                deactivate &= module.IsActive;
                deactivate &= !module.IsDeactivating;
                deactivate &= (!Cache.Instance.Entities.Any(e => e.IsAttacking) || !Settings.Instance.SpeedTank);

                // This only applies when not speed tanking
                if (!Settings.Instance.SpeedTank && Cache.Instance.Approaching != null)
                {
                    // Activate if target is far enough
                    activate &= Cache.Instance.Approaching.Distance > Settings.Instance.MinimumPropulsionModuleDistance;
                    // Deactivate if target is too close
                    deactivate |= Cache.Instance.Approaching.Distance < Settings.Instance.MinimumPropulsionModuleDistance;
                }

                // If we have less then x% cap, do not activate or deactivate the module
                //Logging.Log("Defense: Current Cap [" + Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage + "]" + "Settings: minimumPropulsionModuleCapacitor [" + Settings.Instance.MinimumPropulsionModuleCapacitor + "]");              
                activate &= Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage > Settings.Instance.MinimumPropulsionModuleCapacitor;
                deactivate |= Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage < Settings.Instance.MinimumPropulsionModuleCapacitor;

                if (activate)
                {
                    //More human behaviour
                    //System.Threading.Thread.Sleep(333); 
                    module.Click();
                }
                else if (deactivate && module.IsActive)
                {
                    //More human behaviour
                    //System.Threading.Thread.Sleep(333); 
                    module.Click();
                }
            }
        }

        public void ProcessState()
        {
            // Thank god stations are safe ! :)
            if (Cache.Instance.InStation)
                return;

            // What? No ship entity?
            if (Cache.Instance.DirectEve.ActiveShip.Entity == null)
                return;

            // There is no better defense then being cloaked ;)
            if (Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked)
                return;

            // Cap is SO low that we shouldn't care about hardeners/boosters as we aren't being targeted anyhow
            if (Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage < 10 && Cache.Instance.TargetedBy.Count() == 0)
                return;

            ActivateOnce();
            ActivateRepairModules();
            ActivateAfterburner();
        }
    }
}