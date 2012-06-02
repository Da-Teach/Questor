// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Actions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using DirectEve;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;

    public class AgentInteraction
    {
        public const string RequestMission = "Request Mission";
        public const string ViewMission = "View Mission";
        public const string CompleteMission = "Complete Mission";
        public const string LocateCharacter = "Locate Character";
        public const string Accept = "Accept";
        public const string Decline = "Decline";
        public const string Close = "Close";
        private DateTime _nextAgentAction;
        bool _agentStandingsCheckFlag;  //false;
        DateTime _agentStandingsCheckTimeOut = DateTime.MaxValue;

        //private DateTime _waitingOnAgentResponse;
        private bool _waitingonmission;

        private DateTime _waitingonmissiontimer = DateTime.Now;

        private bool _waitingonagentwindow;
        private DateTime _waitingonagentwindowtimer = DateTime.Now;

        private bool _waitingonagentresponse;
        private DateTime _waitingonagentresponsetimer = DateTime.Now;

        public bool WaitDecline { get; set; }

        public AgentInteraction()
        {
            AmmoToLoad = new List<Ammo>();
        }

        public long AgentId { get; set; }

        public DirectAgent Agent
        {
            get { return Cache.Instance.DirectEve.GetAgentById(AgentId); }
        }

        public bool ForceAccept { get; set; }

        public AgentInteractionPurpose Purpose { get; set; }

        public List<Ammo> AmmoToLoad { get; private set; }

        private void LoadSpecificAmmo(IEnumerable<DamageType> damageTypes)
        {
            AmmoToLoad.Clear();
            AmmoToLoad.AddRange(Settings.Instance.Ammo.Where(a => damageTypes.Contains(a.DamageType)).Select(a => a.Clone()));
        }

        private void WaitForConversation()
        {
            WaitDecline = Settings.Instance.WaitDecline;

            DirectAgentWindow agentWindow = Agent.Window;
            if (agentWindow == null || !agentWindow.IsReady)
                return;

            if (Purpose == AgentInteractionPurpose.AmmoCheck)
            {
                Logging.Log("AgentInteraction", "Checking ammo type", Logging.yellow);
                _States.CurrentAgentInteractionState = AgentInteractionState.WaitForMission;
            }
            else
            {
                Logging.Log("AgentInteraction", "Replying to agent", Logging.yellow);
                _States.CurrentAgentInteractionState = AgentInteractionState.ReplyToAgent;
                _nextAgentAction = DateTime.Now.AddSeconds(7);
            }
        }

        private void ReplyToAgent()
        {
            DirectAgentWindow agentWindow = Agent.Window;
            if (agentWindow == null || !agentWindow.IsReady)
            {
                if (_waitingonagentwindow == false)
                {
                    _waitingonagentwindowtimer = DateTime.Now;
                    _waitingonagentwindow = true;
                }
                if (DateTime.Now.Subtract(_waitingonagentwindowtimer).TotalSeconds > 10)
                {
                    Logging.Log("AgentInteraction", "ReplyToAgent: Agent.window is not yet open : waiting", Logging.yellow);

                    if (DateTime.Now.Subtract(_waitingonagentwindowtimer).TotalSeconds > 15)
                    {
                        Logging.Log("AgentInteraction.Agentid", " [" + AgentId + "] Regular Mission AgentID [ " + Cache.Instance.AgentId + "] these should match when not doing a storyline mission", Logging.yellow);
                    }
                    if (DateTime.Now.Subtract(_waitingonagentwindowtimer).TotalSeconds > 90)
                    {
                        Cache.Instance.CloseQuestorCMDLogoff = false;
                        Cache.Instance.CloseQuestorCMDExitGame = true;
                        Cache.Instance.ReasonToStopQuestor = "AgentInteraction: ReplyToAgent: Agent Window would not open/refresh- agentwindow was null: restarting EVE Session";
                        Logging.Log("ReasonToStopQuestor", Cache.Instance.ReasonToStopQuestor, Logging.yellow);
                        Cache.Instance.SessionState = "Quitting";
                    }
                }
                return;
            }
            else
            {
                _waitingonagentwindow = false;
            }

            List<DirectAgentResponse> responses = agentWindow.AgentResponses;
            if (responses == null || responses.Count == 0)
            {
                if (_waitingonagentresponse == false)
                {
                    _waitingonagentresponsetimer = DateTime.Now;
                    _waitingonagentresponse = true;
                }
                if (DateTime.Now.Subtract(_waitingonagentresponsetimer).TotalSeconds > 15)
                {
                    Logging.Log("AgentInteraction", "ReplyToAgent: agentWindowAgentresponses == null : trying to close the agent window", Logging.yellow);
                    agentWindow.Close();
                    _waitingonagentwindowtimer = DateTime.Now;
                }
                return;
            }
            else
            {
                _waitingonagentresponse = false;
            }

            DirectAgentResponse request = responses.FirstOrDefault(r => r.Text.Contains(RequestMission));
            DirectAgentResponse complete = responses.FirstOrDefault(r => r.Text.Contains(CompleteMission));
            DirectAgentResponse view = responses.FirstOrDefault(r => r.Text.Contains(ViewMission));
            DirectAgentResponse accept = responses.FirstOrDefault(r => r.Text.Contains(Accept));
            DirectAgentResponse decline = responses.FirstOrDefault(r => r.Text.Contains(Decline));

            if (complete != null)
            {
                if (Purpose == AgentInteractionPurpose.CompleteMission)
                {
                    // Complete the mission, close convo
                    Logging.Log("AgentInteraction", "Saying [Complete Mission]", Logging.yellow);
                    complete.Say();

                    Logging.Log("AgentInteraction", "Closing conversation", Logging.yellow);

                    _States.CurrentAgentInteractionState = AgentInteractionState.CloseConversation;
                    _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                }
                else
                {
                    Logging.Log("AgentInteraction", "Waiting for mission", Logging.yellow);

                    // Apparently someone clicked "accept" already
                    _States.CurrentAgentInteractionState = AgentInteractionState.WaitForMission;
                    _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(3, 7));
                }
            }
            else if (request != null)
            {
                if (Purpose == AgentInteractionPurpose.StartMission)
                {
                    // Request a mission and wait for it
                    Logging.Log("AgentInteraction", "Saying [Request Mission]", Logging.yellow);
                    request.Say();

                    Logging.Log("AgentInteraction", "Waiting for mission", Logging.yellow);
                    _States.CurrentAgentInteractionState = AgentInteractionState.WaitForMission;
                    _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                }
                else
                {
                    Logging.Log("AgentInteraction", "Unexpected dialog options", Logging.red);
                    _States.CurrentAgentInteractionState = AgentInteractionState.UnexpectedDialogOptions;
                }
            }
            else if (view != null)
            {
                // View current mission
                Logging.Log("AgentInteraction", "Saying [View Mission]", Logging.yellow);

                view.Say();
                _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                // No state change
            }
            else if (accept != null || decline != null)
            {
                if (Purpose == AgentInteractionPurpose.StartMission)
                {
                    Logging.Log("AgentInteraction", "Waiting for mission", Logging.yellow);

                    _States.CurrentAgentInteractionState = AgentInteractionState.WaitForMission; // Do not say anything, wait for the mission
                    _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(5, 15));
                }
                else
                {
                    Logging.Log("AgentInteraction", "Unexpected dialog options", Logging.red);

                    _States.CurrentAgentInteractionState = AgentInteractionState.UnexpectedDialogOptions;
                }
            }
        }

        private DamageType GetMissionDamageType(string html)
        {
            // We are going to check damage types
            var logoRegex = new Regex("img src=\"factionlogo:(?<factionlogo>\\d+)");

            Match logoMatch = logoRegex.Match(html);
            if (logoMatch.Success)
            {
                var logo = logoMatch.Groups["factionlogo"].Value;

                // Load faction xml
                XDocument xml = XDocument.Load(Path.Combine(Settings.Instance.Path, "Factions.xml"));
                if (xml.Root != null)
                {
                    XElement faction = xml.Root.Elements("faction").FirstOrDefault(f => (string)f.Attribute("logo") == logo);
                    if (faction != null)
                        return (DamageType)Enum.Parse(typeof(DamageType), (string)faction.Attribute("damagetype"));
                }
            }

            return DamageType.EM;
        }

        private void WaitForMission()
        {
            DirectAgentWindow agentWindow = Agent.Window;
            if (agentWindow == null || !agentWindow.IsReady)
            {
                if (_waitingonagentwindow == false)
                {
                    _waitingonagentwindowtimer = DateTime.Now;
                    _waitingonagentwindow = true;
                }
                if (DateTime.Now.Subtract(_waitingonagentwindowtimer).TotalSeconds > 10)
                {
                    Logging.Log("AgentInteraction", "WaitForMission: Agent.window is not yet open : waiting", Logging.yellow);

                    if (DateTime.Now.Subtract(_waitingonagentwindowtimer).TotalSeconds > 15)
                    {
                        Logging.Log("AgentInteraction.Agentid", " [" + AgentId + "] Cache.Instance.AgentId [ " + Cache.Instance.AgentId + "] should be the same if not doing a storyline mission", Logging.yellow);
                    }
                    if (DateTime.Now.Subtract(_waitingonagentwindowtimer).TotalSeconds > 90)
                    {
                        Cache.Instance.CloseQuestorCMDLogoff = false;
                        Cache.Instance.CloseQuestorCMDExitGame = true;
                        Cache.Instance.ReasonToStopQuestor = "AgentInteraction: WaitforMission: Journal would not open/refresh- journalwindows was null: restarting EVE Session";
                        Logging.Log("ReasonToStopQuestor", Cache.Instance.ReasonToStopQuestor, Logging.yellow);
                        Cache.Instance.SessionState = "Quitting";
                    }
                }
                return;
            }
            else
            {
                _waitingonagentwindow = false;
            }

            //open the journal window
            if (!Cache.Instance.OpenJournalWindow("AgentInteraction")) return;

            Cache.Instance.Mission = Cache.Instance.GetAgentMission(AgentId);
            if (Cache.Instance.Mission == null)
            {
                if (_waitingonmission == false)
                {
                    _waitingonmissiontimer = DateTime.Now;
                    _waitingonmission = true;
                }
                if (DateTime.Now.Subtract(_waitingonmissiontimer).TotalSeconds > 30)
                {
                    Logging.Log("AgentInteraction", "WaitForMission: Unable to find mission from that agent (yet?) : AgentInteraction.AgentId [" + AgentId + "] regular Mission AgentID [" + Cache.Instance.AgentId + "]", Logging.yellow);
                    Cache.Instance.JournalWindow.Close();
                    if (DateTime.Now.Subtract(_waitingonmissiontimer).TotalSeconds > 120)
                    {
                        Cache.Instance.CloseQuestorCMDLogoff = false;
                        Cache.Instance.CloseQuestorCMDExitGame = true;
                        Cache.Instance.ReasonToStopQuestor = "AgentInteraction: WaitforMission: Journal would not open/refresh - mission was null: restarting EVE Session";
                        Logging.Log("ReasonToStopQuestor", Cache.Instance.ReasonToStopQuestor, Logging.yellow);
                        Cache.Instance.SessionState = "Quitting";
                    }
                }
                return;
            }
            else
            {
                _waitingonmission = false;
            }

            string missionName = Cache.Instance.FilterPath(Cache.Instance.Mission.Name);

            Logging.Log("AgentInteraction", "[" + Agent.Name + "] standing toward me is [" + Cache.Instance.AgentEffectiveStandingtoMe.ToString("0.00") + "], minAgentGreyListStandings: [" + Settings.Instance.MinAgentGreyListStandings + "]", Logging.yellow);

            string html = agentWindow.Objective;
            if (CheckFaction() || Settings.Instance.MissionBlacklist.Any(m => m.ToLower() == missionName.ToLower()))
            {
                if (Purpose != AgentInteractionPurpose.AmmoCheck)
                    Logging.Log("AgentInteraction", "Declining blacklisted faction mission", Logging.yellow);

                _States.CurrentAgentInteractionState = AgentInteractionState.DeclineMission;
                _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                return;
            }

            if (Settings.Instance.MissionGreylist.Any(m => m.ToLower() == Cache.Instance.MissionName.ToLower()) && Cache.Instance.AgentEffectiveStandingtoMe > Settings.Instance.MinAgentGreyListStandings) //-1.7
            {
                Logging.Log("AgentInteraction", "Declining greylisted mission [" + Cache.Instance.MissionName + "]", Logging.yellow);
                _States.CurrentAgentInteractionState = AgentInteractionState.DeclineMission;
                _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                return;
            }

            if (html.Contains("The route generated by current autopilot settings contains low security systems!"))
            {
                if ((missionName != "Enemies Abound (2 of 5)") || (missionName == "Enemies Abound (2 of 5)" && !Settings.Instance.LowSecMissionsInShuttles))
                {
                    if (Purpose != AgentInteractionPurpose.AmmoCheck)
                        Logging.Log("AgentInteraction", "Declining low-sec mission", Logging.yellow);

                    _States.CurrentAgentInteractionState = AgentInteractionState.DeclineMission;
                    _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(3, 7));
                    return;
                }
            }

            if (!ForceAccept)
            {
                // Is the mission offered?
                if (Cache.Instance.Mission.State == (int)MissionState.Offered && (Cache.Instance.Mission.Type == "Mining" || Cache.Instance.Mission.Type == "Trade" || (Cache.Instance.Mission.Type == "Courier" && missionName != "Enemies Abound (2 of 5)")))
                {
                    Logging.Log("AgentInteraction", "Declining courier/mining/trade", Logging.yellow);

                    _States.CurrentAgentInteractionState = AgentInteractionState.DeclineMission;
                    _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(5, 10));
                    return;
                }
            }

            if (missionName != "Enemies Abound (2 of 5)")
            {
                bool loadedAmmo = false;

                Cache.Instance.missionXmlPath = Path.Combine(Settings.Instance.MissionsPath, missionName + ".xml");
                Cache.Instance.MissionAmmo = new List<Ammo>();
                if (File.Exists(Cache.Instance.missionXmlPath))
                {
                    Logging.Log("AgentInteraction", "Loading mission xml [" + missionName + "] from [" + Cache.Instance.missionXmlPath + "]", Logging.yellow);
                    Cache.Instance.MissionXMLIsAvailable = true;
                    //
                    // this loads the settings global to the mission, NOT individual pockets
                    //
                    try
                    {
                        XDocument missionXml = XDocument.Load(Cache.Instance.missionXmlPath);
                        //load mission specific ammo and weapongroupid if specified in the mission xml
                        XElement ammoTypes = missionXml.Root.Element("missionammo");
                        if (ammoTypes != null)
                            foreach (XElement ammo in ammoTypes.Elements("ammo"))
                                Cache.Instance.MissionAmmo.Add(new Ammo(ammo));
                        Cache.Instance.MissionWeaponGroupId = (int?)missionXml.Root.Element("weaponGroupId") ?? 0;
                        Cache.Instance.MissionUseDrones = (bool?)missionXml.Root.Element("useDrones"); //do not set default here, use character level settings if avail
                        Cache.Instance.MissionKillSentries = (bool?)missionXml.Root.Element("killSentries"); //do not set default here, use character level settings if avail
                        //Cache.Instance.MissionDroneTypeID = (int?)missionXml.Root.Element("DroneTypeId") ?? Settings.Instance.DroneTypeId;
                        IEnumerable<DamageType> damageTypes = missionXml.XPathSelectElements("//damagetype").Select(e => (DamageType)Enum.Parse(typeof(DamageType), (string)e, true)).ToList();
                        if (damageTypes.Any())
                        {
                            LoadSpecificAmmo(damageTypes.Distinct());
                            loadedAmmo = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("AgentInteraction", "Error parsing damage types for mission [" + Cache.Instance.Mission.Name + "], " + ex.Message, Logging.orange);
                    }
                }
                else
                {
                    Logging.Log("AgentInteraction", "Missing mission xml [" + missionName + "] from [" + Cache.Instance.missionXmlPath + "] !!!", Logging.orange);
                    Cache.Instance.MissionXMLIsAvailable = false;
                }

                if (!loadedAmmo)
                {
                    Logging.Log("AgentInteraction", "Detecting damage type for [" + missionName + "]", Logging.yellow);
                    Cache.Instance.DamageType = GetMissionDamageType(html);
                    LoadSpecificAmmo(new[] { Cache.Instance.DamageType });
                }

                if (Purpose == AgentInteractionPurpose.AmmoCheck)
                {
                    Logging.Log("AgentInteraction", "Closing conversation", Logging.yellow);

                    _States.CurrentAgentInteractionState = AgentInteractionState.CloseConversation;
                    return;
                }
            }

            if (missionName == "Enemies Abound (2 of 5)")
                Cache.Instance.CourierMission = true;
            else
                Cache.Instance.CourierMission = false;

            Cache.Instance.MissionName = missionName;

            if (Cache.Instance.Mission.State == (int)MissionState.Offered)
            {
                Logging.Log("AgentInteraction", "Accepting mission [" + missionName + "]", Logging.yellow);

                _States.CurrentAgentInteractionState = AgentInteractionState.AcceptMission;
                _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(3, 7));
            }
            else // If we already accepted the mission, close the convo
            {
                Logging.Log("AgentInteraction", "Mission [" + missionName + "] already accepted", Logging.yellow);
                Logging.Log("AgentInteraction", "Closing conversation", Logging.yellow);
                //CheckFaction();
                _States.CurrentAgentInteractionState = AgentInteractionState.CloseConversation;
                _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(3, 7));
            }
        }

        private void AcceptMission()
        {
            DirectAgentWindow agentWindow = Agent.Window;
            if (agentWindow == null || !agentWindow.IsReady)
                return;

            List<DirectAgentResponse> responses = agentWindow.AgentResponses;
            if (responses == null || responses.Count == 0)
                return;

            DirectAgentResponse accept = responses.FirstOrDefault(r => r.Text.Contains(Accept));
            if (accept == null)
                return;

            Logging.Log("AgentInteraction", "Saying [Accept]", Logging.yellow);
            Cache.Instance.Wealth = Cache.Instance.DirectEve.Me.Wealth;
            accept.Say();

            foreach (DirectWindow window in Cache.Instance.Windows)
            {
                if (window.Name == "modal")
                {
                    bool sayyes = false;
                    if (!string.IsNullOrEmpty(window.Html))
                    {
                        //
                        // Modal Dialogs the need "yes" pressed
                        //
                        sayyes |= window.Html.Contains("objectives requiring a total capacity");
                        sayyes |= window.Html.Contains("your ship only has space for");
                    }
                    if (sayyes)
                    {
                        Logging.Log("AgentInteraction", "Found a window that needs 'yes' chosen...", Logging.yellow);
                        Logging.Log("AgentInteraction", "Content of modal window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]", Logging.yellow);
                        window.AnswerModal("Yes");
                        continue;
                    }
                }
            }
            Logging.Log("AgentInteraction", "Closing conversation", Logging.yellow);
            _States.CurrentAgentInteractionState = AgentInteractionState.CloseConversation;
            _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(3, 7));
        }

        private void DeclineMission()
        {
            // If we are doing an ammo check then Decline Mission is an end-state!
            if (Purpose == AgentInteractionPurpose.AmmoCheck)
                return;

            DirectAgentWindow agentWindow = Agent.Window;
            if (agentWindow == null || !agentWindow.IsReady)
                return;

            List<DirectAgentResponse> responses = agentWindow.AgentResponses;
            if (responses == null || responses.Count == 0)
                return;

            DirectAgentResponse decline = responses.FirstOrDefault(r => r.Text.Contains(Decline));
            if (decline == null)
                return;

            // Check for agent decline timer
            if (WaitDecline)
            {
                string html = agentWindow.Briefing;
                if (html.Contains("Declining a mission from this agent within the next"))
                {
                    //this need to divide by 10 was a remnant of the html scrape method we were using before. this can likely be removed now.
                    if (Cache.Instance.AgentEffectiveStandingtoMe != 0)
                    {
                        if (Cache.Instance.AgentEffectiveStandingtoMe > 10)
                        {
                            Cache.Instance.AgentEffectiveStandingtoMe = Cache.Instance.AgentEffectiveStandingtoMe / 10;
                        }
                        if (Settings.Instance.MinAgentBlackListStandings > 10)
                        {
                            Settings.Instance.MinAgentBlackListStandings = Settings.Instance.MinAgentBlackListStandings / 10;
                        }
                        Logging.Log("AgentInteraction", "Agent decline timer detected. Current standings: " + Math.Round(Cache.Instance.AgentEffectiveStandingtoMe, 2) + ". Minimum standings: " + Math.Round(Settings.Instance.MinAgentBlackListStandings, 2), Logging.yellow);
                    }

                    var hourRegex = new Regex("\\s(?<hour>\\d+)\\shour");
                    var minuteRegex = new Regex("\\s(?<minute>\\d+)\\sminute");
                    Match hourMatch = hourRegex.Match(html);
                    Match minuteMatch = minuteRegex.Match(html);
                    int hours = 0;
                    int minutes = 0;
                    if (hourMatch.Success)
                    {
                        string hourValue = hourMatch.Groups["hour"].Value;
                        hours = Convert.ToInt32(hourValue);
                    }
                    if (minuteMatch.Success)
                    {
                        string minuteValue = minuteMatch.Groups["minute"].Value;
                        minutes = Convert.ToInt32(minuteValue);
                    }

                    int secondsToWait = ((hours * 3600) + (minutes * 60) + 60);
                    AgentsList currentAgent = Settings.Instance.AgentsList.FirstOrDefault(i => i.Name == Cache.Instance.CurrentAgent);

                    if (Cache.Instance.AgentEffectiveStandingtoMe <= Settings.Instance.MinAgentBlackListStandings && Cache.Instance.IsAgentLoop)
                    {
                        _nextAgentAction = DateTime.Now.AddSeconds(secondsToWait);
                        Logging.Log("AgentInteraction", "Current standings [" + Math.Round(Cache.Instance.AgentEffectiveStandingtoMe, 2) + "] at or below configured minimum of [" + Settings.Instance.MinAgentBlackListStandings + "].  Waiting " + (secondsToWait / 60) + " minutes to try decline again.", Logging.yellow);
                        CloseConversation();

                        _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;
                        return;
                    }

                    //add timer to current agent
                    if (!Cache.Instance.IsAgentLoop && Settings.Instance.MultiAgentSupport)
                    {
                        if (currentAgent != null) currentAgent.DeclineTimer = DateTime.Now.AddSeconds(secondsToWait);
                        CloseConversation();

                        Cache.Instance.CurrentAgent = Cache.Instance.SwitchAgent;
                        Logging.Log("AgentInteraction", "new agent is " + Cache.Instance.CurrentAgent, Logging.yellow);
                        _States.CurrentAgentInteractionState = AgentInteractionState.ChangeAgent;
                        return;
                    }
                    Logging.Log("AgentInteraction", "Current standings [" + Cache.Instance.AgentEffectiveStandingtoMe + "] is above or configured minimum [" + Settings.Instance.MinAgentBlackListStandings + "].  Declining [" + Cache.Instance.MissionName + "]", Logging.yellow);
                }
            }
         if (_States.CurrentStorylineState == StorylineState.AcceptMission)
         {
             Logging.Log("AgentInteraction", "Storyline: Storylines cannot be declined, thus we will add this agent to the blacklist for this session.", Logging.yellow);
             _States.CurrentStorylineState = StorylineState.Idle;
             _States.CurrentCombatMissionBehaviorState = CombatMissionsBehaviorState.GotoBase;
             _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
             Cache.Instance.AgentBlacklist.Add(Cache.Instance.CurrentStorylineAgentId);
             return;
         }


            // Decline and request a new mission
            Logging.Log("AgentInteraction", "Saying [Decline]", Logging.yellow);
            decline.Say();

            Logging.Log("AgentInteraction", "Replying to agent", Logging.yellow);
            _States.CurrentAgentInteractionState = AgentInteractionState.ReplyToAgent;
            _nextAgentAction = DateTime.Now.AddSeconds(Cache.Instance.RandomNumber(3, 7));
        }

        public bool CheckFaction()
        {
            DirectAgentWindow agentWindow = Agent.Window;
            string html = agentWindow.Objective;
            var logoRegex = new Regex("img src=\"factionlogo:(?<factionlogo>\\d+)");
            Match logoMatch = logoRegex.Match(html);
            if (logoMatch.Success)
            {
                string logo = logoMatch.Groups["factionlogo"].Value;

                // Load faction xml
                XDocument xml = XDocument.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path2: "Factions.xml"));
                if (xml.Root != null)
                {
                    XElement faction = xml.Root.Elements("faction").FirstOrDefault(f => (string)f.Attribute("logo") == logo);
                    //Cache.Instance.factionFit = "Default";
                    //Cache.Instance.Fitting = "Default";
                    if (faction != null)
                    {
                        var factionName = ((string)faction.Attribute("name"));
                        Cache.Instance.FactionName = factionName;
                        Logging.Log("AgentInteraction", "Mission enemy faction: " + factionName, Logging.yellow);
                        if (Settings.Instance.FactionBlacklist.Any(m => m.ToLower() == factionName.ToLower()))
                            return true;
                        if (Settings.Instance.UseFittingManager && Settings.Instance.FactionFitting.Any(m => m.Faction.ToLower() == factionName.ToLower()))
                        {
                            FactionFitting factionFitting = Settings.Instance.FactionFitting.FirstOrDefault(m => m.Faction.ToLower() == factionName.ToLower());
                            if (factionFitting != null)
                            {
                                Cache.Instance.FactionFit = factionFitting.Fitting;
                                Logging.Log("AgentInteraction", "Faction fitting: " + factionFitting.Faction, Logging.yellow);
                            }
                            else
                            {
                                Logging.Log("AgentInteraction", "Faction fitting: No fittings defined for [ " + factionName + " ]", Logging.yellow);
                            }
                            //Cache.Instance.Fitting = Cache.Instance.factionFit;
                            return false;
                        }
                    }/*
                else if (Settings.Instance.FittingsDefined)
                {
                    Cache.Instance.factionName = "Default";
                    var FactionFitting = Settings.Instance.FactionFitting.FirstOrDefault(m => m.Faction.ToLower() == "default");
                    Cache.Instance.factionFit = (string)FactionFitting.Fitting;
                    Logging.Log("AgentInteraction","Faction fitting " + FactionFitting.Faction);
                    //Cache.Instance.Fitting = Cache.Instance.factionFit;
                    return false;
                }
                return false;  */
                }
                else
                {
                    Logging.Log("AgentInteraction", "Faction fitting: Missing Factions.xml :aborting faction fittings", Logging.yellow);
                }
            }
            if (Settings.Instance.UseFittingManager)
            {
                Cache.Instance.FactionName = "Default";
                FactionFitting factionFitting = Settings.Instance.FactionFitting.FirstOrDefault(m => m.Faction.ToLower() == "default");
                if (factionFitting != null)
                {
                    Cache.Instance.FactionFit = factionFitting.Fitting;
                    Logging.Log("AgentInteraction", "Faction fitting: " + factionFitting.Faction, Logging.yellow);
                }
                else
                {
                    Logging.Log("AgentInteraction", "Faction fitting: No fittings defined for [ " + Cache.Instance.FactionName + " ]", Logging.orange);
                }
                //Cache.Instance.Fitting = Cache.Instance.factionFit;
            }
            return false;
        }

        public void CloseConversation()
        {
            if (DateTime.Now < _nextAgentAction)
            {
                Logging.Log("AgentInteraction.CloseConversation", "will continue in [" + Math.Round(_nextAgentAction.Subtract(DateTime.Now).TotalSeconds, 0) + "]sec", Logging.yellow);
                return;
            }
            DirectAgentWindow agentWindow = Agent.Window;
            if (agentWindow != null)
            {
                Logging.Log("AgentInteraction", "Attempting to close Agent Window", Logging.yellow);
                _nextAgentAction = DateTime.Now.AddSeconds(2);
                agentWindow.Close();
            }
            if (agentWindow == null)
            {
                Logging.Log("AgentInteraction", "Done", Logging.yellow);
                _States.CurrentAgentInteractionState = AgentInteractionState.Done;
                return;
            }
        }

        public void ProcessState()
        {
            if (!Cache.Instance.InStation)
                return;

            if (Cache.Instance.InSpace)
                return;

            // Wait a bit before doing "things"
            if (DateTime.Now < _nextAgentAction)
                return;

            switch (_States.CurrentAgentInteractionState)
            {
                case AgentInteractionState.Idle:
                    break;
                case AgentInteractionState.Done:
                    break;

                case AgentInteractionState.ChangeAgent:
                    Logging.Log("AgentInteraction", "Change Agent", Logging.yellow);
                    break;

                case AgentInteractionState.StartConversation:
                    Cache.Instance.AgentEffectiveStandingtoMe = Cache.Instance.DirectEve.Standings.EffectiveStanding(AgentId, Cache.Instance.DirectEve.Session.CharacterId ?? -1);

                    //
                    // Standings Check: if this is a totally new agent this check will timeout after 20 seconds
                    //
                    if (DateTime.Now < _agentStandingsCheckTimeOut)
                    {
                        if (((int)Cache.Instance.AgentEffectiveStandingtoMe == (int)0.00) && (AgentId == Cache.Instance.AgentId))
                        {
                            if (!_agentStandingsCheckFlag)
                            {
                                _agentStandingsCheckTimeOut = DateTime.Now.AddSeconds(20);
                                _agentStandingsCheckFlag = true;
                            }
                            Logging.Log("AgentInteraction.StandingsCheck", " Agent [" + Cache.Instance.DirectEve.GetAgentById(AgentId).Name + "] Standings show as [" + Cache.Instance.AgentEffectiveStandingtoMe + " and must not yet be available. retrying for [" + Math.Round((double)_agentStandingsCheckTimeOut.Subtract(DateTime.Now).Seconds, 0) + " sec]", Logging.yellow);
                            return;
                        }
                    }
                    Agent.InteractWith();

                    Logging.Log("AgentInteraction", "Waiting for conversation", Logging.yellow);
                    _States.CurrentAgentInteractionState = AgentInteractionState.WaitForConversation;
                    break;

                case AgentInteractionState.WaitForConversation:
                    WaitForConversation();
                    break;

                case AgentInteractionState.ReplyToAgent:
                    ReplyToAgent();
                    break;

                case AgentInteractionState.WaitForMission:
                    WaitForMission();
                    break;

                case AgentInteractionState.AcceptMission:
                    AcceptMission();
                    break;

                case AgentInteractionState.DeclineMission:
                    DeclineMission();
                    break;

                case AgentInteractionState.CloseConversation:
                    CloseConversation();
                    break;
            }
        }
    }
}