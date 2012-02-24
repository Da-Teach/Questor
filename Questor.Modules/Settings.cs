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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml.Linq;

    public class Settings
    {
        /// <summary>
        ///   Singleton implementation
        /// </summary>
        public static Settings Instance = new Settings();

        private string _characterName;
        private DateTime _lastModifiedDate;
        private Random ramdom = new Random();

        public int ramdom_number()
        {
            return ramdom.Next(4, 7);
        }

        public Settings()
        {
            Ammo = new List<Ammo>();
            ItemsBlackList = new List<int>();
            WreckBlackList = new List<int>();
            //AgentsList = new List<AgentsList>();
            FactionFitting = new List<FactionFitting>();
            MissionFitting = new List<MissionFitting>();
            Blacklist = new List<string>();
			FactionBlacklist = new List<string>();
            FittingsDefined = false;
            DefaultFitting = new FactionFitting();
        }

        public bool AtLoginScreen { get; set; }
        
        public bool DebugStates { get; set; }
        public bool DebugPerformance { get; set; }

        public string CharacterMode { get; set; }

        public bool AutoStart { get; set; }

        public bool SaveLog { get; set; }

        public int maxLineConsole { get; set; }

		public bool waitDecline { get; set; }

        public bool Disable3D { get; set; }

        public int MinimumDelay { get; set; }

        public int RandomDelay { get; set; }
		public float minStandings { get; set; }
        public bool UseGatesInSalvage { get; set; }

        public int LocalBadStandingPilotsToTolerate { get; set; }
        public double LocalBadStandingLevelToConsiderBad { get; set; }

        public int BattleshipInvasionLimit { get; set; }
        public int BattlecruiserInvasionLimit { get; set; }
        public int CruiserInvasionLimit { get; set; }
        public int FrigateInvasionLimit { get; set; }
        public int InvasionMinimumDelay { get; set; }
        public int InvasionRandomDelay { get; set; }

        public bool EnableStorylines { get; set; }

        public string CombatShipName { get; set; }
        public string SalvageShipName { get; set; }

        public string LootHangar { get; set; }
        public string AmmoHangar { get; set; }
        public string BookmarkHangar { get; set; }
		public string LootContainer { get; set; }

        public bool CreateSalvageBookmarks { get; set; }
        public string BookmarkPrefix { get; set; }
        public string UndockPrefix { get; set; }
        public int UndockDelay { get; set; }
        public int MinimumWreckCount { get; set; }
        public bool AfterMissionSalvaging { get; set; }
        public bool UnloadLootAtStation { get; set; }

        public string AgentName { get; set; }

        public string bookmarkWarpOut { get; set; }

        public string MissionsPath { get; set; }
        
        public int walletbalancechangelogoffdelay { get; set; }
        public string walletbalancechangelogoffdelayLogofforExit { get; set; }
        
        public Int64 EVEProcessMemoryCeiling { get; set; }
        public string EVEProcessMemoryCeilingLogofforExit { get; set; }

        public bool CloseQuestorCMDUplinkInnerspaceProfile { get; set; }
        public bool CloseQuestorCMDUplinkIsboxerCharacterSet { get; set; }

        //public int missionbookmarktoagentloops { get; set; }  //not yet used - although it is likely a good ide to fix it so it is used - it would eliminate going back and fourth to the same mission over and over
        public string missionName { get; set; }

        public int MaximumHighValueTargets { get; set; }
        public int MaximumLowValueTargets { get; set; }

        public List<Ammo> Ammo { get; private set; }
        public List<int> ItemsBlackList { get; set; }
        public List<int> WreckBlackList { get; set; }
        public bool WreckBlackListSmallWrecks { get; set; }
        public bool WreckBlackListMediumWrecks { get; set; }


        //public string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //public string logpath = Path.Combine(path, "\\log\\");
        //public string logpath = Path.Combine(logpath, Cache.Instance.FilterPath(_characterName));
        //public string logpath = Path.Combine(logpath, "\\");

        public string logpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public bool   SessionsLog { get; set; }
        public string SessionsLogPath { get; set; }
        public string SessionsLogFile { get; set; }
        public bool   DroneStatsLog { get; set; }
        public string DroneStatsLogPath { get; set; }
        public string DroneStatslogFile { get; set; }
        public bool   WreckLootStatistics { get; set; }
        public string WreckLootStatisticsPath { get; set; }
        public string WreckLootStatisticsFile { get; set; }
        public bool   MissionStats1Log { get; set; }
        public string MissionStats1LogPath { get; set; }
        public string MissionStats1LogFile { get; set; }
        public bool   MissionStats2Log { get; set; }
        public string MissionStats2LogPath { get; set; }
        public string MissionStats2LogFile { get; set; }
        public bool   MissionStats3Log { get; set; }
        public string MissionStats3LogPath { get; set; }
        public string MissionStats3LogFile { get; set; }
        public bool   PocketStatistics { get; set; }
        public string PocketStatisticsPath { get; set; }
        public string PocketStatisticsFile { get; set; }

        public List<FactionFitting> FactionFitting { get; private set; }
        public List<MissionFitting> MissionFitting { get; private set; }
        public bool FittingsDefined { get; set; }
        public FactionFitting DefaultFitting { get; set; }

        public int MinimumAmmoCharges { get; set; }

        public int WeaponGroupId { get; set; }

        public int ReserveCargoCapacity { get; set; }

        public int MaximumWreckTargets { get; set; }

        public bool SpeedTank { get; set; }
        public int OrbitDistance { get; set; }
        public int NosDistance { get; set; }
        public int MinimumPropulsionModuleDistance { get; set; }
        public int MinimumPropulsionModuleCapacitor { get; set; }

        public int ActivateRepairModules { get; set; }
        public int DeactivateRepairModules { get; set; }

        public int MinimumShieldPct { get; set; }
        public int MinimumArmorPct { get; set; }
        public int MinimumCapacitorPct { get; set; }
        public int SafeShieldPct { get; set; }
        public int SafeArmorPct { get; set; }
        public int SafeCapacitorPct { get; set; }
        
        public bool LootEverything { get; set; }
        public double IskPerLP { get; set; }

        private bool _UseDrones;

        public bool UseDrones
        {
            get
            {
                if (Cache.Instance.MissionUseDrones != null)
                    return (bool)Cache.Instance.MissionUseDrones;
                else return _UseDrones;
            }
            set
            {
                _UseDrones = value;
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

        public int MaterialsForWarOreID { get; set; }
        public int MaterialsForWarOreQty { get; set; }

        public List<string> Blacklist { get; private set; }
        public List<string> FactionBlacklist { get; private set; }

        public int? WindowXPosition { get; set; }
        public int? WindowYPosition { get; set; }
        public event EventHandler<EventArgs> SettingsLoaded;

        public void LoadSettings()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var settingsPath = Path.Combine(path, Cache.Instance.FilterPath(_characterName) + ".xml");
            var logpath = Path.Combine(path, "\\log\\");
            logpath = Path.Combine(logpath, Cache.Instance.FilterPath(_characterName));
            logpath = Path.Combine(logpath, "\\");

            var repairstopwatch = new Stopwatch();

            var reloadSettings = _characterName != Cache.Instance.DirectEve.Me.Name;
            if (File.Exists(settingsPath))
                reloadSettings = _lastModifiedDate != File.GetLastWriteTime(settingsPath);

            if (!reloadSettings)
                return;

            _characterName = Cache.Instance.DirectEve.Me.Name;
            _lastModifiedDate = File.GetLastWriteTime(settingsPath);

            if (!File.Exists(settingsPath))
            {
                // Clear settings
                AgentName = string.Empty;

                CharacterMode = "dps";

                AutoStart = false;

                SaveLog = true;

                maxLineConsole = 1000;

                waitDecline = false;

                Disable3D = false;

                RandomDelay = 0;

                minStandings = 10;

                MinimumDelay = 0;

                minStandings = 10;

                WindowXPosition = null;
                WindowYPosition = null;

                LootHangar = string.Empty;
                AmmoHangar = string.Empty;
                BookmarkHangar = string.Empty;
				LootContainer = string.Empty;

                MissionsPath = Path.Combine(path, "Missions");

                bookmarkWarpOut = string.Empty;

                MaximumHighValueTargets = 0;
                MaximumLowValueTargets = 0;

                Ammo.Clear();
                ItemsBlackList.Clear();
                WreckBlackList.Clear();
                FactionFitting.Clear();
                MissionFitting.Clear();

                MinimumAmmoCharges = 0;

                WeaponGroupId = 0;

                ReserveCargoCapacity = 0;

                MaximumWreckTargets = 0;

                SpeedTank = false;
                OrbitDistance = 0;
                NosDistance = 38000;
                MinimumPropulsionModuleDistance = 3000;
                MinimumPropulsionModuleCapacitor = 35;

                ActivateRepairModules = 0;
                DeactivateRepairModules = 0;

                MinimumShieldPct = 0;
                MinimumArmorPct = 0;
                MinimumCapacitorPct = 0;
                SafeShieldPct = 0;
                SafeArmorPct = 0;
                SafeCapacitorPct = 0;

                UseDrones = false;
                DroneTypeId = 0;
                DroneControlRange = 0;
                DroneMinimumShieldPct = 0;
                DroneMinimumArmorPct = 0;
                DroneMinimumCapacitorPct = 0;
                DroneRecallCapacitorPct = 0;
                LongRangeDroneRecallCapacitorPct = 0;

                UseGatesInSalvage = false;

                Blacklist.Clear();
                FactionBlacklist.Clear();

                missionName = null;
                //missionbookmarktoagentloops = 0;
                return;
            }

            var xml = XDocument.Load(settingsPath).Root;

            DebugStates = (bool?) xml.Element("debugStates") ?? false;
            DebugPerformance = (bool?) xml.Element("debugPerformance") ?? false;

            CharacterMode = (string) xml.Element("characterMode") ?? "dps"; //other option is "salvage"

            AutoStart = (bool?) xml.Element("autoStart") ?? false;

            SaveLog = (bool?)xml.Element("saveLog") ?? true;

            maxLineConsole = (int?)xml.Element("maxLineConsole") ?? 1000;
            waitDecline = (bool?) xml.Element("waitDecline") ?? false;

            Disable3D = (bool?)xml.Element("disable3D") ?? true;

            RandomDelay = (int?) xml.Element("randomDelay") ?? 0;
            MinimumDelay = (int?)xml.Element("minimumDelay") ?? 0;
			minStandings = (float?) xml.Element("minStandings") ?? 10;

            UseGatesInSalvage = (bool?)xml.Element("useGatesInSalvage") ?? false;
            
            LocalBadStandingPilotsToTolerate = (int?)xml.Element("LocalBadStandingPilotsToTolerate") ?? 1;
            LocalBadStandingLevelToConsiderBad = (double?)xml.Element("LocalBadStandingLevelToConsiderBad") ?? -0.1;

            BattleshipInvasionLimit = (int?)xml.Element("battleshipInvasionLimit") ?? 0;
            BattlecruiserInvasionLimit = (int?)xml.Element("battlecruiserInvasionLimit") ?? 0;
            CruiserInvasionLimit = (int?)xml.Element("cruiserInvasionLimit") ?? 0;
            FrigateInvasionLimit = (int?)xml.Element("frigateInvasionLimit") ?? 0;
            InvasionRandomDelay = (int?)xml.Element("invasionRandomDelay") ?? 0;
            InvasionMinimumDelay = (int?)xml.Element("invasionMinimumDelay") ?? 0;

            EnableStorylines = (bool?) xml.Element("enableStorylines") ?? false;
            IskPerLP = (double?)xml.Element("IskPerLP") ?? 600;

			UndockDelay = (int?)xml.Element("undockdelay") ?? 10;
			UndockPrefix = (string) xml.Element("undockprefix") ?? "Insta";
            WindowXPosition = (int?) xml.Element("windowXPosition") ?? 1600;
            WindowYPosition = (int?) xml.Element("windowYPosition") ?? 1050;

            CombatShipName = (string) xml.Element("combatShipName") ?? "raven";
            SalvageShipName = (string) xml.Element("salvageShipName") ?? "noctis";

            LootHangar = (string) xml.Element("lootHangar");
            AmmoHangar = (string) xml.Element("ammoHangar");
            BookmarkHangar = (string)xml.Element("bookmarkHangar");
			LootContainer = (string)xml.Element("lootContainer");

            CreateSalvageBookmarks = (bool?) xml.Element("createSalvageBookmarks") ?? false;
            BookmarkPrefix = (string) xml.Element("bookmarkPrefix") ?? "Salvage:";
            MinimumWreckCount = (int?) xml.Element("minimumWreckCount") ?? 1;
            AfterMissionSalvaging = (bool?) xml.Element("afterMissionSalvaging") ?? false;
            UnloadLootAtStation = (bool?) xml.Element("unloadLootAtStation") ?? false;

            AgentName = (string) xml.Element("agentName");

            bookmarkWarpOut = (string)xml.Element("bookmarkWarpOut") ?? "insta";

            EVEProcessMemoryCeiling = (int?)xml.Element("EVEProcessMemoryCeiling") ?? 900;
            EVEProcessMemoryCeilingLogofforExit = (string)xml.Element("EVEProcessMemoryCeilingLogofforExit") ?? "exit";
            
            //Assume InnerspaceProfile
            CloseQuestorCMDUplinkInnerspaceProfile = (bool?)xml.Element("CloseQuestorCMDUplinkInnerspaceProfile") ?? true;
            CloseQuestorCMDUplinkIsboxerCharacterSet = (bool?)xml.Element("CloseQuestorCMDUplinkIsboxerProfile") ?? false;

            walletbalancechangelogoffdelay = (int?)xml.Element("walletbalancechangelogoffdelay") ?? 30;
            walletbalancechangelogoffdelayLogofforExit = (string)xml.Element("walletbalancechangelogoffdelayLogofforExit") ?? "exit";

            SessionsLog = (bool?)xml.Element("SessionsLog") ?? true;
            DroneStatsLog = (bool?)xml.Element("DroneStatsLog") ?? true;
            WreckLootStatistics = (bool?)xml.Element("WreckLootStatistics") ?? true;
            MissionStats1Log = (bool?)xml.Element("MissionStats1Log") ?? true;
            MissionStats2Log = (bool?)xml.Element("MissionStats2Log") ?? true;
            MissionStats3Log = (bool?)xml.Element("MissionStats3Log") ?? true;
            PocketStatistics = (bool?)xml.Element("PocketStatistics") ?? true;

            var missionsPath = (string) xml.Element("missionsPath");
            MissionsPath = !string.IsNullOrEmpty(missionsPath) ? Path.Combine(path, missionsPath) : Path.Combine(path, "Missions");

            MaximumHighValueTargets = (int?) xml.Element("maximumHighValueTargets") ?? 2;
            MaximumLowValueTargets = (int?) xml.Element("maximumLowValueTargets") ?? 2;

            Ammo.Clear();
            var ammoTypes = xml.Element("ammoTypes");
            if (ammoTypes != null)
                foreach (var ammo in ammoTypes.Elements("ammoType"))
                    Ammo.Add(new Ammo(ammo));

            MinimumAmmoCharges = (int?) xml.Element("minimumAmmoCharges") ?? 0;

            FactionFitting.Clear();
            var factionFittings = xml.Element("factionfittings");
            if (factionFittings != null)
            {
                foreach (var factionfitting in factionFittings.Elements("factionfitting"))
                    FactionFitting.Add(new FactionFitting(factionfitting));
                if (FactionFitting.Exists(m => m.Faction.ToLower() == "default"))
                {
                    DefaultFitting = FactionFitting.Find(m => m.Faction.ToLower() == "default");
                    if (!(DefaultFitting.Fitting == "") && !(DefaultFitting.Fitting == null))
                        FittingsDefined = true;
                    else
                        Logging.Log("Settings: Error! No default fitting specified or fitting is incorrect.  Fitting manager will not be used.");
                }
                else
                    Logging.Log("Settings: Error! No default fitting specified or fitting is incorrect.  Fitting manager will not be used.");
            }
            else
                Logging.Log("Settings: No faction fittings specified.  Fitting manager will not be used.");

            MissionFitting.Clear();
            var missionFittings = xml.Element("missionfittings");
            if (missionFittings != null)
                foreach (var missionfitting in missionFittings.Elements("missionfitting"))
                    MissionFitting.Add(new MissionFitting(missionfitting));

            WeaponGroupId = (int?) xml.Element("weaponGroupId") ?? 0;

            ReserveCargoCapacity = (int?) xml.Element("reserveCargoCapacity") ?? 0;

            MaximumWreckTargets = (int?) xml.Element("maximumWreckTargets") ?? 0;

            SpeedTank = (bool?) xml.Element("speedTank") ?? false;
            OrbitDistance = (int?) xml.Element("orbitDistance") ?? 0;
            NosDistance = (int?)xml.Element("NosDistance") ?? 38000;
            MinimumPropulsionModuleDistance = (int?) xml.Element("minimumPropulsionModuleDistance") ?? 5000;
            MinimumPropulsionModuleCapacitor = (int?)xml.Element("minimumPropulsionModuleCapacitor") ?? 0;

            ActivateRepairModules = (int?) xml.Element("activateRepairModules") ?? 65;
            DeactivateRepairModules = (int?) xml.Element("deactivateRepairModules") ?? 95;

            MinimumShieldPct = (int?) xml.Element("minimumShieldPct") ?? 100;
            MinimumArmorPct = (int?) xml.Element("minimumArmorPct") ?? 100;
            MinimumCapacitorPct = (int?) xml.Element("minimumCapacitorPct") ?? 50;
            SafeShieldPct = (int?)xml.Element("safeShieldPct") ?? 0;
            SafeArmorPct = (int?)xml.Element("safeArmorPct") ?? 0;
            SafeCapacitorPct = (int?)xml.Element("safeCapacitorPct") ?? 0;

            LootEverything = (bool?) xml.Element("lootEverything") ?? true;

            UseDrones = (bool?) xml.Element("useDrones") ?? true;
            DroneTypeId = (int?) xml.Element("droneTypeId") ?? 0;
            DroneControlRange = (int?) xml.Element("droneControlRange") ?? 0;
            DroneMinimumShieldPct = (int?) xml.Element("droneMinimumShieldPct") ?? 50;
            DroneMinimumArmorPct = (int?) xml.Element("droneMinimumArmorPct") ?? 50;
            DroneMinimumCapacitorPct = (int?) xml.Element("droneMinimumCapacitorPct") ?? 0;
            DroneRecallShieldPct = (int?) xml.Element("droneRecallShieldPct") ?? 0;
            DroneRecallArmorPct = (int?) xml.Element("droneRecallArmorPct") ?? 0;
            DroneRecallCapacitorPct = (int?) xml.Element("droneRecallCapacitorPct") ?? 0;
            LongRangeDroneRecallShieldPct = (int?) xml.Element("longRangeDroneRecallShieldPct") ?? 0;
            LongRangeDroneRecallArmorPct = (int?) xml.Element("longRangeDroneRecallArmorPct") ?? 0;
            LongRangeDroneRecallCapacitorPct = (int?) xml.Element("longRangeDroneRecallCapacitorPct") ?? 0;

            MaterialsForWarOreID = (int?)xml.Element("MaterialsForWarOreID") ?? 20;
            MaterialsForWarOreQty = (int?)xml.Element("MaterialsForWarOreQty") ?? 8000;

            Blacklist.Clear();
            var blacklist = xml.Element("blacklist");
            if (blacklist != null)
                foreach (var mission in blacklist.Elements("mission"))
                    Blacklist.Add((string) mission);

            FactionBlacklist.Clear();
            var factionblacklist = xml.Element("factionblacklist");
            if (factionblacklist != null)
                foreach (var faction in factionblacklist.Elements("faction"))
                    FactionBlacklist.Add((string) faction);
            WreckBlackListSmallWrecks = (bool?) xml.Element("WreckBlackListSmallWrecks") ?? false;
            WreckBlackListMediumWrecks = (bool?) xml.Element("WreckBlackListMediumWrecks") ?? false;


            //
            // if enabled the following would keep you from looting (or salvaging?) small wrecks
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
            // if enabled the following would keep you from looting (or salvaging?) medium wrecks
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

            if (SettingsLoaded != null)
                SettingsLoaded(this, new EventArgs());
        }
    }
}