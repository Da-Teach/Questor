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

    public class Panic
    {
        private double _lastNormalX;
        private double _lastNormalY;
        private double _lastNormalZ;

        private DateTime _nextRepairAction;

        public PanicState State { get; set; }

        public void ProcessState()
        {
            switch (State)
            {
                case PanicState.Normal:
                    if (Cache.Instance.DirectEve.ActiveShip.Entity != null)
                    {
                        _lastNormalX = Cache.Instance.DirectEve.ActiveShip.Entity.X;
                        _lastNormalY = Cache.Instance.DirectEve.ActiveShip.Entity.Y;
                        _lastNormalZ = Cache.Instance.DirectEve.ActiveShip.Entity.Z;
                    }

                    if (Cache.Instance.DirectEve.ActiveShip.GroupId == (int) Group.Capsule)
                    {
                        Logging.Log("Panic: You are in a Capsule, you must have died :(");
                        State = PanicState.StartPanicking;
                    }
                    else if (Cache.Instance.InSpace && Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage < Settings.Instance.MinimumCapacitorPct)
                    {
                        Logging.Log("Panic: Start panicking, capacitor [" + Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage + "%] below [" + Settings.Instance.MinimumCapacitorPct + "%]");
                        State = PanicState.StartPanicking;
                    }
                    else if (Cache.Instance.InSpace && Cache.Instance.DirectEve.ActiveShip.ShieldPercentage < Settings.Instance.MinimumShieldPct)
                    {
                        Logging.Log("Panic: Start panicking, shield [" + Cache.Instance.DirectEve.ActiveShip.ShieldPercentage + "%] below [" + Settings.Instance.MinimumShieldPct + "%]");
                        State = PanicState.StartPanicking;
                    }
                    else if (Cache.Instance.InSpace && Cache.Instance.DirectEve.ActiveShip.ArmorPercentage < Settings.Instance.MinimumArmorPct)
                    {
                        Logging.Log("Panic: Start panicking, armor [" + Cache.Instance.DirectEve.ActiveShip.ArmorPercentage + "%] below [" + Settings.Instance.MinimumArmorPct + "%]");
                        State = PanicState.StartPanicking;
                    }

                    Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsWarpScramblingMe), Priority.WarpScrambler);
                    if (Settings.Instance.SpeedTank)
                    {
                        Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsWebbingMe), Priority.Webbing);
                        Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsTargetPaintingMe), Priority.TargetPainting);
                    }
                    Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsNeutralizingMe), Priority.Neutralizing);
                    Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsJammingMe), Priority.Jamming);
                    Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsSensorDampeningMe), Priority.Dampening);
                    if (Cache.Instance.Modules.Any(m => m.IsTurret))
                        Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsTrackingDisruptingMe), Priority.TrackingDisrupting);
                    break;

                    // NOTE: The difference between Panicking and StartPanicking is that the bot will move to "Panic" state once in warp & Panicking 
                    //       and the bot wont go into Panic mode while still "StartPanicking"
                case PanicState.StartPanicking:
                case PanicState.Panicking:
                    if (Cache.Instance.InStation)
                    {
                        Logging.Log("Panic: Entered a station, lower panic mode");
                        State = PanicState.Panic;
                    }

                    // Once we have warped off 500km, assume we are "safer"
                    if (State == PanicState.StartPanicking && Cache.Instance.DistanceFromMe(_lastNormalX, _lastNormalY, _lastNormalZ) > 500000)
                    {
                        Logging.Log("Panic: We've warped off");
                        State = PanicState.Panicking;
                    }

                    // We leave the panicking state once we actually start warping off
                    var station = Cache.Instance.Stations.FirstOrDefault();
                    if (station != null)
                    {
                        if (Cache.Instance.InWarp)
                            break;

                        if (station.Distance > 150000)
                            station.WarpToAndDock();
                        else
                            station.Dock();

                        break;
                    }

                    // Whats this you say?  No star?
                    if (Cache.Instance.Star == null)
                        break;

                    if (Cache.Instance.Star.Distance > 500000000)
                    {
                        if (Cache.Instance.InWarp)
                            break;

                        Cache.Instance.Star.WarpTo();
                    }
                    else
                    {
                        Logging.Log("Panic: At the star, lower panic mode");
                        State = PanicState.Panic;
                    }
                    break;

                case PanicState.Panic:
                    // Do not resume until your no longer in a capsule
                    if (Cache.Instance.DirectEve.ActiveShip.GroupId == (int) Group.Capsule)
                        break;

                    if (Cache.Instance.InStation)
                    {
                        //var repair = Cache.Instance.DirectEve.ActiveShip.StructurePercentage < 100;
                        //repair |= Cache.Instance.DirectEve.ActiveShip.ArmorPercentage < 100;
                        //if (repair)
                        //{
                        //    State = PanicState.Repair;
                        //    break;
                        //}

                        Logging.Log("Panic: We're in a station, resume mission");
                        State = PanicState.Resume;
                    }

                    var isSafe = Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage > Settings.Instance.SafeCapacitorPct;
                    isSafe &= Cache.Instance.DirectEve.ActiveShip.ShieldPercentage > Settings.Instance.SafeShieldPct;
                    isSafe &= Cache.Instance.DirectEve.ActiveShip.ArmorPercentage > Settings.Instance.SafeArmorPct;
                    if (isSafe)
                    {
                        Logging.Log("Panic: We've recovered, resume mission");
                        State = PanicState.Resume;
                    }
                    break;

                    //case PanicState.Repair:
                    //    if (!Cache.Instance.Windows.Any(w => w.Type == "form.RepairShopWindow"))
                    //    {
                    //        if (_nextRepairAction < DateTime.Now)
                    //        {
                    //            _nextRepairAction = DateTime.Now.AddSeconds(15);
                    //        }
                    //    }
                    //    break;

                case PanicState.Resume:
                    // Dont do anything here
                    break;
            }
        }
    }
}