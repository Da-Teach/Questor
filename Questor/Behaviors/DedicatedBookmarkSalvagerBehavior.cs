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
using System.Globalization;
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
    public class DedicatedBookmarkSalvagerBehavior
    {
        private readonly Arm _arm;
        //private readonly Combat _combat;
        private readonly LocalWatch _localWatch;
        //private readonly Defense _defense;
        //private readonly Drones _drones;

        private DateTime _lastPulse;
        private DateTime _nextSalvageTrip = DateTime.MinValue;
        private readonly Panic _panic;
        private readonly Statistics _statistics;
        private readonly Salvage _salvage;
        private readonly Traveler _traveler;
        private readonly UnloadLoot _unloadLoot;
        public DateTime LastAction;
        private DateTime _nextBookmarksrefresh = DateTime.MinValue;

        //private readonly Random _random;
        public static long AgentID;

        private readonly Stopwatch _watch;
        private DateTime _nextBookmarkRefreshCheck = DateTime.MinValue;

        private double _lastX;
        private double _lastY;
        private double _lastZ;
        private bool _gatesPresent;
        public bool Panicstatereset = false;

        private bool ValidSettings { get; set; }

        public bool CloseQuestorflag = true;

        public string CharacterName { get; set; }

        public List<DirectBookmark> AfterMissionSalvageBookmarks;
        public List<DirectBookmark> BookmarksThatAreNotReadyYet;


        //DateTime _nextAction = DateTime.Now;

        public DedicatedBookmarkSalvagerBehavior()
        {
            _lastPulse = DateTime.MinValue;

            //_random = new Random();
            _salvage = new Salvage();
            _localWatch = new LocalWatch();
            //_combat = new Combat();
            //_drones = new Drones();
            _traveler = new Traveler();
            _unloadLoot = new UnloadLoot();
            _arm = new Arm();
            _panic = new Panic();
            _statistics = new Statistics();
            _watch = new Stopwatch();

            //
            // this is combat mission specific and needs to be generalized
            //
            Settings.Instance.SettingsLoaded += SettingsLoaded;

            _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Idle;
            _States.CurrentArmState = ArmState.Idle;
            //_States.CurrentDroneState = DroneState.Idle;
            _States.CurrentUnloadLootState = UnloadLootState.Idle;
            _States.CurrentTravelerState = TravelerState.Idle;
        }

        public void SettingsLoaded(object sender, EventArgs e)
        {
            ApplySalvageSettings();
            ValidateDedicatedSalvageSettings();
        }

        public void DebugDedicatedBookmarkSalvagerBehaviorStates()
        {
            if (Settings.Instance.DebugStates)
                Logging.Log("DedicateSalvagerBehavior.State is", _States.CurrentDedicatedBookmarkSalvagerBehaviorState.ToString(), Logging.white);
        }

        public void DebugPanicstates()
        {
            if (Settings.Instance.DebugStates)
                Logging.Log("Panic.State is", _States.CurrentPanicState.ToString(), Logging.white);
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

        public void ValidateDedicatedSalvageSettings()
        {
            ValidSettings = true;
            DirectAgent agent = Cache.Instance.DirectEve.GetAgentByName(Cache.Instance.CurrentAgent);

            if (agent == null || !agent.IsValid)
            {
                Logging.Log("Settings", "Unable to locate agent [" + Cache.Instance.CurrentAgent + "]", Logging.white);
                ValidSettings = false;
            }
            else
            {
                //_agentInteraction.AgentId = agent.AgentId;
                //_combatMissionCtrl.AgentId = agent.AgentId;
                _arm.AgentId = agent.AgentId;
                _statistics.AgentID = agent.AgentId;
                AgentID = agent.AgentId;
                _salvage.Ammo = Settings.Instance.Ammo;
                _salvage.MaximumWreckTargets = Settings.Instance.MaximumWreckTargets;
                _salvage.ReserveCargoCapacity = Settings.Instance.ReserveCargoCapacity;
                _salvage.LootEverything = Settings.Instance.LootEverything;
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
            _traveler.ProcessState();
            if (Settings.Instance.DebugStates)
            {
                Logging.Log("Traveler.State is ", _States.CurrentTravelerState.ToString(), Logging.white);
            }
        }

        private void AvoidBumpingThings()
        {
            // always shoot at NPCs while getting un-hung
            //
            if (Cache.Instance.InSpace)
            {
                //_combat.ProcessState();
                //
                // only use drones if warp scrambled as we do not want to leave them behind accidentally
                //
                if (Cache.Instance.TargetedBy.Any(t => t.IsWarpScramblingMe))
                {
                    //_drones.ProcessState();
                }
            }
            else return;
            //
            // if we are "too close" to the bigObject move away... (is orbit the best thing to do here?)
            //
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
                        Logging.Log("DedicatedBookmarkSalvagerBehavior", "" + _States.CurrentDedicatedBookmarkSalvagerBehaviorState +
                                    ": initiating Orbit of [" + thisBigObject.Name +
                                    "] orbiting at [" + Distance.SafeDistancefromStructure + "]", Logging.white);
                        Cache.Instance.NextOrbit = DateTime.Now.AddSeconds((int)Time.OrbitDelay_seconds);
                    }
                    return;
                    //we are still too close, do not continue through the rest until we are not "too close" anymore
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
                    ValidateDedicatedSalvageSettings();
                    LastAction = DateTime.Now;
                }
                return;
            }

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //this local is safe check is useless as their is no localwatch processstate running every tick...
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //If local unsafe go to base and do not start mission again
            if (Settings.Instance.FinishWhenNotSafe && (_States.CurrentDedicatedBookmarkSalvagerBehaviorState != DedicatedBookmarkSalvagerBehaviorState.GotoNearestStation /*|| State!=QuestorState.GotoBase*/))
            {
                //need to remove spam
                if (Cache.Instance.InSpace && !Cache.Instance.LocalSafe(Settings.Instance.LocalBadStandingPilotsToTolerate, Settings.Instance.LocalBadStandingLevelToConsiderBad))
                {
                    var station = Cache.Instance.Stations.OrderBy(x => x.Distance).FirstOrDefault();
                    if (station != null)
                    {
                        Logging.Log("Local not safe", "Station found. Going to nearest station", Logging.white);
                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState != DedicatedBookmarkSalvagerBehaviorState.GotoNearestStation)
                            _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoNearestStation;
                    }
                    else
                    {
                        Logging.Log("Local not safe", "Station not found. Going back to base", Logging.white);
                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState != DedicatedBookmarkSalvagerBehaviorState.GotoBase)
                            _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoBase;
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
                if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState != DedicatedBookmarkSalvagerBehaviorState.GotoBase)
                {
                    _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoBase;
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
                DebugDedicatedBookmarkSalvagerBehaviorStates();
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
            }
            DebugPanicstates();

            //Logging.Log("test");
            switch (_States.CurrentDedicatedBookmarkSalvagerBehaviorState)
            {
                case DedicatedBookmarkSalvagerBehaviorState.Idle:

                    if (Cache.Instance.StopBot)
                        return;

                    if (Cache.Instance.InSpace)
                    {
                        // Questor does not handle in space starts very well, head back to base to try again
                        Logging.Log("DedicatedBookmarkSalvagerBehavior", "Started questor while in space, heading back to base in 15 seconds", Logging.white);
                        LastAction = DateTime.Now;
                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Idle) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.DelayedGotoBase;
                        break;
                    }
                    else
                    {
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
                    //if (Settings.Instance.SalvageStats1Log)
                    //{
                    //    if (!Statistics.Instance.SalvageLoggingCompleted)
                    //    {
                    //        Statistics.WriteSalvagerStatistics();
                    //        break;
                    //    }
                    //}

                    if (Settings.Instance.AutoStart)
                    {
                        //we know we are connected here
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        // Don't start a new action an hour before downtime
                        if (DateTime.UtcNow.Hour == 10)
                            break;

                        // Don't start a new action near downtime
                        if (DateTime.UtcNow.Hour == 11 && DateTime.UtcNow.Minute < 15)
                            break;

                        //Logging.Log("DedicatedBookmarkSalvagerBehavior::: _nextBookmarksrefresh.subtract(datetime.now).totalminutes [" +
                        //            Math.Round(DateTime.Now.Subtract(_nextBookmarkRefreshCheck).TotalMinutes,0) + "]");

                        //Logging.Log("DedicatedBookmarkSalvagerBehavior::: Next Salvage Trip Scheduled in [" +
                        //            _nextSalvageTrip.ToString(CultureInfo.InvariantCulture) + "min]");

                        if (DateTime.Now > _nextBookmarkRefreshCheck)
                        {
                            _nextBookmarkRefreshCheck = DateTime.Now.AddMinutes(1);
                            if (Cache.Instance.InStation && (DateTime.Now > _nextBookmarksrefresh))
                            {
                                _nextBookmarksrefresh = DateTime.Now.AddMinutes(Cache.Instance.RandomNumber(2, 4));
                                Logging.Log("DedicatedBookmarkSalvagerBehavior", "Next Bookmark refresh in [" +
                                               Math.Round(_nextBookmarksrefresh.Subtract(DateTime.Now).TotalMinutes, 0) + "min]", Logging.white);
                                Cache.Instance.DirectEve.RefreshBookmarks();
                            }
                            else
                            {
                                Logging.Log("DedicatedBookmarkSalvagerBehavior", "Next Bookmark refresh in [" +
                                               Math.Round(_nextBookmarksrefresh.Subtract(DateTime.Now).TotalMinutes, 0) + "min]", Logging.white);

                                Logging.Log("DedicatedBookmarkSalvagerBehavior", "Next Salvage Trip Scheduled in [" +
                                               Math.Round(_nextSalvageTrip.Subtract(DateTime.Now).TotalMinutes, 0) + "min]", Logging.white);
                            }
                        }

                        if (DateTime.Now > _nextSalvageTrip)
                        {
                            Logging.Log("DedicatedBookmarkSalvagerBehavior.BeginAftermissionSalvaging", "Starting Another Salvage Trip", Logging.white);
                            LastAction = DateTime.Now;
                            if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Idle) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Start;
                            return;
                        }
                    }
                    else
                    {
                        _States.CurrentQuestorState = QuestorState.Idle;
                    }
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.DelayedGotoBase:
                    if (DateTime.Now.Subtract(LastAction).TotalSeconds < (int)Time.DelayedGotoBase_seconds)
                        break;

                    Logging.Log("DedicatedBookmarkSalvagerBehavior", "Heading back to base", Logging.white);
                    if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.DelayedGotoBase) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoBase;
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.Start:
                    Cache.Instance.OpenWrecks = true;
                    ValidateDedicatedSalvageSettings();
                    if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Start) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.UnloadLoot;
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.LocalWatch:
                    if (Settings.Instance.UseLocalWatch)
                    {
                        Cache.Instance.LastLocalWatchAction = DateTime.Now;
                        if (Cache.Instance.LocalSafe(Settings.Instance.LocalBadStandingPilotsToTolerate, Settings.Instance.LocalBadStandingLevelToConsiderBad))
                        {
                            Logging.Log("DedicatedBookmarkSalvagerBehavior.LocalWatch", "local is clear", Logging.white);
                            if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.LocalWatch) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.BeginAfterMissionSalvaging;
                        }
                        else
                        {
                            Logging.Log("DedicatedBookmarkSalvagerBehavior.LocalWatch", "Bad standings pilots in local: We will stay 5 minutes in the station and then we will check if it is clear again", Logging.white);
                            if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.LocalWatch)
                            {
                                _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.WaitingforBadGuytoGoAway;
                            }
                            Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                            Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        }
                    }
                    else
                    {
                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.LocalWatch) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.BeginAfterMissionSalvaging;
                    }
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.WaitingforBadGuytoGoAway:
                    Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                    Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                    if (DateTime.Now.Subtract(Cache.Instance.LastLocalWatchAction).TotalMinutes < (int)Time.WaitforBadGuytoGoAway_minutes)
                        break;
                    if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.WaitingforBadGuytoGoAway) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.LocalWatch;
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.GotoBase:

                    if (Settings.Instance.DebugGotobase) Logging.Log("DedicatedBookmarkSalvagerBehavior", "GotoBase: AvoidBumpingThings()", Logging.white);
                    AvoidBumpingThings();
                    if (Settings.Instance.DebugGotobase) Logging.Log("DedicatedBookmarkSalvagerBehavior", "GotoBase: TravelToAgentsStation()", Logging.white);
                    TravelToAgentsStation();
                    if (_States.CurrentTravelerState == TravelerState.AtDestination) // || DateTime.Now.Subtract(Cache.Instance.EnteredCloseQuestor_DateTime).TotalMinutes > 10)
                    {
                        if (Settings.Instance.DebugGotobase) Logging.Log("DedicatedBookmarkSalvagerBehavior", "GotoBase: We are at destination", Logging.white);
                        Cache.Instance.GotoBaseNow = false; //we are there - turn off the 'forced' gotobase
                        Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentID);
                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.GotoBase) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.UnloadLoot;
                        _traveler.Destination = null;
                    }
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.UnloadLoot:
                    if (_States.CurrentUnloadLootState == UnloadLootState.Idle)
                    {
                        Logging.Log("DedicatedBookmarkSalvagerBehavior", "UnloadLoot: Begin", Logging.white);
                        _States.CurrentUnloadLootState = UnloadLootState.Begin;
                    }

                    _unloadLoot.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("DedicatedBookmarkSalvagerBehavior", "UnloadLoot.State = " + _States.CurrentUnloadLootState, Logging.white);

                    if (_States.CurrentUnloadLootState == UnloadLootState.Done)
                    {
                        Cache.Instance.LootAlreadyUnloaded = true;
                        _States.CurrentUnloadLootState = UnloadLootState.Idle;

                        AfterMissionSalvageBookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").Where(e => e.CreatedOn > DateTime.Now.AddMinutes(-Settings.Instance.AgeofBookmarksForSalvageBehavior)).ToList();
                        if (AfterMissionSalvageBookmarks.Count == 0)
                        {
                            BookmarksThatAreNotReadyYet = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");
                            if (BookmarksThatAreNotReadyYet.Any())
                            {
                                Logging.Log("DedicatedBookmarkSalvagerBehavior", "Unloadloot: There are [" + BookmarksThatAreNotReadyYet.Count() + "] Salvage Bookmarks that have not yet aged [" + Settings.Instance.AgeofBookmarksForSalvageBehavior + "] min.", Logging.white);
                            }
                            Logging.Log("DedicatedBookmarkSalvagerBehavior", "Unloadloot: Character mode is BookmarkSalvager and no bookmarks are ready to salvage.", Logging.white);
                            //We just need a NextSalvagerSession timestamp to key off of here to add the delay
                            _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Idle;
                            _States.CurrentQuestorState = QuestorState.Idle;

                            Statistics.Instance.FinishedSalvaging = DateTime.Now;
                            return;
                        }
                        else //There is at least 1 salvage bookmark
                        {
                            Logging.Log("DedicatedBookmarkSalvagerBehavior", "Unloadloot: There are [ " + AfterMissionSalvageBookmarks.Count + " ] more salvage bookmarks left to process", Logging.white);
                            Logging.Log("DedicatedBookmarkSalvagerBehavior", "Unloadloot: CharacterMode: [" + Settings.Instance.CharacterMode + "], AfterMissionSalvaging: [" + Settings.Instance.AfterMissionSalvaging + "], DedicatedBookmarkSalvagerBehaviorState: [" + _States.CurrentDedicatedBookmarkSalvagerBehaviorState + "]", Logging.white);
                            _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.BeginAfterMissionSalvaging;
                            Statistics.Instance.StartedSalvaging = DateTime.Now;
                        }
                    }
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.BeginAfterMissionSalvaging:
                    AfterMissionSalvageBookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").Where(e => e.CreatedOn > DateTime.Now.AddMinutes(-Settings.Instance.AgeofBookmarksForSalvageBehavior)).ToList();
                    //AgedAfterMissionSalvageBookmarks = AfterMissionSalvageBookmarks.Where(e => e.CreatedOn > DateTime.Now.AddMinutes(-Settings.Instance.AgeofBookmarksForSalvageBehavior)).ToList();
                    Logging.Log("DedicatedBookmarkSalvagebehavior","Found [" + AfterMissionSalvageBookmarks.Count + "] salvage bookmarks ready to process.", Logging.white);
                    Statistics.Instance.StartedSalvaging = DateTime.Now; //this will be reset for each "run" between the station and the field if using <unloadLootAtStation>true</unloadLootAtStation>
                    _nextSalvageTrip = DateTime.Now.AddMinutes((int)Time.DelayBetweenSalvagingSessions_minutes);
                    //we know we are connected here
                    Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                    Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                    Cache.Instance.OpenWrecks = true;
                    if (Cache.Instance.InStation)
                    {
                        if (_States.CurrentArmState == ArmState.Idle)
                            _States.CurrentArmState = ArmState.SwitchToSalvageShip;

                        _arm.ProcessState();
                    }
                    if (_States.CurrentArmState == ArmState.Done || Cache.Instance.InSpace)
                    {
                        DirectBookmark bookmark = AfterMissionSalvageBookmarks.OrderBy(b => b.CreatedOn).FirstOrDefault();
                        _States.CurrentArmState = ArmState.Idle;
                        if (Settings.Instance.FirstSalvageBookmarksInSystem)
                        {
                            Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvager", "Salvaging at first bookmark from system", Logging.white);
                            bookmark = AfterMissionSalvageBookmarks.OrderBy(b => b.CreatedOn).FirstOrDefault(c => c.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId);
                        }
                        else Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvager", "Salvaging at first oldest bookmarks", Logging.white);
                        if (bookmark == null)
                        {
                            bookmark = AfterMissionSalvageBookmarks.OrderBy(b => b.CreatedOn).FirstOrDefault();
                            if (bookmark == null)
                            {
                                _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoBase;
                                return;
                            }
                        }

                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.BeginAfterMissionSalvaging)
                        {
                            _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoSalvageBookmark;
                            _nextSalvageTrip = DateTime.Now.AddMinutes((int)Time.DelayBetweenSalvagingSessions_minutes);
                            //we know we are connected here
                            Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                            Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        }
                        _traveler.Destination = new BookmarkDestination(bookmark);
                        return;
                    }
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.GotoSalvageBookmark:
                    _traveler.ProcessState();
                    string target = "Acceleration Gate";
                    Cache.Instance.EntitiesByName(target);
                    if (_States.CurrentTravelerState == TravelerState.AtDestination || GateInSalvage())
                    {
                        //we know we are connected here
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.GotoSalvageBookmark) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Salvage;
                        _traveler.Destination = null;
                        return;
                    }

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Traveler.State is ", _States.CurrentTravelerState.ToString(), Logging.white);
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.Salvage:
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
                        Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "could not be completed because of NPCs left in the mission: deleting salvage bookmarks", Logging.white);
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
                            Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "could not be completed because of NPCs left in the mission: salvage bookmarks deleted", Logging.white);
                            Cache.Instance.SalvageAll = false;
                            if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Salvage)
                            {
                                Statistics.Instance.FinishedSalvaging = DateTime.Now;
                                _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoBase;
                            }
                            return;
                        }
                    }
                    else
                    {
                        if (!Cache.Instance.OpenCargoHold("DedicatedSalvageBehavior: Salvage")) break;

                        if (Settings.Instance.UnloadLootAtStation && salvageCargo.Window.IsReady && (salvageCargo.Capacity - salvageCargo.UsedCapacity) < 100)
                        {
                            Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "We are full, go to base to unload", Logging.white);
                            if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Salvage)
                            {
                                _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoBase;
                            }
                            break;
                        }

                        if (!Cache.Instance.UnlootedContainers.Any())
                        {
                            Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "Finished salvaging the room: removing salvage bookmarks that we have already processed.", Logging.white);
                            //
                            // this removes bookmarks that:
                            // a) have the configured bookmark prefix
                            // b) are in local with us
                            // c) on grid with us (250k iirc, you can see this definition in distances.cs)

                            bool gatesInRoom = GateInSalvage();
                            var bookmarksinlocal = new List<DirectBookmark>(Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").
                                                       Where(b => b.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId).
                                                       OrderBy(b => b.CreatedOn));
                            while (true)
                            {
                                // Remove all bookmarks from address book
                                var bookmark = bookmarksinlocal.FirstOrDefault(b => Cache.Instance.DistanceFromMe(b.X ?? 0, b.Y ?? 0, b.Z ?? 0) < (int)Distance.OnGridWithMe);
                                if (!gatesInRoom && _gatesPresent) // if there were gates, but we've gone through them all, delete all bookmarks
                                    bookmark = bookmarksinlocal.FirstOrDefault();
                                else if (gatesInRoom)
                                    break;
                                if (bookmark == null)
                                    break;

                                bookmark.Delete();
                                bookmarksinlocal.Remove(bookmark);
                                Cache.Instance.NextRemoveBookmarkAction = DateTime.Now.AddSeconds((int)Time.RemoveBookmarkDelay_seconds);
                                return;
                            }

                            if (bookmarksinlocal.Count == 0 && !gatesInRoom)
                            {
                                Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "We have salvaged all bookmarks in system, check other systems and if needed gotobase", Logging.white);
                                Cache.Instance.SalvageAll = false;
                                if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Salvage)
                                {
                                    Statistics.Instance.FinishedSalvaging = DateTime.Now;
                                    _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.BeginAfterMissionSalvaging;
                                }
                                return;
                            }
                            else
                            {
                                if (!gatesInRoom)
                                {
                                    Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "Go to the next salvage bookmark", Logging.white);
                                    var bookmark = bookmarksinlocal.FirstOrDefault(c => c.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId) ?? bookmarksinlocal.FirstOrDefault();
                                    if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Salvage)
                                    {
                                        _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoSalvageBookmark;
                                    }
                                    _traveler.Destination = new BookmarkDestination(bookmark);
                                }
                                else if (Settings.Instance.UseGatesInSalvage)
                                {
                                    Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", " Acceleration gate found - moving to next pocket", Logging.white);
                                    if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Salvage)
                                    {
                                        _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.SalvageUseGate;
                                    }
                                }
                                else
                                {
                                    Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "Acceleration gate found, useGatesInSalvage set to false - Returning to base", Logging.white);
                                    if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Salvage)
                                    {
                                        Statistics.Instance.FinishedSalvaging = DateTime.Now;
                                        _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoBase;
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

                case DedicatedBookmarkSalvagerBehaviorState.SalvageUseGate:
                    Cache.Instance.OpenWrecks = true;

                    target = "Acceleration Gate";
                    IEnumerable<EntityCache> targets = Cache.Instance.EntitiesByName(target).ToList();
                    if (targets == null || !targets.Any())
                    {
                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.SalvageUseGate)
                        {
                            _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.GotoSalvageBookmark;
                        }
                        return;
                    }

                    _lastX = Cache.Instance.DirectEve.ActiveShip.Entity.X;
                    _lastY = Cache.Instance.DirectEve.ActiveShip.Entity.Y;
                    _lastZ = Cache.Instance.DirectEve.ActiveShip.Entity.Z;

                    EntityCache closest = targets.OrderBy(t => t.Distance).First();
                    if (closest.Distance < (int)Distance.DecloakRange)
                    {
                        Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "Acceleration gate found - GroupID=" + closest.GroupId, Logging.white);

                        // Activate it and move to the next Pocket
                        closest.Activate();

                        // Do not change actions, if NextPocket gets a timeout (>2 mins) then it reverts to the last action
                        Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "Activate [" + closest.Name + "] and change States.CurrentDedicatedBookmarkSalvagerBehaviorState to 'NextPocket'", Logging.white);

                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.SalvageUseGate) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.SalvageNextPocket;
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
                                Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "Approaching target [" + closest.Name + "][ID: " + closest.Id + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.white);
                                closest.Approach();
                            }
                        }
                        else
                        {
                            // Probably never happens
                            if (DateTime.Now > Cache.Instance.NextWarpTo)
                            {
                                Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "Warping to [" + closest.Name + "] which is [" + Math.Round(closest.Distance / 1000, 0) + "k away]", Logging.white);
                                closest.WarpTo();
                                Cache.Instance.NextWarpTo = DateTime.Now.AddSeconds((int)Time.WarptoDelay_seconds);
                            }
                        }
                    }
                    _lastPulse = DateTime.Now.AddSeconds(10);
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.SalvageNextPocket:
                    Cache.Instance.OpenWrecks = true;
                    double distance = Cache.Instance.DistanceFromMe(_lastX, _lastY, _lastZ);
                    if (distance > (int)Distance.NextPocketDistance)
                    {
                        //we know we are connected here...
                        Cache.Instance.LastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "We've moved to the next Pocket [" + Math.Round(distance / 1000, 0) + "k away]", Logging.white);

                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.SalvageNextPocket) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Salvage;
                        return;
                    }
                    else //we have not moved to the next pocket quite yet
                    {
                        if (DateTime.Now.Subtract(_lastPulse).TotalMinutes > 2)
                        {
                            Logging.Log("DedicatedBookmarkSalvagerBehavior.Salvage", "We've timed out, retry last action", Logging.white);
                            // We have reached a timeout, revert to ExecutePocketActions (e.g. most likely Activate)
                            if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.SalvageNextPocket) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.SalvageUseGate;
                        }
                    }
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.Traveler:
                    Cache.Instance.OpenWrecks = false;
                    List<long> destination = Cache.Instance.DirectEve.Navigation.GetDestinationPath();
                    if (destination == null || destination.Count == 0)
                    {
                        // happens if autopilot isn't set and this questorstate is chosen manually
                        // this also happens when we get to destination (!?)
                        Logging.Log("DedicatedBookmarkSalvagerBehavior", "Traveler: No destination?", Logging.white);
                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Traveler) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Error;
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
                            Logging.Log("DedicatedBookmarkSalvagerBehavior.Traveler", "Destination: [" + Cache.Instance.DirectEve.Navigation.GetLocation(destination.Last()).Name + "]", Logging.white);
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
                            if (Cache.Instance.InSpace)
                            {
                                Logging.Log("DedicatedBookmarkSalvagerBehavior.Traveler", "Arrived at destination (in space, Questor stopped)", Logging.white);
                                if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Traveler) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Error;
                                return;
                            }
                            else
                            {
                                Logging.Log("DedicatedBookmarkSalvagerBehavior.Traveler", "Arrived at destination", Logging.white);
                                if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Traveler)
                                {
                                    _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Idle;
                                    _States.CurrentQuestorState = QuestorState.Idle;
                                }
                                return;
                            }
                        }
                    }
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.GotoNearestStation:
                    if (!Cache.Instance.InSpace || Cache.Instance.InWarp) return;
                    var station = Cache.Instance.Stations.OrderBy(x => x.Distance).FirstOrDefault();
                    if (station != null)
                    {
                        if (station.Distance > (int)Distance.WarptoDistance)
                        {
                            Logging.Log("DedicatedBookmarkSalvagerBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.white);
                            station.WarpToAndDock();
                            Cache.Instance.NextWarpTo = DateTime.Now.AddSeconds((int)Time.WarptoDelay_seconds);
                            if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.GotoNearestStation) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Salvage;
                            break;
                        }
                        else
                        {
                            if (station.Distance < 1900)
                            {
                                if (DateTime.Now > Cache.Instance.NextDockAction)
                                {
                                    Logging.Log("DedicatedBookmarkSalvagerBehavior.GotoNearestStation", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.white);
                                    station.Dock();
                                    Cache.Instance.NextDockAction = DateTime.Now.AddSeconds((int)Time.DockingDelay_seconds);
                                }
                            }
                            else
                            {
                                if (Cache.Instance.NextApproachAction < DateTime.Now && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != station.Id))
                                {
                                    Cache.Instance.NextApproachAction = DateTime.Now.AddSeconds((int)Time.ApproachDelay_seconds);
                                    Logging.Log("DedicatedBookmarkSalvagerBehavior.GotoNearestStation Approaching", "[" + station.Name + "] which is [" + Math.Round(station.Distance / 1000, 0) + "k away]", Logging.white);
                                    station.Approach();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.GotoNearestStation) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Error; //should we goto idle here?
                    }
                    break;

                case DedicatedBookmarkSalvagerBehaviorState.Default:
                    if (_States.CurrentDedicatedBookmarkSalvagerBehaviorState == DedicatedBookmarkSalvagerBehaviorState.Default) _States.CurrentDedicatedBookmarkSalvagerBehaviorState = DedicatedBookmarkSalvagerBehaviorState.Idle;
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