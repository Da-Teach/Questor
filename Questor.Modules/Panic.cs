﻿﻿// ------------------------------------------------------------------------------
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
        private Random _random = new Random();

        private double _lastNormalX;
        private double _lastNormalY;
        private double _lastNormalZ;

        private DateTime _resumeTime;
        private bool _delayedResume;
        private int _randomDelay;

        public PanicState State { get; set; }
        public bool InMission { get; set; }

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

                    if (Cache.Instance.DirectEve.ActiveShip.GroupId == (int)Group.Capsule)
                    {
                        Logging.Log("Panic: You are in a Capsule, you must have died :(");
                        State = PanicState.StartPanicking;
                    }
                    else if (InMission && Cache.Instance.InSpace && Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage < Settings.Instance.MinimumCapacitorPct)
                    {
                        // Only check for cap-panic while in a mission, not while doing anything else
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

                    _delayedResume = false;
                    if (InMission)
                    {
                        var frigates = Cache.Instance.Entities.Count(e => e.IsFrigate && e.IsPlayer);
                        var cruisers = Cache.Instance.Entities.Count(e => e.IsCruiser && e.IsPlayer);
                        var battlecruisers = Cache.Instance.Entities.Count(e => e.IsBattlecruiser && e.IsPlayer);
                        var battleships = Cache.Instance.Entities.Count(e => e.IsBattleship && e.IsPlayer);

                        if (Settings.Instance.FrigateInvasionLimit > 0 && frigates >= Settings.Instance.FrigateInvasionLimit)
                        {
                            _delayedResume = true;

                            State = PanicState.StartPanicking;
                            Logging.Log("Panic: Start panicking, mission invaded by [" + frigates + "] frigates");
                        }

                        if (Settings.Instance.CruiserInvasionLimit > 0 && cruisers >= Settings.Instance.CruiserInvasionLimit)
                        {
                            _delayedResume = true;

                            State = PanicState.StartPanicking;
                            Logging.Log("Panic: Start panicking, mission invaded by [" + cruisers + "] cruisers");
                        }

                        if (Settings.Instance.BattlecruiserInvasionLimit > 0 && battlecruisers >= Settings.Instance.BattlecruiserInvasionLimit)
                        {
                            _delayedResume = true;

                            State = PanicState.StartPanicking;
                            Logging.Log("Panic: Start panicking, mission invaded by [" + battlecruisers + "] battlecruisers");
                        }

                        if (Settings.Instance.BattleshipInvasionLimit > 0 && battleships >= Settings.Instance.BattleshipInvasionLimit)
                        {
                            _delayedResume = true;

                            State = PanicState.StartPanicking;
                            Logging.Log("Panic: Start panicking, mission invaded by [" + battleships + "] battleships");
                        }

                        if (_delayedResume)
                        {
                            _randomDelay = (Settings.Instance.InvasionRandomDelay > 0 ? _random.Next(Settings.Instance.InvasionRandomDelay) : 0);
                            _randomDelay += Settings.Instance.InvasionMinimumDelay;
                        }
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
                    // Do not resume until you're no longer in a capsule
                    if (Cache.Instance.DirectEve.ActiveShip.GroupId == (int)Group.Capsule)
                        break;

                    if (Cache.Instance.InStation)
                    {
                        Logging.Log("Panic: We're in a station, resume mission");
                        State = _delayedResume ? PanicState.DelayedResume : PanicState.Resume;
                    }

                    var isSafe = Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage > Settings.Instance.SafeCapacitorPct;
                    isSafe &= Cache.Instance.DirectEve.ActiveShip.ShieldPercentage > Settings.Instance.SafeShieldPct;
                    isSafe &= Cache.Instance.DirectEve.ActiveShip.ArmorPercentage > Settings.Instance.SafeArmorPct;
                    if (isSafe)
                    {
                        Logging.Log("Panic: We've recovered, resume mission");
                        State = _delayedResume ? PanicState.DelayedResume : PanicState.Resume;
                    }

                    if (State == PanicState.DelayedResume)
                    {
                        Logging.Log("Panic: Delaying resume for " + _randomDelay + " seconds");
                        _resumeTime = DateTime.Now.AddSeconds(_randomDelay);
                    }
                    break;

                case PanicState.DelayedResume:
                    if (DateTime.Now > _resumeTime)
                        State = PanicState.Resume;
                    break;

                case PanicState.Resume:
                    // Dont do anything here
                    break;
            }
        }
    }
}