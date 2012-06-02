// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DirectEve;
using Questor.Modules.Caching;
using Questor.Modules.Logging;
using Questor.Modules.Lookup;
using Questor.Modules.Activities;
using Questor.Modules.States;
using Questor.Modules.Combat;
using Questor.Modules.Actions;
using Questor.Modules.BackgroundTasks;

namespace Questor.Behaviors
{
    public class DirectionalScannerBehavior
    {
        private readonly Arm _arm;
        private readonly Combat _combat;
        //private readonly Defense _defense;
        private readonly Drones _drones;

        private DateTime _lastPulse;
        private readonly Panic _panic;
        private readonly Salvage _salvage;
        private readonly Traveler _traveler;
        private readonly UnloadLoot _unloadLoot;
        public DateTime LastAction;
        private readonly Random _random;
        //private int _randomDelay;
        public static long AgentID;
        private readonly Stopwatch _watch;

        public bool Panicstatereset; //false;
        private bool ValidSettings { get; set; }
        public bool CloseQuestorflag = true;
        public string CharacterName { get; set; }
        
        //DateTime _nextAction = DateTime.Now;

        public DirectionalScannerBehavior()
        {
            _lastPulse = DateTime.MinValue;
            _random = new Random();
            _salvage = new Salvage();
            _combat = new Combat();
            _drones = new Drones();
            _traveler = new Traveler();
            _unloadLoot = new UnloadLoot();
            _arm = new Arm();
            _panic = new Panic();
            _watch = new Stopwatch();

            //
            // this is combat mission specific and needs to be generalized
            //
            Settings.Instance.SettingsLoaded += SettingsLoaded;
            //Settings.Instance.UseFittingManager = false;

            // States.CurrentDirectionalScannerBehaviorState fixed on ExecuteMission
            _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Idle;
            _States.CurrentArmState = ArmState.Idle;
            _States.CurrentCombatState = CombatState.Idle;
            _States.CurrentDroneState = DroneState.Idle;
            _States.CurrentUnloadLootState = UnloadLootState.Idle;
            _States.CurrentTravelerState = TravelerState.Idle;
        }

        public void SettingsLoaded(object sender, EventArgs e)
        {
            ApplyDirectionalScannerSettings();
            ValidateCombatMissionSettings();
        }

        public void DebugDirectionalScannerBehaviorStates()
        {
            if (Settings.Instance.DebugStates)
                Logging.Log("DirectionalScannerBehavior.State is", _States.CurrentDirectionalScannerBehaviorState.ToString(),Logging.white);
        }

        public void DebugPanicstates()
        {
            if (Settings.Instance.DebugStates)
                Logging.Log("Panic.State is ",_States.CurrentPanicState.ToString(),Logging.white);
        }

        public void DebugPerformanceClearandStartTimer()
        {
            _watch.Reset();
            _watch.Start();
        }

        public void DebugPerformanceStopandDisplayTimer(string whatWeAreTiming)
        {
            _watch.Stop();
            if (Settings.Instance.DebugPerformance)
                Logging.Log(whatWeAreTiming ," took " + _watch.ElapsedMilliseconds + "ms",Logging.white);
        }

        public void ValidateCombatMissionSettings()
        {
            ValidSettings = true;
            if (Settings.Instance.Ammo.Select(a => a.DamageType).Distinct().Count() != 4)
            {
                if (!Settings.Instance.Ammo.Any(a => a.DamageType == DamageType.EM))
                    Logging.Log("Settings", ": Missing EM damage type!", Logging.orange);
                if (!Settings.Instance.Ammo.Any(a => a.DamageType == DamageType.Thermal))
                    Logging.Log("Settings", "Missing Thermal damage type!", Logging.orange);
                if (!Settings.Instance.Ammo.Any(a => a.DamageType == DamageType.Kinetic))
                    Logging.Log("Settings", "Missing Kinetic damage type!", Logging.orange);
                if (!Settings.Instance.Ammo.Any(a => a.DamageType == DamageType.Explosive))
                    Logging.Log("Settings", "Missing Explosive damage type!", Logging.orange);

                Logging.Log("Settings", "You are required to specify all 4 damage types in your settings xml file!", Logging.white);
                ValidSettings = false;
            }

            DirectAgent agent = Cache.Instance.DirectEve.GetAgentByName(Cache.Instance.CurrentAgent);

            if (agent == null || !agent.IsValid)
            {
                Logging.Log("Settings", "Unable to locate agent [" + Cache.Instance.CurrentAgent + "]", Logging.white);
                ValidSettings = false;
            }
            else
            {
                _arm.AgentId = agent.AgentId;
                AgentID = agent.AgentId;
            }
        }
        
        public void ApplyDirectionalScannerSettings()
        {
            _salvage.Ammo = Settings.Instance.Ammo;
            _salvage.MaximumWreckTargets = Settings.Instance.MaximumWreckTargets;
            _salvage.ReserveCargoCapacity = Settings.Instance.ReserveCargoCapacity;
            _salvage.LootEverything = Settings.Instance.LootEverything;
        }

        private void BeginClosingQuestor()
        {
            Cache.Instance.EnteredCloseQuestor_DateTime = DateTime.Now;
            _States.CurrentQuestorState = QuestorState.CloseQuestor;
        }

        private void TravelToAgentsStation()
        {
            try
            {
           var baseDestination = _traveler.Destination as StationDestination;
           if (baseDestination == null || baseDestination.StationId != Cache.Instance.Agent.StationId)
                    _traveler.Destination = new StationDestination(Cache.Instance.Agent.SolarSystemId,
                                                                   Cache.Instance.Agent.StationId,
                                                                   Cache.Instance.DirectEve.GetLocationName(
                                                                       Cache.Instance.Agent.StationId));
            }
            catch (Exception ex)
            {
                Logging.Log("DirectionalScanner","TravelToAgentsStation: Exception caught: [" + ex.Message + "]",Logging.red);
                return;
            }
            if (Cache.Instance.InSpace)
            {
                if (!Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked || (Cache.Instance.LastSessionChange.AddSeconds(60) > DateTime.Now))
                {
                   _combat.ProcessState();
                   _drones.ProcessState(); //do we really want to use drones here? 
                }
            }
            if (Cache.Instance.InSpace && !Cache.Instance.TargetedBy.Any(t => t.IsWarpScramblingMe))
            {
                Cache.Instance.IsMissionPocketDone = true; //tells drones.cs that we can pull drones
                //Logging.Log("CombatmissionBehavior","TravelToAgentStation: not pointed",Logging.white);
            }
            _traveler.ProcessState();
            if (Settings.Instance.DebugStates)
            {
               Logging.Log("Traveler.State", "is " + _States.CurrentTravelerState,Logging.white);
            }
        }

        private void AvoidBumpingThings()
        {
            //if It hasn't been at least 60 seconds since we last session changed do not do anything
         if (Cache.Instance.InStation || !Cache.Instance.InSpace || Cache.Instance.DirectEve.ActiveShip.Entity.IsCloaked || (Cache.Instance.InSpace && Cache.Instance.LastSessionChange.AddSeconds(60) < DateTime.Now))
                return;
            //
            // if we are "too close" to the bigObject move away... (is orbit the best thing to do here?)
            //
         if (Cache.Instance.ClosestStargate.Distance > 9000 || Cache.Instance.ClosestStation.Distance > 5000)
         {
            EntityCache thisBigObject = Cache.Instance.BigObjects.FirstOrDefault();
            if (thisBigObject != null)
            {
                if (thisBigObject.Distance >= (int) Distance.TooCloseToStructure)
                {
                    //we are no longer "too close" and can proceed. 
                }
                else
                {
                    if (DateTime.Now > Cache.Instance.NextOrbit)
                    {
                        thisBigObject.Orbit((int) Distance.SafeDistancefromStructure);
                        Logging.Log("DirectionalScannerBehavior", _States.CurrentDirectionalScannerBehaviorState +
                                    ": initiating Orbit of [" + thisBigObject.Name +
                                    "] orbiting at [" + Distance.SafeDistancefromStructure + "]", Logging.white);
                        Cache.Instance.NextOrbit = DateTime.Now.AddSeconds((int) Time.OrbitDelay_seconds);
                    }
                    return;
                    //we are still too close, do not continue through the rest until we are not "too close" anymore
                }
            }
        }
      }

        public void ProcessState()
        {
            // Invalid settings, quit while we're ahead
            if (!ValidSettings)
            {
                if (DateTime.Now.Subtract(LastAction).TotalSeconds < (int)Time.ValidateSettings_seconds) //default is a 15 second interval
                {
                    ValidateCombatMissionSettings();
                    LastAction = DateTime.Now;
                }
                return;
            }

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //this local is safe check is useless as their is no localwatch processstate running every tick... 
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //If local unsafe go to base and do not start mission again
            if (Settings.Instance.FinishWhenNotSafe && (_States.CurrentDirectionalScannerBehaviorState != DirectionalScannerBehaviorState.GotoNearestStation /*|| State!=QuestorState.GotoBase*/))
            {
                //need to remove spam
                if (Cache.Instance.InSpace && !Cache.Instance.LocalSafe(Settings.Instance.LocalBadStandingPilotsToTolerate, Settings.Instance.LocalBadStandingLevelToConsiderBad))
                {
                    var station = Cache.Instance.Stations.OrderBy(x => x.Distance).FirstOrDefault();
                    if (station != null)
                    {
                        Logging.Log("Local not safe", "Station found. Going to nearest station", Logging.white);
                        if (_States.CurrentDirectionalScannerBehaviorState != DirectionalScannerBehaviorState.GotoNearestStation)
                            _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.GotoNearestStation;
                    }
                    else
                    {
                        Logging.Log("Local not safe", "Station not found. Going back to base", Logging.white);
                        if (_States.CurrentDirectionalScannerBehaviorState != DirectionalScannerBehaviorState.GotoBase)
                            _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.GotoBase;
                    }
                    Cache.Instance.StopBot = true;
                }
            }
            
            if (Cache.Instance.SessionState == "Quitting")
            {
                BeginClosingQuestor();
            }

            if (Cache.Instance.GotoBaseNow)
            {
                if (_States.CurrentDirectionalScannerBehaviorState != DirectionalScannerBehaviorState.GotoBase)
                {
                    _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.GotoBase;
                }
            }

            if ((DateTime.Now.Subtract(Cache.Instance.QuestorStarted_DateTime).TotalSeconds > 10) && (DateTime.Now.Subtract(Cache.Instance.QuestorStarted_DateTime).TotalSeconds < 60))
            {
                if (Cache.Instance.QuestorJustStarted)
                {
                    Cache.Instance.QuestorJustStarted = false;
                    Cache.Instance.SessionState = "Starting Up";

                    // write session log
                    Statistics.WriteSessionLogStarting();
                }
            }
            
            //
            // Panic always runs, not just in space
            //
            DebugPerformanceClearandStartTimer();
            _panic.ProcessState();
            DebugPerformanceStopandDisplayTimer("Panic.ProcessState");
            if (_States.CurrentPanicState == PanicState.Panic || _States.CurrentPanicState == PanicState.Panicking)
            {
                // If Panic is in panic state, questor is in panic States.CurrentDirectionalScannerBehaviorState :)
                _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Panic;

                DebugDirectionalScannerBehaviorStates();
                if (Panicstatereset)
                {
                    _States.CurrentPanicState = PanicState.Normal;
                    Panicstatereset = false;
                }
            }
            else if (_States.CurrentPanicState == PanicState.Resume)
            {
                // Reset panic state
                _States.CurrentPanicState = PanicState.Normal;

                // Sit Idle and wait for orders. 
                _States.CurrentTravelerState = TravelerState.Idle;
                _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Idle;
                
                DebugDirectionalScannerBehaviorStates();
            }
            DebugPanicstates();

            //Logging.Log("test");
            switch (_States.CurrentDirectionalScannerBehaviorState)
            {
                case DirectionalScannerBehaviorState.Idle:

                    if (Cache.Instance.StopBot)
                    {
                        if (Settings.Instance.DebugIdle) Logging.Log("DirectionalScannerBehavior", "if (Cache.Instance.StopBot)", Logging.white);
                        return;
                    }

                    if (Settings.Instance.DebugIdle) Logging.Log("DirectionalScannerBehavior", "if (Cache.Instance.InSpace) else", Logging.white);    
                    _States.CurrentArmState = ArmState.Idle;
                    _States.CurrentDroneState = DroneState.Idle;
                    _States.CurrentSalvageState = SalvageState.Idle;
                    _States.CurrentTravelerState = TravelerState.Idle; 
                    _States.CurrentUnloadLootState = UnloadLootState.Idle;
                    _States.CurrentTravelerState = TravelerState.Idle;
                    
                    Logging.Log("DirectionalScannerBehavior", "Started questor in Directional Scanner (test) mode", Logging.white);
                    LastAction = DateTime.Now;
                    break;

                case DirectionalScannerBehaviorState.DelayedGotoBase:
                    if (DateTime.Now.Subtract(LastAction).TotalSeconds < (int)Time.DelayedGotoBase_seconds)
                        break;

                    Logging.Log("DirectionalScannerBehavior", "Heading back to base", Logging.white);
                    if (_States.CurrentDirectionalScannerBehaviorState == DirectionalScannerBehaviorState.DelayedGotoBase) _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.GotoBase;                    
                    break;

                
                case DirectionalScannerBehaviorState.GotoBase:
                    if (Settings.Instance.DebugGotobase) Logging.Log("DirectionalScannerBehavior", "GotoBase: AvoidBumpingThings()",Logging.white);

                    AvoidBumpingThings();

                    if (Settings.Instance.DebugGotobase) Logging.Log("DirectionalScannerBehavior", "GotoBase: TravelToAgentsStation()", Logging.white);
                    
                    TravelToAgentsStation();

                    if (_States.CurrentTravelerState == TravelerState.AtDestination) // || DateTime.Now.Subtract(Cache.Instance.EnteredCloseQuestor_DateTime).TotalMinutes > 10)
                    {
                        if (Settings.Instance.DebugGotobase) Logging.Log("DirectionalScannerBehavior", "GotoBase: We are at destination", Logging.white);
                        Cache.Instance.GotoBaseNow = false; //we are there - turn off the 'forced' gotobase
                        Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentID);

                        if (_States.CurrentDirectionalScannerBehaviorState == DirectionalScannerBehaviorState.GotoBase) _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Idle;
                        
                        _traveler.Destination = null;
                    }
                    break;

                case DirectionalScannerBehaviorState.Traveler:
                    Cache.Instance.OpenWrecks = false;
                    List<long> destination = Cache.Instance.DirectEve.Navigation.GetDestinationPath();
                    if (destination == null || destination.Count == 0)
                    {
                        // happens if autopilot isn't set and this questorstate is chosen manually
                        // this also happens when we get to destination (!?)
                        Logging.Log("DirectionalScannerBehavior.Traveler", "No destination?", Logging.white);
                        if (_States.CurrentDirectionalScannerBehaviorState == DirectionalScannerBehaviorState.Traveler) _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Error;
                        return;
                    }
                    else
                        if (destination.Count == 1 && destination.First() == 0)
                            destination[0] = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
                    if (_traveler.Destination == null || _traveler.Destination.SolarSystemId != destination.Last())
                    {
                        IEnumerable<DirectBookmark> bookmarks = Cache.Instance.DirectEve.Bookmarks.Where(b => b.LocationId == destination.Last()).ToList();
                        if (bookmarks != null && bookmarks.Any())
                            _traveler.Destination = new BookmarkDestination(bookmarks.OrderBy(b => b.CreatedOn).First());
                        else
                        {
                            Logging.Log("DirectionalScannerBehavior.Traveler", "Destination: [" + Cache.Instance.DirectEve.Navigation.GetLocation(destination.Last()).Name + "]", Logging.white);
                            _traveler.Destination = new SolarSystemDestination(destination.Last());
                        }
                    }
                    else
                    {
                        _traveler.ProcessState();
                        //we also assume you are connected during a manual set of questor into travel mode (safe assumption considering someone is at the kb)
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        if (_States.CurrentTravelerState == TravelerState.AtDestination)
                        {
                            if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Error)
                            {
                                Logging.Log("DirectionalScannerBehavior.Traveler", "an error has occurred", Logging.white);
                                if (_States.CurrentDirectionalScannerBehaviorState == DirectionalScannerBehaviorState.Traveler)
                                {
                                    _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Error;
                                }
                                return;
                            }
                            else if (Cache.Instance.InSpace)
                            {
                                Logging.Log("DirectionalScannerBehavior.Traveler", "Arrived at destination (in space, Questor stopped)", Logging.white);
                                if (_States.CurrentDirectionalScannerBehaviorState == DirectionalScannerBehaviorState.Traveler) _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Error;
                                return;
                            }
                            else
                            {
                                Logging.Log("DirectionalScannerBehavior.Traveler", "Arrived at destination", Logging.white);
                                if (_States.CurrentDirectionalScannerBehaviorState == DirectionalScannerBehaviorState.Traveler) _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Idle;
                                return;
                            }
                        }
                    }
                    break;

                case DirectionalScannerBehaviorState.GotoNearestStation:
                    if (!Cache.Instance.InSpace || Cache.Instance.InWarp) return;
                    var station = Cache.Instance.Stations.OrderBy(x => x.Distance).FirstOrDefault();
                    if (station != null)
                    {
                        if (station.Distance > (int)Distance.WarptoDistance)
                        {
                            Logging.Log("DirectionalScannerBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.white);
                            station.WarpToAndDock();
                            Cache.Instance.NextWarpTo = DateTime.Now.AddSeconds((int)Time.WarptoDelay_seconds);
                            if (_States.CurrentDirectionalScannerBehaviorState == DirectionalScannerBehaviorState.GotoNearestStation) _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Idle;
                            break;
                        }
                        else
                        {
                            if (station.Distance < 1900)
                            {
                                if (DateTime.Now > Cache.Instance.NextDockAction)
                                {
                                    Logging.Log("DirectionalScannerBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.white);
                                    station.Dock();
                                    Cache.Instance.NextDockAction = DateTime.Now.AddSeconds((int)Time.DockingDelay_seconds);
                                }
                            }
                            else
                            {
                                if (Cache.Instance.NextApproachAction < DateTime.Now && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != station.Id))
                                {
                                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                                    Logging.Log("DirectionalScannerBehavior.GotoNearestStation", "Approaching [" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.white);
                                    station.Approach();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_States.CurrentDirectionalScannerBehaviorState == DirectionalScannerBehaviorState.GotoNearestStation) _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Error; //should we goto idle here?
                    }
                    break;

                case DirectionalScannerBehaviorState.PVPDirectionalScanHalfanAU:
                    Logging.Log("DirectionalScannerBehavior","PVPDirectionalScanhalfanAU - Starting",Logging.white);
                    List<EntityCache> pvpDirectionalScanHalfanAUentitiesInList = Cache.Instance.Entities.Where(t => t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pvpDirectionalScanHalfanAUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVPDirectionalScan1AU:
                    Logging.Log("DirectionalScannerBehavior","PVPDirectionalScan1AU - Starting",Logging.white);
                    List<EntityCache> pvpDirectionalScan1AUentitiesInList = Cache.Instance.Entities.Where(t => t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 1).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pvpDirectionalScan1AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVPDirectionalScan5AU:
                    Logging.Log("DirectionalScannerBehavior","PVPDirectionalScan5AU - Starting",Logging.white);
                    List<EntityCache> pvpDirectionalScan5AUentitiesInList = Cache.Instance.Entities.Where(t => t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 5).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pvpDirectionalScan5AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVPDirectionalScan10AU:
                    Logging.Log("DirectionalScannerBehavior","PVPDirectionalScan10AU - Starting",Logging.white);
                    List<EntityCache> pvpDirectionalScan10AUentitiesInList = Cache.Instance.Entities.Where(t => t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 10).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pvpDirectionalScan10AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVPDirectionalScan15AU:
                    Logging.Log("DirectionalScannerBehavior","PVPDirectionalScan15AU - Starting",Logging.white);
                    List<EntityCache> pvpDirectionalScan15AUentitiesInList = Cache.Instance.Entities.Where(t => t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 15).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pvpDirectionalScan15AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVPDirectionalScan20AU:
                    Logging.Log("DirectionalScannerBehavior","PVPDirectionalScan20AU - Starting",Logging.white);
                    List<EntityCache> pvpDirectionalScan20AUentitiesInList = Cache.Instance.Entities.Where(t => t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 20).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pvpDirectionalScan20AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVPDirectionalScan50AU:
                    Logging.Log("DirectionalScannerBehavior","PVPDirectionalScan50AU - Starting",Logging.white);
                    List<EntityCache> pvpDirectionalScan50AUentitiesInList = Cache.Instance.Entities.Where(t => t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 50).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pvpDirectionalScan50AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVEDirectionalScanHalfanAU:
                    Logging.Log("DirectionalScannerBehavior","PVEDirectionalScanhalfanAU - Starting",Logging.white);
                    List<EntityCache> pveDirectionalScanHalfanAUentitiesInList = Cache.Instance.Entities.Where(t => !t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pveDirectionalScanHalfanAUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVEDirectionalScan1AU:
                    Logging.Log("DirectionalScannerBehavior","PVEDirectionalScan1AU - Starting",Logging.white);
                    List<EntityCache> pveDirectionalScan1AUentitiesInList = Cache.Instance.Entities.Where(t => !t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 1).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pveDirectionalScan1AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVEDirectionalScan5AU:
                    Logging.Log("DirectionalScannerBehavior","PVEDirectionalScan5AU - Starting",Logging.white);
                    List<EntityCache> pveDirectionalScan5AUentitiesInList = Cache.Instance.Entities.Where(t => !t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 5).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pveDirectionalScan5AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVEDirectionalScan10AU:
                    Logging.Log("DirectionalScannerBehavior","PVEDirectionalScan10AU - Starting",Logging.white);
                    List<EntityCache> pveDirectionalScan10AUentitiesInList = Cache.Instance.Entities.Where(t => !t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 10).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pveDirectionalScan10AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVEDirectionalScan15AU:
                    Logging.Log("DirectionalScannerBehavior","PVEDirectionalScan15AU - Starting",Logging.white);
                    List<EntityCache> pveDirectionalScan15AUentitiesInList = Cache.Instance.Entities.Where(t => !t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 15).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pveDirectionalScan15AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVEDirectionalScan20AU:
                    Logging.Log("DirectionalScannerBehavior","PVEDirectionalScan20AU - Starting",Logging.white);
                    List<EntityCache> pveDirectionalScan20AUentitiesInList = Cache.Instance.Entities.Where(t => !t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 20).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pveDirectionalScan20AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.PVEDirectionalScan50AU:
                    Logging.Log("DirectionalScannerBehavior","PVEDirectionalScan50AU - Starting",Logging.white);
                    List<EntityCache> pveDirectionalScan50AUentitiesInList = Cache.Instance.Entities.Where(t => !t.IsPlayer && t.Distance < (double)Distance.DirectionalScannerCloseRange * 50).OrderBy(t => t.Distance).ToList();
                    Statistics.EntityStatistics(pveDirectionalScan50AUentitiesInList);
                    Cache.Instance.Paused = true;
                    break;



                case DirectionalScannerBehaviorState.LogCombatTargets:
                    //combat targets
                    //List<EntityCache> combatentitiesInList =  Cache.Instance.Entities.Where(t => t.IsNpc && !t.IsBadIdea && t.CategoryId == (int)CategoryID.Entity && !t.IsContainer && t.Distance < Cache.Instance.MaxRange && !Cache.Instance.IgnoreTargets.Contains(t.Name.Trim())).ToList();
                    List<EntityCache> combatentitiesInList =  Cache.Instance.Entities.Where(t => t.IsNpc && !t.IsBadIdea && t.CategoryId == (int)CategoryID.Entity && !t.IsContainer).ToList();
                    Statistics.EntityStatistics(combatentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.LogDroneTargets:
                    //drone targets
                    List<EntityCache> droneentitiesInList = Cache.Instance.Entities.Where(e => e.IsNpc && !e.IsBadIdea && e.CategoryId == (int)CategoryID.Entity && !e.IsContainer && !e.IsSentry && e.GroupId != (int)Group.LargeCollidableStructure).ToList();
                    Statistics.EntityStatistics(droneentitiesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.LogStationEntities:
                    //stations
                    List<EntityCache> stationsInList = Cache.Instance.Entities.Where(e => !e.IsSentry && e.GroupId == (int)Group.Station).ToList();
                    Statistics.EntityStatistics(stationsInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.LogStargateEntities:
                    //stargates
                    List<EntityCache> stargatesInList = Cache.Instance.Entities.Where(e => !e.IsSentry && e.GroupId == (int)Group.Stargate).ToList();
                    Statistics.EntityStatistics(stargatesInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.LogAsteroidBelts:
                    //Asteroid Belts
                    List<EntityCache> asteroidbeltsInList = Cache.Instance.Entities.Where(e => !e.IsSentry && e.GroupId == (int)Group.AsteroidBelt).ToList();
                    Statistics.EntityStatistics(asteroidbeltsInList);
                    Cache.Instance.Paused = true;
                    break;

                case DirectionalScannerBehaviorState.Default:
                    if (_States.CurrentDirectionalScannerBehaviorState == DirectionalScannerBehaviorState.Default) _States.CurrentDirectionalScannerBehaviorState = DirectionalScannerBehaviorState.Idle;
                    break;
            }
        }
    }
}
