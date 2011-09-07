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
    using System.Linq;

    public class Defense
    {
        private void ActivateOnce()
        {
            foreach (var module in Cache.Instance.Modules)
            {
                if (!module.IsActivatable)
                    continue;

                var activate = false;
                activate |= module.GroupId == (int) Group.ShieldHardeners;
                activate |= module.GroupId == (int) Group.DamageControl;
                activate |= module.GroupId == (int) Group.ArmorHardeners;
                activate |= module.GroupId == (int) Group.SensorBooster;
                activate |= module.GroupId == (int) Group.TrackingComputer;
                activate |= module.GroupId == (int) Group.ECCM;

                if (!activate)
                    continue;

                if (module.IsActive)
                    continue;

                module.Click();
            }
        }

        private void ActivateRepairModules()
        {
            foreach (var module in Cache.Instance.Modules)
            {
                if (module.InLimboState)
                    continue;

                double perc;
                if (module.GroupId == (int) Group.ShieldBoosters)
                    perc = Cache.Instance.DirectEve.ActiveShip.ShieldPercentage;
                else if (module.GroupId == (int) Group.ArmorRepairer)
                    perc = Cache.Instance.DirectEve.ActiveShip.ArmorPercentage;
                else
                    continue;

                var inCombat = Cache.Instance.TargetedBy.Count() > 0;
                if (!module.IsActive && ((inCombat && perc < Settings.Instance.ActivateRepairModules) || (!inCombat && perc < Settings.Instance.DeactivateRepairModules && Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage > Settings.Instance.SafeCapacitorPct)))
                    module.Click();
                else if (module.IsActive && perc >= Settings.Instance.DeactivateRepairModules)
                    module.Click();
            }
        }

        private void ActivateAfterburner()
        {
            foreach (var module in Cache.Instance.Modules)
            {
                if (module.GroupId != (int) Group.Afterburner)
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
                    deactivate |= Cache.Instance.Approaching.Distance <  Settings.Instance.MinimumPropulsionModuleDistance;
                }

                // If we have less then x% cap, do not activate or deactivate the module
                activate &= Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage > Settings.Instance.MinimumPropulsionModuleCapacitor;
                deactivate |= Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage < Settings.Instance.MinimumPropulsionModuleCapacitor;

                if (activate)
                    module.Click();
                else if (deactivate)
                    module.Click();
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

            // Cap is SO low that we shouldn't care about hardeners/boosters as we arent being targeted anyhow
            if (Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage < 10 && Cache.Instance.TargetedBy.Count() == 0)
                return;

            ActivateOnce();
            ActivateRepairModules();
            ActivateAfterburner();
        }
    }
}