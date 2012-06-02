// ------------------------------------------------------------------------------
// <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
// Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
// Please look in the accompanying license.htm file for the license that
// applies to this source code. (a copy can also be found at:
// http://www.thehackerwithin.com/license.htm)
// </copyright>
// -------------------------------------------------------------------------------

using Questor.Modules.Caching;

namespace Questor.Modules.Lookup
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml.Linq;
    using LavishScriptAPI;
    using System.Globalization;
    using InnerSpaceAPI;
    using Questor.Modules.Actions;
    using Questor.Modules.Logging;

    public class Settings
    {
        /// <summary>
        /// Singleton implementation
        /// </summary>
        public static Settings Instance = new Settings();

        public string CharacterName;
        private DateTime _lastModifiedDate;
        private readonly Random _random = new Random();

        public Settings()
        {
            Ammo = new List<Ammo>();
            ItemsBlackList = new List<int>();
            WreckBlackList = new List<int>();
            AgentsList = new List<AgentsList>();
            FactionFitting = new List<FactionFitting>();
            MissionFitting = new List<MissionFitting>();
            MissionBlacklist = new List<string>();
            MissionGreylist = new List<string>();

            FactionBlacklist = new List<string>();
            UseFittingManager = true;
            DefaultFitting = new FactionFitting();
        }

        public bool AtLoginScreen { get; set; }

        public bool CharacterXMLExists = true;
        public bool SchedulesXMLExists = true;
        public bool FactionXMLExists = true;
        public bool QuestorStatisticsExists = true;
        public bool QuestorSettingsExists = true;
        public bool QuestorManagerExists = true;

        //
        // Debug Variables
        //
        public bool DebugStates { get; set; }

        public bool DebugPerformance { get; set; }

        public bool DetailedCurrentTargetHealthLogging { get; set; }

        public bool DebugLootWrecks { get; set; }

        public bool DebugActivateWeapons { get; set; }
        public bool DebugReloadorChangeAmmo { get; set; }
        public bool DebugStatistics { get; set; }

        public bool DebugGotobase { get; set; }

        public bool DebugIdle { get; set; }

        public bool DebugAutoStart { get; set; }

        public bool UseInnerspace { get; set; }

        //
        // Misc Settings
        //
        public string CharacterMode { get; set; }

        public bool AutoStart { get; set; }

        public bool Disable3D { get; set; }

        public int MinimumDelay { get; set; }

        public int RandomDelay { get; set; }

        //
        // Console Log Settings
        //
        public bool SaveConsoleLog { get; set; }

        public int MaxLineConsole { get; set; }

        //
        // Enable / Disable Major Features that do not have categories of their own below
        //
        public bool EnableStorylines { get; set; }

        public bool UseLocalWatch { get; set; }

        public bool UseFittingManager { get; set; }

        //
        // Agent and mission settings
        //
        public string MissionName { get; set; }

        public float MinAgentBlackListStandings { get; set; }

        public float MinAgentGreyListStandings { get; set; }

        public string MissionsPath { get; set; }

        public bool LowSecMissionsInShuttles { get; set; }

        public bool WaitDecline { get; set; }

        public bool MultiAgentSupport { get; private set; }
        //
        // KillSentries Setting
        //
        private bool _killSentries;
        public bool KillSentries
        {
            get
            {
                if (Cache.Instance.MissionKillSentries != null)
                    return (bool)Cache.Instance.MissionKillSentries;
                return _killSentries;
            }
            set
            {
                _killSentries = value;
            }
        }

        //
        // Local Watch settings - if enabled
        //
        public int LocalBadStandingPilotsToTolerate { get; set; }

        public double LocalBadStandingLevelToConsiderBad { get; set; }

        public bool FinishWhenNotSafe { get; set; }

        //
        // Invasion Settings
        //
        public int BattleshipInvasionLimit { get; set; }

        public int BattlecruiserInvasionLimit { get; set; }

        public int CruiserInvasionLimit { get; set; }

        public int FrigateInvasionLimit { get; set; }

        public int InvasionMinimumDelay { get; set; }

        public int InvasionRandomDelay { get; set; }

        //
        // Ship Names
        //
        public string CombatShipName { get; set; }

        public string SalvageShipName { get; set; }

        public string TransportShipName { get; set; }

        public string TravelShipName { get; set; }

        //
        // Storage location for loot, ammo, and bookmarks
        //
        public string LootHangar { get; set; }

        public string AmmoHangar { get; set; }

        public string BookmarkHangar { get; set; }

        public string LootContainer { get; set; }

        public bool MoveCommonMissionCompletionItemsToAmmoHangar { get; set; }

        //
        // Salvage and Loot settings
        //
        public bool CreateSalvageBookmarks { get; set; }

        public string CreateSalvageBookmarksIn { get; set; }

        public bool SalvageMultpleMissionsinOnePass { get; set; }

        public bool FirstSalvageBookmarksInSystem { get; set; }

        public string BookmarkPrefix { get; set; }

        public string UndockPrefix { get; set; }

        public int UndockDelay { get; set; }

        public int MinimumWreckCount { get; set; }

        public bool AfterMissionSalvaging { get; set; }

        public bool UnloadLootAtStation { get; set; }

        public bool UseGatesInSalvage { get; set; }

        public bool LootEverything { get; set; }

        public int ReserveCargoCapacity { get; set; }

        public int MaximumWreckTargets { get; set; }
        public int AgeofBookmarksForSalvageBehavior { get; set; } //in minutes

        //
        // undocking settings
        //
        public string BookmarkWarpOut { get; set; }

        //
        // EVE Process Memory Ceiling and EVE wallet balance Change settings
        //
        public int Walletbalancechangelogoffdelay { get; set; }

        public string WalletbalancechangelogoffdelayLogofforExit { get; set; }

        public Int64 EVEProcessMemoryCeiling { get; set; }

        public string EVEProcessMemoryCeilingLogofforExit { get; set; }

        public bool CloseQuestorCMDUplinkInnerspaceProfile { get; set; }

        public bool CloseQuestorCMDUplinkIsboxerCharacterSet { get; set; }

        public bool CloseQuestorArbitraryOSCmd { get; set; }

        public string CloseQuestorOSCmdContents { get; set; }

        public int SecondstoWaitAfterExteringCloseQuestorBeforeExitingEVE = 240;

        public string LavishIsBoxerCharacterSet { get; set; }

        public string LavishInnerspaceProfile { get; set; }

        public string LavishGame { get; set; }

        //public int missionbookmarktoagentloops { get; set; }  //not yet used - although it is likely a good ide to fix it so it is used - it would eliminate going back and fourth to the same mission over and over

        public List<int> ItemsBlackList { get; set; }

        public List<int> WreckBlackList { get; set; }

        public bool WreckBlackListSmallWrecks { get; set; }

        public bool WreckBlackListMediumWrecks { get; set; }

        public string Logpath { get; set; }

        public bool SessionsLog { get; set; }

        public string SessionsLogPath { get; set; }

        public string SessionsLogFile { get; set; }

        public bool ConsoleLog { get; set; }

        public string ConsoleLogPath { get; set; }

        public string ConsoleLogFile { get; set; }

        public bool DroneStatsLog { get; set; }

        public string DroneStatsLogPath { get; set; }

        public string DroneStatslogFile { get; set; }

        public bool WreckLootStatistics { get; set; }

        public string WreckLootStatisticsPath { get; set; }

        public string WreckLootStatisticsFile { get; set; }

        public bool MissionStats1Log { get; set; }

        public string MissionStats1LogPath { get; set; }

        public string MissionStats1LogFile { get; set; }

        public bool MissionStats2Log { get; set; }

        public string MissionStats2LogPath { get; set; }

        public string MissionStats2LogFile { get; set; }

        public bool MissionStats3Log { get; set; }

        public string MissionStats3LogPath { get; set; }

        public string MissionStats3LogFile { get; set; }

        public bool PocketStatistics { get; set; }

        public string PocketStatisticsPath { get; set; }

        public string PocketStatisticsFile { get; set; }

        public bool PocketObjectStatistics { get; set; }

        public string PocketObjectStatisticsPath { get; set; }

        public string PocketObjectStatisticsFile { get; set; }

        public bool PocketStatsUseIndividualFilesPerPocket = true;

        //
        // Fitting Settings - if enabled
        //
        public List<FactionFitting> FactionFitting { get; private set; }

        public List<AgentsList> AgentsList { get; set; }

        public List<MissionFitting> MissionFitting { get; private set; }

        public FactionFitting DefaultFitting { get; set; }

        //
        // Weapon Settings
        //
        public bool DontShootFrigatesWithSiegeorAutoCannons { get; set; }

        public int WeaponGroupId { get; set; }

        public int MaximumHighValueTargets { get; set; }

        public int MaximumLowValueTargets { get; set; }

        public int MinimumAmmoCharges { get; set; }

        public List<Ammo> Ammo { get; private set; }

        //
        // Speed and Movement Settings
        //
        public bool SpeedTank { get; set; }

        public int OrbitDistance { get; set; }

        public int OptimalRange { get; set; }

        public int NosDistance { get; set; }

        public int MinimumPropulsionModuleDistance { get; set; }

        public int MinimumPropulsionModuleCapacitor { get; set; }

        //
        // Tank Settings
        //
        public int ActivateRepairModules { get; set; }

        public int DeactivateRepairModules { get; set; }

        //
        // Panic Settings
        //
        public int MinimumShieldPct { get; set; }

        public int MinimumArmorPct { get; set; }

        public int MinimumCapacitorPct { get; set; }

        public int SafeShieldPct { get; set; }

        public int SafeArmorPct { get; set; }

        public int SafeCapacitorPct { get; set; }

        public double IskPerLP { get; set; }

        //
        // Drone Settings
        //
        private bool _useDrones;

        public bool UseDrones
        {
            get
            {
                if (Cache.Instance.MissionUseDrones != null)
                    return (bool)Cache.Instance.MissionUseDrones;
                return _useDrones;
            }
            set
            {
                _useDrones = value;
            }
        }

        public int DroneTypeId { get; set; }

        public int DroneControlRange { get; set; }

        public int DroneMinimumShieldPct { get; set; }

        public int DroneMinimumArmorPct { get; set; }

        public int DroneMinimumCapacitorPct { get; set; }

        public int DroneRecallShieldPct { get; set; }

        public int DroneRecallArmorPct { get; set; }

        public int DroneRecallCapacitorPct { get; set; }

        public int LongRangeDroneRecallShieldPct { get; set; }

        public int LongRangeDroneRecallArmorPct { get; set; }

        public int LongRangeDroneRecallCapacitorPct { get; set; }

        public bool DronesKillHighValueTargets { get; set; }

        public int MaterialsForWarOreID { get; set; }

        public int MaterialsForWarOreQty { get; set; }

        //
        // Mission Blacklist / Greylist Settings
        //
        public List<string> MissionBlacklist { get; private set; }

        public List<string> MissionGreylist { get; private set; }

        public List<string> FactionBlacklist { get; private set; }

        //
        // Questor GUI location settings
        //
        public int? WindowXPosition { get; set; }

        public int? WindowYPosition { get; set; }

        //
        // path information - used to load the XML and used in other modules
        //
        public string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public string CharacterNameXML { get; private set; }

        public string SettingsPath { get; private set; }

        public event EventHandler<EventArgs> SettingsLoaded;

        public bool Defaultsettingsloaded;

        public void LoadSettings()
        {
            Settings.Instance.CharacterNameXML = Cache.Instance.DirectEve.Me.Name;
            Settings.Instance.SettingsPath = System.IO.Path.Combine(Settings.Instance.Path, Cache.Instance.FilterPath(Settings.Instance.CharacterNameXML) + ".xml");
            bool reloadSettings = true;
            if (File.Exists(Settings.Instance.SettingsPath))
                reloadSettings = _lastModifiedDate != File.GetLastWriteTime(Settings.Instance.SettingsPath);

            if (!reloadSettings)
                return;

            _lastModifiedDate = File.GetLastWriteTime(SettingsPath);

            Settings.Instance.FactionXMLExists = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "faction.XML"));
            Settings.Instance.SchedulesXMLExists = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "schedules.XML"));
            Settings.Instance.QuestorManagerExists = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "QuestorManager.exe"));
            Settings.Instance.QuestorSettingsExists = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "QuestorSettings.exe"));
            Settings.Instance.QuestorStatisticsExists = File.Exists(System.IO.Path.Combine(Settings.Instance.Path, "QuestorStatistics.exe"));

            if (!File.Exists(Settings.Instance.SettingsPath) && !Defaultsettingsloaded) //if the settings file does not exist initialize these values. Should we not halt when missing the settings XML?
            {
                Settings.Instance.CharacterXMLExists = false;
                Defaultsettingsloaded = true;
                //LavishScript.ExecuteCommand("log " + Cache.Instance.DirectEve.Me.Name + ".log");
                //LavishScript.ExecuteCommand("uplink echo Settings: unable to find [" + Settings.Instance.SettingsPath + "] loading default (bad! bad! bad!) settings: you should fix this! NOW.");
                Logging.Log("Settings", "WARNING! unable to find [" + Settings.Instance.SettingsPath + "] loading default generic, and likely incorrect, settings: WARNING!", Logging.orange);
                DebugStates = false;
                //enables more console logging having to do with the sub-states within each state
                DebugPerformance = false;
                //enabled more console logging having to do with the time it takes to execute each state
                DetailedCurrentTargetHealthLogging = false;
                DebugLootWrecks = false;
                DebugActivateWeapons = false;
                DebugReloadorChangeAmmo = false;
                DebugStatistics = false;
                DebugGotobase = false;
                DebugIdle = false;
                DebugAutoStart = false;
                UseInnerspace = true;
                //
                // Misc Settings
                //
                CharacterMode = "none";
                AutoStart = false; // auto Start enabled or disabled by default?
                SaveConsoleLog = true; // save the console log to file
                MaxLineConsole = 1000;
                // maximum console log lines to show in the GUI
                Disable3D = false; // Disable3d graphics while in space
                RandomDelay = 15;
                MinimumDelay = 20;
                //
                // Enable / Disable Major Features that do not have categories of their own below
                //
                UseFittingManager = false;
                EnableStorylines = false;
                UseLocalWatch = false;

                // Console Log Settings
                //
                SaveConsoleLog = false;
                MaxLineConsole = 1000;
                //
                // Agent Standings and Mission Settings
                //
                MinAgentBlackListStandings = 1;
                MinAgentGreyListStandings = (float)-1.7;
                WaitDecline = false;
                const string relativeMissionsPath = "Missions";
                MissionsPath = System.IO.Path.Combine(Settings.Instance.Path, relativeMissionsPath);
                Logging.Log("Settings", "MissionsPath is: [" + MissionsPath + "]", Logging.white);

                LowSecMissionsInShuttles = false;
                MaterialsForWarOreID = 20;
                MaterialsForWarOreQty = 8000;
                KillSentries = false;
                //
                // Local Watch Settings - if enabled
                //
                LocalBadStandingPilotsToTolerate = 1;
                LocalBadStandingLevelToConsiderBad = -0.1;
                //
                // Invasion Settings
                //
                BattleshipInvasionLimit = 2;
                // if this number of battleships lands on grid while in a mission we will enter panic
                BattlecruiserInvasionLimit = 2;
                // if this number of battlecruisers lands on grid while in a mission we will enter panic
                CruiserInvasionLimit = 2;
                // if this number of cruisers lands on grid while in a mission we will enter panic
                FrigateInvasionLimit = 2;
                // if this number of frigates lands on grid while in a mission we will enter panic
                InvasionRandomDelay = 30; // random relay to stay docked
                InvasionMinimumDelay = 30; // minimum delay to stay docked

                //
                // Questor GUI Window Position
                //
                WindowXPosition = 400;
                WindowYPosition = 600;
                //
                // Salvage and loot settings
                //
                ReserveCargoCapacity = 0;
                MaximumWreckTargets = 0;

                //
                // at what memory usage do we need to restart this session?
                //
                EVEProcessMemoryCeiling = 900;
                EVEProcessMemoryCeilingLogofforExit = "exit";

                CloseQuestorCMDUplinkInnerspaceProfile = true;
                CloseQuestorCMDUplinkIsboxerCharacterSet = false;

                CloseQuestorArbitraryOSCmd = false; //true or false
                CloseQuestorOSCmdContents = string.Empty;
                //the above setting can be set to any script or commands available on the system. make sure you test it from a command prompt while in your .net programs directory

                Walletbalancechangelogoffdelay = 30;
                WalletbalancechangelogoffdelayLogofforExit = "exit";
                SecondstoWaitAfterExteringCloseQuestorBeforeExitingEVE = 240;

                //
                // Value - Used in calculations
                //
                IskPerLP = 600; //used in value calculations

                //
                // Undock settings
                //
                UndockDelay = 10; //Delay when undocking - not in use
                UndockPrefix = "Insta";
                //Undock bookmark prefix - used by traveler - not in use
                BookmarkWarpOut = "";

                //
                // Location of the Questor GUI on startup (default is off the screen)
                //
                WindowXPosition = 600;
                //windows position (needs to be changed, default is off screen)
                WindowYPosition = 400;
                //windows position (needs to be changed, default is off screen)

                //
                // Ship Names
                //
                CombatShipName = "Raven";
                SalvageShipName = "Noctis";
                TransportShipName = "Transport";
                TravelShipName = "Travel";

                //
                // Storage Location for Loot, Ammo, Bookmarks
                //
                LootHangar = String.Empty;
                AmmoHangar = String.Empty;
                BookmarkHangar = String.Empty;
                LootContainer = String.Empty;
                MoveCommonMissionCompletionItemsToAmmoHangar = false;
                //
                // Loot and Salvage Settings
                //
                LootEverything = true;
                UseGatesInSalvage = false;
                // if our mission does not despawn (likely someone in the mission looting our stuff?) use the gates when salvaging to get to our bookmarks
                CreateSalvageBookmarks = false;
                CreateSalvageBookmarksIn = "Player"; //Player or Corp
                //other setting is "Corp"
                BookmarkPrefix = "Salvage:";
                MinimumWreckCount = 1;
                AfterMissionSalvaging = false;
                FirstSalvageBookmarksInSystem = false;
                SalvageMultpleMissionsinOnePass = false;
                UnloadLootAtStation = false;
                ReserveCargoCapacity = 100;
                MaximumWreckTargets = 0;
                WreckBlackListSmallWrecks = false;
                WreckBlackListMediumWrecks = false;
                AgeofBookmarksForSalvageBehavior = 45;

                //
                // Enable / Disable the different types of logging that are available
                //
                SessionsLog = false;
                DroneStatsLog = false;
                WreckLootStatistics = false;
                MissionStats1Log = false;
                MissionStats2Log = false;
                MissionStats3Log = false;
                PocketStatistics = false;
                PocketStatsUseIndividualFilesPerPocket = false;

                //
                // Weapon and targeting Settings
                //
                WeaponGroupId = 506; //cruise
                DontShootFrigatesWithSiegeorAutoCannons = false;
                MaximumHighValueTargets = 2;
                MaximumLowValueTargets = 2;

                //
                // Speed and Movement Settings
                //
                SpeedTank = false;
                OrbitDistance = 0;
                OptimalRange = 0;
                NosDistance = 38000;
                MinimumPropulsionModuleDistance = 5000;
                MinimumPropulsionModuleCapacitor = 0;

                //
                // Tanking Settings
                //
                ActivateRepairModules = 65;
                DeactivateRepairModules = 95;

                //
                // Panic Settings
                //
                MinimumShieldPct = 50;
                MinimumArmorPct = 50;
                MinimumCapacitorPct = 50;
                SafeShieldPct = 0;
                SafeArmorPct = 0;
                SafeCapacitorPct = 0;

                //
                // Drone Settings
                //
                UseDrones = true;
                DroneTypeId = 2488;
                DroneControlRange = 25000;
                DroneMinimumShieldPct = 50;
                DroneMinimumArmorPct = 50;
                DroneMinimumCapacitorPct = 0;
                DroneRecallShieldPct = 0;
                DroneRecallArmorPct = 0;
                DroneRecallCapacitorPct = 0;
                LongRangeDroneRecallShieldPct = 0;
                LongRangeDroneRecallArmorPct = 0;
                LongRangeDroneRecallCapacitorPct = 0;
                DronesKillHighValueTargets = false;

                //
                // Storage Location for Loot, Ammo, Bookmarks, default is local hangar
                //
                LootHangar = string.Empty;
                AmmoHangar = string.Empty;
                BookmarkHangar = string.Empty;
                LootContainer = string.Empty;
                MoveCommonMissionCompletionItemsToAmmoHangar = false;

                MaximumHighValueTargets = 0;
                MaximumLowValueTargets = 0;

                //
                // Clear various lists
                //
                Ammo.Clear();
                ItemsBlackList.Clear();
                WreckBlackList.Clear();
                FactionFitting.Clear();
                AgentsList.Clear();
                MissionFitting.Clear();

                //
                // Clear the Blacklist
                //
                MissionBlacklist.Clear();
                MissionGreylist.Clear();
                FactionBlacklist.Clear();

                MissionName = null;
                //missionbookmarktoagentloops = 0;
                //return;
            }
            else //if the settings file exists - load the characters settings XML
            {
                Settings.Instance.CharacterXMLExists = true;
                XElement xml = XDocument.Load(Settings.Instance.SettingsPath).Root;
                if (xml == null)
                {
                    Logging.Log("Settings", "unable to find [" + Settings.Instance.SettingsPath +
                           "] FATAL ERROR - use the provided settings.xml to create that file.", Logging.red);
                }
                else
                {
                    Logging.Log("Settings", "Loading Settings from [" + Settings.Instance.SettingsPath + "]", Logging.green);
                    //
                    // these are listed by feature and should likely be re-ordered to reflect that
                    //

                    //
                    // Debug Settings
                    //

                    DebugStates = (bool?)xml.Element("debugStates") ?? false;
                    //enables more console logging having to do with the sub-states within each state
                    DebugPerformance = (bool?)xml.Element("debugPerformance") ?? false;
                    //enabled more console logging having to do with the time it takes to execute each state
                    DetailedCurrentTargetHealthLogging = (bool?)xml.Element("detailedCurrentTargetHealthLogging") ?? true;
                    DebugLootWrecks = (bool?)xml.Element("debugLootWrecks") ?? false;
                    DebugActivateWeapons = (bool?)xml.Element("debugActivateWeapons") ?? false;
                    DebugReloadorChangeAmmo = (bool?)xml.Element("debugreloadorChangeAmmo") ?? false;
                    DebugStatistics = (bool?)xml.Element("debugStatistics") ?? false;
                    DebugGotobase = (bool?)xml.Element("debugGotobase") ?? false;
                    DebugIdle = (bool?)xml.Element("debugIdle") ?? false;
                    DebugAutoStart = (bool?)xml.Element("debugAutoStart") ?? false;
                    UseInnerspace = (bool?)xml.Element("useInnerspace") ?? true;

                    //
                    // Misc Settings
                    //
                    CharacterMode = (string)xml.Element("characterMode") ?? "Combat Missions".ToLower();
                    //other option is "salvage"

                    if (Settings.Instance.CharacterMode.ToLower() == "dps".ToLower())
                    {
                        Settings.Instance.CharacterMode = "Combat Missions".ToLower();
                    }
                    AutoStart = (bool?)xml.Element("autoStart") ?? false; // auto Start enabled or disabled by default?
                    SaveConsoleLog = (bool?)xml.Element("saveLog") ?? true; // save the console log to file
                    MaxLineConsole = (int?)xml.Element("maxLineConsole") ?? 1000;
                    // maximum console log lines to show in the GUI
                    Disable3D = (bool?)xml.Element("disable3D") ?? false; // Disable3d graphics while in space
                    RandomDelay = (int?)xml.Element("randomDelay") ?? 0;
                    MinimumDelay = (int?)xml.Element("minimumDelay") ?? 0;
                    //
                    // Enable / Disable Major Features that do not have categories of their own below
                    //
                    UseFittingManager = (bool?)xml.Element("UseFittingManager") ?? true;
                    EnableStorylines = (bool?)xml.Element("enableStorylines") ?? false;
                    UseLocalWatch = (bool?)xml.Element("UseLocalWatch") ?? true;

                    //
                    // Agent Standings and Mission Settings
                    //
                    MinAgentBlackListStandings = (float?)xml.Element("minAgentBlackListStandings") ?? (float)1;
                    MinAgentGreyListStandings = (float?)xml.Element("minAgentGreyListStandings") ?? (float)-1.7;
                    WaitDecline = (bool?)xml.Element("waitDecline") ?? false;
                    var relativeMissionsPath = (string)xml.Element("missionsPath");
                    MissionsPath = System.IO.Path.Combine(Settings.Instance.Path, relativeMissionsPath);
                    Logging.Log("Settings", "MissionsPath is: [" + MissionsPath + "]", Logging.white);
                    LowSecMissionsInShuttles = (bool?)xml.Element("LowSecMissions") ?? false;
                    MaterialsForWarOreID = (int?)xml.Element("MaterialsForWarOreID") ?? 20;
                    MaterialsForWarOreQty = (int?)xml.Element("MaterialsForWarOreQty") ?? 8000;
                    KillSentries = (bool?)xml.Element("killSentries") ?? false;

                    //
                    // Local Watch Settings - if enabled
                    //
                    LocalBadStandingPilotsToTolerate = (int?)xml.Element("LocalBadStandingPilotsToTolerate") ?? 1;
                    LocalBadStandingLevelToConsiderBad = (double?)xml.Element("LocalBadStandingLevelToConsiderBad") ??
                                                         -0.1;
                    //
                    // Invasion Settings
                    //
                    BattleshipInvasionLimit = (int?)xml.Element("battleshipInvasionLimit") ?? 0;
                    // if this number of battleships lands on grid while in a mission we will enter panic
                    BattlecruiserInvasionLimit = (int?)xml.Element("battlecruiserInvasionLimit") ?? 0;
                    // if this number of battlecruisers lands on grid while in a mission we will enter panic
                    CruiserInvasionLimit = (int?)xml.Element("cruiserInvasionLimit") ?? 0;
                    // if this number of cruisers lands on grid while in a mission we will enter panic
                    FrigateInvasionLimit = (int?)xml.Element("frigateInvasionLimit") ?? 0;
                    // if this number of frigates lands on grid while in a mission we will enter panic
                    InvasionRandomDelay = (int?)xml.Element("invasionRandomDelay") ?? 0; // random relay to stay docked
                    InvasionMinimumDelay = (int?)xml.Element("invasionMinimumDelay") ?? 0;
                    // minimum delay to stay docked

                    //
                    // Value - Used in calculations
                    //
                    IskPerLP = (double?)xml.Element("IskPerLP") ?? 600; //used in value calculations

                    //
                    // Undock settings
                    //
                    UndockDelay = (int?)xml.Element("undockdelay") ?? 10; //Delay when undocking - not in use
                    UndockPrefix = (string)xml.Element("undockprefix") ?? "Insta";
                    //Undock bookmark prefix - used by traveler - not in use
                    BookmarkWarpOut = (string)xml.Element("bookmarkWarpOut") ?? "";

                    //
                    // Location of the Questor GUI on startup (default is off the screen)
                    //
                    WindowXPosition = (int?)xml.Element("windowXPosition") ?? 1600;
                    //windows position (needs to be changed, default is off screen)
                    WindowYPosition = (int?)xml.Element("windowYPosition") ?? 1050;
                    //windows position (needs to be changed, default is off screen)

                    //
                    // Ship Names
                    //
                    CombatShipName = (string)xml.Element("combatShipName") ?? "";
                    SalvageShipName = (string)xml.Element("salvageShipName") ?? "";
                    TransportShipName = (string)xml.Element("transportShipName") ?? "";
                    TravelShipName = (string)xml.Element("travelShipName") ?? "";

                    //
                    // Storage Location for Loot, Ammo, Bookmarks
                    //
                    LootHangar = (string)xml.Element("lootHangar");
                    AmmoHangar = (string)xml.Element("ammoHangar");
                    BookmarkHangar = (string)xml.Element("bookmarkHangar");
                    LootContainer = (string)xml.Element("lootContainer");
                    MoveCommonMissionCompletionItemsToAmmoHangar =
                        (bool?)xml.Element("MoveCommonMissionCompletionItemsToAmmoHangar") ?? false;
                    //
                    // Loot and Salvage Settings
                    //
                    LootEverything = (bool?)xml.Element("lootEverything") ?? true;
                    UseGatesInSalvage = (bool?)xml.Element("useGatesInSalvage") ?? false;
                    // if our mission does not despawn (likely someone in the mission looting our stuff?) use the gates when salvaging to get to our bookmarks
                    CreateSalvageBookmarks = (bool?)xml.Element("createSalvageBookmarks") ?? false;
                    CreateSalvageBookmarksIn = (string)xml.Element("createSalvageBookmarksIn") ?? "Player";
                    //Player or Corp
                    //other setting is "Corp"
                    BookmarkPrefix = (string)xml.Element("bookmarkPrefix") ?? "Salvage:";
                    MinimumWreckCount = (int?)xml.Element("minimumWreckCount") ?? 1;
                    AfterMissionSalvaging = (bool?)xml.Element("afterMissionSalvaging") ?? false;
                    FirstSalvageBookmarksInSystem = (bool?)xml.Element("FirstSalvageBookmarksInSystem") ?? false;
                    SalvageMultpleMissionsinOnePass = (bool?)xml.Element("salvageMultpleMissionsinOnePass") ?? false;
                    UnloadLootAtStation = (bool?)xml.Element("unloadLootAtStation") ?? false;
                    ReserveCargoCapacity = (int?)xml.Element("reserveCargoCapacity") ?? 0;
                    MaximumWreckTargets = (int?)xml.Element("maximumWreckTargets") ?? 0;
                    WreckBlackListSmallWrecks = (bool?)xml.Element("WreckBlackListSmallWrecks") ?? false;
                    WreckBlackListMediumWrecks = (bool?)xml.Element("WreckBlackListMediumWrecks") ?? false;
                    AgeofBookmarksForSalvageBehavior = (int?) xml.Element("ageofBookmarksForSalvageBehavior") ?? 45;

                    //
                    // at what memory usage do we need to restart this session?
                    //
                    EVEProcessMemoryCeiling = (int?)xml.Element("EVEProcessMemoryCeiling") ?? 900;
                    EVEProcessMemoryCeilingLogofforExit = (string)xml.Element("EVEProcessMemoryCeilingLogofforExit") ??
                                                          "exit";

                    CloseQuestorCMDUplinkInnerspaceProfile =
                        (bool?)xml.Element("CloseQuestorCMDUplinkInnerspaceProfile") ?? true;
                    CloseQuestorCMDUplinkIsboxerCharacterSet =
                        (bool?)xml.Element("CloseQuestorCMDUplinkIsboxerCharacterSet") ?? false;

                    CloseQuestorArbitraryOSCmd = (bool?)xml.Element("CloseQuestorArbitraryOSCmd") ?? false;
                    //true or false
                    CloseQuestorOSCmdContents = (string)xml.Element("CloseQuestorOSCmdContents") ??
                                                "cmd /k (date /t && time /t && echo. && echo. && echo Questor is configured to use the feature: CloseQuestorArbitraryOSCmd && echo But No actual command was specified in your characters settings xml! && pause)";
                    //the above setting can be set to any script or commands available on the system. make sure you test it from a command prompt while in your .net programs directory

                    Walletbalancechangelogoffdelay = (int?)xml.Element("walletbalancechangelogoffdelay") ?? 30;
                    WalletbalancechangelogoffdelayLogofforExit =
                        (string)xml.Element("walletbalancechangelogoffdelayLogofforExit") ?? "exit";
                    SecondstoWaitAfterExteringCloseQuestorBeforeExitingEVE = 240;

                    if (UseInnerspace)
                    {
                        LavishScriptObject lavishsriptObject = LavishScript.Objects.GetObject("LavishScript");
                        if (lavishsriptObject == null)
                        {
                            InnerSpace.Echo("Testing: object not found");
                        }
                        else
                        {
                            /* "LavishScript" object's ToString value is its version number, which follows the form of a typical float */
                            var version = lavishsriptObject.GetValue<float>();
                            //var TestISVariable = "Game"
                            //LavishIsBoxerCharacterSet = LavishsriptObject.
                            Logging.Log("Settings", "Testing: LavishScript Version " +
                                        version.ToString(CultureInfo.InvariantCulture), Logging.white);
                        }
                    }

                    //
                    // Enable / Disable the different types of logging that are available
                    //
                    SessionsLog = (bool?)xml.Element("SessionsLog") ?? true;
                    DroneStatsLog = (bool?)xml.Element("DroneStatsLog") ?? true;
                    WreckLootStatistics = (bool?)xml.Element("WreckLootStatistics") ?? true;
                    MissionStats1Log = (bool?)xml.Element("MissionStats1Log") ?? true;
                    MissionStats2Log = (bool?)xml.Element("MissionStats2Log") ?? true;
                    MissionStats3Log = (bool?)xml.Element("MissionStats3Log") ?? true;
                    PocketStatistics = (bool?)xml.Element("PocketStatistics") ?? true;
                    PocketStatsUseIndividualFilesPerPocket =
                        (bool?)xml.Element("PocketStatsUseIndividualFilesPerPocket") ??
                                                        true;

                    //
                    // Weapon and targeting Settings
                    //
                    WeaponGroupId = (int?)xml.Element("weaponGroupId") ?? 0;
                    DontShootFrigatesWithSiegeorAutoCannons =
                        (bool?)xml.Element("DontShootFrigatesWithSiegeorAutoCannons") ?? false;
                    MaximumHighValueTargets = (int?)xml.Element("maximumHighValueTargets") ?? 2;
                    MaximumLowValueTargets = (int?)xml.Element("maximumLowValueTargets") ?? 2;

                    //
                    // Speed and Movement Settings
                    //
                    SpeedTank = (bool?)xml.Element("speedTank") ?? false;
                    OrbitDistance = (int?)xml.Element("orbitDistance") ?? 0;
                    OptimalRange = (int?)xml.Element("optimalRange") ?? 0;
                    NosDistance = (int?)xml.Element("NosDistance") ?? 38000;
                    MinimumPropulsionModuleDistance = (int?)xml.Element("minimumPropulsionModuleDistance") ?? 5000;
                    MinimumPropulsionModuleCapacitor = (int?)xml.Element("minimumPropulsionModuleCapacitor") ?? 0;

                    //
                    // Tanking Settings
                    //
                    ActivateRepairModules = (int?)xml.Element("activateRepairModules") ?? 65;
                    DeactivateRepairModules = (int?)xml.Element("deactivateRepairModules") ?? 95;

                    //
                    // Panic Settings
                    //
                    MinimumShieldPct = (int?)xml.Element("minimumShieldPct") ?? 100;
                    MinimumArmorPct = (int?)xml.Element("minimumArmorPct") ?? 100;
                    MinimumCapacitorPct = (int?)xml.Element("minimumCapacitorPct") ?? 50;
                    SafeShieldPct = (int?)xml.Element("safeShieldPct") ?? 0;
                    SafeArmorPct = (int?)xml.Element("safeArmorPct") ?? 0;
                    SafeCapacitorPct = (int?)xml.Element("safeCapacitorPct") ?? 0;

                    //
                    // Drone Settings
                    //
                    UseDrones = (bool?)xml.Element("useDrones") ?? true;
                    DroneTypeId = (int?)xml.Element("droneTypeId") ?? 0;
                    DroneControlRange = (int?)xml.Element("droneControlRange") ?? 0;
                    DroneMinimumShieldPct = (int?)xml.Element("droneMinimumShieldPct") ?? 50;
                    DroneMinimumArmorPct = (int?)xml.Element("droneMinimumArmorPct") ?? 50;
                    DroneMinimumCapacitorPct = (int?)xml.Element("droneMinimumCapacitorPct") ?? 0;
                    DroneRecallShieldPct = (int?)xml.Element("droneRecallShieldPct") ?? 0;
                    DroneRecallArmorPct = (int?)xml.Element("droneRecallArmorPct") ?? 0;
                    DroneRecallCapacitorPct = (int?)xml.Element("droneRecallCapacitorPct") ?? 0;
                    LongRangeDroneRecallShieldPct = (int?)xml.Element("longRangeDroneRecallShieldPct") ?? 0;
                    LongRangeDroneRecallArmorPct = (int?)xml.Element("longRangeDroneRecallArmorPct") ?? 0;
                    LongRangeDroneRecallCapacitorPct = (int?)xml.Element("longRangeDroneRecallCapacitorPct") ?? 0;
                    DronesKillHighValueTargets = (bool?)xml.Element("dronesKillHighValueTargets") ?? false;

                    //
                    // Ammo settings
                    //
                    Ammo.Clear();
                    XElement ammoTypes = xml.Element("ammoTypes");
                    if (ammoTypes != null)
                        foreach (XElement ammo in ammoTypes.Elements("ammoType"))
                            Ammo.Add(new Ammo(ammo));

                    MinimumAmmoCharges = (int?)xml.Element("minimumAmmoCharges") ?? 0;

                    //
                    // List of Agents we should use
                    //
                    AgentsList.Clear();
                    XElement agentList = xml.Element("agentsList");
                    if (agentList != null)
                    {
                        if (agentList.HasElements)
                        {
                            int i = 0;
                            foreach (XElement agent in agentList.Elements("agentList"))
                            {
                                AgentsList.Add(new AgentsList(agent));
                                i++;
                            }
                            if (i >= 2)
                            {
                                MultiAgentSupport = true;
                                Logging.Log(
                                            "Settings", "Found more than one agent in your character XML: MultiAgentSupport is [" +
                                            MultiAgentSupport.ToString(CultureInfo.InvariantCulture) + "]", Logging.white);
                            }
                            else
                            {
                                MultiAgentSupport = false;
                                Logging.Log(
                                            "Settings", "Found only one agent in your character XML: MultiAgentSupport is [" +
                                            MultiAgentSupport.ToString(CultureInfo.InvariantCulture) + "]", Logging.white);
                            }
                        }
                        else
                        {
                            Logging.Log(
                                       "Settings", "agentList exists in your characters config but no agents were listed.", Logging.red);
                        }
                    }
                    else
                        Logging.Log("Settings", "Error! No Agents List specified.", Logging.red);

                    //
                    // Fittings chosen based on the faction of the mission
                    //
                    FactionFitting.Clear();
                    XElement factionFittings = xml.Element("factionfittings");
                    if (UseFittingManager) //no need to look for or load these settings if FittingManager is disabled
                    {
                        if (factionFittings != null)
                        {
                            foreach (XElement factionfitting in factionFittings.Elements("factionfitting"))
                                FactionFitting.Add(new FactionFitting(factionfitting));
                            if (FactionFitting.Exists(m => m.Faction.ToLower() == "default"))
                            {
                                DefaultFitting = FactionFitting.Find(m => m.Faction.ToLower() == "default");
                                if (string.IsNullOrEmpty(DefaultFitting.Fitting))
                                {
                                    UseFittingManager = false;
                                    Logging.Log(
                                                 "Settings", "Error! No default fitting specified or fitting is incorrect.  Fitting manager will not be used.", Logging.orange);
                                }
                                Logging.Log(
                                            "Settings", "Faction Fittings defined. Fitting manager will be used when appropriate.", Logging.white);
                            }
                            else
                            {
                                UseFittingManager = false;
                                Logging.Log(
                                            "Settings", "Error! No default fitting specified or fitting is incorrect.  Fitting manager will not be used.", Logging.orange);
                            }
                        }
                        else
                        {
                            UseFittingManager = false;
                            Logging.Log("Settings", "No faction fittings specified.  Fitting manager will not be used.", Logging.orange);
                        }
                    }
                    //
                    // Fitting based on the name of the mission
                    //
                    MissionFitting.Clear();
                    XElement missionFittings = xml.Element("missionfittings");
                    if (UseFittingManager) //no need to look for or load these settings if FittingManager is disabled
                    {
                        if (missionFittings != null)
                            foreach (XElement missionfitting in missionFittings.Elements("missionfitting"))
                                MissionFitting.Add(new MissionFitting(missionfitting));
                    }

                    //
                    // Mission Blacklist
                    //
                    MissionBlacklist.Clear();
                    XElement blacklist = xml.Element("blacklist");
                    if (blacklist != null)
                        foreach (XElement blacklistedmission in blacklist.Elements("mission"))
                            MissionBlacklist.Add((string)blacklistedmission);
                    //
                    // Mission Greylist
                    //
                    MissionGreylist.Clear();
                    XElement greylist = xml.Element("greylist");
                    if (greylist != null)
                        foreach (XElement greylistedmission in greylist.Elements("mission"))
                            MissionGreylist.Add((string)greylistedmission);

                    //
                    // Faction Blacklist
                    //
                    FactionBlacklist.Clear();
                    XElement factionblacklist = xml.Element("factionblacklist");
                    if (factionblacklist != null)
                        foreach (XElement faction in factionblacklist.Elements("faction"))
                            FactionBlacklist.Add((string)faction);
                }
            }
            //
            // if enabled the following would keep you from looting or salvaging small wrecks
            //
            //list of small wreck
            if (WreckBlackListSmallWrecks)
            {
                WreckBlackList.Add(26557);
                WreckBlackList.Add(26561);
                WreckBlackList.Add(26564);
                WreckBlackList.Add(26567);
                WreckBlackList.Add(26570);
                WreckBlackList.Add(26573);
                WreckBlackList.Add(26576);
                WreckBlackList.Add(26579);
                WreckBlackList.Add(26582);
                WreckBlackList.Add(26585);
                WreckBlackList.Add(26588);
                WreckBlackList.Add(26591);
                WreckBlackList.Add(26594);
                WreckBlackList.Add(26935);
            }

            //
            // if enabled the following would keep you from looting or salvaging medium wrecks
            //
            //list of medium wreck
            if (WreckBlackListMediumWrecks)
            {
                WreckBlackList.Add(26558);
                WreckBlackList.Add(26562);
                WreckBlackList.Add(26568);
                WreckBlackList.Add(26574);
                WreckBlackList.Add(26580);
                WreckBlackList.Add(26586);
                WreckBlackList.Add(26592);
                WreckBlackList.Add(26934);
            }

            //
            // Log location and log names defined here
            //
            Logpath = (System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log\\" + Cache.Instance.DirectEve.Me.Name + "\\");
            //logpath_s = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log\\";
            ConsoleLogPath = System.IO.Path.Combine(Logpath, "Console\\");
            ConsoleLogFile = System.IO.Path.Combine(ConsoleLogPath, string.Format("{0:MM-dd-yyyy}", DateTime.Today) + "-" + Cache.Instance.DirectEve.Me.Name + "-" + "console" + ".log");
            SessionsLogPath = Logpath;
            SessionsLogFile = System.IO.Path.Combine(SessionsLogPath, Cache.Instance.DirectEve.Me.Name + ".Sessions.log");
            DroneStatsLogPath = Logpath;
            DroneStatslogFile = System.IO.Path.Combine(DroneStatsLogPath, Cache.Instance.DirectEve.Me.Name + ".DroneStats.log");
            WreckLootStatisticsPath = Logpath;
            WreckLootStatisticsFile = System.IO.Path.Combine(WreckLootStatisticsPath, Cache.Instance.DirectEve.Me.Name + ".WreckLootStatisticsDump.log");
            MissionStats1LogPath = System.IO.Path.Combine(Logpath, "missionstats\\");
            MissionStats1LogFile = System.IO.Path.Combine(MissionStats1LogPath, Cache.Instance.DirectEve.Me.Name + ".Statistics.log");
            MissionStats2LogPath = System.IO.Path.Combine(Logpath, "missionstats\\");
            MissionStats2LogFile = System.IO.Path.Combine(MissionStats2LogPath, Cache.Instance.DirectEve.Me.Name + ".DatedStatistics.log");
            MissionStats3LogPath = System.IO.Path.Combine(Logpath, "missionstats\\");
            MissionStats3LogFile = System.IO.Path.Combine(MissionStats3LogPath, Cache.Instance.DirectEve.Me.Name + ".CustomDatedStatistics.csv");
            PocketStatisticsPath = System.IO.Path.Combine(Logpath, "pocketstats\\");
            PocketStatisticsFile = System.IO.Path.Combine(PocketStatisticsPath, Cache.Instance.DirectEve.Me.Name + "pocketstats-combined.csv");
            PocketObjectStatisticsPath = System.IO.Path.Combine(Logpath, "pocketobjectstats\\");
            PocketObjectStatisticsFile = System.IO.Path.Combine(PocketObjectStatisticsPath, Cache.Instance.DirectEve.Me.Name + "pocketobjectstats-combined.csv");
            //create all the logging directories even if they aren't configured to be used - we can adjust this later if it really bugs people to have some potentially empty directories.
            Directory.CreateDirectory(Logpath);

            Directory.CreateDirectory(ConsoleLogPath);
            Directory.CreateDirectory(SessionsLogPath);
            Directory.CreateDirectory(DroneStatsLogPath);
            Directory.CreateDirectory(WreckLootStatisticsPath);
            Directory.CreateDirectory(MissionStats1LogPath);
            Directory.CreateDirectory(MissionStats2LogPath);
            Directory.CreateDirectory(MissionStats3LogPath);
            Directory.CreateDirectory(PocketStatisticsPath);
            Directory.CreateDirectory(PocketObjectStatisticsPath);
            if (!Defaultsettingsloaded)
            {
                if (SettingsLoaded != null)
                    SettingsLoaded(this, new EventArgs());
            }
        }
    }
}