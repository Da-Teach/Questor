// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace Questor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using DirectEve;
    using global::Questor.Modules;
    using global::Questor.Storylines;
    using LavishScriptAPI;
    using System.Windows.Forms;

    public class Questor
    {
        private frmMain m_Parent;
        private AgentInteraction _agentInteraction;
        private Arm _arm;
        private SwitchShip _switch;
        private Combat _combat;
        private CourierMission _courier;
        private LocalWatch _localwatch;
        private ScanInteraction _scanInteraction;
        private Defense _defense;
        private DirectEve _directEve;
        private Drones _drones;

        private DateTime _lastPulse;
        private MissionController _missionController;
        private Panic _panic;
        private Storyline _storyline;
        private Cleanup _cleanup;
        private Statistics _statistics;

        //private Scoop _scoop;
        private Salvage _salvage;
        private Traveler _traveler;
        private UnloadLoot _unloadLoot;

        private DateTime _lastAction;
        private DateTime _lastOrbit;
        private DateTime _lastLocalWatchAction;
        private DateTime _lastWalletCheck;
        private DateTime _lastupdateofSessionRunningTime;
        //private DateTime _lastCheckWindowsAction;
        private DateTime _lastTimeCheckAction;
        private DateTime _lastWarpTo;
        private DateTime _questorStarted;
        private Random _random;
        private int _randomDelay;

        private double _lastX;
        private double _lastY;
        private double _lastZ;
        private bool _GatesPresent;
        private bool first_start = true;
        DateTime nextAction = DateTime.Now;

        public Questor(frmMain form1)
        {
            m_Parent = form1;
            _lastPulse = DateTime.MinValue;

            _random = new Random();

            //_debugmodule = new DebugModule();

            //_scoop = new Scoop();
            _salvage = new Salvage();
            _defense = new Defense();
            _localwatch = new LocalWatch();
            _scanInteraction = new ScanInteraction();
            _combat = new Combat();
            _traveler = new Traveler();
            _unloadLoot = new UnloadLoot();
            _agentInteraction = new AgentInteraction();
            _arm = new Arm();
            _courier = new CourierMission();
            _switch = new SwitchShip();
            _missionController = new MissionController();
            _drones = new Drones();
            _panic = new Panic();
            _storyline = new Storyline();
            _cleanup = new Cleanup();
            _statistics = new Statistics();

            Settings.Instance.SettingsLoaded += SettingsLoaded;

            // State fixed on ExecuteMission
            State = QuestorState.Idle;

            _directEve = new DirectEve();
            Cache.Instance.DirectEve = _directEve;

            Cache.Instance.StopTimeSpecified = Program.stopTimeSpecified;
            Cache.Instance.MaxRuntime = Program.maxRuntime;
            Cache.Instance.StopTime = Program._stopTime;
            _questorStarted = DateTime.Now;

            _directEve.OnFrame += OnFrame;
        }

        public QuestorState State { get; set; }

        public bool AutoStart { get; set; }
        public bool Paused { get; set; }
        public bool Disable3D { get; set; }
        public bool ValidSettings { get; set; }
        public bool ExitWhenIdle { get; set; }
        //public bool LogPathsNotSetupYet = true;
        public bool CloseQuestorCMDUplink = true;
        public bool CloseQuestorflag = true;
        public DateTime _CloseQuestorDelay { get; set; }
        private bool CloseQuestor10SecWarningDone = false;
        private bool MissionLoggingCompleted = false;

        public string CharacterName { get; set; }

        // Statistics information
        //public DateTime StartedMission { get; set; }
        //public DateTime FinishedMission { get; set; }
        //public DateTime StartedSalvaging { get; set; }
        //public DateTime FinishedSalvaging { get; set; }

        //public string Mission { get; set; }
        //public double LootValue { get; set; }
        //public int LoyaltyPoints { get; set; }
        //public int LostDrones { get; set; }
        //public double AmmoValue { get; set; }
        //public double AmmoConsumption { get; set; }

        public void SettingsLoaded(object sender, EventArgs e)
        {
            ApplySettings();
            ValidateSettings();

            AutoStart = Settings.Instance.AutoStart;
            Disable3D = Settings.Instance.Disable3D;
        }

        public void ValidateSettings()
        {
            ValidSettings = true;
            if (Settings.Instance.Ammo.Select(a => a.DamageType).Distinct().Count() != 4)
            {
                if (!Settings.Instance.Ammo.Any(a => a.DamageType == DamageType.EM))
                    Logging.Log("Settings: Missing EM damage type!");
                if (!Settings.Instance.Ammo.Any(a => a.DamageType == DamageType.Thermal))
                    Logging.Log("Settings: Missing Thermal damage type!");
                if (!Settings.Instance.Ammo.Any(a => a.DamageType == DamageType.Kinetic))
                    Logging.Log("Settings: Missing Kinetic damage type!");
                if (!Settings.Instance.Ammo.Any(a => a.DamageType == DamageType.Explosive))
                    Logging.Log("Settings: Missing Explosive damage type!");

                Logging.Log("Settings: You are required to specify all 4 damage types in your settings xml file!");
                ValidSettings = false;
            }

            var agent = Cache.Instance.DirectEve.GetAgentByName(Cache.Instance.CurrentAgent);

            if (agent == null || !agent.IsValid)
            {
                Logging.Log("Settings: Unable to locate agent [" + Cache.Instance.CurrentAgent + "]");
                ValidSettings = false;
            }
            else
            {
                _agentInteraction.AgentId = agent.AgentId;
                _missionController.AgentId = agent.AgentId;
                _arm.AgentId = agent.AgentId;
            }
        }

        public void ApplySettings()
        {
            _salvage.Ammo = Settings.Instance.Ammo;
            _salvage.MaximumWreckTargets = Settings.Instance.MaximumWreckTargets;
            _salvage.ReserveCargoCapacity = Settings.Instance.ReserveCargoCapacity;
            _salvage.LootEverything = Settings.Instance.LootEverything;
        }

        private void OnFrame(object sender, EventArgs e)
        {
            var watch = new Stopwatch();
            // Only pulse state changes every 1.5s
            if (DateTime.Now.Subtract(_lastPulse).TotalMilliseconds < (int)Time.QuestorPulse_milliseconds) //default: 1500ms
                return;
            _lastPulse = DateTime.Now;

            // Session is not ready yet, do not continue
            if (!Cache.Instance.DirectEve.Session.IsReady)
                return;


            // We are not in space or station, don't do shit yet!
            if (!Cache.Instance.InSpace && !Cache.Instance.InStation)
                return;

            // New frame, invalidate old cache
            Cache.Instance.InvalidateCache();

            // Update settings (settings only load if character name changed)
            Settings.Instance.LoadSettings();
            CharacterName = Cache.Instance.DirectEve.Me.Name;

            // Check 3D rendering
            if (Cache.Instance.DirectEve.Session.IsInSpace && Cache.Instance.DirectEve.Rendering3D != !Disable3D)
                Cache.Instance.DirectEve.Rendering3D = !Disable3D;

            // Invalid settings, quit while we're ahead
            if (!ValidSettings)
            {
                if (DateTime.Now.Subtract(_lastAction).TotalSeconds < (int)Time.ValidateSettings_seconds) //default is a 15 second interval
                {
                    ValidateSettings();
                    _lastAction = DateTime.Now;
                }
                return;
            }
            
            // Start _cleanup.ProcessState
            // Description: Closes Windows, and eventually other things considered 'cleanup' useful to more than just Questor(Missions) but also Anomolies, Mining, etc
            //
            watch.Reset();
            watch.Start();
            _cleanup.ProcessState();
            watch.Stop();
            if (Settings.Instance.DebugPerformance)
                Logging.Log("Cleanup.ProcessState took " + watch.ElapsedMilliseconds + "ms");
            // Done
            // Cleanup State: ProcessState

            if (DateTime.Now.Subtract(_lastupdateofSessionRunningTime).TotalSeconds < (int)Time.SessionRunningTimeUpdate_seconds)
            {
                Cache.Instance.SessionRunningTime = (int)DateTime.Now.Subtract(_questorStarted).TotalMinutes;
                _lastupdateofSessionRunningTime = DateTime.Now;
            }

            if ((DateTime.Now.Subtract(_questorStarted).TotalSeconds > 10) && (DateTime.Now.Subtract(_questorStarted).TotalSeconds < 60))
            {
                if (Cache.Instance.QuestorJustStarted)
                {
                    Cache.Instance.QuestorJustStarted = false;
                    Cache.Instance.SessionState = "Starting Up";

                    // get the current process
                    Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

                    // get the physical mem usage
                    Cache.Instance.totalMegaBytesOfMemoryUsed = ((currentProcess.WorkingSet64 / 1024) / 1024);
                    Logging.Log("Questor: EVE instance: totalMegaBytesOfMemoryUsed - " + Cache.Instance.totalMegaBytesOfMemoryUsed + " MB");
                    Cache.Instance.SessionIskGenerated = 0;
                    Cache.Instance.SessionLootGenerated = 0;
                    Cache.Instance.SessionLPGenerated = 0;
                    
                    if (Settings.Instance.SessionsLog)
                    {
                        if (Cache.Instance.DirectEve.Me.Wealth != 0 || Cache.Instance.DirectEve.Me.Wealth != -2147483648) // this hopefully resolves having negative maxint in the session logs occassionally
                        {
                            //
                            // prepare the Questor Session Log - keeps track of starts, restarts and exits, and hopefully the reasons
                            //
                            // Get the path
                            if (!Directory.Exists(Settings.Instance.SessionsLogPath)) 
                                Directory.CreateDirectory(Settings.Instance.SessionsLogPath);

                            // Write the header
                            if (!File.Exists(Settings.Instance.SessionsLogFile))
                                File.AppendAllText(Settings.Instance.SessionsLogFile, "Date;RunningTime;SessionState;LastMission;WalletBalance;MemoryUsage;Reason;IskGenerated;LootGenerated;LPGenerated;Isk/Hr;Loot/Hr;LP/HR;Total/HR;\r\n");

                            // Build the line
                            var line = DateTime.Now + ";";                           //Date
                            line += "0" + ";";                                       //RunningTime
                            line += Cache.Instance.SessionState + ";";               //SessionState
                            line += "" + ";";                                        //LastMission
                            line += Cache.Instance.DirectEve.Me.Wealth + ";";        //WalletBalance
                            line += Cache.Instance.totalMegaBytesOfMemoryUsed + ";"; //MemoryUsage
                            line += "Starting" + ";";                                //Reason
                            line += ";";                                             //IskGenerated
                            line += ";";                                             //LootGenerated
                            line += ";";                                             //LPGenerated
                            line += ";";                                             //Isk/Hr
                            line += ";";                                             //Loot/Hr
                            line += ";";                                             //LP/HR
                            line += ";\r\n";                                         //Total/HR

                            // The mission is finished
                            File.AppendAllText(Settings.Instance.SessionsLogFile, line);

                            Cache.Instance.SessionState = "";
                            Logging.Log("Questor: Writing session data to [ " + Settings.Instance.SessionsLogFile);
                        }
                    }
                }
            }

            if (!Paused)
            {
                if (DateTime.Now.Subtract(_lastWalletCheck).TotalMinutes > (int)Time.WalletCheck_minutes)
                {
                    _lastWalletCheck = DateTime.Now;
                    //Logging.Log("[Questor] Wallet Balance Debug Info: lastknowngoodconnectedtime = " + Settings.Instance.lastKnownGoodConnectedTime);
                    //Logging.Log("[Questor] Wallet Balance Debug Info: DateTime.Now - lastknowngoodconnectedtime = " + DateTime.Now.Subtract(Settings.Instance.lastKnownGoodConnectedTime).TotalSeconds);
                    if (Math.Round(DateTime.Now.Subtract(Cache.Instance.lastKnownGoodConnectedTime).TotalMinutes) > 1)
                    {
                        Logging.Log(String.Format("Questor: Wallet Balance Has Not Changed in [ {0} ] minutes.", Math.Round(DateTime.Now.Subtract(Cache.Instance.lastKnownGoodConnectedTime).TotalMinutes, 0)));
                    }

                    //Settings.Instance.walletbalancechangelogoffdelay = 2;  //used for debugging purposes
                    //Logging.Log("Cache.Instance.lastKnownGoodConnectedTime is currently: " + Cache.Instance.lastKnownGoodConnectedTime);
                    if (Math.Round(DateTime.Now.Subtract(Cache.Instance.lastKnownGoodConnectedTime).TotalMinutes) < Settings.Instance.walletbalancechangelogoffdelay)
                    {
                        if (State == QuestorState.Salvage)
                        {
                            Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                            Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        }
                        else
                        {
                            if (Cache.Instance.MyWalletBalance != Cache.Instance.DirectEve.Me.Wealth)
                            {
                                Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                                Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                            }
                        }
                    }
                    else
                    {

                        Logging.Log(String.Format("Questor: Wallet Balance Has Not Changed in [ {0} ] minutes. Switching to QuestorState.CloseQuestor", Math.Round(DateTime.Now.Subtract(Cache.Instance.lastKnownGoodConnectedTime).TotalMinutes, 0)));
                        Cache.Instance.ReasonToStopQuestor = "Wallet Balance did not change for over " + Settings.Instance.walletbalancechangelogoffdelay + "min";

                        if (Settings.Instance.walletbalancechangelogoffdelayLogofforExit == "logoff")
                        {
                            Logging.Log("Questor: walletbalancechangelogoffdelayLogofforExit is set to: " + Settings.Instance.walletbalancechangelogoffdelayLogofforExit);
                            Cache.Instance.CloseQuestorCMDLogoff = true;
                            Cache.Instance.CloseQuestorCMDExitGame = false;
                            Cache.Instance.SessionState = "LoggingOff";
                        }
                        if (Settings.Instance.walletbalancechangelogoffdelayLogofforExit == "exit")
                        {
                            Logging.Log("Questor: walletbalancechangelogoffdelayLogofforExit is set to: " + Settings.Instance.walletbalancechangelogoffdelayLogofforExit);
                            Cache.Instance.CloseQuestorCMDLogoff = false;
                            Cache.Instance.CloseQuestorCMDExitGame = true;
                            Cache.Instance.SessionState = "Exiting";
                        }
                        State = QuestorState.CloseQuestor;
                        return;
                    }
                }
            }

            // We always check our defense state if we're in space, regardless of questor state
            // We also always check panic
            if (Cache.Instance.InSpace)
            {
                watch.Reset();
                watch.Start();
                if (!Cache.Instance.DoNotBreakInvul)
                    _defense.ProcessState();
                watch.Stop();

                if (Settings.Instance.DebugPerformance)
                    Logging.Log("Defense.ProcessState took " + watch.ElapsedMilliseconds + "ms");
            }

            if (Paused)
            {
                Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                return;
            }

            if (Cache.Instance.SessionState == "Quitting")
            {
                State = QuestorState.CloseQuestor;
            }
            // Defense is more important then pause, rest (even panic) isnt!
            // Panic always runs, not just in space
            watch.Reset();
            watch.Start();
            _panic.InMission = State == QuestorState.ExecuteMission;
            if (State == QuestorState.Storyline && _storyline.State == StorylineState.ExecuteMission)
            {
                _panic.InMission |= _storyline.StorylineHandler is GenericCombatStoryline && (_storyline.StorylineHandler as GenericCombatStoryline).State == GenericCombatStorylineState.ExecuteMission;
            }
            _panic.ProcessState();
            watch.Stop();

            if (Settings.Instance.DebugPerformance)
                Logging.Log("Panic.ProcessState took " + watch.ElapsedMilliseconds + "ms");

            if (_panic.State == PanicState.Panic || _panic.State == PanicState.Panicking)
            {
                // If Panic is in panic state, questor is in panic state :)
                State = State == QuestorState.Storyline ? QuestorState.StorylinePanic : QuestorState.Panic;

                if (Settings.Instance.DebugStates)
                    Logging.Log("State = " + State);
            }
            else if (_panic.State == PanicState.Resume)
            {
                // Reset panic state
                _panic.State = PanicState.Normal;

                // Ugly storyline resume hack
                if (State == QuestorState.StorylinePanic)
                {
                    State = QuestorState.Storyline;

                    if (_storyline.StorylineHandler is GenericCombatStoryline)
                        (_storyline.StorylineHandler as GenericCombatStoryline).State = GenericCombatStorylineState.GotoMission;
                }
                else
                {
                    // Head back to the mission
                    _traveler.State = TravelerState.Idle;
                    State = QuestorState.GotoMission;
                }

                if (Settings.Instance.DebugStates)
                    Logging.Log("State = " + State);
            }

            if (Settings.Instance.DebugStates)
                Logging.Log("Panic.State = " + _panic.State);

            // When in warp there's nothing we can do, so ignore everything
            if (Cache.Instance.InWarp)
                return;

            DirectAgentMission mission;
            switch (State)
            {
                case QuestorState.Idle:

                    // Every 5 min of idle check and make sure we arent supposed to stop... 
                    if (Math.Round(DateTime.Now.Subtract(_lastTimeCheckAction).TotalMinutes) > 5)
                    {
                        _lastTimeCheckAction = DateTime.Now;
                        if (DateTime.Now.Subtract(_questorStarted).Minutes > Cache.Instance.MaxRuntime)
                        {
                            // quit questor
                            Logging.Log("Questor: Maximum runtime exceeded.  Quiting...");
                            Cache.Instance.ReasonToStopQuestor = "Maximum runtime specified and reached.";
                            AutoStart = false;
                            Cache.Instance.CloseQuestorCMDLogoff = false;
                            Cache.Instance.CloseQuestorCMDExitGame = true;
                            Cache.Instance.SessionState = "Exiting";
                            State = QuestorState.CloseQuestor;
                            return;
                        }
                        if (Cache.Instance.StopTimeSpecified)
                        {
                             if (DateTime.Now >= Cache.Instance.StopTime)
                            {
                                Logging.Log("Questor: Time to stop.  Quitting game.");
                                Cache.Instance.ReasonToStopQuestor = "StopTimeSpecified and reached.";
                                AutoStart = false;
                                Cache.Instance.CloseQuestorCMDLogoff = false;
                                Cache.Instance.CloseQuestorCMDExitGame = true;
                                Cache.Instance.SessionState = "Exiting";
                                State = QuestorState.CloseQuestor;
                                return;
                            }
                        }
                        if (ExitWhenIdle && !AutoStart)
                        {
                            
                            //LavishScript.ExecuteCommand("exit");
                            Cache.Instance.ReasonToStopQuestor = "Settings: ExitWhenIdle is true, and we are idle... exiting";
                            Logging.Log(Cache.Instance.ReasonToStopQuestor);
                            AutoStart = false;
                            Cache.Instance.CloseQuestorCMDLogoff = false;
                            Cache.Instance.CloseQuestorCMDExitGame = true;
                            Cache.Instance.SessionState = "Exiting";
                            State = QuestorState.CloseQuestor;
                            return;
                        }
                    }

                    if (Cache.Instance.InSpace)
                    {
                        // Questor doesnt handle inspace-starts very well, head back to base to try again
                        Logging.Log("Questor: Started questor while in space, heading back to base in 15 seconds");
                        _lastAction = DateTime.Now;
                        State = QuestorState.DelayedGotoBase;
                        break;
                    }

                    // only attempt to write the mission statistics logs if one of the mission stats logs is enabled in settings
                    //if (Settings.Instance.MissionStats1Log || Settings.Instance.MissionStats3Log || Settings.Instance.MissionStats3Log)
                    //{   
                        //
                        // Start _statistics.ProcessState
                        // Description: Closes Windows, and eventually other things considered 'cleanup' useful to more than just Questor(Missions) but also Anomalies, Mining, etc
                        //
                        watch.Reset();
                        watch.Start();
                        _statistics.State = StatisticsState.MissionLog;
                        if (Settings.Instance.DebugStates)
                            Logging.Log("statistics.State = " + _statistics.State);
                        _statistics.ProcessState(); 
                        watch.Stop();
                        if (Settings.Instance.DebugPerformance)
                            Logging.Log("statistics.ProcessState took " + watch.ElapsedMilliseconds + "ms");
                        if (Settings.Instance.DebugStates)
                            Logging.Log("statistics.State = " + _statistics.State);
                        // Done
                        // Statistics State: ProcessState
                    //}
   
                    if (AutoStart)
                    {
                        // Dont start missions hour before downtime
                        if (DateTime.UtcNow.Hour == 10)
                            break;

                        // Dont start missions in downtime
                        if (DateTime.UtcNow.Hour == 11 && DateTime.UtcNow.Minute < 15)
                            break;

                        if (Settings.Instance.RandomDelay > 0 || Settings.Instance.MinimumDelay > 0)
                        {
                            _randomDelay = (Settings.Instance.RandomDelay > 0 ? _random.Next(Settings.Instance.RandomDelay) : 0) + Settings.Instance.MinimumDelay;
                            _lastAction = DateTime.Now;

                            State = QuestorState.DelayedStart;
                            Logging.Log("Questor: Random mission start delay of [" + _randomDelay + "] seconds");
                            return;
                        }
                        else
                        {
                            State = QuestorState.Cleanup;
                            return;
                        }
                    }
                    break;

                case QuestorState.DelayedStart:
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < _randomDelay)
                        break;

                    _storyline.Reset();
                    State = QuestorState.Cleanup;
                    break;


                case QuestorState.DelayedGotoBase:
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < (int)Time.DelayedGotoBase_seconds)
                        break;

                    Logging.Log("Questor: Heading back to base");
                    State = QuestorState.GotoBase;
                    break;

                case QuestorState.Cleanup:
                    //
                    // this state is needed because forced disconnects 
                    // and crashes can leave "extra" cargo in the 
                    // cargo hold that is undesirable and causes
                    // problems loading the correct ammo on occasion
                    //
                    if (Cache.Instance.LootAlreadyUnloaded == false)
                    {
                        State = QuestorState.GotoBase;
                        break;
                    }
                    else
                    {
                        State = QuestorState.CheckEVEStatus;
                        break;
                    }

                case QuestorState.Start:
                    if (first_start && Settings.Instance.MultiAgentSupport)
                    {
                        //if you are in wrong station and is not first agent
                        State = QuestorState.Switch;
                        first_start = false;
                        break;
                    }
                    Cache.Instance.OpenWrecks = false;
                    if (_agentInteraction.State == AgentInteractionState.Idle)
                    {
                        if (Settings.Instance.CharacterMode == "salvage")
                        {
                            State = QuestorState.BeginAfterMissionSalvaging;
                            break;
                        }
                        if (Settings.Instance.EnableStorylines && _storyline.HasStoryline())
                        {
                            Logging.Log("Questor: Storyline detected, doing storyline.");

                            _storyline.Reset();
                            State = QuestorState.Storyline;
                            break;
                        }
                        Logging.Log("AgentInteraction: Start conversation [Start Mission]");

                        _agentInteraction.State = AgentInteractionState.StartConversation;
                        _agentInteraction.Purpose = AgentInteractionPurpose.StartMission;

                        // Update statistic values - these should be cleared in statistics.cs!!!!!!!!!!!!
                        Cache.Instance.Wealth = Cache.Instance.DirectEve.Me.Wealth;
                        Statistics.Instance.LootValue = 0;
                        Statistics.Instance.LoyaltyPoints = Cache.Instance.Agent.LoyaltyPoints;
                        Statistics.Instance.StartedMission = DateTime.Now;
                        Statistics.Instance.FinishedMission = DateTime.MaxValue;
                        Cache.Instance.MissionName = string.Empty;
                        Statistics.Instance.LostDrones = 0;
                        Statistics.Instance.AmmoConsumption = 0;
                        Statistics.Instance.AmmoValue = 0;

                        Cache.Instance.panic_attempts_this_mission = 0;
                        Cache.Instance.lowest_shield_percentage_this_mission = 101;
                        Cache.Instance.lowest_armor_percentage_this_mission = 101;
                        Cache.Instance.lowest_capacitor_percentage_this_mission = 101;
                        Cache.Instance.repair_cycle_time_this_mission = 0;
                        Cache.Instance.TimeSpentReloading_seconds = 0;   // this will need to be added to whenever we reload or switch ammo
                        Cache.Instance.TimeSpentInMission_seconds = 0;   // from landing on grid (loading mission actions) to going to base (changing to gotbase state)
                        Cache.Instance.TimeSpentInMissionInRange = 0;    // time spent toally out of range, no targets
                        Cache.Instance.TimeSpentInMissionOutOfRange = 0; // time sprnt in range - with targets to kill (or no targets?!)
                    }

                    _agentInteraction.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("AgentInteraction.State = " + _agentInteraction.State);

                    if (_agentInteraction.State == AgentInteractionState.Done)
                    {
                        Cache.Instance.mission = Cache.Instance.GetAgentMission(Cache.Instance.AgentId);
                        if (Cache.Instance.mission != null)
                        {
                            // Update loyalty points again (the first time might return -1)
                            Statistics.Instance.LoyaltyPoints = Cache.Instance.Agent.LoyaltyPoints;
                            Cache.Instance.MissionName = Cache.Instance.mission.Name;
                        }

                        _agentInteraction.State = AgentInteractionState.Idle;
                        State = QuestorState.Arm;
                        return;
                    }

                    if (_agentInteraction.State == AgentInteractionState.ChangeAgent)
                    {
                        _agentInteraction.State = AgentInteractionState.Idle;
                        ValidateSettings();
                        State = QuestorState.Switch;
                        break;
                    }

                    break;

                case QuestorState.Switch:

                    if (_switch.State == SwitchShipState.Idle)
                    {
                        Logging.Log("Switch: Begin");
                        _switch.State = SwitchShipState.Begin;
                    }

                    _switch.ProcessState();

                    if (_switch.State == SwitchShipState.Done)
                    {
                        _switch.State = SwitchShipState.Idle;
                        State = QuestorState.GotoBase;
                    }
                    break;

                case QuestorState.Arm:
                    if (_arm.State == ArmState.Idle)
                    {
                        if (Cache.Instance.CourierMission)
                            _arm.State = ArmState.SwitchToTransportShip;
                        else
                        {
                            Logging.Log("Arm: Begin");
                            _arm.State = ArmState.Begin;

                            // Load right ammo based on mission
                            _arm.AmmoToLoad.Clear();
                            _arm.AmmoToLoad.AddRange(_agentInteraction.AmmoToLoad);
                        }
                    }

                    _arm.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Arm.State = " + _arm.State);

                    if (_arm.State == ArmState.NotEnoughAmmo)
                    {
                        // we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        // we may be out of drones/ammo but disconnecting/reconnecting will not fix that so update the timestamp
                        Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        Logging.Log("Arm: Armstate.NotEnoughAmmo");
                        _arm.State = ArmState.Idle;
                        State = QuestorState.Error;
                    }

                    if (_arm.State == ArmState.NotEnoughDrones)
                    {
                        // we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        // we may be out of drones/ammo but disconnecting/reconnecting will not fix that so update the timestamp
                        Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        Logging.Log("Arm: Armstate.NotEnoughDrones");
                        _arm.State = ArmState.Idle;
                        State = QuestorState.Error;
                    }

                    if (_arm.State == ArmState.Done)
                    {
                        //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        _arm.State = ArmState.Idle;
                        _drones.State = DroneState.WaitingForTargets;

                        if (Cache.Instance.CourierMission)
                            State = QuestorState.CourierMission;
                        else
                            State = QuestorState.LocalWatch;
                    }

                    break;

                case QuestorState.LocalWatch:
                    if (Settings.Instance.UseLocalWatch)
                    {
                        _lastLocalWatchAction = DateTime.Now; 
                        if (Cache.Instance.Local_safe(Settings.Instance.LocalBadStandingPilotsToTolerate, Settings.Instance.LocalBadStandingLevelToConsiderBad))
                        {
                            Logging.Log("Questor.LocalWatch: local is clear");
                            State = QuestorState.WarpOutStation;
                        }
                        else
                        {
                            Logging.Log("Questor.LocalWatch: Bad standings pilots in local: We will stay 5 minutes in the station and then we will check if it is clear again");
                            State = QuestorState.WaitingforBadGuytoGoAway;
                            Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                            Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;
                        }
                    }
                    else
                    {
                        State = QuestorState.WarpOutStation;
                    }
                    break;

                case QuestorState.WaitingforBadGuytoGoAway:
                    if(DateTime.Now.Subtract(_lastLocalWatchAction).Minutes < (int)Time.WaitforBadGuytoGoAway_minutes)
                        break;
                    State = QuestorState.LocalWatch;
                    break;

                case QuestorState.WarpOutStation:
                    var _bookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.bookmarkWarpOut ?? "").OrderByDescending(b => b.CreatedOn).Where(b => b.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId).FirstOrDefault();
                    //var _bookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.bookmarkWarpOut + "-" + Cache.Instance.CurrentAgent ?? "").OrderBy(b => b.CreatedOn).FirstOrDefault();
                    var _solarid = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;

                    if (_bookmark == null)
                    {
                        Logging.Log("WarpOut: No Bookmark");
                        State = QuestorState.GotoMission;
                    }
                    else if (_bookmark.LocationId == _solarid)
                    {
                        if (_traveler.Destination == null)
                        {
                            Logging.Log("WarpOut: Warp at " + _bookmark.Title);
                            _traveler.Destination = new BookmarkDestination(_bookmark);
                            Cache.Instance.DoNotBreakInvul = true;
                        }

                        _traveler.ProcessState();
                        if (_traveler.State == TravelerState.AtDestination)
                        {
                            Logging.Log("WarpOut: Safe!");
                            Cache.Instance.DoNotBreakInvul = false;
                            State = QuestorState.GotoMission;
                            _traveler.Destination = null;
                        }
                    }
                    else
                    {
                        Logging.Log("WarpOut: No Bookmark in System");
                        State = QuestorState.GotoMission;
                    }
                    break;

                case QuestorState.GotoMission:
                    Cache.Instance.OpenWrecks = false;
                    Statistics.Instance.MissionLoggingCompleted = false;
                    var missionDestination = _traveler.Destination as MissionBookmarkDestination;
                    if (missionDestination == null || missionDestination.AgentId != Cache.Instance.AgentId) // We assume that this will always work "correctly" (tm)
                        _traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(Cache.Instance.AgentId, "Encounter"));
                    //if (missionDestination == null)
                    //{
                    //    Logging.Log("Invalid bookmark loop! Mission Controller: Error");
                    //    State = QuestorState.Error;
                    //}
                    if (Cache.Instance.PriorityTargets.Any(pt => pt != null && pt.IsValid))
                    {
                        Logging.Log("GotoMission: Priority targets found, engaging!");
                        _combat.ProcessState();
                    }

                    _traveler.ProcessState();
                    if (Settings.Instance.DebugStates)
                        Logging.Log("Traveler.State = " + _traveler.State);

                    if (_traveler.State == TravelerState.AtDestination)
                    {
                        State = QuestorState.ExecuteMission;

                        // Seeing as we just warped to the mission, start the mission controller
                        _missionController.State = MissionControllerState.Start;
                        _combat.State = CombatState.CheckTargets;

                        _traveler.Destination = null;
                    }
                    break;

                case QuestorState.CombatHelper:
                    _combat.ProcessState();
                    _drones.ProcessState();
                    _salvage.ProcessState();
                    break;

                case QuestorState.Scanning:
                    //_localwatch.ProcessState();
                    _scanInteraction.ProcessState();
                    if (_scanInteraction.State == ScanInteractionState.Idle)
                        _scanInteraction.State = ScanInteractionState.Scan;
                    /*
                    if(_scanInteraction.State == ScanInteractionState.Done)
                        State = QuestorState.CombatHelper_anomaly;
                    */
                    break;

                case QuestorState.CombatHelper_anomaly:
                    _combat.ProcessState();
                    _drones.ProcessState();
                    _salvage.ProcessState();
                    _localwatch.ProcessState();
                    break;

                case QuestorState.ExecuteMission:
                    watch.Reset();
                    watch.Start();
                    _combat.ProcessState();
                    watch.Stop();

                    if (Settings.Instance.DebugPerformance)
                        Logging.Log("Combat.ProcessState took " + watch.ElapsedMilliseconds + "ms");

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Combat.State = " + _combat.State);

                    watch.Reset();
                    watch.Start();
                    _drones.ProcessState();
                    watch.Stop();

                    if (Settings.Instance.DebugPerformance)
                        Logging.Log("Drones.ProcessState took " + watch.ElapsedMilliseconds + "ms");

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Drones.State = " + _drones.State);

                    watch.Reset();
                    watch.Start();
                    _salvage.ProcessState();
                    watch.Stop();

                    if (Settings.Instance.DebugPerformance)
                        Logging.Log("Salvage.ProcessState took " + watch.ElapsedMilliseconds + "ms");

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Salvage.State = " + _salvage.State);

                    watch.Reset();
                    watch.Start();
                    _missionController.ProcessState();
                    watch.Stop();

                    if (Settings.Instance.DebugPerformance)
                        Logging.Log("MissionController.ProcessState took " + watch.ElapsedMilliseconds + "ms");

                    if (Settings.Instance.DebugStates)
                        Logging.Log("MissionController.State = " + _missionController.State);

                    // If we are out of ammo, return to base, the mission will fail to complete and the bot will reload the ship
                    // and try the mission again
                    if (_combat.State == CombatState.OutOfAmmo)
                    {
                        Logging.Log("Combat: Out of Ammo!");
                        //_drone.State = DroneState.Recalling;
                        State = QuestorState.GotoBase;

                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();
                    }

                    if (_missionController.State == MissionControllerState.Done)
                    {
                        State = QuestorState.GotoBase;

                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();
                    }

                    // If in error state, just go home and stop the bot
                    if (_missionController.State == MissionControllerState.Error)
                    {
                        Logging.Log("MissionController: Error");
                        State = QuestorState.GotoBase;

                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();
                    }
                    break;


                case QuestorState.GotoBase:

                    // anti bump
                    var structure = Cache.Instance.Entities.Where(i => i.GroupId == (int)Group.LargeCollidableStructure).OrderBy(t => t.Distance).FirstOrDefault();
                    if (Cache.Instance.TargetedBy.Any(t => t.IsWarpScramblingMe))
                    {
                        _combat.ProcessState();
                        _drones.ProcessState();
                    }
                    else if (structure != null && structure.Distance < (int)Distance.TooCloseToStructure)
                    {
                        if ((DateTime.Now.Subtract(_lastOrbit).TotalSeconds > 15))
                        {
                            structure.Orbit((int)Distance.SafeDistancefromStructure);
                            Logging.Log("Questor: GotoBase: initiating Orbit of [" + structure.Name + "] orbiting at [" + Cache.Instance.OrbitDistance + "]");
                            _lastOrbit = DateTime.Now;
                        }
                    }
                    else
                    {

                        var baseDestination = _traveler.Destination as StationDestination;
                        if (baseDestination == null || baseDestination.StationId != Cache.Instance.Agent.StationId)
                            _traveler.Destination = new StationDestination(Cache.Instance.Agent.SolarSystemId, Cache.Instance.Agent.StationId, Cache.Instance.DirectEve.GetLocationName(Cache.Instance.Agent.StationId));

                        if (Cache.Instance.PriorityTargets.Any(pt => pt != null && pt.IsValid))
                        {
                            Logging.Log("GotoBase: Priority targets found, engaging!");
                            _combat.ProcessState();
                        }

                        _traveler.ProcessState();
                        if (Settings.Instance.DebugStates)
                        {
                            Logging.Log("Traveler.State = " + _traveler.State);
                        }
                        if (_traveler.State == TravelerState.AtDestination)
                        {
                            Cache.Instance.mission = Cache.Instance.GetAgentMission(Cache.Instance.AgentId);

                            if (_missionController.State == MissionControllerState.Error)
                                State = QuestorState.Error;
                            else if (_combat.State != CombatState.OutOfAmmo && Cache.Instance.mission != null && Cache.Instance.mission.State == (int)MissionState.Accepted)
                                State = QuestorState.CompleteMission;
                            else
                                State = QuestorState.UnloadLoot;

                            _traveler.Destination = null;
                        }
                    }
                    break;

                case QuestorState.CheckEVEStatus:
                    // get the current process
                    Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

                    // get the physical mem usage (this only runs between missions)
                    Cache.Instance.totalMegaBytesOfMemoryUsed = ((currentProcess.WorkingSet64 / 1024) / 1024);
                    Logging.Log("Questor: EVE instance: totalMegaBytesOfMemoryUsed - " + Cache.Instance.totalMegaBytesOfMemoryUsed + " MB");

                    // If Questor window not visible, schedule a restart of questor in the uplink so that the GUI will start normally
                    if (!m_Parent.Visible && CloseQuestorflag) //GUI isnt visible and CloseQuestorflag is true, so that his code block only runs once
                    {
                        CloseQuestorflag = false;
                        //m_Parent.Visible = true; //this does not work for some reason - innerspace bug?
                        Cache.Instance.ReasonToStopQuestor = "The Questor GUI is not visible: did EVE get restarted due to a crash or lag?";
                        Logging.Log(Cache.Instance.ReasonToStopQuestor);
                        Cache.Instance.CloseQuestorCMDLogoff = false;
                        Cache.Instance.CloseQuestorCMDExitGame = true;
                        Cache.Instance.SessionState = "Exiting";
                        State = QuestorState.CloseQuestor;
                        return;
                    }
                    else if (Cache.Instance.totalMegaBytesOfMemoryUsed > (Settings.Instance.EVEProcessMemoryCeiling - 50) && Settings.Instance.EVEProcessMemoryCeilingLogofforExit != "")
                    {
                        Logging.Log("Questor: Memory usage is above the EVEProcessMemoryCeiling threshold. EVE instance: totalMegaBytesOfMemoryUsed - " + Cache.Instance.totalMegaBytesOfMemoryUsed + " MB");
                        Cache.Instance.ReasonToStopQuestor = "Memory usage is above the EVEProcessMemoryCeiling threshold. EVE instance: totalMegaBytesOfMemoryUsed - " + Cache.Instance.totalMegaBytesOfMemoryUsed + " MB";
                        if (Settings.Instance.EVEProcessMemoryCeilingLogofforExit == "logoff")
                        {
                            Cache.Instance.CloseQuestorCMDLogoff = true;
                            Cache.Instance.CloseQuestorCMDExitGame = false;
                            Cache.Instance.SessionState = "LoggingOff";
                            State = QuestorState.CloseQuestor;
                            return;
                        }
                        if (Settings.Instance.EVEProcessMemoryCeilingLogofforExit == "exit")
                        {
                            Cache.Instance.CloseQuestorCMDLogoff = false;
                            Cache.Instance.CloseQuestorCMDExitGame = true;
                            Cache.Instance.SessionState = "Exiting";
                            State = QuestorState.CloseQuestor;
                            return;
                        }
                        Logging.Log("Questor: EVEProcessMemoryCeilingLogofforExit was not set to exit or logoff - doing nothing ");
                        return;
                    }
                    else
                    {
                        Cache.Instance.SessionState = "Running";
                        State = QuestorState.Start;
                    }
                    break;

                case QuestorState.CloseQuestor:
                    if (!Cache.Instance.CloseQuestorCMDLogoff && !Cache.Instance.CloseQuestorCMDExitGame)
                    {
                        Cache.Instance.CloseQuestorCMDExitGame = true;
                    }
                    if (_traveler.State == TravelerState.Idle)
                    {
                        Logging.Log("QuestorState.CloseQuestor: Entered Traveler - making sure we will be docked at Home Station");
                    }
                    var baseDestination2 = _traveler.Destination as StationDestination;
                    if (baseDestination2 == null || baseDestination2.StationId != Cache.Instance.Agent.StationId)
                        _traveler.Destination = new StationDestination(Cache.Instance.Agent.SolarSystemId, Cache.Instance.Agent.StationId, Cache.Instance.DirectEve.GetLocationName(Cache.Instance.Agent.StationId));

                    if (Cache.Instance.PriorityTargets.Any(pt => pt != null && pt.IsValid))
                    {
                        Logging.Log("QuestorState.CloseQuestor: GoToBase: Priority targets found, engaging!");
                        _combat.ProcessState();
                    }
                    _traveler.ProcessState();
                    if (_traveler.State == TravelerState.AtDestination)
                    {
                        //Logging.Log("QuestorState.CloseQuestor: At Station: Docked");
                        if (Settings.Instance.SessionsLog) // if false we do not write a sessionlog, doubles as a flag so we dont write the sessionlog more than once
                        {
                            //
                            // prepare the Questor Session Log - keeps track of starts, restarts and exits, and hopefully the reasons
                            //

                            // Get the path

                            if (!Directory.Exists(Settings.Instance.SessionsLogPath))
                                Directory.CreateDirectory(Settings.Instance.SessionsLogPath);

                            Cache.Instance.SessionIskPerHrGenerated = ((int)Cache.Instance.SessionIskGenerated / (DateTime.Now.Subtract(_questorStarted).TotalMinutes / 60));
                            Cache.Instance.SessionLootPerHrGenerated = ((int)Cache.Instance.SessionLootGenerated / (DateTime.Now.Subtract(_questorStarted).TotalMinutes / 60));
                            Cache.Instance.SessionLPPerHrGenerated = (((int)Cache.Instance.SessionLPGenerated * (int)Settings.Instance.IskPerLP) / (DateTime.Now.Subtract(_questorStarted).TotalMinutes / 60));
                            Cache.Instance.SessionTotalPerHrGenerated = ((int)Cache.Instance.SessionIskPerHrGenerated + (int)Cache.Instance.SessionLootPerHrGenerated + (int)Cache.Instance.SessionLPPerHrGenerated);
                            Logging.Log("QuestorState.CloseQuestor: Writing Session Data [1]");

                            // Write the header
                            if (!File.Exists(Settings.Instance.SessionsLogFile))
                                File.AppendAllText(Settings.Instance.SessionsLogFile, "Date;RunningTime;SessionState;LastMission;WalletBalance;MemoryUsage;Reason;IskGenerated;LootGenerated;LPGenerated;Isk/Hr;Loot/Hr;LP/HR;Total/HR;\r\n");

                            // Build the line
                            var line = DateTime.Now + ";";                                  // Date
                            line += Cache.Instance.SessionRunningTime + ";";                // RunningTime
                            line += Cache.Instance.SessionState + ";";                      // SessionState
                            line += Cache.Instance.MissionName + ";";                                          // LastMission
                            line += ((int)Cache.Instance.DirectEve.Me.Wealth + ";");        // WalletBalance
                            line += ((int)Cache.Instance.totalMegaBytesOfMemoryUsed + ";"); // MemoryUsage
                            line += Cache.Instance.ReasonToStopQuestor + ";";               // Reason to Stop Questor
                            line += Cache.Instance.SessionIskGenerated + ";";               // Isk Generated This Session
                            line += Cache.Instance.SessionLootGenerated + ";";              // Loot Generated This Session
                            line += Cache.Instance.SessionLPGenerated + ";";                // LP Generated This Session
                            line += Cache.Instance.SessionIskPerHrGenerated + ";";          // Isk Generated per hour this session
                            line += Cache.Instance.SessionLootPerHrGenerated + ";";         // Loot Generated per hour This Session
                            line += Cache.Instance.SessionLPPerHrGenerated + ";";           // LP Generated per hour This Session
                            line += Cache.Instance.SessionTotalPerHrGenerated + ";\r\n";    // Total Per Hour This Session

                            // The mission is finished
                            Logging.Log(line);
                            File.AppendAllText(Settings.Instance.SessionsLogFile, line);

                            Logging.Log("Questor: Writing to session log [ " + Settings.Instance.SessionsLogFile);
                            Logging.Log("Questor is stopping because: " + Cache.Instance.ReasonToStopQuestor);
                            Settings.Instance.SessionsLog = false; //so we don't write the sessionlog more than once per session
                        }
                        if (AutoStart)
                        {
                            if (Cache.Instance.CloseQuestorCMDLogoff)
                            {
                                if (CloseQuestorflag)
                                {
                                    Logging.Log("Questor: We are in station: Logging off EVE: In theory eve and questor will restart on their own when the client comes back up");
                                    LavishScript.ExecuteCommand("uplink echo Logging off EVE:  \\\"${Game}\\\" \\\"${Profile}\\\"");
                                    Logging.Log("Questor: you can change this option by setting the wallet and eveprocessmemoryceiling options to use exit instead of logoff: see the settings.xml file");
                                    Logging.Log("Questor: Logging Off eve in 15 seconds.");
                                    CloseQuestorflag = false;
                                    _CloseQuestorDelay = DateTime.Now.AddSeconds((int)Time.CloseQuestorDelayBeforeExit_seconds);
                                }
                                if (_CloseQuestorDelay.AddSeconds(-10) < DateTime.Now)
                                {
                                    Logging.Log("Questor: Exiting eve in 10 seconds");
                                }
                                if (_CloseQuestorDelay < DateTime.Now)
                                {
                                    Logging.Log("Questor: Exiting eve now.");
                                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdLogOff);
                                }
                                Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdLogOff);
                                break;
                            }
                            if (Cache.Instance.CloseQuestorCMDExitGame)
                            {
                                //Logging.Log("Questor: We are in station: Exit option has been configured.");
                                if ((Settings.Instance.CloseQuestorCMDUplinkIsboxerCharacterSet) && (Settings.Instance.CloseQuestorCMDUplinkInnerspaceProfile))
                                {
                                    Logging.Log("Questor: We are in station: Don't be silly you cant use both the CloseQuestorCMDUplinkIsboxerProfile and the CloseQuestorCMDUplinkIsboxerProfile setting, choose one");
                                }
                                else
                                {
                                    if (Settings.Instance.CloseQuestorCMDUplinkInnerspaceProfile) //if configured as true we will use the innerspace profile to restart this session
                                    {
                                        //Logging.Log("Questor: We are in station: CloseQuestorCMDUplinkInnerspaceProfile is true");
                                        if (CloseQuestorCMDUplink)
                                        {
                                            Logging.Log("Questor: We are in station: Starting a timer in the innerspace uplink to restart this innerspace profile session");
                                            LavishScript.ExecuteCommand("uplink exec timedcommand 350 open \\\"${Game}\\\" \\\"${Profile}\\\"");
                                            Logging.Log("Questor: Done: quitting this session so the new innerspace session can take over");
                                            Logging.Log("Questor: Exiting eve in 15 seconds.");
                                            CloseQuestorCMDUplink = false;
                                            _CloseQuestorDelay = DateTime.Now.AddSeconds((int)Time.CloseQuestorDelayBeforeExit_seconds);
                                        }
                                        if ((_CloseQuestorDelay.AddSeconds(-10) == DateTime.Now) && (!CloseQuestor10SecWarningDone))
                                        {
                                            CloseQuestor10SecWarningDone = true;
                                            Logging.Log("Questor: Exiting eve in 10 seconds");
                                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdQuitGame);
                                        }
                                        if (_CloseQuestorDelay < DateTime.Now)
                                        {
                                            Logging.Log("Questor: Exiting eve now.");
                                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdQuitGame);
                                        }
                                        return;
                                    }
                                    else if (Settings.Instance.CloseQuestorCMDUplinkIsboxerCharacterSet) //if configured as true we will use isboxer to restart this session
                                    {
                                        //Logging.Log("Questor: We are in station: CloseQuestorCMDUplinkIsboxerProfile is true");
                                        if (CloseQuestorCMDUplink)
                                        {
                                            Logging.Log("Questor: We are in station: Starting a timer in the innerspace uplink to restart this isboxer character set");
                                            LavishScript.ExecuteCommand("uplink timedcommand 350 runscript isboxer -launch \\\"${ISBoxerCharacterSet}\\\"");
                                            Logging.Log("Questor: Done: quitting this session so the new isboxer session can take over");
                                            Logging.Log("Questor: We are in station: Exiting eve.");
                                            CloseQuestorCMDUplink = false;
                                            _CloseQuestorDelay = DateTime.Now.AddSeconds((int)Time.CloseQuestorDelayBeforeExit_seconds);
                                        }
                                        if ((_CloseQuestorDelay.AddSeconds(-10) == DateTime.Now) && (!CloseQuestor10SecWarningDone))
                                        {
                                            CloseQuestor10SecWarningDone = true;
                                            Logging.Log("Questor: Exiting eve in 10 seconds");
                                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdQuitGame);
                                        }
                                        if (_CloseQuestorDelay < DateTime.Now)
                                        {
                                            Logging.Log("Questor: Exiting eve now.");
                                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdQuitGame);
                                        }
                                        return;
                                    }
                                    else if (!Settings.Instance.CloseQuestorCMDUplinkInnerspaceProfile && !Settings.Instance.CloseQuestorCMDUplinkIsboxerCharacterSet)
                                    {
                                        Logging.Log("Questor: CloseQuestorCMDUplinkInnerspaceProfile and CloseQuestorCMDUplinkIsboxerProfile both false");
                                        if (CloseQuestorCMDUplink)
                                        {
                                            CloseQuestorCMDUplink = false;
                                            _CloseQuestorDelay = DateTime.Now.AddSeconds((int)Time.CloseQuestorDelayBeforeExit_seconds);
                                        }
                                        if ((_CloseQuestorDelay.AddSeconds(-10) == DateTime.Now) && (!CloseQuestor10SecWarningDone))
                                        {
                                            CloseQuestor10SecWarningDone = true;
                                            Logging.Log("Questor: Exiting eve in 10 seconds");
                                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdQuitGame);
                                        }
                                        if (_CloseQuestorDelay < DateTime.Now)
                                        {
                                            Logging.Log("Questor: Exiting eve now.");
                                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdQuitGame);
                                        }
                                        return;
                                    }
                                }
                            }
                        }
                        Logging.Log("Autostart is false: Stopping EVE with quit command");
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdQuitGame);
                        break;
                    }
                    if (Settings.Instance.DebugStates)
                        Logging.Log("Traveler.State = " + _traveler.State);
                    break;
                
                case QuestorState.CompleteMission:
                    if (_agentInteraction.State == AgentInteractionState.Idle)
                    {
                        if (Settings.Instance.DroneStatsLog)
                        {
                            // Lost drone statistics
                            // (inelegantly located here so as to avoid the necessity to switch to a combat ship after salvaging)
                            if (Settings.Instance.UseDrones && (Cache.Instance.DirectEve.ActiveShip.GroupId != 31 && Cache.Instance.DirectEve.ActiveShip.GroupId != 28 && Cache.Instance.DirectEve.ActiveShip.GroupId != 380))
                            {
                                var droneBay = Cache.Instance.DirectEve.GetShipsDroneBay();
                                if (droneBay.Window == null)
                                {
                                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenDroneBayOfActiveShip);
                                    break;
                                }
                                if (!droneBay.IsReady)
                                    break;
                                if (Cache.Instance.InvTypesById.ContainsKey(Settings.Instance.DroneTypeId))
                                {
                                    var drone = Cache.Instance.InvTypesById[Settings.Instance.DroneTypeId];
                                    Statistics.Instance.LostDrones = (int)Math.Floor((droneBay.Capacity - droneBay.UsedCapacity) / drone.Volume);
                                    Logging.Log("DroneStats: Logging the number of lost drones: " + Statistics.Instance.LostDrones.ToString());

                                    if (!File.Exists(Settings.Instance.DroneStatslogFile))
                                        File.AppendAllText(Settings.Instance.DroneStatslogFile, "Mission;Number of lost drones\r\n");
                                    var droneline = Cache.Instance.MissionName + ";";
                                    droneline += ((int)Statistics.Instance.LostDrones) + ";\r\n";
                                    File.AppendAllText(Settings.Instance.DroneStatslogFile, droneline);
                                }
                                else
                                {
                                    Logging.Log("DroneStats: Couldn't find the drone TypeID specified in the settings.xml; this shouldn't happen!");
                                }
                            }
                        }
                        // Lost drone statistics stuff ends here


                        // Ammo Consumption statistics
                        // Is cargo open?
                        var cargoship = Cache.Instance.DirectEve.GetShipsCargo();
                        if (cargoship.Window == null)
                        {
                            // No, command it to open
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                            break;
                        }

                        if (!cargoship.IsReady)
                            break;

                        var correctAmmo1 = Settings.Instance.Ammo.Where(a => a.DamageType == Cache.Instance.DamageType);
                        var AmmoCargo = cargoship.Items.Where(i => correctAmmo1.Any(a => a.TypeId == i.TypeId));
                        foreach (var item in AmmoCargo)
                        {
                            var Ammo1 = Settings.Instance.Ammo.Where(a => a.TypeId == item.TypeId).FirstOrDefault();
                            var AmmoType = Cache.Instance.InvTypesById[item.TypeId];
                            Statistics.Instance.AmmoConsumption = (Ammo1.Quantity - item.Quantity);
                            Statistics.Instance.AmmoValue = ((int?)AmmoType.MedianSell ?? 0) * (int)Statistics.Instance.AmmoConsumption;
                        }
                        Logging.Log("AgentInteraction: Start Conversation [Complete Mission]");

                        _agentInteraction.State = AgentInteractionState.StartConversation;
                        _agentInteraction.Purpose = AgentInteractionPurpose.CompleteMission;
                    }

                    _agentInteraction.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("AgentInteraction.State = " + _agentInteraction.State);

                    if (_agentInteraction.State == AgentInteractionState.Done)
                    {
                        // Cache.Instance.MissionName = String.Empty;  // Do Not clear the 'current' mission name until after we have done the mission logging
                        _agentInteraction.State = AgentInteractionState.Idle;
                        if (Cache.Instance.CourierMission)
                        {
                            Cache.Instance.CourierMission = false;
                            State = QuestorState.Idle;
                        }
                        else
                            State = QuestorState.UnloadLoot;
                        return;
                    }
                    break;

                case QuestorState.UnloadLoot:
                    if (_unloadLoot.State == UnloadLootState.Idle)
                    {
                        Logging.Log("UnloadLoot: Begin");
                        _unloadLoot.State = UnloadLootState.Begin;

                    }

                    _unloadLoot.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("UnloadLoot.State = " + _unloadLoot.State);

                    if (_unloadLoot.State == UnloadLootState.Done)
                    {
                        Logging.Log("UnloadLoot: Done");
                        Cache.Instance.LootAlreadyUnloaded = true;
                        _unloadLoot.State = UnloadLootState.Idle;

                        // Update total loot value
                        Statistics.Instance.LootValue += (int)_unloadLoot.LootValue;

                        Cache.Instance.mission = Cache.Instance.GetAgentMission(Cache.Instance.AgentId);
                        if (_combat.State != CombatState.OutOfAmmo && Settings.Instance.AfterMissionSalvaging && Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").Count > 0 && (Cache.Instance.mission == null || Cache.Instance.mission.State == (int)MissionState.Offered))
                        {
                            Statistics.Instance.FinishedMission = DateTime.Now;
                            if (Settings.Instance.SalvageMultpleMissionsinOnePass) // Salvage only after multiple missions have been completed
                            {   
                                //if we can still complete another mission before the Wrecks disappear and still have time to salvage
                                if (DateTime.Now.Subtract(Statistics.Instance.FinishedSalvaging).Minutes > ((int)Time.WrecksDisappearAfter_minutes - (int)Time.AverageTimeToCompleteAMission_minutes - (int)Time.AverageTimetoSalvageMultipleMissions_minutes))
                                {
                                    Logging.Log("Questor: UnloadLoot: The last after mission salvaging session was [" + DateTime.Now.Subtract(Statistics.Instance.FinishedSalvaging).Minutes + "] ago ");
                                    Logging.Log("Questor: UnloadLoot: we are after mission salvaging again because it has been at least [" + ((int)Time.WrecksDisappearAfter_minutes - (int)Time.AverageTimeToCompleteAMission_minutes - (int)Time.AverageTimetoSalvageMultipleMissions_minutes) + "] min since the last session. ");
                                    State = QuestorState.BeginAfterMissionSalvaging;
                                }
                                else
                                {
                                    Logging.Log("Questor: UnloadLoot: The last after mission salvaging session was [" + DateTime.Now.Subtract(Statistics.Instance.FinishedSalvaging).Minutes + "] ago ");
                                    Logging.Log("Questor: UnloadLoot: we are going to the next mission because it has not been [" + ((int)Time.WrecksDisappearAfter_minutes - (int)Time.AverageTimeToCompleteAMission_minutes - (int)Time.AverageTimetoSalvageMultipleMissions_minutes) + "] min since the last session. ");
                                    Statistics.Instance.FinishedMission = DateTime.Now;
                                    State = QuestorState.Idle;
                                }
                            }
                            else // Normal Salvaging
                            {
                                State = QuestorState.BeginAfterMissionSalvaging;
                            }
                            return;
                        }
                        else if (_combat.State == CombatState.OutOfAmmo)
                        {
                            State = QuestorState.Start;
                            return;
                        }
                        else //If we arent after mission salvaging and we arent out of ammo we must be done. 
                        {
                            Statistics.Instance.FinishedMission = DateTime.Now;
                            Statistics.Instance.StartedSalvaging = DateTime.Now;
                            Statistics.Instance.FinishedSalvaging = DateTime.Now;
                            State = QuestorState.Idle;
                            return;
                        }
                    }
                    break;

                case QuestorState.BeginAfterMissionSalvaging:
                    StartedSalvaging = DateTime.Now;
                    _GatesPresent = false;
                    Cache.Instance.OpenWrecks = true;
                    if (_arm.State == ArmState.Idle)
                        _arm.State = ArmState.SwitchToSalvageShip;

                    _arm.ProcessState();
                    if (_arm.State == ArmState.Done)
                    {
                        _arm.State = ArmState.Idle;

                        var bookmark = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").OrderBy(b => b.CreatedOn).FirstOrDefault();
                        if (bookmark == null)
                        {
                            State = QuestorState.Idle;
                            return;
                        }

                        State = QuestorState.GotoSalvageBookmark;
                        _traveler.Destination = new BookmarkDestination(bookmark);
                        return;
                    }
                    break;

                case QuestorState.GotoSalvageBookmark:
                    _traveler.ProcessState();
                    string target = "Acceleration Gate";
                    var targets = Cache.Instance.EntitiesByName(target);
                    if (_traveler.State == TravelerState.AtDestination || GateInSalvage())
                    {
                        //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        State = QuestorState.Salvage;
                        _traveler.Destination = null;
                        return;
                    }

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Traveler.State = " + _traveler.State);
                    break;

                case QuestorState.Salvage:
                    var SalvageCargo = Cache.Instance.DirectEve.GetShipsCargo();
                    Cache.Instance.SalvageAll = true;
                    Cache.Instance.OpenWrecks = true;

                    // Is our cargo window open?
                    if (SalvageCargo.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                        break;
                    }

                    if (Settings.Instance.UnloadLootAtStation && SalvageCargo.IsReady && (SalvageCargo.Capacity - SalvageCargo.UsedCapacity) < 100)
                    {
                        Logging.Log("Salvage: We are full, goto base to unload");
                        State = QuestorState.GotoBase;
                        break;
                    }

                    if (Cache.Instance.UnlootedContainers.Count() == 0)
                    {
                        Logging.Log("Salvage: Finished salvaging the room");

                        bool GatesInRoom = GateInSalvage();
                        var bookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");
                        do
                        {
                            // Remove all bookmarks from address book
                            var bookmark = bookmarks.FirstOrDefault(b => Cache.Instance.DistanceFromMe(b.X ?? 0, b.Y ?? 0, b.Z ?? 0) < (int)Distance.BookmarksOnGridWithMe);
                            if (!GatesInRoom && _GatesPresent) // if there were gates, but we've gone through them all, delete all bookmarks
                                bookmark = bookmarks.FirstOrDefault();
                            else if (GatesInRoom)
                                break;
                            if (bookmark == null)
                                break;

                            bookmark.Delete();
                            bookmarks.Remove(bookmark);
                        } while (true);

                        if (bookmarks.Count == 0 && !GatesInRoom)
                        {
                            Logging.Log("Salvage: We have salvaged all bookmarks, goto base");
                            Cache.Instance.SalvageAll = false;
                            State = QuestorState.GotoBase;
                            return;
                        }
                        else
                        {

                            if (!GatesInRoom)
                            {
                                Logging.Log("Salvage: Goto the next salvage bookmark");

                                State = QuestorState.GotoSalvageBookmark;
                                _traveler.Destination = new BookmarkDestination(bookmarks.OrderBy(b => b.CreatedOn).First());
                            }
                            else if (Settings.Instance.UseGatesInSalvage)
                            {
                                Logging.Log("Salvage: Acceleration gate found - moving to next pocket");
                                State = QuestorState.SalvageUseGate;
                            }
                            else
                            {
                                Logging.Log("Salvage: Acceleration gate found, useGatesInSalvage set to false - Returning to base");
                                State = QuestorState.GotoBase;
                                _traveler.Destination = null;
                            }
                        }
                        break;
                    }

                    var closestWreck = Cache.Instance.UnlootedContainers.First();
                    if (Math.Round(closestWreck.Distance,0) > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closestWreck.Id))
                    {
                        if (closestWreck.Distance > (int)Distance.WarptoDistance)
                        {
                            if (DateTime.Now.Subtract(_lastWarpTo).TotalSeconds > 10)
                            {
                                Logging.Log("Salvage: Warping to [" + closestWreck.Name + "] which is [" + Math.Round(closestWreck.Distance/1000,0) +"k away]");
                                closestWreck.WarpTo();

                            }
                        }
                        else
                            closestWreck.Approach();
                    }
                    else if (closestWreck.Distance <= (int)Distance.SafeScoopRange && Cache.Instance.Approaching != null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                        Logging.Log("Questor: Salvage: Stop ship, ClosestWreck [" + Math.Round(closestWreck.Distance,0) + "] is in scooprange + [" + (int)Distance.SafeScoopRange + "] and we were approaching");
                    }
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
                        ApplySettings();
                    }
                    break;

                //case QuestorState.ScoopStep1SetupBookmarkLocation:
                //    //if (_arm.State == ArmState.Idle)
                //    //    _arm.State = ArmState.SwitchToLootWrecksShip;
                //    //_arm.ProcessState();
                //    //if (_arm.State == ArmState.Done)
                //    //{
                //    _arm.State = ArmState.Idle;
                //    var Scoopbookmark = Cache.Instance.BookmarksByLabel("ScoopSpot").OrderBy(b => b.CreatedOn).FirstOrDefault();
                //    if (Scoopbookmark == null)
                //    {
                //        Logging.Log("Bookmark named [ ScoopSpot ] not found");
                //        State = QuestorState.Idle;
                //        break;
                //    }
                //
                //    State = QuestorState.ScoopStep2GotoScoopBookmark;
                //    _traveler.Destination = new BookmarkDestination(Scoopbookmark);
                //
                //    //}
                //    break;

                //case QuestorState.ScoopStep2GotoScoopBookmark:
                //
                //
                //    _traveler.ProcessState();
                //    if (_traveler.State == TravelerState.AtDestination)
                //    {
                //        State = QuestorState.ScoopStep3WaitForWrecks;
                //        _scoop.State = ScoopState.LootHostileWrecks;
                //        _traveler.Destination = null;
                //    }
                //
                //    if (Settings.Instance.DebugStates)
                //        Logging.Log("Traveler.State = " + _traveler.State);
                //    break;
                //
                //case QuestorState.ScoopStep3WaitForWrecks:
                //    // We are not in space yet, wait...
                //    if (!Cache.Instance.InSpace)
                //        break;
                //
                //    //
                //    // Loot All wrecks on grid of 'here'
                //    //
                //    var MyScoopshipCargo = Cache.Instance.DirectEve.GetShipsCargo();
                //
                //    // Is our cargo window open?
                //    if (MyScoopshipCargo.Window == null)
                //    {
                //        // No, command it to open
                //        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                //        break;
                //    }
                //
                //    //if (MyScoopshipCargo.IsReady && (MyScoopshipCargo.Capacity - MyScoopshipCargo.UsedCapacity) < 3500)
                //    //{
                //    //Logging.Log("Salvage: We are full, goto base to unload");
                //    //this needs to be changed to dock at the closest station
                //    //State = QuestorState . DockAtNearestStation;
                //    //    break;
                //    //}
                //
                //    if (Cache.Instance.UnlootedContainers.Count() == 0)
                //    {
                //        break;
                //    }
                //    var closestWreck2 = Cache.Instance.UnlootedWrecksAndSecureCans.First();
                //    if (closestWreck2.Distance > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closestWreck2.Id))
                //    {
                //        if (closestWreck2.Distance > (int)Distance.WarptoDistance)
                //        {
                //            closestWreck2.WarpTo();
                //            break;
                //        }
                //        else
                //            closestWreck2.Approach();
                //    }
                //    else if (closestWreck2.Distance <= (int)Distance.SafeScoopRange && Cache.Instance.Approaching != null)
                //        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                //        Logging.Log("Questor: ScoopStep3WaitForWrecks: Stop ship, ClosestWreck [" + closestWreck2.Distance + "] is in scooprange + [" + (int)Distance.SafeScoopRange + "] and we were approaching");
                //
                //    try
                //    {
                //        // Overwrite settings, as the 'normal' settings do not apply
                //        _scoop.MaximumWreckTargets = Math.Min(Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets, Cache.Instance.DirectEve.Me.MaxLockedTargets);
                //        _scoop.ReserveCargoCapacity = 5;
                //        _scoop.ProcessState();
                //    }
                //    finally
                //    {
                //        ApplySettings();
                //    }
                //    break;

                case QuestorState.SalvageUseGate:
                    Cache.Instance.OpenWrecks = true;

                    target = "Acceleration Gate";

                    targets = Cache.Instance.EntitiesByName(target);
                    if (targets == null || targets.Count() == 0)
                    {
                        State = QuestorState.GotoSalvageBookmark;
                        return;
                    }

                    _lastX = Cache.Instance.DirectEve.ActiveShip.Entity.X;
                    _lastY = Cache.Instance.DirectEve.ActiveShip.Entity.Y;
                    _lastZ = Cache.Instance.DirectEve.ActiveShip.Entity.Z;

                    var closest = targets.OrderBy(t => t.Distance).First();
                    if (closest.Distance < (int)Distance.DecloakRange)
                    {
                        Logging.Log("Salvage: Acceleration gate found - GroupID=" + closest.GroupId);

                        // Activate it and move to the next Pocket
                        closest.Activate();

                        // Do not change actions, if NextPocket gets a timeout (>2 mins) then it reverts to the last action
                        Logging.Log("Salvage: Activate [" + closest.Name + "] and change state to 'NextPocket'");

                        State = QuestorState.SalvageNextPocket;
                        _lastPulse = DateTime.Now;
                        return;
                    }
                    else if (closest.Distance < (int)Distance.WarptoDistance)
                    {
                        // Move to the target
                        if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)
                        {
                            Logging.Log("Salvage: Approaching target [" + closest.Name + "][ID: " + closest.Id + "][" + Math.Round(closest.Distance / 1000, 0) + "k away]");
                            closest.Approach();
                        }
                    }
                    else
                    {
                        // Probably never happens
                        if (DateTime.Now.Subtract(_lastWarpTo).TotalSeconds > 10)
                        {
                            Logging.Log("Salvage: Warping to [" + closest.Name + "] which is [" + Math.Round(closest.Distance/1000, 0) + "k away]");
                            closest.WarpTo();
                            _lastWarpTo = DateTime.Now;
                        }
                    }
                    _lastPulse = DateTime.Now.AddSeconds(10);
                    break;

                case QuestorState.SalvageNextPocket:
                    Cache.Instance.OpenWrecks = true;
                    var distance = Cache.Instance.DistanceFromMe(_lastX, _lastY, _lastZ);
                    if (distance > (int)Distance.NextPocketDistance)
                    {
                        //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        Logging.Log("Salvage: We've moved to the next Pocket [" + Math.Round(distance/1000,0) + "k away]");

                        State = QuestorState.Salvage;
                        return;
                    }
                    else if (DateTime.Now.Subtract(_lastPulse).TotalMinutes > 2)
                    {
                        Logging.Log("Salvage: We've timed out, retry last action");

                        // We have reached a timeout, revert to ExecutePocketActions (e.g. most likely Activate)
                        State = QuestorState.SalvageUseGate;
                        return;
                    }
                    break;

                case QuestorState.Storyline:
                    _storyline.ProcessState();

                    if (_storyline.State == StorylineState.Done)
                    {
                        Logging.Log("Questor: We have completed the storyline, returning to base");

                        State = QuestorState.GotoBase;
                        break;
                    }
                    break;

                case QuestorState.CourierMission:

                    if (_courier.State == CourierMissionState.Idle)
                        _courier.State = CourierMissionState.GotoPickupLocation;

                    _courier.ProcessState();

                    if (_courier.State == CourierMissionState.Done)
                    {
                        _courier.State = CourierMissionState.Idle;
                        Cache.Instance.CourierMission = false;

                        State = QuestorState.GotoBase;
                    }
                    break;

                case QuestorState.Debug_CloseQuestor:
                    //Logging.Log("ISBoxerCharacterSet: " + Settings.Instance.Lavish_ISBoxerCharacterSet);
                    //Logging.Log("Profile: " + Settings.Instance.Lavish_InnerspaceProfile);
                    //Logging.Log("Game: " + Settings.Instance.Lavish_Game);
                    Logging.Log("CloseQuestorCMDUplinkInnerspaceProfile: " + Settings.Instance.CloseQuestorCMDUplinkInnerspaceProfile);
                    Logging.Log("CloseQuestorCMDUplinkISboxerCharacterSet: " + Settings.Instance.CloseQuestorCMDUplinkIsboxerCharacterSet);
                    Logging.Log("walletbalancechangelogoffdelay: " + Settings.Instance.walletbalancechangelogoffdelay);
                    Logging.Log("walletbalancechangelogoffdelayLogofforExit: " + Settings.Instance.walletbalancechangelogoffdelayLogofforExit);
                    Logging.Log("walletbalancechangelogoffdelayLogofforExit: " + Settings.Instance.walletbalancechangelogoffdelayLogofforExit);
                    Logging.Log("EVEProcessMemoryCeiling: " + Settings.Instance.EVEProcessMemoryCeiling);
                    Logging.Log("EVEProcessMemoryCielingLogofforExit: " + Settings.Instance.EVEProcessMemoryCeilingLogofforExit);
                    State = QuestorState.Error;
                    return;
                    
                    
               case QuestorState.Debug_Windows:
                    var windows = new List<DirectWindow>();
            
                    foreach (var window in windows)
                    {
                        Logging.Log("Debug_Questor_WindowNames: [" + window.Name + "]");              
                    }
                    foreach (var window in windows)
                    {
                        Logging.Log("Debug_Windowcaptions: [" + window.Name + window.Caption + "]");              
                    }
                    foreach (var window in windows)
                    {
                        Logging.Log("Debug_WindowTypes: [" + window.Name + window.Type + "]");
                    }
                    State = QuestorState.Error;
                    return;

                case QuestorState.SalvageOnly:
                    Cache.Instance.OpenWrecks = true;
                    var SalvageOnlyCargo = Cache.Instance.DirectEve.GetShipsCargo();

                    // Is our cargo window open?
                    if (SalvageOnlyCargo.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                        break;
                    }

                    if (Cache.Instance.UnlootedContainers.Count() == 0)
                    {
                        Logging.Log("Salvage: Finished salvaging the room");

                        var bookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");

                        Logging.Log("Salvage: We have salvaged all bookmarks, waiting.");
                        State = QuestorState.Idle;
                        AutoStart = false;
                        Paused = true;
                        return;
                    }

                    closestWreck = Cache.Instance.UnlootedContainers.First();
                    if (closestWreck.Distance > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closestWreck.Id))
                    {
                        if (closestWreck.Distance > (int)Distance.WarptoDistance)
                            if (DateTime.Now.Subtract(_lastWarpTo).TotalSeconds > 10)
                            {
                                Logging.Log("Salvage: Warping to [" + closestWreck.Name + "] which is [" + Math.Round(closestWreck.Distance, 0) + "] meters away");
                                closestWreck.WarpTo();
                                _lastWarpTo = DateTime.Now;
                            }
                        else
                            closestWreck.Approach();
                    }
                    else if (closestWreck.Distance <= (int)Distance.SafeScoopRange && Cache.Instance.Approaching != null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                        Logging.Log("Questor: SalvageOnly: Stop ship, ClosestWreck [" + Math.Round(closestWreck.Distance,0) + "] is in scooprange + [" + (int)Distance.SafeScoopRange + "] and we were approaching");
                    }

                    try
                    {
                        // Overwrite settings, as the 'normal' settings do not apply
                        _salvage.MaximumWreckTargets = Math.Min(Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets, Cache.Instance.DirectEve.Me.MaxLockedTargets);
                        _salvage.ReserveCargoCapacity = 80;
                        _salvage.LootEverything = true;
                        _salvage.ProcessState();
                    }
                    finally
                    {
                        ApplySettings();
                    }
                    break;

                case QuestorState.GotoSalvageOnlyBookmark:
                    _traveler.ProcessState();
                    if (_traveler.State == TravelerState.AtDestination)
                    {
                        State = QuestorState.SalvageOnlyBookmarks;
                        _traveler.Destination = null;
                    }
                    if (Settings.Instance.DebugStates)
                        Logging.Log("Traveler.State = " + _traveler.State);
                    break;

                case QuestorState.SalvageOnlyBookmarks:
                    var SalvageOnlyBookmarksCargo = Cache.Instance.DirectEve.GetShipsCargo();
                    if (Cache.Instance.InStation)
                    {
                        // We are in a station,
                        Logging.Log("SalvageOnlyBookmarks: We're docked, undocking");
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdExitStation);
                    }
                    // Is our cargo window open?
                    if (SalvageOnlyBookmarksCargo.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                        break;
                    }
                    if (Settings.Instance.UnloadLootAtStation && SalvageOnlyBookmarksCargo.IsReady && (SalvageOnlyBookmarksCargo.Capacity - SalvageOnlyBookmarksCargo.UsedCapacity) < 100)
                    {
                        Logging.Log("Salvage: We are full");
                        State = QuestorState.Error;
                        return;
                    }
                    if (Cache.Instance.UnlootedContainers.Count() == 0)
                    {
                        Logging.Log("Salvage: Finished salvaging the room");
                        var bookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");
                        do
                        {
                            // Remove all bookmarks from address book
                            var bookmark = bookmarks.FirstOrDefault(b => Cache.Instance.DistanceFromMe(b.X ?? 0, b.Y ?? 0, b.Z ?? 0) < (int)Distance.BookmarksOnGridWithMe);
                            if (bookmark == null)
                                break;
                            bookmark.Delete();
                            bookmarks.Remove(bookmark);
                        } while (true);
                        if (bookmarks.Count == 0)
                        {
                            Logging.Log("Salvage: We have salvaged all bookmarks. Going to nearest station. ");
                            State = QuestorState.GotoNearestStation;
                        }
                        else
                        {
                            Logging.Log("Salvage: Goto the next salvage bookmark");
                            _traveler.Destination = new BookmarkDestination(bookmarks.OrderBy(b => b.CreatedOn).First());
                            State = QuestorState.GotoSalvageOnlyBookmark;
                            return;
                        }
                        break;
                    }
                    closestWreck = Cache.Instance.UnlootedContainers.First();
                    if (closestWreck.Distance > (int)Distance.SafeScoopRange && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closestWreck.Id))
                    {
                        if (closestWreck.Distance > (int)Distance.WarptoDistance)
                            if (DateTime.Now.Subtract(_lastWarpTo).TotalSeconds > 10)
                            {
                                Logging.Log("Salvage: Warping to [" + closestWreck.Name + "] which is [" + Math.Round(closestWreck.Distance/1000, 0) + "k away]");
                                closestWreck.WarpTo();
                                _lastWarpTo = DateTime.Now;
                            }
                        else
                            closestWreck.Approach();
                    }
                    else if (closestWreck.Distance <= (int)Distance.SafeScoopRange && Cache.Instance.Approaching != null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);
                        Logging.Log("Questor: SalvageOnlyBookmarks: Stop ship, ClosestWreck [" + Math.Round(closestWreck.Distance,0) + "] is in scooprange + [" + (int)Distance.SafeScoopRange + "] and we were approaching");
                    }

                    try
                    {
                        // Overwrite settings, as the 'normal' settings do not apply
                        _salvage.MaximumWreckTargets = Math.Min(Cache.Instance.DirectEve.ActiveShip.MaxLockedTargets,
                            Cache.Instance.DirectEve.Me.MaxLockedTargets);
                        _salvage.ReserveCargoCapacity = 80;
                        _salvage.LootEverything = true;
                        _salvage.ProcessState();
                    }
                    finally
                    {
                        ApplySettings();
                    }
                    break;

                case QuestorState.Traveler:
                    Cache.Instance.OpenWrecks = false;
                    var destination = Cache.Instance.DirectEve.Navigation.GetDestinationPath();
                    if (destination == null || destination.Count == 0)
                    {
                        // should never happen, but still...
                        Logging.Log("Traveler: (questor.cs) No destination?");
                        State = QuestorState.Error;
                        return;
                    }
                    else
                        if (destination.Count == 1 && destination.First() == 0)
                            destination[0] = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;
                    if (_traveler.Destination == null || _traveler.Destination.SolarSystemId != destination.Last())
                    {
                        var bookmarks = Cache.Instance.DirectEve.Bookmarks.Where(b => b.LocationId == destination.Last());
                        if (bookmarks != null && bookmarks.Count() > 0)
                            _traveler.Destination = new BookmarkDestination(bookmarks.OrderBy(b => b.CreatedOn).First());
                        else
                        {
                            Logging.Log("Traveler: (questor.cs) Destination: [" + Cache.Instance.DirectEve.Navigation.GetLocation(destination.Last()).Name + "]");
                            _traveler.Destination = new SolarSystemDestination(destination.Last());
                        }
                    }
                    else
                    {
                        _traveler.ProcessState();
                        //we know we are connected if we were able to arm the ship - update the lastknownGoodConnectedTime
                        //we also assume you are connected during a manul set of questor into travel mode (safe assumption considering someone is at the kb)
                        Cache.Instance.lastKnownGoodConnectedTime = DateTime.Now;
                        Cache.Instance.MyWalletBalance = Cache.Instance.DirectEve.Me.Wealth;

                        if (_traveler.State == TravelerState.AtDestination)
                        {
                            if (_missionController.State == MissionControllerState.Error)
                            {
                                Logging.Log("Questor stopped: (questor.cs) an error has occured");
                                State = QuestorState.Error;
                                return;
                            }
                            else if (Cache.Instance.InSpace)
                            {
                                Logging.Log("Traveler: (questor.cs) Arrived at destination (in space, Questor stopped)");
                                State = QuestorState.Error;
                                return;
                            }
                            else
                            {
                                Logging.Log("Traveler: (questor.cs) Arrived at destination");
                                State = QuestorState.Idle;
                                return;
                            }
                        }
                    }
                    break;
                case QuestorState.GotoNearestStation:
                    var station = Cache.Instance.Stations.OrderBy(x=>x.Distance).FirstOrDefault();
                    if (station != null)
                    {
                        if (station.Distance > (int)Distance.WarptoDistance)
                        {
                            station.WarpToAndDock();
                            State = QuestorState.Salvage;
                            break;
                        }
                        else
                        {
                           
                            if (station.Distance < 1900)
                            {
                                station.Dock();
 
                            }
                            else
                            {
                                if (Cache.Instance.DirectEve.ActiveShip.Entity.Mode == 1)
                                {
                                    if (Cache.Instance.Approaching.Id != station.Id)
                                        station.Approach();
                                }
                                else station.Approach();
                            }
                        }
                    }
                    else State=QuestorState.Error;
                    break;
            }
        }

        private bool GateInSalvage()
        {
            string target = "Acceleration Gate";

            var targets = Cache.Instance.EntitiesByName(target);
            if (targets == null || targets.Count() == 0)
                return false;
            _GatesPresent = true;
            return true;
        }
    }
}