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
    using System.Linq;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.States;

    public class Panic
    {
        private readonly Random _random = new Random();

        private double _lastNormalX;
        private double _lastNormalY;
        private double _lastNormalZ;

        private DateTime _resumeTime;
        private DateTime _nextWrapScrambledWarning = DateTime.MinValue;

        //private DateTime _lastDockedorJumping;
        private DateTime _lastWarpScrambled = DateTime.MinValue;
        private bool _delayedResume;
        private int _randomDelay;

        //public bool InMission { get; set; }

        public void ProcessState()
        {
            switch (_States.CurrentPanicState)
            {
                case PanicState.Idle:
                    //
                    // below is the reasons we will start the panic state(s) - if the below is not met do nothing
                    //
                    if (Cache.Instance.InSpace &&
                        Cache.Instance.DirectEve.ActiveShip.Entity != null &&
                        !Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked)
                    {
                        _States.CurrentPanicState = PanicState.Normal;
                        return;
                    }
                    break;

                case PanicState.Normal:
                    if (Cache.Instance.InStation)
                    {
                        _States.CurrentPanicState = PanicState.Idle;
                    }
                    if (Cache.Instance.DirectEve.ActiveShip.Entity != null)
                    {
                        _lastNormalX = Cache.Instance.DirectEve.ActiveShip.Entity.X;
                        _lastNormalY = Cache.Instance.DirectEve.ActiveShip.Entity.Y;
                        _lastNormalZ = Cache.Instance.DirectEve.ActiveShip.Entity.Z;
                    }
                    if (Cache.Instance.DirectEve.ActiveShip.Entity == null)
                        return;

                        if ((long)Cache.Instance.DirectEve.ActiveShip.StructurePercentage == 0) //if your hull is 0 you are dead or bugged, wait. 
                            return;

                    if (Cache.Instance.DirectEve.ActiveShip.GroupId == (int)Group.Capsule)
                    {
                        Logging.Log("Panic", "You are in a Capsule, you must have died :(", Logging.red);
                        _States.CurrentPanicState = PanicState.StartPanicking;
                    }
                    else if (Cache.Instance.InMission && Cache.Instance.InSpace && Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage < Settings.Instance.MinimumCapacitorPct && Cache.Instance.DirectEve.ActiveShip.GroupId != 31)
                    {
                        // Only check for cap-panic while in a mission, not while doing anything else
                        Logging.Log("Panic", "Start panicking, capacitor [" + Math.Round(Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage, 0) + "%] below [" + Settings.Instance.MinimumCapacitorPct + "%]", Logging.red);
                        //Questor.panic_attempts_this_mission;
                        Cache.Instance.PanicAttemptsThisMission = (Cache.Instance.PanicAttemptsThisMission + 1);
                        Cache.Instance.PanicAttemptsThisPocket = (Cache.Instance.PanicAttemptsThisPocket + 1);
                        _States.CurrentPanicState = PanicState.StartPanicking;
                    }
                    else if (Cache.Instance.InSpace && Cache.Instance.DirectEve.ActiveShip.ShieldPercentage < Settings.Instance.MinimumShieldPct)
                    {
                        Logging.Log("Panic", "Start panicking, shield [" + Math.Round(Cache.Instance.DirectEve.ActiveShip.ShieldPercentage, 0) + "%] below [" + Settings.Instance.MinimumShieldPct + "%]", Logging.red);
                        Cache.Instance.PanicAttemptsThisMission = (Cache.Instance.PanicAttemptsThisMission + 1);
                        Cache.Instance.PanicAttemptsThisPocket = (Cache.Instance.PanicAttemptsThisPocket + 1);
                        _States.CurrentPanicState = PanicState.StartPanicking;
                    }
                    else if (Cache.Instance.InSpace && Cache.Instance.DirectEve.ActiveShip.ArmorPercentage < Settings.Instance.MinimumArmorPct)
                    {
                        Logging.Log("Panic", "Start panicking, armor [" + Math.Round(Cache.Instance.DirectEve.ActiveShip.ArmorPercentage, 0) + "%] below [" + Settings.Instance.MinimumArmorPct + "%]", Logging.red);
                        Cache.Instance.PanicAttemptsThisMission = (Cache.Instance.PanicAttemptsThisMission + 1);
                        Cache.Instance.PanicAttemptsThisPocket = (Cache.Instance.PanicAttemptsThisPocket + 1);
                        _States.CurrentPanicState = PanicState.StartPanicking;
                    }

                    _delayedResume = false;
                    if (Cache.Instance.InMission)
                    {
                        int frigates = Cache.Instance.Entities.Count(e => e.IsFrigate && e.IsPlayer);
                        int cruisers = Cache.Instance.Entities.Count(e => e.IsCruiser && e.IsPlayer);
                        int battlecruisers = Cache.Instance.Entities.Count(e => e.IsBattlecruiser && e.IsPlayer);
                        int battleships = Cache.Instance.Entities.Count(e => e.IsBattleship && e.IsPlayer);

                        if (Settings.Instance.FrigateInvasionLimit > 0 && frigates >= Settings.Instance.FrigateInvasionLimit)
                        {
                            _delayedResume = true;

                            Cache.Instance.PanicAttemptsThisMission = (Cache.Instance.PanicAttemptsThisMission + 1);
                            Cache.Instance.PanicAttemptsThisPocket = (Cache.Instance.PanicAttemptsThisPocket + 1);
                            _States.CurrentPanicState = PanicState.StartPanicking;
                            Logging.Log("Panic", "Start panicking, mission invaded by [" + frigates + "] frigates", Logging.red);
                        }

                        if (Settings.Instance.CruiserInvasionLimit > 0 && cruisers >= Settings.Instance.CruiserInvasionLimit)
                        {
                            _delayedResume = true;

                            Cache.Instance.PanicAttemptsThisMission = (Cache.Instance.PanicAttemptsThisMission + 1);
                            Cache.Instance.PanicAttemptsThisPocket = (Cache.Instance.PanicAttemptsThisPocket + 1);
                            _States.CurrentPanicState = PanicState.StartPanicking;
                            Logging.Log("Panic", "Start panicking, mission invaded by [" + cruisers + "] cruisers", Logging.red);
                        }

                        if (Settings.Instance.BattlecruiserInvasionLimit > 0 && battlecruisers >= Settings.Instance.BattlecruiserInvasionLimit)
                        {
                            _delayedResume = true;

                            Cache.Instance.PanicAttemptsThisMission = (Cache.Instance.PanicAttemptsThisMission + 1);
                            Cache.Instance.PanicAttemptsThisPocket = (Cache.Instance.PanicAttemptsThisPocket + 1);
                            _States.CurrentPanicState = PanicState.StartPanicking;
                            Logging.Log("Panic", "Start panicking, mission invaded by [" + battlecruisers + "] battlecruisers", Logging.red);
                        }

                        if (Settings.Instance.BattleshipInvasionLimit > 0 && battleships >= Settings.Instance.BattleshipInvasionLimit)
                        {
                            _delayedResume = true;

                            Cache.Instance.PanicAttemptsThisMission = (Cache.Instance.PanicAttemptsThisMission + 1);
                            Cache.Instance.PanicAttemptsThisPocket = (Cache.Instance.PanicAttemptsThisPocket + 1);
                            _States.CurrentPanicState = PanicState.StartPanicking;
                            Logging.Log("Panic", "Start panicking, mission invaded by [" + battleships + "] battleships", Logging.red);
                        }

                        if (_delayedResume)
                        {
                            _randomDelay = (Settings.Instance.InvasionRandomDelay > 0 ? _random.Next(Settings.Instance.InvasionRandomDelay) : 0);
                            _randomDelay += Settings.Instance.InvasionMinimumDelay;
                        }
                    }

                    if (Cache.Instance.InSpace)
                    {
                        Cache.Instance.AddPriorityTargets(
                            Cache.Instance.TargetedBy.Where(t => t.IsWarpScramblingMe), Priority.WarpScrambler);
                        if (Settings.Instance.SpeedTank)
                        {
                            Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsWebbingMe), Priority.Webbing);
                            Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsTargetPaintingMe), Priority.TargetPainting);
                        }
                        Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsNeutralizingMe), Priority.Neutralizing);
                        Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsJammingMe), Priority.Jamming);
                        Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsSensorDampeningMe), Priority.Dampening);
                        if (Cache.Instance.Modules.Any(m => m.IsTurret))
                            Cache.Instance.AddPriorityTargets(
                                Cache.Instance.TargetedBy.Where(t => t.IsTrackingDisruptingMe),
                                Priority.TrackingDisrupting);
                    }
                    break;

                // NOTE: The difference between Panicking and StartPanicking is that the bot will move to "Panic" state once in warp & Panicking
                //       and the bot wont go into Panic mode while still "StartPanicking"
                case PanicState.StartPanicking:
                case PanicState.Panicking:
                    // Add any warp scramblers to the priority list
                    Cache.Instance.AddPriorityTargets(Cache.Instance.TargetedBy.Where(t => t.IsWarpScramblingMe), Priority.WarpScrambler);

                    // Failsafe, in theory would/should never happen
                    if (_States.CurrentPanicState == PanicState.Panicking && Cache.Instance.TargetedBy.Any(t => t.IsWarpScramblingMe))
                    {
                        // Resume is the only state that will make Questor revert to combat mode
                        _States.CurrentPanicState = PanicState.Resume;
                        return;
                    }

                    if (Cache.Instance.InStation)
                    {
                        Logging.Log("Panic", "Entered a station, lower panic mode", Logging.white);
                        _States.CurrentPanicState = PanicState.Panic;
                    }

                    // Once we have warped off 500km, assume we are "safer"
                    if (_States.CurrentPanicState == PanicState.StartPanicking && Cache.Instance.DistanceFromMe(_lastNormalX, _lastNormalY, _lastNormalZ) > (int)Distance.PanicDistanceToConsiderSafelyWarpedOff)
                    {
                        Logging.Log("Panic", "We've warped off", Logging.white);
                        _States.CurrentPanicState = PanicState.Panicking;
                    }

                    // We leave the panicking state once we actually start warping off
                    EntityCache station = Cache.Instance.Stations.FirstOrDefault();
                    if (station != null && Cache.Instance.InSpace)
                    {
                        if (Cache.Instance.InWarp)
                            break;

                        if (station.Distance > (int)Distance.WarptoDistance)
                        {
                            if (Cache.Instance.PriorityTargets.Any(pt => pt.IsWarpScramblingMe))
                            {
                                EntityCache warpscrambledby = Cache.Instance.PriorityTargets.FirstOrDefault(pt => pt.IsWarpScramblingMe);
                                if (warpscrambledby != null && _nextWrapScrambledWarning > DateTime.Now)
                                {
                                    _nextWrapScrambledWarning = DateTime.Now.AddSeconds(20);
                                    Logging.Log("Panic", "We are scrambled by: [" + Logging.white + warpscrambledby.Name + Logging.orange + "][" + Logging.white + Math.Round(warpscrambledby.Distance, 0) + Logging.orange + "][" + Logging.white + warpscrambledby.Id + Logging.orange + "]",
                                                Logging.orange);
                                    _lastWarpScrambled = DateTime.Now;
                                }
                            }
                            else
                            {
                                if (DateTime.Now > Cache.Instance.NextWarpTo || DateTime.Now.Subtract(_lastWarpScrambled).TotalSeconds < (int)Time.WarpScrambledNoDelay_seconds) //this will effectively spam warpto as soon as you are free of warp disruption if you were warp disrupted in the past 10 seconds)
                                {
                                    Logging.Log("Panic", "Warping to [" + station.Name + "][" + Math.Round((station.Distance / 1000) / 149598000, 2) + " AU away]", Logging.red);
                                    Cache.Instance.IsMissionPocketDone = true;
                                    station.WarpToAndDock();
                                    Cache.Instance.NextWarpTo = DateTime.Now.AddSeconds((int)Time.WarptoDelay_seconds);
                                    //_nextDock = DateTime.MinValue;
                                }
                                else
                                {
                                    Logging.Log("Panic", "Warping will be attempted again after [" + Math.Round(Cache.Instance.NextWarpTo.Subtract(DateTime.Now).TotalSeconds,0) + "sec]", Logging.red);
                                }
                            }
                        }
                        else
                        {
                            if (station.Distance < (int)Distance.DockingRange)
                            {
                                if (DateTime.Now > Cache.Instance.NextUndockAction)
                                {
                                    Logging.Log("Panic", "Docking with [" + station.Name + "][" + Math.Round((station.Distance / 1000) / 149598000, 2) + " AU away]", Logging.red);
                                    station.Dock();
                                    Cache.Instance.NextUndockAction = DateTime.Now.AddSeconds((int)Time.DockingDelay_seconds);
                                }
                                else
                                {
                                    Logging.Log("Panic", "Docking will be attempted in [" + Math.Round(Cache.Instance.NextWarpTo.Subtract(DateTime.Now).TotalSeconds, 0) + "sec]", Logging.red);
                                }
                            }
                            else
                            {
                                if (DateTime.Now > Cache.Instance.NextTravelerAction)
                                {
                                    if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != station.Id)
                                    {
                                        Logging.Log("Panic", "Approaching to [" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.red);
                                        station.Approach();
                                        Cache.Instance.NextTravelerAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                                    }
                                    else
                                    {
                                        Logging.Log("Panic", "Already Approaching to: [" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.red);
                                    }
                                }
                                else
                                {
                                    Logging.Log("Panic", "Approaching has been delayed for [" + Math.Round(Cache.Instance.NextWarpTo.Subtract(DateTime.Now).TotalSeconds, 0) + "sec]", Logging.red);
                                }
                            }
                        }
                        break;
                    }
                    else if (Cache.Instance.InSpace)
                    {
                        if (DateTime.Now.Subtract(Cache.Instance.LastLoggingAction).TotalSeconds > 15)
                        {
                            Logging.Log("Panic", "No station found in local?", Logging.red);
                        }
                    }

                    // What is this you say?  No star?
                    if (Cache.Instance.Star == null)
                        break;

                    if (Cache.Instance.Star.Distance > (int)Distance.WeCanWarpToStarFromHere)
                    {
                        if (Cache.Instance.InWarp)
                            break;

                        if (Cache.Instance.TargetedBy.Any(t => t.IsWarpScramblingMe))
                        {
                            Logging.Log("Panic", "We are still warp scrambled!", Logging.red); //This runs every 'tick' so we should see it every 1.5 seconds or so
                            _lastWarpScrambled = DateTime.Now;
                        }
                        else if (DateTime.Now > Cache.Instance.NextWarpTo | DateTime.Now.Subtract(_lastWarpScrambled).TotalSeconds < 10) //this will effectively spam warpto as soon as you are free of warp disruption if you were warp disrupted in the past 10 seconds
                        {
                            Logging.Log("Panic", "Warping to [" + Cache.Instance.Star.Name + "][" + Math.Round((Cache.Instance.Star.Distance / 1000) / 149598000, 2) + " AU away]", Logging.red);
                            Cache.Instance.IsMissionPocketDone = true;
                            Cache.Instance.Star.WarpTo();
                            Cache.Instance.NextWarpTo = DateTime.Now.AddSeconds((int)Time.WarptoDelay_seconds);
                        }
                        else
                        {
                            Logging.Log("Panic", "Warping has been delayed for [" + Math.Round(Cache.Instance.NextWarpTo.Subtract(DateTime.Now).TotalSeconds, 0) + "sec]", Logging.red);
                        }
                    }
                    else
                    {
                        Logging.Log("Panic", "At the star, lower panic mode", Logging.red);
                        _States.CurrentPanicState = PanicState.Panic;
                    }
                    break;

                case PanicState.Panic:
                    // Do not resume until you're no longer in a capsule
                    if (Cache.Instance.DirectEve.ActiveShip.GroupId == (int)Group.Capsule)
                        break;

                    if (Cache.Instance.InStation)
                    {
                        Logging.Log("Panic", "We're in a station, resume mission", Logging.red);
                        _States.CurrentPanicState = _delayedResume ? PanicState.DelayedResume : PanicState.Resume;
                    }

                    bool isSafe = Cache.Instance.DirectEve.ActiveShip.CapacitorPercentage > Settings.Instance.SafeCapacitorPct;
                    isSafe &= Cache.Instance.DirectEve.ActiveShip.ShieldPercentage > Settings.Instance.SafeShieldPct;
                    isSafe &= Cache.Instance.DirectEve.ActiveShip.ArmorPercentage > Settings.Instance.SafeArmorPct;
                    if (isSafe)
                    {
                        Logging.Log("Panic", "We've recovered, resume mission", Logging.red);
                        Cache.Instance.IsMissionPocketDone = false;
                        _States.CurrentPanicState = _delayedResume ? PanicState.DelayedResume : PanicState.Resume;
                    }

                    if (_States.CurrentPanicState == PanicState.DelayedResume)
                    {
                        Logging.Log("Panic", "Delaying resume for " + _randomDelay + " seconds", Logging.red);
                        Cache.Instance.IsMissionPocketDone = false;
                        _resumeTime = DateTime.Now.AddSeconds(_randomDelay);
                    }
                    break;

                case PanicState.DelayedResume:
                    if (DateTime.Now > _resumeTime)
                        _States.CurrentPanicState = PanicState.Resume;
                    break;

                case PanicState.Resume:
                    // Don't do anything here
                    break;
            }
        }
    }
}