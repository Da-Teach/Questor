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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Forms;
    using DirectEve;
    using global::Questor.Modules;
    using global::Questor.Storylines;
    using LavishScriptAPI;

    public class Questor
    {
        private frmMain m_Parent;
        private AgentInteraction _agentInteraction;
        private Arm _arm;
        private Combat _combat;
        private Defense _defense;
        private DirectEve _directEve;
        private Drones _drones;

        private DateTime _lastPulse;
        private MissionController _missionController;
        private Panic _panic;
        private Storyline _storyline;

        private Salvage _salvage;
        private Traveler _traveler;
        private UnloadLoot _unloadLoot;

        private DateTime _lastAction;
        private Random _random;
        private int _randomDelay;

        private double _lastX;
        private double _lastY;
        private double _lastZ;
        private bool _GatesPresent;

        public Questor(frmMain form1)
        {
            m_Parent = form1;
            _lastPulse = DateTime.MinValue;

            _random = new Random();

            _salvage = new Salvage();
            _defense = new Defense();
            _combat = new Combat();
            _traveler = new Traveler();
            _unloadLoot = new UnloadLoot();
            _agentInteraction = new AgentInteraction();
            _arm = new Arm();
            _missionController = new MissionController();
            _drones = new Drones();
            _panic = new Panic();
            _storyline = new Storyline();

            Settings.Instance.SettingsLoaded += SettingsLoaded;

            // State fixed on ExecuteMission
            State = QuestorState.Idle;

            _directEve = new DirectEve();
            Cache.Instance.DirectEve = _directEve;

            Cache.Instance.StopTimeSpecified = Program.stopTimeSpecified;
            Cache.Instance.StopTime = Program._stopTime;

            _directEve.OnFrame += OnFrame;
        }

        public QuestorState State { get; set; }

        public bool AutoStart { get; set; }
        public bool Paused { get; set; }
        public bool Disable3D { get; set; }
        public bool ValidSettings { get; set; }
        public bool ExitWhenIdle { get; set; }

        public string CharacterName { get; set; }

        // Statistics information
        public DateTime Started { get; set; }
        public string Mission { get; set; }
        public double Wealth { get; set; }
        public double LootValue { get; set; }
        public int LoyaltyPoints { get; set; }
        public int LostDrones { get; set; }

   
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

            var agent = Cache.Instance.DirectEve.GetAgentByName(Settings.Instance.AgentName);
            if (agent == null || !agent.IsValid)
            {
                Logging.Log("Settings: Unable to locate agent [" + Settings.Instance.AgentName + "]");
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
            if (DateTime.Now.Subtract(_lastPulse).TotalMilliseconds < 1500)
                return;
            _lastPulse = DateTime.Now;

            // Session is not ready yet, do not continue
            if (!Cache.Instance.DirectEve.Session.IsReady)
                return;

            // If Questor window not visible, show it
            if (!m_Parent.Visible)
                m_Parent.Visible = true;

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
                if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 15)
                {
                    ValidateSettings();
                    _lastAction = DateTime.Now;
                }
                return;
            }

            foreach (var window in Cache.Instance.Windows)
            {
                // Telecom messages are generally mission info messages
                if (window.Name == "telecom")
                {
                    Logging.Log("Questor: Closing telecom message...");
                    Logging.Log("Questor: Content of telecom window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                    window.Close();
                }

                // Modal windows must be closed
                // But lets only close known modal windows
                if (window.Name == "modal")
                {
                    bool close = false;
                    if (!string.IsNullOrEmpty(window.Html))
                    {
                        // Server going down
                        close |= window.Html.Contains("Please make sure your characters are out of harms way");
                        // In space "shit"
                        close |= window.Html.Contains("Item cannot be moved back to a loot container.");
                        close |= window.Html.Contains("you do not have the cargo space");
                        close |= window.Html.Contains("cargo units would be required to complete this operation.");
                        close |= window.Html.Contains("You are too far away from the acceleration gate to activate it!");
                        close |= window.Html.Contains("maximum distance is 2500 meters");
                        // Stupid warning, lets see if we can find it
                        close |= window.Html.Contains("Do you wish to proceed with this dangerous action?");
                        // Yes we know the mission isnt complete, Questor will just redo the mission
                        close |= window.Html.Contains("Please check your mission journal for further information.");
			            // Lag :/
                        close |= window.Html.Contains("This gate is locked!");
                        close |= window.Html.Contains("The Zbikoki's Hacker Card");
                        close |= window.Html.Contains(" units free.");
                    }

                    if (close)
                    {
                        Logging.Log("Questor: Closing modal window...");
                        Logging.Log("Questor: Content of modal window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                        window.Close();
                    }
                }
            }

            // We always check our defense state if we're in space, regardless of questor state
            // We also always check panic
            if (Cache.Instance.InSpace)
            {
                watch.Reset();
                watch.Start();
                _defense.ProcessState();
                watch.Stop();

                if (Settings.Instance.DebugPerformance)
                    Logging.Log("Defense.ProcessState took " + watch.ElapsedMilliseconds + "ms");
            }

            // Defense is more important then pause, rest (even panic) isnt!
            if (Paused)
                return;

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
                    if (Cache.Instance.StopTimeSpecified)
                    {
                        if (DateTime.Now >= Cache.Instance.StopTime)
                        {
                            Logging.Log("Time to stop.  Quitting game.");
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdQuitGame);
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
                    
                    mission = Cache.Instance.GetAgentMission(Cache.Instance.AgentId);
                    if (!string.IsNullOrEmpty(Mission) && (mission == null || mission.Name != Mission || mission.State != (int)MissionState.Accepted))
                    {
                        // Do not save statistics if loyalty points == -1
                        // Seeing as we completed a mission, we will have loyalty points for this agent
                        if (Cache.Instance.Agent.LoyaltyPoints == -1)
                            return;

                        // Get the path
                        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        var filename = Path.Combine(path, Cache.Instance.FilterPath(CharacterName) + ".statistics.log");

                        // Write the header
                        if (!File.Exists(filename))
                            File.AppendAllText(filename, "Date;Mission;Time;Isk;Loot;LP;\r\n");

                        // Build the line
                        var line = DateTime.Now + ";";
                        line += Mission + ";";
                        line += ((int)DateTime.Now.Subtract(Started).TotalMinutes) + ";";
                        line += ((int)(Cache.Instance.DirectEve.Me.Wealth - Wealth)) + ";";
                        line += ((int)LootValue) + ";";
                        line += (Cache.Instance.Agent.LoyaltyPoints - LoyaltyPoints) + ";\r\n";

                        // The mission is finished
                        File.AppendAllText(filename, line);
                        
                        // Disable next log line
                        Mission = null;
                    }

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
                        }
                        else
                            State = QuestorState.Start;
                    }
                    else if (ExitWhenIdle)
                        LavishScript.ExecuteCommand("exit");
                    break;

                case QuestorState.DelayedStart:
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < _randomDelay)
                        break;

                    State = QuestorState.Start;
                    break;


                case QuestorState.DelayedGotoBase:
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 15)
                        break;

                    Logging.Log("Questor: Heading back to base");
                    State = QuestorState.GotoBase;
                    break;

                case QuestorState.Start:
                    if (_agentInteraction.State == AgentInteractionState.Idle)
                    {
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

                        // Update statistic values
                        Wealth = Cache.Instance.DirectEve.Me.Wealth;
                        LootValue = 0;
                        LoyaltyPoints = Cache.Instance.Agent.LoyaltyPoints;
                        Started = DateTime.Now;
                        Mission = string.Empty;
                        LostDrones = 0;
                    }

                    _agentInteraction.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("AgentInteraction.State = " + _agentInteraction.State);

                    if (_agentInteraction.State == AgentInteractionState.Done)
                    {
                        mission = Cache.Instance.GetAgentMission(Cache.Instance.AgentId);
                        if (mission != null)
                        {
                            // Update loyalty points again (the first time might return -1)
                            LoyaltyPoints = Cache.Instance.Agent.LoyaltyPoints;
                            Mission = mission.Name;
                        }

                        _agentInteraction.State = AgentInteractionState.Idle;
                        State = QuestorState.Arm;
                    }
                    break;

                case QuestorState.Arm:
                    if (_arm.State == ArmState.Idle)
                    {
                        Logging.Log("Arm: Begin");
                        _arm.State = ArmState.Begin;

                        // Load right ammo based on mission
                        _arm.AmmoToLoad.Clear();
                        _arm.AmmoToLoad.AddRange(_agentInteraction.AmmoToLoad);
                    }

                    _arm.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Arm.State = " + _arm.State);

                    if (_arm.State == ArmState.Done)
                    {
                        _arm.State = ArmState.Idle;
                        State = QuestorState.GotoMission;
                    }
                    break;

                case QuestorState.GotoMission:
                    var missionDestination = _traveler.Destination as MissionBookmarkDestination;
                    if (missionDestination == null || missionDestination.AgentId != Cache.Instance.AgentId) // We assume that this will always work "correctly" (tm)
                        _traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(Cache.Instance.AgentId, "Encounter"));

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
                    var baseDestination = _traveler.Destination as StationDestination;
                    if (baseDestination == null || baseDestination.StationId != Cache.Instance.Agent.StationId)
                        _traveler.Destination = new StationDestination(Cache.Instance.Agent.SolarSystemId, Cache.Instance.Agent.StationId, Cache.Instance.DirectEve.GetLocationName(Cache.Instance.Agent.StationId));

                    if (Cache.Instance.PriorityTargets.Any(pt => pt != null && pt.IsValid))
                    {
                        Logging.Log("GotoBase: Priority targets found, engaging!");
                        _combat.ProcessState();
                    }

                    _traveler.ProcessState();
                    if (_traveler.State == TravelerState.AtDestination)
                    {
                        mission = Cache.Instance.GetAgentMission(Cache.Instance.AgentId);
                        if (_missionController.State == MissionControllerState.Error)
                            State = QuestorState.Error;
                        else if (_combat.State != CombatState.OutOfAmmo && mission != null && mission.State == (int)MissionState.Accepted)
                            State = QuestorState.CompleteMission;
                        else
                            State = QuestorState.UnloadLoot;

                        _traveler.Destination = null;
                    }

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Traveler.State = " + _traveler.State);
                    break;

                case QuestorState.CompleteMission:
                    if (_agentInteraction.State == AgentInteractionState.Idle)
                    {
                        // Lost drone statistics
                        // (inelegantly located here so as to avoid the necessity to switch to a combat ship after salvaging)
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
                            LostDrones = (int)Math.Floor((droneBay.Capacity - droneBay.UsedCapacity) / drone.Volume);
                            Logging.Log("DroneStats: Logging the number of lost drones: " + LostDrones.ToString());
                            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                            var dronelogfilename = Path.Combine(path, Cache.Instance.FilterPath(CharacterName) + ".dronestats.log");
                            if (!File.Exists(dronelogfilename))
                                File.AppendAllText(dronelogfilename, "Mission;Number of lost drones\r\n");
                            var droneline = Mission + ";";
                            droneline += ((int)LostDrones) + ";\r\n";
                            File.AppendAllText(dronelogfilename, droneline);
                        }
                        else
                        {
                            Logging.Log("DroneStats: Couldn't find the drone TypeID specified in the settings.xml; this shouldn't happen!");
                        }                   
                        // Lost drone statistics stuff ends here

                        Logging.Log("AgentInteraction: Start Conversation [Complete Mission]");

                        _agentInteraction.State = AgentInteractionState.StartConversation;
                        _agentInteraction.Purpose = AgentInteractionPurpose.CompleteMission;
                    }

                    _agentInteraction.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("AgentInteraction.State = " + _agentInteraction.State);

                    if (_agentInteraction.State == AgentInteractionState.Done)
                    {
                        _agentInteraction.State = AgentInteractionState.Idle;
                        State = QuestorState.UnloadLoot;
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

                        _unloadLoot.State = UnloadLootState.Idle;

                        // Update total loot value
                        LootValue += _unloadLoot.LootValue;

                        mission = Cache.Instance.GetAgentMission(Cache.Instance.AgentId);
                        if (_combat.State != CombatState.OutOfAmmo && Settings.Instance.AfterMissionSalvaging && Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ").Count > 0 && (mission == null || mission.State == (int)MissionState.Offered))
                            State = QuestorState.BeginAfterMissionSalvaging;
                        else if (_combat.State == CombatState.OutOfAmmo)
                            State = QuestorState.Start;
                        else
                            State = QuestorState.Idle;
                    }
                    break;

                case QuestorState.BeginAfterMissionSalvaging:
                    _GatesPresent = false;
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
                            break;
                        }

                        State = QuestorState.GotoSalvageBookmark;
                        _traveler.Destination = new BookmarkDestination(bookmark);
                    }
                    break;

                case QuestorState.GotoSalvageBookmark:
                    _traveler.ProcessState();
                    string target = "Acceleration Gate";
                    var targets = Cache.Instance.EntitiesByName(target);
                    if (_traveler.State == TravelerState.AtDestination || GateInSalvage())
                    {
                        State = QuestorState.Salvage;
                        _traveler.Destination = null;
                    }

                    if (Settings.Instance.DebugStates)
                        Logging.Log("Traveler.State = " + _traveler.State);
                    break;

                case QuestorState.Salvage:
                    var cargo = Cache.Instance.DirectEve.GetShipsCargo();
                    
                    // Is our cargo window open?
                    if (cargo.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                        break;
                    }

                    if (Settings.Instance.UnloadLootAtStation && cargo.IsReady && (cargo.Capacity - cargo.UsedCapacity) < 100)
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
                            var bookmark = bookmarks.FirstOrDefault(b => Cache.Instance.DistanceFromMe(b.X ?? 0, b.Y ?? 0, b.Z ?? 0) < 250000);
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
                            State = QuestorState.GotoBase;
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
                    if (closestWreck.Distance > 2500 && (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closestWreck.Id))
                    {
                        if (closestWreck.Distance > 150000)
                            closestWreck.WarpTo();
                        else
                            closestWreck.Approach();
                    }
                    else if (closestWreck.Distance <= 2500 && Cache.Instance.Approaching != null)
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdStopShip);

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


                case QuestorState.SalvageUseGate:

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
                    if (closest.Distance < 2500)
                    {
                        Logging.Log("Salvage: Acceleration gate found - GroupID=" + closest.GroupId);

                        // Activate it and move to the next Pocket
                        closest.Activate();

                        // Do not change actions, if NextPocket gets a timeout (>2 mins) then it reverts to the last action
                        Logging.Log("Salvage: Activate [" + closest.Name + "] and change state to 'NextPocket'");

                        State = QuestorState.SalvageNextPocket;
                        _lastPulse = DateTime.Now;
                    }
                    else if (closest.Distance < 150000)
                    {
                        // Move to the target
                        if (Cache.Instance.Approaching == null || Cache.Instance.Approaching.Id != closest.Id)
                        {
                            Logging.Log("Salvage: Approaching target [" + closest.Name + "][" + closest.Id + "]");
                            closest.Approach();
                        }
                    }
                    else
                    {
                        // Probably never happens
                        closest.WarpTo();
                    }
                    _lastPulse = DateTime.Now.AddSeconds(10);
                    break;

                case QuestorState.SalvageNextPocket:
                    var distance = Cache.Instance.DistanceFromMe(_lastX, _lastY, _lastZ);
                    if (distance > 100000)
                    {
                        Logging.Log("Salvage: We've moved to the next Pocket [" + distance + "]");

                        State = QuestorState.Salvage;
                    }
                    else if (DateTime.Now.Subtract(_lastPulse).TotalMinutes > 2)
                    {
                        Logging.Log("Salvage: We've timed out, retry last action");

                        // We have reached a timeout, revert to ExecutePocketActions (e.g. most likely Activate)
                        State = QuestorState.SalvageUseGate;
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

				case QuestorState.Traveler:
					var destination = Cache.Instance.DirectEve.Navigation.GetDestinationPath();
					if (destination == null || destination.Count == 0)
					{
						// should never happen, but still...
						Logging.Log("Traveler: No destination?");
						State = QuestorState.Error;
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
							Logging.Log("Traveler: Destination: [" + Cache.Instance.DirectEve.Navigation.GetLocation(destination.Last()).Name + "]");
							_traveler.Destination = new SolarSystemDestination(destination.Last());
						}
					}
					else
					{
						_traveler.ProcessState();
						if (_traveler.State == TravelerState.AtDestination)
						{
							if (_missionController.State == MissionControllerState.Error)
							{
								Logging.Log("Questor stopped: an error has occured");
								State = QuestorState.Error;
							}
							else if (Cache.Instance.InSpace)
							{
								Logging.Log("Traveler: Arrived at destination (in space, Questor stopped)");
								State = QuestorState.Error;
							}
							else
							{
								Logging.Log("Traveler: Arrived at destination");
								State = QuestorState.Idle;
							}
						}		
					}
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