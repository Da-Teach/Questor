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
using Questor.Storylines;
using LavishScriptAPI;

namespace Questor.Behaviors
{
    public class CombatMissionsBehavior
    {
        private readonly AgentInteraction _agentInteraction;
        private readonly Arm _arm;
        private readonly SwitchShip _switchShip;
        private readonly Combat _combat;
        private readonly CourierMissionCtrl _courierMissionCtrl;
        private readonly LocalWatch _localWatch;
        //private readonly Defense _defense;
        private readonly Drones _drones;

        private DateTime _lastPulse;
        private DateTime _lastSalvageTrip = DateTime.MinValue;
        private readonly CombatMissionCtrl _combatMissionCtrl;
        private readonly Panic _panic;
        private readonly Storyline _storyline;
        private readonly Statistics _statistics;
        private readonly Salvage _salvage;
        private readonly Traveler _traveler;
        private readonly UnloadLoot _unloadLoot;
        public DateTime LastAction;
        private readonly Random _random;
        private int _randomDelay;
        public static long AgentID;
        private readonly Stopwatch _watch;

        private double _lastX;
        private double _lastY;
        private double _lastZ;
        private bool _gatesPresent;
        private bool _firstStart = true;
        public bool Panicstatereset; //false;

        private bool ValidSettings { get; set; }

        public bool CloseQuestorflag = true;

        public string CharacterName { get; set; }

        //DateTime _nextAction = DateTime.Now;

        public CombatMissionsBehavior()
        {
            _lastPulse = DateTime.MinValue;

            _random = new Random();
            _salvage = new Salvage();
            _localWatch = new LocalWatch();
            _combat = new Combat();
            _drones = new Drones();
            _traveler = new Traveler();
            _unloadLoot = new UnloadLoot();
            _agentInteraction = new AgentInteraction();
            _arm = new Arm();
            _courierMissionCtrl = new CourierMissionCtrl();
            _switchShip = new SwitchShip();
            _combatMissionCtrl = new CombatMissionCtrl();
            _panic = new Panic();
            _storyline = new Storyline();
            _statistics = new Statistics();
            _watch = new Stopwatch();

            //
            // this is combat mission specific and needs to be generalized
            //
            Settings.Instance.SettingsLoaded += SettingsLoaded;

            // States.CurrentCombatMissionBehaviorState fixed on ExecuteMission
            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
            _States.CurrentArmState = ArmState.Idle;
            _States.CurrentCombatState = CombatState.Idle;
            _States.CurrentDroneState = DroneState.Idle;
            _States.CurrentUnloadLootState = UnloadLootState.Idle;
            _States.CurrentTravelerState = TravelerState.Idle;
        }

        public void SettingsLoaded(object sender, EventArgs e)
        {
            ApplySalvageSettings();
            ValidateCombatMissionSettings();
        }

        public void DebugCombatMissionsBehaviorStates()
        {
            if (Settings.Instance.DebugStates)
                Logging.Log("CombatMissionsBehavior.State is", _States.CurrentCombatMissionBehaviorState.ToString(), Logging.white);
        }

        public void DebugPanicstates()
        {
            if (Settings.Instance.DebugStates)
                Logging.Log("Panic.State is ", _States.CurrentPanicState.ToString(), Logging.white);
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
                Logging.Log(whatWeAreTiming, " took " + _watch.ElapsedMilliseconds + "ms", Logging.white);
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
                _agentInteraction.AgentId = agent.AgentId;
                _combatMissionCtrl.AgentId = agent.AgentId;
                _arm.AgentId = agent.AgentId;
                _statistics.AgentID = agent.AgentId;
                AgentID = agent.AgentId;
            }
        }

        public void ApplySalvageSettings()
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
            var baseDestination = _traveler.Destination as StationDestination;
            if (baseDestination == null || baseDestination.StationId != Cache.Instance.Agent.StationId)
                _traveler.Destination = new StationDestination(Cache.Instance.Agent.SolarSystemId, Cache.Instance.Agent.StationId, Cache.Instance.DirectEve.GetLocationName(Cache.Instance.Agent.StationId));

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
                Logging.Log("Traveler.State", "is " + _States.CurrentTravelerState, Logging.white);
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
                    if (thisBigObject.Distance >= (int)Distance.TooCloseToStructure)
                    {
                        //we are no longer "too close" and can proceed.
                    }
                    else
                    {
                        if (DateTime.Now > Cache.Instance.NextOrbit)
                        {
                            thisBigObject.Orbit((int)Distance.SafeDistancefromStructure);
                            Logging.Log("CombatMissionsBehavior", _States.CurrentCombatMissionBehaviorState +
                                       ": initiating Orbit of [" + thisBigObject.Name +
                                          "] orbiting at [" + Distance.SafeDistancefromStructure + "]", Logging.white);
                            Cache.Instance.NextOrbit = DateTime.Now.AddSeconds((int)Time.OrbitDelay_seconds);
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
            if (Settings.Instance.FinishWhenNotSafe && (_States.CurrentCombatMissionBehaviorState != CombatMissionsBehaviorState.GotoNearestStation /*|| State!=QuestorState.GotoBase*/))
            {
                //need to remove spam
                if (Cache.Instance.InSpace && !Cache.Instance.LocalSafe(Settings.Instance.LocalBadStandingPilotsToTolerate, Settings.Instance.LocalBadStandingLevelToConsiderBad))
                {
                    var station = Cache.Instance.Stations.OrderBy(x => x.Distance).FirstOrDefault();
                    if (station != null)
                    {
                        Logging.Log("Local not safe", "Station found. Going to nearest station", Logging.white);
                        if (_States.CurrentCombatMissionBehaviorState != CombatMissionsBehaviorState.GotoNearestStation)
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoNearestStation;
                    }
                    else
                    {
                        Logging.Log("Local not safe", "Station not found. Going back to base", Logging.white);
                        if (_States.CurrentCombatMissionBehaviorState != CombatMissionsBehaviorState.GotoBase)
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
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
                if (_States.CurrentCombatMissionBehaviorState != CombatMissionsBehaviorState.GotoBase)
                {
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
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

            Cache.Instance.InMission = _States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.ExecuteMission;
            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Storyline && _States.CurrentStorylineState == StorylineState.ExecuteMission)
            {
                Cache.Instance.InMission |= _storyline.StorylineHandler is GenericCombatStoryline && (_storyline.StorylineHandler as GenericCombatStoryline).State == GenericCombatStorylineState.ExecuteMission;
            }
            //
            // Panic always runs, not just in space
            //
            DebugPerformanceClearandStartTimer();
            _panic.ProcessState();
            DebugPerformanceStopandDisplayTimer("Panic.ProcessState");
            if (_States.CurrentPanicState == PanicState.Panic || _States.CurrentPanicState == PanicState.Panicking)
            {
                // If Panic is in panic state, questor is in panic States.CurrentCombatMissionBehaviorState :)
                _States.CurrentCombatMissionBehaviorState = _States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Storyline ? CombatMissionsBehaviorState.StorylinePanic : CombatMissionsBehaviorState.Panic;

                DebugCombatMissionsBehaviorStates();
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

                // Ugly storyline resume hack
                if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.StorylinePanic)
                {
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Storyline;
                    if (_storyline.StorylineHandler is GenericCombatStoryline)
                        (_storyline.StorylineHandler as GenericCombatStoryline).State = GenericCombatStorylineState.GotoMission;
                }
                else
                {
                    // Head back to the mission
                    _States.CurrentTravelerState = TravelerState.Idle;
                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
                }
                DebugCombatMissionsBehaviorStates();
            }
            DebugPanicstates();

            switch (_States.CurrentCombatMissionBehaviorState)
            {
                case CombatMissionsBehaviorState.Idle:

                    if (Cache.Instance.StopBot)
                    {
                        if (Settings.Instance.DebugIdle) Logging.Log("CombatMissionsBehavior", "if (Cache.Instance.StopBot)", Logging.white);
                        return;
                    }

                    if (Cache.Instance.InSpace)
                    {
                        if (Settings.Instance.DebugIdle) Logging.Log("CombatMissionsBehavior", "if (Cache.Instance.InSpace)", Logging.white);
                        // Questor does not handle in space starts very well, head back to base to try again
                        Logging.Log("CombatMissionsBehavior", "Started questor while in space, heading back to base in 15 seconds", Logging.white);
                        LastAction = DateTime.Now;
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Idle) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.DelayedGotoBase;
                        break;
                    }
                    else
                    {
                        if (Settings.Instance.DebugIdle) Logging.Log("CombatMissionsBehavior", "if (Cache.Instance.InSpace) else", Logging.white);
                        _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                        _States.CurrentArmState = ArmState.Idle;
                        _States.CurrentDroneState = DroneState.Idle;
                        _States.CurrentSalvageState = SalvageState.Idle;
                        _States.CurrentStorylineState = StorylineState.Idle;
                        _States.CurrentTravelerState = TravelerState.Idle;
                        _States.CurrentUnloadLootState = UnloadLootState.Idle;
                        _States.CurrentTravelerState = TravelerState.Idle;
                    }

                    // only attempt to write the mission statistics logs if one of the mission stats logs is enabled in settings
                    if (Settings.Instance.MissionStats1Log || Settings.Instance.MissionStats3Log || Settings.Instance.MissionStats3Log)
                    {
                        if (!Statistics.Instance.MissionLoggingCompleted)
                        {
                            Statistics.WriteMissionStatistics();
                            break;
                        }
                    }

                    if (Settings.Instance.AutoStart)
                    {
                        // Don't start a new action an hour before downtime
                        if (DateTime.UtcNow.Hour == 10)
                        {
                            if (Settings.Instance.DebugAutoStart) Logging.Log("CombatMissionsBehavior", "Autostart: if (DateTime.UtcNow.Hour == 10)", Logging.white);
                            break;
                        }

                        // Don't start a new action near downtime
                        if (DateTime.UtcNow.Hour == 11 && DateTime.UtcNow.Minute < 15)
                        {
                            if (Settings.Instance.DebugAutoStart) Logging.Log("CombatMissionsBehavior", "if (DateTime.UtcNow.Hour == 11 && DateTime.UtcNow.Minute < 15)", Logging.white);
                            break;
                        }

                        if (Settings.Instance.RandomDelay > 0 || Settings.Instance.MinimumDelay > 0)
                        {
                            _randomDelay = (Settings.Instance.RandomDelay > 0 ? _random.Next(Settings.Instance.RandomDelay) : 0) + Settings.Instance.MinimumDelay;
                            LastAction = DateTime.Now;
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Idle) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.DelayedStart;
                            Logging.Log("CombatMissionsBehavior", "Random start delay of [" + _randomDelay + "] seconds", Logging.white);
                            return;
                        }
                        else
                        {
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Idle) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Cleanup;
                            return;
                        }
                    }
                    else
                    {
                        Cache.Instance.LastScheduleCheck = DateTime.Now;
                        // Every 5 min of idle check and make sure we aren't supposed to stop...
                        if (Math.Round(DateTime.Now.Subtract(Cache.Instance.LastTimeCheckAction).TotalMinutes) > 5)
                        {
                            Questor.TimeCheck();   //Should we close questor due to stoptime or runtime?
                            //Questor.WalletCheck(); //Should we close questor due to no wallet balance change? (stuck?)
                        }
                    }
                    break;

                case CombatMissionsBehaviorState.DelayedStart:
                    if (DateTime.Now.Subtract(LastAction).TotalSeconds < _randomDelay)
                        break;

                    _storyline.Reset();
                    if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.DelayedStart) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Cleanup;
                    break;

                case CombatMissionsBehaviorState.DelayedGotoBase:
                    if (DateTime.Now.Subtract(LastAction).TotalSeconds < (int)Time.DelayedGotoBase_seconds)
                        break;

                    Logging.Log("CombatMissionsBehavior", "Heading back to base", Logging.white);
                    if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.DelayedGotoBase) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                    break;

                case CombatMissionsBehaviorState.Cleanup:
                    //
                    // this States.CurrentCombatMissionBehaviorState is needed because forced disconnects
                    // and crashes can leave "extra" cargo in the
                    // cargo hold that is undesirable and causes
                    // problems loading the correct ammo on occasion
                    //
                    if (Cache.Instance.LootAlreadyUnloaded == false)
                    {
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Cleanup) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                        break;
                    }
                    else
                    {
                        Questor.CheckEVEStatus();
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Cleanup) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Start;
                        break;
                    }

                case CombatMissionsBehaviorState.Start:
                    if (_firstStart && Settings.Instance.MultiAgentSupport)
                    {
                        //if you are in wrong station and is not first agent
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Start) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Switch;
                        _firstStart = false;
                        break;
                    }
                    Cache.Instance.OpenWrecks = false;
                    if (_States.CurrentAgentInteractionState == AgentInteractionState.Idle)
                    {
                        Cache.Instance.Wealth = Cache.Instance.DirectEve.Me.Wealth;

                        Cache.Instance.WrecksThisMission = 0;
                        if (Settings.Instance.EnableStorylines && _storyline.HasStoryline())
                        {
                            Logging.Log("CombatMissionsBehavior", "Storyline detected, doing storyline.", Logging.white);
                            _storyline.Reset();
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Start) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Storyline;
                            break;
                        }
                        Logging.Log("AgentInteraction", "Start conversation [Start Mission]", Logging.white);
                        _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
                        _agentInteraction.Purpose = AgentInteractionPurpose.StartMission;
                    }

                    _agentInteraction.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("AgentInteraction.State", "is " + _States.CurrentAgentInteractionState, Logging.white);

                    if (_States.CurrentAgentInteractionState == AgentInteractionState.Done)
                    {
                        Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentID);
                        if (Cache.Instance.Mission != null)
                        {
                            // Update loyalty points again (the first time might return -1)
                            Statistics.Instance.LoyaltyPoints = Cache.Instance.Agent.LoyaltyPoints;
                            Cache.Instance.MissionName = Cache.Instance.Mission.Name;
                        }

                        _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Start) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Arm;
                        return;
                    }

                    if (_States.CurrentAgentInteractionState == AgentInteractionState.ChangeAgent)
                    {
                        _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                        ValidateCombatMissionSettings();
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Start) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Switch;
                        break;
                    }

                    break;

                case CombatMissionsBehaviorState.Switch:

                    if (_States.CurrentSwitchShipState == SwitchShipState.Idle)
                    {
                        Logging.Log("Switch", "Begin", Logging.white);
                        _States.CurrentSwitchShipState = SwitchShipState.Begin;
                    }

                    _switchShip.ProcessState();

                    if (_States.CurrentSwitchShipState == SwitchShipState.Done)
                    {
                        _States.CurrentSwitchShipState = SwitchShipState.Idle;
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Switch) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                    }
                    break;

                case CombatMissionsBehaviorState.Arm:
                    if (_States.CurrentArmState == ArmState.Idle)
                    {
                        if (Cache.Instance.CourierMission)
                            _States.CurrentArmState = ArmState.SwitchToTransportShip;
                        else
                        {
                            Logging.Log("Arm", "Begin", Logging.white);
                            _States.CurrentArmState = ArmState.Begin;

                            // Load right ammo based on mission
                            _arm.AmmoToLoad.Clear();
                            _arm.AmmoToLoad.AddRange(_agentInteraction.AmmoToLoad);
                        }
                    }

                    _arm.ProcessState();

                    if (Settings.Instance.DebugStates) Logging.Log("Arm.State", "is" + _States.CurrentArmState, Logging.white);

                    if (_States.CurrentArmState == ArmState.NotEnoughAmmo)
                    {
                        // we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        // we may be out of drones/ammo but disconnecting/reconnecting will not fix that so update the timestamp
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        Logging.Log("Arm", "Armstate.NotEnoughAmmo", Logging.orange);
                        _States.CurrentArmState = ArmState.Idle;
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Arm) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                    }

                    if (_States.CurrentArmState == ArmState.NotEnoughDrones)
                    {
                        // we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        // we may be out of drones/ammo but disconnecting/reconnecting will not fix that so update the timestamp
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        Logging.Log("Arm", "Armstate.NotEnoughDrones", Logging.orange);
                        _States.CurrentArmState = ArmState.Idle;
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Arm) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                    }

                    if (_States.CurrentArmState == ArmState.Done)
                    {
                        //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        _States.CurrentArmState = ArmState.Idle;
                        _States.CurrentDroneState = DroneState.WaitingForTargets;

                        if (Cache.Instance.CourierMission)
                        {
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Arm) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.CourierMission;
                        }
                        else
                        {
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Arm) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.LocalWatch;
                        }
                    }

                    break;

                case CombatMissionsBehaviorState.LocalWatch:
                    if (Settings.Instance.UseLocalWatch)
                    {
                        Cache.Instance.LastLocalWatchAction = DateTime.Now;
                        if (Cache.Instance.LocalSafe(Settings.Instance.LocalBadStandingPilotsToTolerate, Settings.Instance.LocalBadStandingLevelToConsiderBad))
                        {
                            Logging.Log("CombatMissionsBehavior.LocalWatch", "local is clear", Logging.white);
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.LocalWatch) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.WarpOutStation;
                        }
                        else
                        {
                            Logging.Log("CombatMissionsBehavior.LocalWatch", "Bad standings pilots in local: We will stay 5 minutes in the station and then we will check if it is clear again", Logging.orange);
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.LocalWatch)
                            {
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.WaitingforBadGuytoGoAway;
                            }
                            Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                            Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        }
                    }
                    else
                    {
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.LocalWatch) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.WarpOutStation;
                    }
                    break;

                case CombatMissionsBehaviorState.WaitingforBadGuytoGoAway:
                    Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                    Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                    if (DateTime.Now.Subtract(Cache.Instance.LastLocalWatchAction).TotalMinutes < (int)Time.WaitforBadGuytoGoAway_minutes)
                        break;
                    if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.WaitingforBadGuytoGoAway) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.LocalWatch;
                    break;

                case CombatMissionsBehaviorState.WarpOutStation:
                    DirectBookmark warpOutBookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkWarpOut ?? "").OrderByDescending(b => b.CreatedOn).FirstOrDefault(b => b.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId);
                    //DirectBookmark _bookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.bookmarkWarpOut + "-" + Cache.Instance.CurrentAgent ?? "").OrderBy(b => b.CreatedOn).FirstOrDefault();
                    long solarid = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;

                    if (warpOutBookmark == null)
                    {
                        Logging.Log("CombatMissionsBehavior.WarpOut", "No Bookmark", Logging.white);
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.WarpOutStation) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
                    }
                    else if (warpOutBookmark.LocationId == solarid)
                    {
                        if (_traveler.Destination == null)
                        {
                            Logging.Log("CombatMissionsBehavior.WarpOut", "Warp at " + warpOutBookmark.Title, Logging.white);
                            _traveler.Destination = new BookmarkDestination(warpOutBookmark);
                            Cache.Instance.DoNotBreakInvul = true;
                        }

                        _traveler.ProcessState();
                        if (_States.CurrentTravelerState == TravelerState.AtDestination)
                        {
                            Logging.Log("CombatMissionsBehavior.WarpOut", "Safe!", Logging.white);
                            Cache.Instance.DoNotBreakInvul = false;
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.WarpOutStation) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
                            _traveler.Destination = null;
                        }
                    }
                    else
                    {
                        Logging.Log("CombatMissionsBehavior.WarpOut", "No Bookmark in System", Logging.orange);
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.WarpOutStation) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoMission;
                    }
                    break;

                case CombatMissionsBehaviorState.GotoMission:
                    Statistics.Instance.MissionLoggingCompleted = false;
                    var missionDestination = _traveler.Destination as MissionBookmarkDestination;

                    if (missionDestination == null || missionDestination.AgentId != AgentID) // We assume that this will always work "correctly" (tm)
                    {
                        const string nameOfBookmark = "Encounter";
                        Logging.Log("CombatMissionsBehavior", "Setting Destination to 1st bookmark from AgentID: " + AgentID + " with [" + nameOfBookmark + "] in the title", Logging.white);
                        _traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(AgentID, nameOfBookmark));
                    }

                    if (Cache.Instance.PriorityTargets.Any(pt => pt != null && pt.IsValid))
                    {
                        Logging.Log("CombatMissionsBehavior.GotoMission", "Priority targets found, engaging!", Logging.white);
                        _combat.ProcessState();
                    }

                    _traveler.ProcessState();
                    if (Settings.Instance.DebugStates)
                        Logging.Log("Traveler.State", "is " + _States.CurrentTravelerState, Logging.white);

                    if (_States.CurrentTravelerState == TravelerState.AtDestination)
                    {
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.GotoMission) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.ExecuteMission;

                        // Seeing as we just warped to the mission, start the mission controller
                        _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.Start;
                        _States.CurrentCombatState = CombatState.CheckTargets;
                        _traveler.Destination = null;
                    }
                    break;

                case CombatMissionsBehaviorState.ExecuteMission:
                    DebugPerformanceClearandStartTimer();
                    _combat.ProcessState();
                    DebugPerformanceStopandDisplayTimer("Combat.ProcessState");

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Combat.State is", _States.CurrentCombatState.ToString(), Logging.white);

                    DebugPerformanceClearandStartTimer();
                    _drones.ProcessState();
                    DebugPerformanceStopandDisplayTimer("Drones.ProcessState");

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Drones.State is", _States.CurrentDroneState.ToString(), Logging.white);

                    DebugPerformanceClearandStartTimer();
                    _salvage.ProcessState();
                    DebugPerformanceStopandDisplayTimer("Salvage.ProcessState");

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Salvage.State is", _States.CurrentSalvageState.ToString(), Logging.white);

                    DebugPerformanceClearandStartTimer();
                    _combatMissionCtrl.ProcessState();
                    DebugPerformanceStopandDisplayTimer("MissionController.ProcessState");

                    if (Settings.Instance.DebugStates)
                        Logging.Log("CombatMissionsBehavior.State is", _States.CurrentCombatMissionCtrlState.ToString(), Logging.white);

                    // If we are out of ammo, return to base, the mission will fail to complete and the bot will reload the ship
                    // and try the mission again
                    if (_States.CurrentCombatState == CombatState.OutOfAmmo)
                    {
                        Logging.Log("Combat", "Out of Ammo!", Logging.orange);
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.ExecuteMission) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();
                        //Cache.Instance.InvalidateBetweenMissionsCache();
                    }

                    if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Done)
                    {
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.ExecuteMission) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;

                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();
                        //Cache.Instance.InvalidateBetweenMissionsCache();
                    }

                    // If in error state, just go home and stop the bot
                    if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Error)
                    {
                        Logging.Log("MissionController", "Error", Logging.red);
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.ExecuteMission) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;

                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();
                        //Cache.Instance.InvalidateBetweenMissionsCache();
                    }
                    break;

                case CombatMissionsBehaviorState.GotoBase:
                    if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "GotoBase: AvoidBumpingThings()", Logging.white);

                    AvoidBumpingThings();

                    if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "GotoBase: TravelToAgentsStation()", Logging.white);

                    TravelToAgentsStation();

                    if (_States.CurrentTravelerState == TravelerState.AtDestination) // || DateTime.Now.Subtract(Cache.Instance.EnteredCloseQuestor_DateTime).TotalMinutes > 10)
                    {
                        if (Settings.Instance.DebugGotobase) Logging.Log("CombatMissionsBehavior", "GotoBase: We are at destination", Logging.white);
                        Cache.Instance.GotoBaseNow = false; //we are there - turn off the 'forced' gotobase
                        Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentID);

                        if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Error)
                        {
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.GotoBase) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                        }
                        else if (_States.CurrentCombatState != CombatState.OutOfAmmo && Cache.Instance.Mission != null && Cache.Instance.Mission.State == (int)MissionState.Accepted)
                        {
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.GotoBase) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.CompleteMission;
                        }
                        else
                        {
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.GotoBase) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.UnloadLoot;
                        }
                        _traveler.Destination = null;
                    }
                    break;

                case CombatMissionsBehaviorState.CompleteMission:
                    if (_States.CurrentAgentInteractionState == AgentInteractionState.Idle)
                    {
                        //Logging.Log("CombatMissionsBehavior: Starting: Statistics.WriteDroneStatsLog");
                        if (!Statistics.WriteDroneStatsLog()) break;
                        //Logging.Log("CombatMissionsBehavior: Starting: Statistics.AmmoConsumptionStatistics");
                        if (!Statistics.AmmoConsumptionStatistics()) break;

                        Logging.Log("AgentInteraction", "Start Conversation [Complete Mission]", Logging.white);

                        _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
                        _agentInteraction.Purpose = AgentInteractionPurpose.CompleteMission;
                    }

                    _agentInteraction.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("AgentInteraction.State is ", _States.CurrentAgentInteractionState.ToString(), Logging.white);

                    if (_States.CurrentAgentInteractionState == AgentInteractionState.Done)
                    {
                        // Cache.Instance.MissionName = String.Empty;  // Do Not clear the 'current' mission name until after we have done the mission logging
                        _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                        if (Cache.Instance.CourierMission)
                        {
                            Cache.Instance.CourierMission = false;
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.CompleteMission)
                            {
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                                _States.CurrentQuestorState = QuestorState.Idle;
                            }
                        }
                        else
                        {
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.CompleteMission) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.UnloadLoot;
                        }
                        return;
                    }
                    break;

                case CombatMissionsBehaviorState.UnloadLoot:
                    if (_States.CurrentUnloadLootState == UnloadLootState.Idle)
                    {
                        Logging.Log("CombatMissionsBehavior", "UnloadLoot: Begin", Logging.white);
                        _States.CurrentUnloadLootState = UnloadLootState.Begin;
                    }

                    _unloadLoot.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("CombatMissionsBehavior", "UnloadLoot.State is " + _States.CurrentUnloadLootState, Logging.white);

                    if (_States.CurrentUnloadLootState == UnloadLootState.Done)
                    {
                        Cache.Instance.LootAlreadyUnloaded = true;
                        _States.CurrentUnloadLootState = UnloadLootState.Idle;
                        Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentID);
                        if (_States.CurrentCombatState == CombatState.OutOfAmmo) // on mission
                        {
                            Logging.Log("CombatMissionsBehavior.UnloadLoot", "We are out of ammo", Logging.orange);
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                            return;
                        }
                        else if ((Cache.Instance.Mission != null) && (Cache.Instance.Mission.State != (int)MissionState.Offered)) // on mission
                        {
                            Logging.Log("CombatMissionsBehavior.Unloadloot", "We are on mission", Logging.white);
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                            return;
                        }
                        //This salvaging decision tree does not belong here and should be separated out into a different questorstate
                        if (Settings.Instance.AfterMissionSalvaging)
                        {
                            if (Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").Count == 0)
                            {
                                Logging.Log("CombatMissionsBehavior.Unloadloot", " No more salvaging bookmarks. Setting FinishedSalvaging Update.", Logging.white);
                                //if (Settings.Instance.CharacterMode == "Salvager")
                                //{
                                //    Logging.Log("Salvager mode set and no bookmarks making delay");
                                //    States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorStateState.Error; //or salvageonly. need to check difference
                                //}

                                if (Settings.Instance.CharacterMode.ToLower() == "salvage".ToLower())
                                {
                                    Logging.Log("CombatMissionsBehavior.UnloadLoot", "Character mode is BookmarkSalvager and no bookmarks salvage.", Logging.white);
                                    //We just need a NextSalvagerSession timestamp to key off of here to add the delay
                                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                                }
                                else
                                {
                                    //Logging.Log("CombatMissionsBehavior: Character mode is not salvage going to next mission.");
                                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle; //add pause here
                                    _States.CurrentQuestorState = QuestorState.Idle;
                                }
                                Statistics.Instance.FinishedSalvaging = DateTime.Now;
                                return;
                            }
                            else //There is at least 1 salvage bookmark
                            {
                                Logging.Log("CombatMissionsBehavior.Unloadloot", "There are [ " + Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").Count + " ] more salvage bookmarks left to process", Logging.white);
                                // Salvage only after multiple missions have been completed
                                if (Settings.Instance.SalvageMultpleMissionsinOnePass)
                                {
                                    //if we can still complete another mission before the Wrecks disappear and still have time to salvage
                                    if (DateTime.Now.Subtract(Statistics.Instance.FinishedSalvaging).TotalMinutes > ((int)Time.WrecksDisappearAfter_minutes - (int)Time.AverageTimeToCompleteAMission_minutes - (int)Time.AverageTimetoSalvageMultipleMissions_minutes))
                                    {
                                        Logging.Log("CombatMissionsBehavior.UnloadLoot", "The last finished after mission salvaging session was [" + DateTime.Now.Subtract(Statistics.Instance.FinishedSalvaging).TotalMinutes + "] ago ", Logging.white);
                                        Logging.Log("CombatMissionsBehavior.UnloadLoot", "we are after mission salvaging again because it has been at least [" + ((int)Time.WrecksDisappearAfter_minutes - (int)Time.AverageTimeToCompleteAMission_minutes - (int)Time.AverageTimetoSalvageMultipleMissions_minutes) + "] min since the last session. ", Logging.white);
                                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.UnloadLoot)
                                        {
                                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.BeginAfterMissionSalvaging;
                                            Statistics.Instance.StartedSalvaging = DateTime.Now;
                                            //FIXME: should we be overwriting this timestamp here? What if this is the 3rd run back and fourth to the station?
                                        }
                                    }
                                    else //we are salvaging mission 'in one pass' and it has not been enough time since our last run... do another mission
                                    {
                                        Logging.Log("CombatMissionsBehavior.UnloadLoot", "The last finished after mission salvaging session was [" + DateTime.Now.Subtract(Statistics.Instance.FinishedSalvaging).TotalMinutes + "] ago ", Logging.white);
                                        Logging.Log("CombatMissionsBehavior.UnloadLoot", "we are going to the next mission because it has not been [" + ((int)Time.WrecksDisappearAfter_minutes - (int)Time.AverageTimeToCompleteAMission_minutes - (int)Time.AverageTimetoSalvageMultipleMissions_minutes) + "] min since the last session. ", Logging.white);
                                        Statistics.Instance.FinishedMission = DateTime.Now;
                                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.UnloadLoot) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                                    }
                                }
                                else //begin after mission salvaging now, rather than later
                                {
                                    if (Settings.Instance.CharacterMode == "salvage".ToLower())
                                    {
                                        Logging.Log("CombatMissionsBehavior.Unloadloot", "CharacterMode: [" + Settings.Instance.CharacterMode + "], AfterMissionSalvaging: [" + Settings.Instance.AfterMissionSalvaging + "], CombatMissionsBehaviorState: [" + _States.CurrentCombatMissionBehaviorState + "]", Logging.white);
                                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.BeginAfterMissionSalvaging;
                                        Statistics.Instance.StartedSalvaging = DateTime.Now;
                                    }
                                    else
                                    {
                                        Logging.Log("CombatMissionsBehavior.UnloadLoot", "The last after mission salvaging session was [" + Math.Round(DateTime.Now.Subtract(Statistics.Instance.FinishedSalvaging).TotalMinutes, 0) + "min] ago ", Logging.white);
                                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.BeginAfterMissionSalvaging;
                                        Statistics.Instance.StartedSalvaging = DateTime.Now;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                            _States.CurrentQuestorState = QuestorState.Idle;
                            Logging.Log("CombatMissionsBehavior.Unloadloot", "CharacterMode: [" + Settings.Instance.CharacterMode + "], AfterMissionSalvaging: [" + Settings.Instance.AfterMissionSalvaging + "], CombatMissionsBehaviorState: [" + _States.CurrentCombatMissionBehaviorState + "]", Logging.white);
                            Statistics.Instance.FinishedMission = DateTime.Now;
                            return;
                        }
                    }
                    break;

                case CombatMissionsBehaviorState.BeginAfterMissionSalvaging:
                    Statistics.Instance.StartedSalvaging = DateTime.Now; //this will be reset for each "run" between the station and the field if using <unloadLootAtStation>true</unloadLootAtStation>
                    if (DateTime.Now.Subtract(_lastSalvageTrip).TotalMinutes < (int)Time.DelayBetweenSalvagingSessions_minutes && Settings.Instance.CharacterMode.ToLower() == "salvage".ToLower())
                    {
                        Logging.Log("CombatMissionsBehavior.BeginAftermissionSalvaging", "Too early for next salvage trip", Logging.white);
                        break;
                    }
                    Cache.Instance.OpenWrecks = true;
                    if (_States.CurrentArmState == ArmState.Idle)
                        _States.CurrentArmState = ArmState.SwitchToSalvageShip;

                    _arm.ProcessState();
                    if (_States.CurrentArmState == ArmState.Done)
                    {
                        DirectBookmark bookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").OrderBy(b => b.CreatedOn).FirstOrDefault();
                        _States.CurrentArmState = ArmState.Idle;
                        if (Settings.Instance.FirstSalvageBookmarksInSystem)
                        {
                            Logging.Log("CombatMissionsBehavior.BeginAftermissionSalvaging", "Salvaging at first bookmark from system", Logging.white);
                            bookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").OrderBy(b => b.CreatedOn).FirstOrDefault(c => c.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId);
                        }
                        else Logging.Log("CombatMissionsBehavior.BeginAftermissionSalvaging", "Salvaging at first oldest bookmarks", Logging.white);
                        if (bookmark == null)
                        {
                            bookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").OrderBy(b => b.CreatedOn).FirstOrDefault();
                            if (bookmark == null)
                            {
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                                return;
                            }
                        }

                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.BeginAfterMissionSalvaging)
                        {
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoSalvageBookmark;
                            _lastSalvageTrip = DateTime.Now;
                        }
                        _traveler.Destination = new BookmarkDestination(bookmark);
                        return;
                    }
                    break;

                case CombatMissionsBehaviorState.GotoSalvageBookmark:
                    _traveler.ProcessState();
                    string target = "Acceleration Gate";
                    Cache.Instance.EntitiesByName(target);
                    if (_States.CurrentTravelerState == TravelerState.AtDestination || GateInSalvage())
                    {
                        //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.GotoSalvageBookmark) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Salvage;
                        _traveler.Destination = null;
                        return;
                    }

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Traveler.State is ", _States.CurrentTravelerState.ToString(), Logging.white);
                    break;

                case CombatMissionsBehaviorState.Salvage:
                    DirectContainer salvageCargo = Cache.Instance.DirectEve.GetShipsCargo();
                    Cache.Instance.SalvageAll = true;
                    Cache.Instance.OpenWrecks = true;

                    const int distancetoccheck = (int)Distance.OnGridWithMe;
                    // is there any NPCs within distancetoconsidertargets?
                    EntityCache deadlyNPC = Cache.Instance.Entities.Where(t => t.Distance < distancetoccheck && !t.IsEntityIShouldLeaveAlone && !t.IsContainer && t.IsNpc && t.CategoryId == (int)CategoryID.Entity && t.GroupId != (int)Group.LargeCollidableStructure).OrderBy(t => t.Distance).FirstOrDefault();

                    if (deadlyNPC != null)
                    {
                        // found NPCs that will likely kill out fragile salvage boat!
                        List<DirectBookmark> missionSalvageBookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");
                        Logging.Log("CombatMissionsBehavior.Salvage", "could not be completed because of NPCs left in the mission: deleting salvage bookmarks", Logging.white);
                        bool _deleteBookmarkWithNpc_tmp = true;
                        if (_deleteBookmarkWithNpc_tmp)
                        {
                            while (true)
                            {
                                // Remove all bookmarks from address book
                                DirectBookmark pocketSalvageBookmark = missionSalvageBookmarks.FirstOrDefault(b => Cache.Instance.DistanceFromMe(b.X ?? 0, b.Y ?? 0, b.Z ?? 0) < (int)Distance.DirectionalScannerCloseRange);
                                if (pocketSalvageBookmark == null)
                                    break;
                                else
                                {
                                    pocketSalvageBookmark.Delete();
                                    missionSalvageBookmarks.Remove(pocketSalvageBookmark);
                                }
                                return;
                            }
                        }
                        if (!missionSalvageBookmarks.Any())
                        {
                            Logging.Log("CombatMissionsBehavior.Salvage", "could not be completed because of NPCs left in the mission: salvage bookmarks deleted", Logging.orange);
                            Cache.Instance.SalvageAll = false;
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Salvage)
                            {
                                Statistics.Instance.FinishedSalvaging = DateTime.Now;
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                            }
                            return;
                        }
                    }
                    else
                    {
                        if (!Cache.Instance.OpenCargoHold("CombatMissionsBehavior: Salvage")) break;

                        if (Settings.Instance.UnloadLootAtStation && salvageCargo.Window.IsReady && (salvageCargo.Capacity - salvageCargo.UsedCapacity) < 100)
                        {
                            Logging.Log("CombatMissionsBehavior.Salvage", "We are full, go to base to unload", Logging.white);
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Salvage)
                            {
                                _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                            }
                            break;
                        }

                        if (!Cache.Instance.UnlootedContainers.Any())
                        {
                            Logging.Log("CombatMissionsBehavior.Salvage", "Finished salvaging the room", Logging.white);

                            bool gatesInRoom = GateInSalvage();
                            List<DirectBookmark> bookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");

                            while (true)
                            {
                                // Remove all bookmarks from address book
                                var bookmark = bookmarks.FirstOrDefault(b => Cache.Instance.DistanceFromMe(b.X ?? 0, b.Y ?? 0, b.Z ?? 0) < (int)Distance.OnGridWithMe);
                                if (!gatesInRoom && _gatesPresent) // if there were gates, but we've gone through them all, delete all bookmarks
                                    bookmark = bookmarks.FirstOrDefault();
                                else if (gatesInRoom)
                                    break;
                                if (bookmark == null)
                                    break;

                                bookmark.Delete();
                                bookmarks.Remove(bookmark);
                                Cache.Instance.NextRemoveBookmarkAction = DateTime.Now.AddSeconds((int)Time.RemoveBookmarkDelay_seconds);
                                return;
                            }

                            if (bookmarks.Count == 0 && !gatesInRoom)
                            {
                                Logging.Log("CombatMissionsBehavior.Salvage", "We have salvaged all bookmarks, go to base", Logging.white);
                                Cache.Instance.SalvageAll = false;
                                if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Salvage)
                                {
                                    Statistics.Instance.FinishedSalvaging = DateTime.Now;
                                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                                }
                                return;
                            }
                            else
                            {
                                if (!gatesInRoom)
                                {
                                    Logging.Log("CombatMissionsBehavior.Salvage", "Go to the next salvage bookmark", Logging.white);
                                    var bookmark = bookmarks.FirstOrDefault(c => c.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId) ?? bookmarks.FirstOrDefault();
                                    if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Salvage)
                                    {
                                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoSalvageBookmark;
                                    }
                                    _traveler.Destination = new BookmarkDestination(bookmark);
                                }
                                else if (Settings.Instance.UseGatesInSalvage)
                                {
                                    Logging.Log("CombatMissionsBehavior.Salvage", "Acceleration gate found - moving to next pocket", Logging.white);
                                    if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Salvage)
                                    {
                                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.SalvageUseGate;
                                    }
                                }
                                else
                                {
                                    Logging.Log("CombatMissionsBehavior.Salvage", "Acceleration gate found, useGatesInSalvage set to false - Returning to base", Logging.white);
                                    if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Salvage)
                                    {
                                        Statistics.Instance.FinishedSalvaging = DateTime.Now;
                                        _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                                    }
                                    _traveler.Destination = null;
                                }
                            }
                            break;
                        }
                        //we __cannot ever__ approach in salvage.cs so this section _is_ needed.
                        Salvage.MoveIntoRangeOfWrecks();
                        try
                        {
                            // Overwrite settings, as the 'normal' settings do not apply
                            _salvage.MaximumWreckTargets = Math.Min(Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets, Cache.Instance.DirectEve.Me.MaxLockedTargets);
                            _salvage.ReserveCargoCapacity = 80;
                            _salvage.LootEverything = true;
                            _salvage.ProcessState();
                            //Logging.Log("number of max cache ship: " + Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets);
                            //Logging.Log("number of max cache me: " + Cache.Instance.DirectEve.Me.MaxLockedTargets);
                            //Logging.Log("number of max math.min: " + _salvage.MaximumWreckTargets);
                        }
                        finally
                        {
                            ApplySalvageSettings();
                        }
                    }
                    break;

                case CombatMissionsBehaviorState.SalvageUseGate:
                    Cache.Instance.OpenWrecks = true;

                    target = "Acceleration Gate";
                    IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByName(target).ToList();
                    if (targets == null || !targets.Any())
                    {
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.SalvageUseGate)
                        {
                            _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoSalvageBookmark;
                        }
                        return;
                    }

                    _lastX = Cache.Instance.DirectEve.ActiveShip.Entity.X;
                    _lastY = Cache.Instance.DirectEve.ActiveShip.Entity.Y;
                    _lastZ = Cache.Instance.DirectEve.ActiveShip.Entity.Z;

                    EntityCache closest = targets.OrderBy(t => t.Distance).First();
                    if (closest.Distance < (int)Distance.DecloakRange)
                    {
                        Logging.Log("CombatMissionsBehavior.Salvage", "Acceleration gate found - GroupID=" + closest.GroupId, Logging.white);

                        // Activate it and move to the next Pocket
                        closest.Activate();

                        // Do not change actions, if NextPocket gets a timeout (>2 mins) then it reverts to the last action
                        Logging.Log("CombatMissionsBehavior.Salvage", "Activate [" + closest.Name + "] and change States.CurrentCombatMissionBehaviorState to 'NextPocket'", Logging.white);

                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.SalvageUseGate) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.SalvageNextPocket;
                        _lastPulse = DateTime.Now;
                        return;
                    }
                    else
                    {
                        if (closest.Distance < (int)Distance.WarptoDistance)
                        {
                            // Move to the target
                            if (Cache.Instance.NextApproachAction < DateTime.Now && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id))
                            {
                                Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                                Logging.Log("CombatMissionsBehavior.Salvage", "Approaching target [" + closest.Name + "][ID: " + closest.Id + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.white);
                                closest.Approach();
                            }
                        }
                        else
                        {
                            // Probably never happens
                            if (DateTime.Now > Cache.Instance.NextWarpTo)
                            {
                                Logging.Log("CombatMissionsBehavior.Salvage", "Warping to [" + closest.Name + "] which is [" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.white);
                                closest.WarpTo();
                                Cache.Instance.NextWarpTo = DateTime.Now.AddSeconds((int)Time.WarptoDelay_seconds);
                            }
                        }
                    }
                    _lastPulse = DateTime.Now.AddSeconds(10);
                    break;

                case CombatMissionsBehaviorState.SalvageNextPocket:
                    Cache.Instance.OpenWrecks = true;
                    double distance = Cache.Instance.DistanceFromMe(_lastX, _lastY, _lastZ);
                    if (distance > (int)Distance.NextPocketDistance)
                    {
                        //we know we are connected here...
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        Logging.Log("CombatMissionsBehavior.Salvage", "We've moved to the next Pocket [" + Math.Round(distance / 1000, 0) + "k away]", Logging.white);

                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.SalvageNextPocket) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Salvage;
                        return;
                    }
                    else //we have not moved to the next pocket quite yet
                    {
                        if (DateTime.Now.Subtract(_lastPulse).TotalMinutes > 2)
                        {
                            Logging.Log("CombatMissionsBehavior.Salvage", "We've timed out, retry last action", Logging.white);
                            // We have reached a timeout, revert to ExecutePocketActions (e.g. most likely Activate)
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.SalvageNextPocket) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.SalvageUseGate;
                        }
                    }
                    break;

                case CombatMissionsBehaviorState.Storyline:
                    _storyline.ProcessState();

                    if (_States.CurrentStorylineState == StorylineState.Done)
                    {
                        Logging.Log("CombatMissionsBehavior.Storyline", "We have completed the storyline, returning to base", Logging.white);
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Storyline) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                        break;
                    }
                    break;

                case CombatMissionsBehaviorState.CourierMission:

                    if (_States.CurrentCourierMissionCtrlState == CourierMissionCtrlState.Idle)
                        _States.CurrentCourierMissionCtrlState = CourierMissionCtrlState.GotoPickupLocation;

                    _courierMissionCtrl.ProcessState();

                    if (_States.CurrentCourierMissionCtrlState == CourierMissionCtrlState.Done)
                    {
                        _States.CurrentCourierMissionCtrlState = CourierMissionCtrlState.Idle;
                        Cache.Instance.CourierMission = false;
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.CourierMission) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
                    }
                    break;

                case CombatMissionsBehaviorState.Traveler:
                    Cache.Instance.OpenWrecks = false;
                    List<long> destination = Cache.Instance.DirectEve.Navigation.GetDestinationPath();
                    if (destination == null || destination.Count == 0)
                    {
                        // happens if autopilot isn't set and this questorstate is chosen manually
                        // this also happens when we get to destination (!?)
                        Logging.Log("CombatMissionsBehavior.Traveler", "No destination?", Logging.white);
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Traveler) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
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
                            Logging.Log("CombatMissionsBehavior.Traveler", "Destination: [" + Cache.Instance.DirectEve.Navigation.GetLocation(destination.Last()).Name + "]", Logging.white);
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
                                Logging.Log("CombatMissionsBehavior.Traveler", "an error has occurred", Logging.white);
                                if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Traveler)
                                {
                                    _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                                }
                                return;
                            }
                            else if (Cache.Instance.InSpace)
                            {
                                Logging.Log("CombatMissionsBehavior.Traveler", "Arrived at destination (in space, Questor stopped)", Logging.white);
                                if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Traveler) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error;
                                return;
                            }
                            else
                            {
                                Logging.Log("CombatMissionsBehavior.Traveler", "Arrived at destination", Logging.white);
                                if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Traveler) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                                return;
                            }
                        }
                    }
                    break;

                case CombatMissionsBehaviorState.GotoNearestStation:
                    if (!Cache.Instance.InSpace || Cache.Instance.InWarp) return;
                    var station = Cache.Instance.Stations.OrderBy(x => x.Distance).FirstOrDefault();
                    if (station != null)
                    {
                        if (station.Distance > (int)Distance.WarptoDistance)
                        {
                            Logging.Log("CombatMissionsBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.white);
                            station.WarpToAndDock();
                            Cache.Instance.NextWarpTo = DateTime.Now.AddSeconds((int)Time.WarptoDelay_seconds);
                            if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.GotoNearestStation) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Salvage;
                            break;
                        }
                        else
                        {
                            if (station.Distance < 1900)
                            {
                                if (DateTime.Now > Cache.Instance.NextDockAction)
                                {
                                    Logging.Log("CombatMissionsBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.white);
                                    station.Dock();
                                    Cache.Instance.NextDockAction = DateTime.Now.AddSeconds((int)Time.DockingDelay_seconds);
                                }
                            }
                            else
                            {
                                if (Cache.Instance.NextApproachAction < DateTime.Now && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != station.Id))
                                {
                                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                                    Logging.Log("CombatMissionsBehavior.GotoNearestStation", "Approaching [" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.white);
                                    station.Approach();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.GotoNearestStation) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Error; //should we goto idle here?
                    }
                    break;

                case CombatMissionsBehaviorState.Default:
                    if (_States.CurrentCombatMissionBehaviorState == CombatMissionsBehaviorState.Default) _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.Idle;
                    break;
            }
        }

        private bool GateInSalvage()
        {
            const string target = "Acceleration Gate";

            var targets = Cache.Instance.EntitiesByName(target);
            if (targets == null || !targets.Any())
                return false;
            _gatesPresent = true;
            return true;
        }
    }
}