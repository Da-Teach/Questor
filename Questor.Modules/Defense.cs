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
                if (!activate)
                    continue;

                if (module.IsActive)
                    continue;

                module.Activate();
            }
        }

        private void ActivateRepairModules()
        {
            foreach (var module in Cache.Instance.Modules)
            {
                double perc;
                if (module.GroupId == (int) Group.ShieldBoosters)
                    perc = Cache.Instance.DirectEve.ActiveShip.ShieldPercentage;
                else if (module.GroupId == (int) Group.ArmorRepairer)
                    perc = Cache.Instance.DirectEve.ActiveShip.ArmorPercentage;
                else
                    continue;

                var inCombat = Cache.Instance.TargetedBy.Count() > 0;
                if (!module.IsActive && !module.IsDeactivating && ((inCombat && perc < Settings.Instance.ActivateRepairModules) || (!inCombat && perc < Settings.Instance.DeactivateRepairModules && Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage > Settings.Instance.SafeCapacitorPct)))
                    module.Activate();
                else if (module.IsActive && !module.IsDeactivating && perc >= Settings.Instance.DeactivateRepairModules)
                    module.Deactivate();
            }
        }

        private void ActivateAfterburner()
        {
            foreach (var module in Cache.Instance.Modules)
            {
                if (module.GroupId != (int) Group.Afterburner)
                    continue;

                if (Cache.Instance.Approaching != null && !module.IsActive && !module.IsDeactivating)
                    module.Activate();
                else if (Cache.Instance.Approaching == null && module.IsActive && !module.IsDeactivating && (!Cache.Instance.Entities.Any(e => e.IsAttacking) || !Settings.Instance.SpeedTank))
                    module.Deactivate();
            }
        }

        public void ProcessState()
        {
            // Thank god stations are safe ! :)
            if (Cache.Instance.InStation)
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