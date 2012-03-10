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
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using DirectEve;

    public class Cache
    {
        /// <summary>
        ///   Singleton implementation
        /// </summary>
        private static Cache _instance = new Cache();

        /// <summary>
        ///   Active Drones
        /// </summary>
        private List<EntityCache> _activeDrones;

        private DirectAgent _agent;

        /// <summary>
        ///   Agent cache
        /// </summary>
        private long? _agentId;

        /// <summary>
        ///   Approaching cache
        /// </summary>
        //private int? _approachingId;
        private EntityCache _approaching;

        /// <summary>
        ///   Returns all non-empty wrecks and all containers
        /// </summary>
        private List<EntityCache> _containers;

        /// <summary>
        ///   Entities cache (all entities within 256km)
        /// </summary>
        private List<EntityCache> _entities;

        /// <summary>
        ///   Entities by Id
        /// </summary>
        private Dictionary<long, EntityCache> _entitiesById;

        /// <summary>
        ///   Module cache
        /// </summary>
        private List<ModuleCache> _modules;

        /// <summary>
        ///   Priority targets (e.g. warp scramblers or mission kill targets)
        /// </summary>
        private List<PriorityTarget> _priorityTargets;

        /// <summary>
        ///   Star cache
        /// </summary>
        private EntityCache _star;

        /// <summary>
        ///   Station cache
        /// </summary>
        private List<EntityCache> _stations;

        /// <summary>
        ///   Stargate cache
        /// </summary>
        private List<EntityCache> _stargates;

        /// <summary>
        ///   Targeted by cache
        /// </summary>
        private List<EntityCache> _targetedBy;

        /// <summary>
        ///   Targeting cache
        /// </summary>
        private List<EntityCache> _targeting;

        /// <summary>
        ///   Targets cache
        /// </summary>
        private List<EntityCache> _targets;

        /// <summary>
        ///   Returns all unlooted wrecks & containers
        /// </summary>
        private List<EntityCache> _unlootedContainers;

        private List<EntityCache> _unlootedWrecksAndSecureCans;
        
        private List<DirectWindow> _windows;

        public Cache()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            ShipTargetValues = new List<ShipTargetValue>();
            var values = XDocument.Load(Path.Combine(path, "ShipTargetValues.xml"));
            foreach (var value in values.Root.Elements("ship"))
                ShipTargetValues.Add(new ShipTargetValue(value));

            InvTypesById = new Dictionary<int, InvType>();
            var invTypes = XDocument.Load(Path.Combine(path, "InvTypes.xml"));
            foreach (var element in invTypes.Root.Elements("invtype"))
                InvTypesById.Add((int) element.Attribute("id"), new InvType(element));

            _priorityTargets = new List<PriorityTarget>();
            LastModuleTargetIDs = new Dictionary<long, long>();
            TargetingIDs = new Dictionary<long, DateTime>();
            _entitiesById = new Dictionary<long, EntityCache>();

            LootedContainers = new HashSet<long>();
            IgnoreTargets = new HashSet<string>();
            MissionItems = new List<string>();
            ChangeMissionShipFittings = false;
            UseMissionShip = false;
            ArmLoadedCache = false;
            missionAmmo = new List<Ammo>();
            MissionUseDrones = null;

            panic_attempts_this_pocket = 0;
            lowest_shield_percentage_this_pocket = 100;
            lowest_armor_percentage_this_pocket = 100;
            lowest_capacitor_percentage_this_pocket = 100;
            panic_attempts_this_mission = 0;
            lowest_shield_percentage_this_mission = 100;
            lowest_armor_percentage_this_mission = 100;
            lowest_capacitor_percentage_this_mission = 100;
            lastKnownGoodConnectedTime = DateTime.Now;
        }

        /// <summary>
        ///   List of containers that have been looted
        /// </summary>
        public HashSet<long> LootedContainers { get; private set; }

        /// <summary>
        ///   List of targets to ignore
        /// </summary>
        public HashSet<string> IgnoreTargets { get; private set; }

        public static Cache Instance
        {
            get { return _instance; }
        }

        public bool DoNotBreakInvul = false;

        public bool LootAlreadyUnloaded = false;

        public bool MissionLoot = false;

        public bool SalvageAll = false;

        public double Wealth { get; set; }
        public bool OpenWrecks = false;
        public bool NormalApproch = true;
        public bool CourierMission = false;
        public string MissionName = "";
        public bool ConsoleLogOpened = false;

        public bool Local_safe(int max_bad, double stand)
        {
            int number = 0;
            DirectChatWindow Local = (DirectChatWindow)GetWindowByName("Local");
            foreach(var LocalMember in Local.Members)
            {
                float[] alliance = {DirectEve.Standings.GetPersonalRelationship(LocalMember.AllianceId), DirectEve.Standings.GetCorporationRelationship(LocalMember.AllianceId), DirectEve.Standings.GetAllianceRelationship(LocalMember.AllianceId)};
                float[] corporation = {DirectEve.Standings.GetPersonalRelationship(LocalMember.CorporationId), DirectEve.Standings.GetCorporationRelationship(LocalMember.CorporationId), DirectEve.Standings.GetAllianceRelationship(LocalMember.CorporationId)};
                float[] personal = {DirectEve.Standings.GetPersonalRelationship(LocalMember.CharacterId), DirectEve.Standings.GetCorporationRelationship(LocalMember.CharacterId), DirectEve.Standings.GetAllianceRelationship(LocalMember.CharacterId)};


                if(alliance.Min() <= stand || corporation.Min() <= stand || personal.Min() <= stand)
                {
                    Logging.Log("Cache.WatchLocal: Bad Standing Pilot Detected: [ " + LocalMember.Name + "] " + " [ " + number + " ] so far... of [ " + max_bad + " ] allowed");
                    number++;
                }
                if(number > max_bad)
                {
                    Logging.Log("Cache.WatchLocal: [" + number + "] Bad Standing pilots in local, We should stay in station");
                    return false;
                }
            }
            return true;
        }

        public DirectEve DirectEve { get; set; }

        public Dictionary<int, InvType> InvTypesById { get; private set; }

        /// <summary>
        ///   List of ship target values, higher target value = higher kill priority
        /// </summary>
        public List<ShipTargetValue> ShipTargetValues { get; private set; }

        /// <summary>
        ///   Best damage type for the mission
        /// </summary>
        public DamageType DamageType { get; set; }

        /// <summary>
        ///   Best orbit distance for the mission
        /// </summary>
        public int OrbitDistance { get; set; }
		
		/// <summary>
        ///   Force Salvaging after mission
        /// </summary>
        public bool afterMissionSalvaging { get; set; }

		
		/// <summary>
        ///   Returns the maximum weapon distance
        /// </summary>
        public int WeaponRange
        {
            get
            {
                // Get ammmo based on current damage type
                var ammo = Settings.Instance.Ammo.Where(a => a.DamageType == DamageType);

                // Is our ship's cargo available?
                var cargo = DirectEve.GetShipsCargo();
                if (cargo.IsReady)
                    ammo = ammo.Where(a => cargo.Items.Any(i => a.TypeId == i.TypeId && i.Quantity >= Settings.Instance.MinimumAmmoCharges));

                // Return ship range if there's no ammo left
                if (ammo.Count() == 0)
                    return System.Convert.ToInt32(Cache.Instance.DirectEve.ActiveShip.MaxTargetRange);

                // Return max range
                return ammo.Max(a => a.Range);
            }
        }

        /// <summary>
        ///   Last target for a certain module
        /// </summary>
        public Dictionary<long, long> LastModuleTargetIDs { get; private set; }

        /// <summary>
        ///   Targeting delay cache (used by LockTarget)
        /// </summary>
        public Dictionary<long, DateTime> TargetingIDs { get; private set; }

        /// <summary>
        ///   Used for Drones to know that it should retract drones
        /// </summary>
        public bool IsMissionPocketDone { get; set; }
        public string ExtConsole { get; set; }
        public string ConsoleLog { get; set; }
        public bool IsAgentLoop { get; set; }
        private string AgentName = "";

        public DateTime _lastModuleActivation;
        public int panic_attempts_this_mission { get; set; }
        public double lowest_shield_percentage_this_pocket { get; set; }
        public double lowest_armor_percentage_this_pocket { get; set; }
        public double lowest_capacitor_percentage_this_pocket { get; set; }
        public int repair_cycle_time_this_pocket { get; set; }
        public int panic_attempts_this_pocket { get; set; }
        public double lowest_shield_percentage_this_mission { get; set; }
        public double lowest_armor_percentage_this_mission { get; set; }
        public double lowest_capacitor_percentage_this_mission { get; set; }
        public DateTime StartedBoosting { get; set; }
        public int repair_cycle_time_this_mission { get; set; }
        public DateTime lastKnownGoodConnectedTime { get; set; }
        public long totalMegaBytesOfMemoryUsed { get; set; }
        public double MyWalletBalance { get; set; }
        public string CurrentAgent
        {
            get
            {
                if(AgentName == "")
                {
                    AgentName = SwitchAgent;
                    Logging.Log("Cache.CurrentAgent is null set first agent: " + CurrentAgent);
                }

                return AgentName;
            }
            set
            {
                AgentName = value;
            }
        }

        public string SwitchAgent
        {
            get
            {
                var agent = Settings.Instance.AgentsList.OrderBy(j => j.Priorit).FirstOrDefault(i => DateTime.Now >= i.Decline_timer);
                if(agent == null)
                {
                    agent = Settings.Instance.AgentsList.OrderBy(j => j.Priorit).FirstOrDefault();
                    IsAgentLoop = true;
                }
                else
                    IsAgentLoop = false;

                return agent.Name;
            }
        }

        public long AgentId
        {
            get
            {
                _agent = DirectEve.GetAgentByName(CurrentAgent);
                _agentId = _agent.AgentId;

                return _agentId ?? -1;
            }
        }

        public DirectAgent Agent
        {
            get
            {
				_agent = DirectEve.GetAgentByName(CurrentAgent);
                _agentId = _agent.AgentId;

                if (_agent == null)
                    _agent = DirectEve.GetAgentById(_agentId.Value);

                return _agent;
            }
        }

        public IEnumerable<ModuleCache> Modules
        {
            get
            {
                if (_modules == null)
                    _modules = DirectEve.Modules.Select(m => new ModuleCache(m)).ToList();

                return _modules;
            }
        }

        public IEnumerable<ModuleCache> Weapons
        {
            get
            { 
                if(Cache.Instance.MissionWeaponGroupId != 0)
                    return Modules.Where(m => m.GroupId == Cache.Instance.MissionWeaponGroupId); 
                else return Modules.Where(m => m.GroupId == Settings.Instance.WeaponGroupId); 
            }
        }

        public IEnumerable<EntityCache> Containers
        {
            get
            {
                if (_containers == null)
				
				//
				// edit 12-18-2011
				//
                    _containers = Entities.Where(e => e.IsContainer && e.HaveLootRights && (e.GroupId != (int) Group.Wreck || !e.IsWreckEmpty) && (e.Name != (String) "Abandoned Container")).ToList();

                return _containers;
            }
        }

        public IEnumerable<EntityCache> Wrecks
        {
            get
            {
                if (_containers == null)
                    _containers = Entities.Where(e => (e.GroupId != (int)Group.Wreck)).ToList();

                return _containers;
            }
        }

        public IEnumerable<EntityCache> UnlootedContainers
        {
            get
            {
                if (_unlootedContainers == null)
                    _unlootedContainers = Entities.Where(e => e.IsContainer && e.HaveLootRights && (!LootedContainers.Contains(e.Id) || e.GroupId == (int) Group.Wreck)).OrderBy(e => e.Distance).ToList();

                return _unlootedContainers;
            }
        }

        //This needs to include items you can steal from (thus gain aggro)
        public IEnumerable<EntityCache> UnlootedWrecksAndSecureCans
        {
            get
            {
                if (_unlootedWrecksAndSecureCans == null)
                    _unlootedWrecksAndSecureCans = Entities.Where(e => (e.GroupId == (int)Group.Wreck || e.GroupId == (int)Group.SecureContainer || e.GroupId == (int)Group.AuditLogSecureContainer || e.GroupId == (int)Group.FreightContainer) && !e.IsWreckEmpty).OrderBy(e => e.Distance).ToList();

                return _unlootedWrecksAndSecureCans;
            }
        }
        
        
        public IEnumerable<EntityCache> Targets
        {
            get
            {
                if (_targets == null)
                    _targets = Entities.Where(e => e.IsTarget).ToList();

                // Remove the target info (its been targeted)
                foreach (var target in _targets.Where(t => TargetingIDs.ContainsKey(t.Id)))
                    TargetingIDs.Remove(target.Id);

                return _targets;
            }
        }

        public IEnumerable<EntityCache> Targeting
        {
            get
            {
                if (_targeting == null)
                    _targeting = Entities.Where(e => e.IsTargeting).ToList();

                return _targeting;
            }
        }

        public IEnumerable<EntityCache> TargetedBy
        {
            get
            {
                if (_targetedBy == null)
                    _targetedBy = Entities.Where(e => e.IsTargetedBy).ToList();

                return _targetedBy;
            }
        }

        public IEnumerable<EntityCache> Entities
        {
            get
            {
                if (!InSpace)
                    return new List<EntityCache>();

                if (_entities == null)
                    _entities = DirectEve.Entities.Select(e => new EntityCache(e)).Where(e => e.IsValid).ToList();

                return _entities;
            }
        }

        public bool InSpace
        {
            get { return DirectEve.Session.IsInSpace && !DirectEve.Session.IsInStation && DirectEve.Session.IsReady && DirectEve.ActiveShip.Entity != null; }
        }

        public bool InStation
        {
            get { return DirectEve.Session.IsInStation && !DirectEve.Session.IsInSpace && DirectEve.Session.IsReady; }
        }

        public bool InWarp
        {
            get { return DirectEve.ActiveShip.Entity != null ? DirectEve.ActiveShip.Entity.Mode == 3 : false; }
        }

        public IEnumerable<EntityCache> ActiveDrones
        {
            get
            {
                if (_activeDrones == null)
                    _activeDrones = DirectEve.ActiveDrones.Select(d => new EntityCache(d)).ToList();

                return _activeDrones;
            }
        }

        public IEnumerable<EntityCache> Stations
        {
            get
            {
                if (_stations == null)
                    _stations = Entities.Where(e => e.CategoryId == (int) CategoryID.Station).ToList();

                return _stations;
            }
        }

        public IEnumerable<EntityCache> Stargates
        {
            get
            {
                if (_stargates == null)
                    _stargates = Entities.Where(e => e.GroupId == (int)Group.Stargate).ToList();

                return _stargates;
            }
        }

        public EntityCache Star
        {
            get
            {
                if (_star == null)
                    _star = Entities.Where(e => e.CategoryId == (int) CategoryID.Celestial && e.GroupId == (int) Group.Star).FirstOrDefault();

                return _star;
            }
        }

        public IEnumerable<EntityCache> PriorityTargets
        {
            get
            {
                _priorityTargets.RemoveAll(pt => pt.Entity == null);
                return _priorityTargets.OrderBy(pt => pt.Priority).ThenBy(pt => (pt.Entity.ShieldPct + pt.Entity.ArmorPct + pt.Entity.StructurePct)).ThenBy(pt => pt.Entity.Distance).Select(pt => pt.Entity);
            }
        }

        public EntityCache Approaching
        {
            get
            {
                if (_approaching == null)
                {
                    var ship = DirectEve.ActiveShip.Entity;
                    if (ship != null && ship.IsValid)
                        _approaching = EntityById(ship.FollowId);
                }

                return _approaching != null && _approaching.IsValid ? _approaching : null;
            }
            set { _approaching = value; }
        }

        public List<DirectWindow> Windows
        {
            get
            {
                if (_windows == null)
                    _windows = DirectEve.Windows;

                return _windows;
            }
        }

        /// <summary>
        ///   Returns the mission for a specific agent
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns>null if no mission could be found</returns>
        public DirectAgentMission GetAgentMission(long agentId)
        {
            return DirectEve.AgentMissions.FirstOrDefault(m => m.AgentId == agentId);
        }

        /// <summary>
        ///   Returns the mission objectives from
        /// </summary>
        public List<string> MissionItems { get; private set; }

        /// <summary>
        ///   Returns the item that needs to be brought on the mission
        /// </summary>
        /// <returns></returns>
        public string BringMissionItem { get; private set; }



        public string Fitting { get; set; } // stores name of the final fitting we want to use
        public string MissionShip { get; set; } //stores name of mission specific ship
        public string DefaultFitting { get; set; } //stores name of the default fitting
        public string currentFit { get; set; }
        public string factionFit { get; set; }
        public string factionName { get; set; }
        public bool ArmLoadedCache { get; set; } // flags whether arm has already loaded the mission
        public bool UseMissionShip { get; set; } // flags whether we're using a mission specific ship
        public bool ChangeMissionShipFittings { get; set; } // used for situations in which missionShip's specified, but no faction or mission fittings are; prevents default
                                                            // fitting from being loaded in arm.cs
        public List<Ammo> missionAmmo;
        public int MissionWeaponGroupId = 0;
        public bool? MissionUseDrones;
        public bool StopTimeSpecified { get; set; }
        public DateTime StopTime { get; set; }
        public bool CloseQuestorCMDLogoff = false;
        public bool CloseQuestorCMDExitGame = true;
        public string ReasonToStopQuestor { get; set; }
        public string SessionState { get; set; }
        public double SessionIskGenerated { get; set; }
        public double SessionLootGenerated { get; set; }
        public double SessionLPGenerated { get; set; }
        public int SessionRunningTime { get; set; }
        public double SessionIskPerHrGenerated { get; set; }
        public double SessionLootPerHrGenerated { get; set; }
        public double SessionLPPerHrGenerated { get; set; }
        public double SessionTotalPerHrGenerated { get; set; }
        public bool QuestorJustStarted = true;

        public DirectWindow GetWindowByCaption(string caption)
        {
            return Windows.FirstOrDefault(w => w.Caption.Contains(caption));
        }

        public DirectWindow GetWindowByName(string name)
        {
            // Special cases
            if (name == "Local")
                return Windows.FirstOrDefault(w => w.Name.StartsWith("chatchannel_solarsystemid"));

            return Windows.FirstOrDefault(w => w.Name == name);
        }

        /// <summary>
        ///   Return entities by name
        /// </summary>
        /// <param name = "name"></param>
        /// <returns></returns>
        public IEnumerable<EntityCache> EntitiesByName(string name)
        {
            return Entities.Where(e => e.Name == name).ToList();
        }

        /// <summary>
        ///   Return a cached entity by Id
        /// </summary>
        /// <param name = "id"></param>
        /// <returns></returns>
        public EntityCache EntityById(long id)
        {
            if (_entitiesById.ContainsKey(id))
                return _entitiesById[id];

            var entity = Entities.FirstOrDefault(e => e.Id == id);
            _entitiesById[id] = entity;
            return entity;
        }

        /// <summary>
        ///   Returns the first mission bookmark that starts with a certain string
        /// </summary>
        /// <returns></returns>
        public DirectAgentMissionBookmark GetMissionBookmark(long agentId, string startsWith)
        {
            // Get the missons
            var mission = GetAgentMission(agentId);
            if (mission == null)
                return null;

            // Did we accept this mission?
            if (mission.State != (int) MissionState.Accepted || mission.AgentId != agentId)
                return null;

            return mission.Bookmarks.FirstOrDefault(b => b.Title.ToLower().StartsWith(startsWith.ToLower()));
        }

        /// <summary>
        ///   Return a bookmark by id
        /// </summary>
        /// <param name = "bookmarkId"></param>
        /// <returns></returns>
        public DirectBookmark BookmarkById(long bookmarkId)
        {
            return DirectEve.Bookmarks.FirstOrDefault(b => b.BookmarkId == bookmarkId);
        }

        /// <summary>
        ///   Returns bookmarks that start with the supplied label
        /// </summary>
        /// <param name = "label"></param>
        /// <returns></returns>
        public List<DirectBookmark> BookmarksByLabel(string label)
        {
            return DirectEve.Bookmarks.Where(b => !string.IsNullOrEmpty(b.Title) && b.Title.StartsWith(label)).ToList();
        }

        /// <summary>
        ///   Invalidate the cached items
        /// </summary>
        public void InvalidateCache()
        {
            _windows = null;
            _unlootedContainers = null;
            _star = null;
            _stations = null;
            _stargates = null;
            _modules = null;
            _targets = null;
            _targeting = null;
            _targetedBy = null;
            _entities = null;
            _agent = null;
            _approaching = null;
            _activeDrones = null;
            _containers = null;
            _priorityTargets.ForEach(pt => pt.ClearCache());
            _entitiesById.Clear();
        }

        public string FilterPath(string path)
        {
            if (path == null)
                return string.Empty;

            path = path.Replace("\"", "");
            path = path.Replace("?", "");
            path = path.Replace("\\", "");
            path = path.Replace("/", "");
            path = path.Replace("'", "");
            path = path.Replace("*", "");
            path = path.Replace(":", "");
            path = path.Replace(">", "");
            path = path.Replace("<", "");
            path = path.Replace(".", "");
            path = path.Replace(",", "");
            while (path.IndexOf("  ") >= 0)
                path = path.Replace("  ", " ");
            return path.Trim();
        }

        /// <summary>
        ///   Loads mission objectives from XML file
        /// </summary>
        /// <param name = "agentId"></param>
        /// <param name = "pocketId"></param>
        /// <returns></returns>
        public IEnumerable<Action> LoadMissionActions(long agentId, int pocketId, bool mission_mode)
        {
            var mission = GetAgentMission(agentId);
            if(mission == null && mission_mode)
                return new Action[0];

            var missionName = FilterPath(mission.Name);
            var missionXmlPath = Path.Combine(Settings.Instance.MissionsPath, missionName + ".xml");
            if (!File.Exists(missionXmlPath))
            {
                //No mission file but we need to set some cache settings
                OrbitDistance = Settings.Instance.OrbitDistance;
                afterMissionSalvaging = Settings.Instance.AfterMissionSalvaging;
                return new Action[0];
            }

            try
            {
                var xdoc = XDocument.Load(missionXmlPath);
                var pockets = xdoc.Root.Element("pockets").Elements("pocket");
                foreach (var pocket in pockets)
                {
                    if ((int) pocket.Attribute("id") != pocketId)
                        continue;

                    if (pocket.Element("damagetype") != null)
                        DamageType = (DamageType) Enum.Parse(typeof (DamageType), (string) pocket.Element("damagetype"), true);

					if (pocket.Element("orbitdistance") != null) 	//Load OrbitDistance from mission.xml, if present
                    {
                        
                        OrbitDistance = (int) pocket.Element("orbitdistance");
                        Logging.Log(string.Format("Cache: Using Mission Orbit distance {0}",OrbitDistance));
                    }
					else											//Otherwise, use value defined in charname.xml file
                    {
						OrbitDistance = Settings.Instance.OrbitDistance;
                        Logging.Log(string.Format("Cache: Using Settings Orbit distance {0}",OrbitDistance));
                    }
					if (pocket.Element("afterMissionSalvaging") != null) 	//Load afterMissionSalvaging setting from mission.xml, if present
                    {
    					afterMissionSalvaging = (bool)pocket.Element("afterMissionSalvaging");
					}

						
                    var actions = new List<Action>();
                    var elements = pocket.Element("actions");
                    if (elements != null)
                    {
                        foreach (var element in elements.Elements("action"))
                        {
                            var action = new Action();
                            action.State = (ActionState) Enum.Parse(typeof (ActionState), (string) element.Attribute("name"), true);
                            if ((string)element.Attribute("name").Value == "ClearPocket")
                            {
                                action.AddParameter("", "");
                            }
                            else
                            {
                            foreach (var parameter in element.Elements("parameter"))
                                action.AddParameter((string) parameter.Attribute("name"), (string) parameter.Attribute("value"));
                            }
                            actions.Add(action);
                        }
                    }
                    return actions;
                }

                // if we reach this code there is no mission XML file, so we set some things -- Assail

                OrbitDistance = Settings.Instance.OrbitDistance;
                Logging.Log(string.Format("Cache: Using Settings Orbit distance {0}", OrbitDistance));

                return new Action[0];
            }
            catch (Exception ex)
            {
                Logging.Log("Error loading mission XML file [" + ex.Message + "]");
                return new Action[0];
            }
        }

        /// <summary>
        ///   Refresh the mission items
        /// </summary>
        public void RefreshMissionItems(long agentId)
        {
            // Clear out old items
            MissionItems.Clear();
            BringMissionItem = string.Empty;

            var mission = GetAgentMission(agentId);
            if (mission == null)
                return;
            if (factionName == null || factionName == "")
                factionName = "Default";

            if (Settings.Instance.UseFittingManager)
            {
                //Set fitting to default
                DefaultFitting = (string)Settings.Instance.DefaultFitting.Fitting;
                Fitting = DefaultFitting;
                MissionShip = "";
                ChangeMissionShipFittings = false;
                if (Settings.Instance.MissionFitting.Any(m => m.Mission.ToLower() == mission.Name.ToLower())) //priority goes to mission-specific fittings
                {
                    string _missionFit;
                    string _missionShip;
                    MissionFitting _missionFitting;

                    // if we've got multiple copies of the same mission, find the one with the matching faction
                    if (Settings.Instance.MissionFitting.Any(m => m.Faction.ToLower() == factionName.ToLower() && (m.Mission.ToLower() == mission.Name.ToLower())))
                        _missionFitting = Settings.Instance.MissionFitting.FirstOrDefault(m => m.Faction.ToLower() == factionName.ToLower() && (m.Mission.ToLower() == mission.Name.ToLower()));
                    else //otherwise just use the first copy of that mission
                        _missionFitting = Settings.Instance.MissionFitting.FirstOrDefault(m => m.Mission.ToLower() == mission.Name.ToLower());

                    _missionFit = (string)_missionFitting.Fitting;
                    _missionShip = (string)_missionFitting.Ship;
                    if (!(_missionFit == "" && !(_missionShip == ""))) // if we've both specified a mission specific ship and a fitting, then apply that fitting to the ship
                    {
                        ChangeMissionShipFittings = true;
                        Fitting = _missionFit;
                    }
                    else if (!((factionFit == null) || (factionFit == "")))
                        Fitting = factionFit;
                    Logging.Log("Cache: Mission: " + _missionFitting.Mission + " - Faction: " + factionName + " - Fitting: " + _missionFit + " - Ship: " + _missionShip + " - ChangeMissionShipFittings: " + ChangeMissionShipFittings);
                    MissionShip = _missionShip;
                }
                else if (!((factionFit == null) || (factionFit == ""))) // if no mission fittings defined, try to match by faction
                    Fitting = factionFit;

                if (Fitting == "") // otherwise use the default
                    Fitting = DefaultFitting;
            }

            var missionName = FilterPath(mission.Name);
            var missionXmlPath = Path.Combine(Settings.Instance.MissionsPath, missionName + ".xml");
            if (!File.Exists(missionXmlPath))
                return;

            try
            {
                var xdoc = XDocument.Load(missionXmlPath);
                var items = ((IEnumerable)xdoc.XPathEvaluate("//action[(translate(@name, 'LOT', 'lot')='loot') or (translate(@name, 'LOTIEM', 'lotiem')='lootitem')]/parameter[translate(@name, 'TIEM', 'tiem')='item']/@value")).Cast<XAttribute>().Select(a => ((string)a ?? string.Empty).ToLower());
                MissionItems.AddRange(items);

                BringMissionItem = (string) xdoc.Root.Element("bring") ?? string.Empty;
                BringMissionItem = BringMissionItem.ToLower();

                //load fitting setting from the mission file
                //Fitting = (string)xdoc.Root.Element("fitting") ?? "default";  
            }
            catch (Exception ex)
            {
                Logging.Log("Error loading mission XML file [" + ex.Message + "]");
            }
        }

        /// <summary>
        ///   Remove targets from priority list
        /// </summary>
        /// <param name = "targets"></param>
        public bool RemovePriorityTargets(IEnumerable<EntityCache> targets)
        {
            return _priorityTargets.RemoveAll(pt => targets.Any(t => t.Id == pt.EntityID)) > 0;
        }

        /// <summary>
        ///   Add priority targets
        /// </summary>
        /// <param name = "targets"></param>
        /// <param name = "priority"></param>
        public void AddPriorityTargets(IEnumerable<EntityCache> targets, Priority priority)
        {
            foreach (var target in targets)
            {
                if (_priorityTargets.Any(pt => pt.EntityID == target.Id))
                    continue;

                _priorityTargets.Add(new PriorityTarget {EntityID = target.Id, Priority = priority});
            }
        }

        /// <summary>
        ///   Calculate distance from me
        /// </summary>
        /// <param name = "x"></param>
        /// <param name = "y"></param>
        /// <param name = "z"></param>
        /// <returns></returns>
        public double DistanceFromMe(double x, double y, double z)
        {
            if (DirectEve.ActiveShip.Entity == null)
                return double.MaxValue;

            var curX = DirectEve.ActiveShip.Entity.X;
            var curY = DirectEve.ActiveShip.Entity.Y;
            var curZ = DirectEve.ActiveShip.Entity.Z;

            return Math.Sqrt((curX - x)*(curX - x) + (curY - y)*(curY - y) + (curZ - z)*(curZ - z));
        }

        /// <summary>
        ///   Create a bookmark
        /// </summary>
        /// <param name = "label"></param>
        public void CreateBookmark(string label)
        {
            DirectEve.BookmarkCurrentLocation(label, "", null);
        }

        /// <summary>
        ///   Create a bookmark of the closest wreck
        /// </summary>
        /// <param name = "label"></param>
        //public void CreateBookmarkofWreck(IEnumerable<EntityCache> containers, string label)
        //{
        //    DirectEve.BookmarkEntity(Cache.Instance.Containers.FirstOrDefault, "a", "a", null);
        //}

        private Func<EntityCache, int> OrderByLowestHealth()
        {
            return t => (int)(t.ShieldPct + t.ArmorPct + t.StructurePct);
        }

        /// <summary>
        ///   Return the best possible target (based on current target, distance and low value first)
        /// </summary>
        /// <param name="currentTarget"></param>
        /// <param name="distance"></param>
        /// <param name="lowValueFirst"></param>
        /// <returns></returns>
        public EntityCache GetBestTarget(EntityCache currentTarget, double distance, bool lowValueFirst)
        {
            // Do we have a 'current target' and if so, is it an actual target?
            // If not, clear current target
            if (currentTarget != null && !currentTarget.IsTarget)
                currentTarget = null;

            // Is our current target a warp scrambling priority target?
            if (currentTarget != null && PriorityTargets.Any(pt => pt.Id == currentTarget.Id && pt.IsWarpScramblingMe && pt.IsTarget))
                return currentTarget;

            // Get the closest warp scrambling priority target
            var target = PriorityTargets.OrderBy(OrderByLowestHealth()).ThenBy(t => t.Distance).FirstOrDefault(pt => pt.Distance < distance && pt.IsWarpScramblingMe && pt.IsTarget);
            if (target != null)
                return target;

            // Is our current target any other priority target?
            if (currentTarget != null && PriorityTargets.Any(pt => pt.Id == currentTarget.Id))
                return currentTarget;

            // Get the closest priority target
            target = PriorityTargets.OrderBy(OrderByLowestHealth()).ThenBy(t => t.Distance).FirstOrDefault(pt => pt.Distance < distance && pt.IsTarget);
            if (target != null)
                return target;

            // Do we have a target?
            if (currentTarget != null)
                return currentTarget;

            // Get all entity targets
            var targets = Targets.Where(e => e.CategoryId == (int)CategoryID.Entity && e.IsNpc && !e.IsContainer && e.GroupId != (int)Group.LargeCollidableStructure);

            // Get the closest high value target
            var highValueTarget = targets.Where(t => t.TargetValue.HasValue && t.Distance < distance).OrderByDescending(t => t.TargetValue.Value).ThenBy(OrderByLowestHealth()).ThenBy(t => t.Distance).FirstOrDefault();
            // Get the closest low value target
            var lowValueTarget = targets.Where(t => !t.TargetValue.HasValue && t.Distance < distance).OrderBy(OrderByLowestHealth()).ThenBy(t => t.Distance).FirstOrDefault();
            
            if (Settings.Instance.DontShootFrigatesWithSiegeorAutoCannons)
            {
                if (Cache.Instance.MissionWeaponGroupId == 55 || Cache.Instance.MissionWeaponGroupId == 508)
                {
                    if (lowValueTarget != null && !lowValueFirst && lowValueTarget.Distance > (int)Distance.InsideThisRangeIsLIkelyToBeMostlyFrigates)
                        return targets.Where(t => !t.TargetValue.HasValue && t.Distance > (int)Distance.InsideThisRangeIsLIkelyToBeMostlyFrigates && t.Distance < distance).OrderByDescending(t => t.Distance).FirstOrDefault();
                    if (lowValueTarget != null && !lowValueFirst && lowValueTarget.Distance < (int)Distance.InsideThisRangeIsLIkelyToBeMostlyFrigates)
                        lowValueTarget = null;
                }
            }
            if (lowValueFirst && lowValueTarget != null)
                return lowValueTarget;
            if (!lowValueFirst && highValueTarget != null)
                return highValueTarget;

            // Return either one or the other
            return lowValueTarget ?? highValueTarget;
        }
    }
}