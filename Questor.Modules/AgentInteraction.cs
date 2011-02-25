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
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    using DirectEve;

    public class AgentInteraction
    {
        public const string RequestMission = "Request Mission";
        public const string ViewMission = "View Mission";
        public const string CompleteMission = "Complete Mission";
        public const string LocateCharacter = "Locate Character";
        public const string Accept = "Accept";
        public const string Decline = "Decline";
        public const string Close = "Close";
        private DateTime _lastMissionOpenRequest;
        private DateTime _nextAction;

        public AgentInteraction()
        {
            AmmoToLoad = new List<Ammo>();
        }

        public AgentInteractionState State { get; set; }
        public AgentInteractionPurpose Purpose { get; set; }

        public List<Ammo> AmmoToLoad { get; private set; }

        private void LoadSpecificAmmo(IEnumerable<DamageType> damageTypes)
        {
            AmmoToLoad.Clear();
            AmmoToLoad.AddRange(Settings.Instance.Ammo.Where(a => damageTypes.Contains(a.DamageType)).Select(a => a.Clone()));
        }

        private void WaitForConversation()
        {
            var agentWindow = Cache.Instance.Agent.Window;
            if (agentWindow == null || !agentWindow.IsReady)
                return;

            Logging.Log("AgentInteraction: Replying to agent");
            State = AgentInteractionState.ReplyToAgent;
            _nextAction = DateTime.Now.AddSeconds(7);
        }

        private void ReplyToAgent()
        {
            var agentWindow = Cache.Instance.Agent.Window;
            if (agentWindow == null || !agentWindow.IsReady)
                return;

            var responses = agentWindow.AgentResponses;
            if (responses == null || responses.Count == 0)
                return;

            var request = responses.FirstOrDefault(r => r.Text.Contains(RequestMission));
            var complete = responses.FirstOrDefault(r => r.Text.Contains(CompleteMission));
            var view = responses.FirstOrDefault(r => r.Text.Contains(ViewMission));
            var accept = responses.FirstOrDefault(r => r.Text.Contains(Accept));
            var decline = responses.FirstOrDefault(r => r.Text.Contains(Decline));

            if (complete != null)
            {
                if (Purpose == AgentInteractionPurpose.CompleteMission)
                {
                    // Complete the mission, close convo
                    Logging.Log("AgentInteraction: Saying [Complete Mission]");
                    complete.Say();

                    Logging.Log("AgentInteraction: Closing conversation");

                    State = AgentInteractionState.CloseConversation;
                    _nextAction = DateTime.Now.AddSeconds(7);
                }
                else
                {
                    Logging.Log("AgentInteraction: Waiting for mission");

                    // Apparently someone clicked "accept" already
                    State = AgentInteractionState.WaitForMission;
                    _nextAction = DateTime.Now.AddSeconds(7);
                }
            }
            else if (request != null)
            {
                if (Purpose == AgentInteractionPurpose.StartMission)
                {
                    // Request a mission and wait for it
                    Logging.Log("AgentInteraction: Saying [Request Mission]");
                    request.Say();

                    Logging.Log("AgentInteraction: Waiting for mission");
                    State = AgentInteractionState.WaitForMission;
                    _nextAction = DateTime.Now.AddSeconds(7);
                }
                else
                {
                    Logging.Log("AgentInteraction: Unexpected dialog options");
                    State = AgentInteractionState.UnexpectedDialogOptions;
                }
            }
            else if (view != null)
            {
                // View current mission
                Logging.Log("AgentInteraction: Saying [View Mission]");

                view.Say();
                _nextAction = DateTime.Now.AddSeconds(7);
                // No state change
            }
            else if (accept != null || decline != null)
            {
                if (Purpose == AgentInteractionPurpose.StartMission)
                {
                    Logging.Log("AgentInteraction: Waiting for mission");

                    State = AgentInteractionState.WaitForMission; // Dont say anything, wait for the mission
                    _nextAction = DateTime.Now.AddSeconds(7);
                }
                else
                {
                    Logging.Log("AgentInteraction: Unexpected dialog options");

                    State = AgentInteractionState.UnexpectedDialogOptions;
                }
            }
        }

        private DamageType GetMissionDamageType(string html)
        {
            // We are going to check damage types
            var logoRegex = new Regex("img src=\"factionlogo:(?<factionlogo>\\d+)");

            var logoMatch = logoRegex.Match(html);
            if (logoMatch.Success)
            {
                var logo = logoMatch.Groups["factionlogo"].Value;

                // Load faction xml
                var xml = XDocument.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Factions.xml"));
                var faction = xml.Root.Elements("faction").Where(f => (string) f.Attribute("logo") == logo).FirstOrDefault();
                if (faction != null)
                    return (DamageType) Enum.Parse(typeof (DamageType), (string) faction.Attribute("damagetype"));
            }

            return DamageType.EM;
        }

        private void WaitForMission()
        {
            var agentWindow = Cache.Instance.Agent.Window;
            if (agentWindow == null || !agentWindow.IsReady)
                return;

            var journalWindow = Cache.Instance.GetWindowByName("journal");
            if (journalWindow == null)
            {
                if (DateTime.Now.Subtract(_lastMissionOpenRequest).TotalSeconds > 10)
                {
                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenJournal);
                    _lastMissionOpenRequest = DateTime.Now;
                }
                return;
            }

            var mission = Cache.Instance.Mission;
            if (mission == null)
                return;

            // Is the mission offered?
            if (mission.State == (int) MissionState.Offered && (mission.Type == "Courier" || mission.Type == "Mining" || mission.Type == "Trade" || Settings.Instance.Blacklist.Any(m => m.ToLower() == Cache.Instance.MissionName.ToLower())))
            {
                Logging.Log("AgentInteraction: Declining courier/mining/trade/blacklisted mission [" + Cache.Instance.MissionName + "]");

                State = AgentInteractionState.DeclineMission;
                _nextAction = DateTime.Now.AddSeconds(7);
                return;
            }

            var html = agentWindow.Objective;
            if (html.Contains("The route generated by current autopilot settings contains low security systems!"))
            {
                Logging.Log("AgentInteraction: Declining low-sec mission");

                State = AgentInteractionState.DeclineMission;
                _nextAction = DateTime.Now.AddSeconds(7);
                return;
            }

            var loadedAmmo = false;

            var missionXmlPath = Path.Combine(Settings.Instance.MissionsPath, Cache.Instance.MissionName + ".xml");
            if (File.Exists(missionXmlPath))
            {
                Logging.Log("AgentInteraction: Loading mission xml [" + Cache.Instance.MissionName + "]");
                try
                {
                    var missionXml = XDocument.Load(missionXmlPath);
                    var damageTypes = missionXml.XPathSelectElements("//damagetype").Select(e => (DamageType) Enum.Parse(typeof (DamageType), (string) e, true));
                    if (damageTypes.Any())
                    {
                        LoadSpecificAmmo(damageTypes.Distinct());
                        loadedAmmo = true;
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("AgentInteraction: Error parsing damage types for mission [" + mission.Name + "], " + ex.Message);
                }
            }

            if (!loadedAmmo)
            {
                Logging.Log("AgentInteraction: No damage type for [" + Cache.Instance.MissionName + "] specified");

                Cache.Instance.DamageType = GetMissionDamageType(html);
                LoadSpecificAmmo(new[] {Cache.Instance.DamageType});
            }

            if (mission.State == (int) MissionState.Offered)
            {
                Logging.Log("AgentInteraction: Accepting mission [" + Cache.Instance.MissionName + "]");

                State = AgentInteractionState.AcceptMission;
                _nextAction = DateTime.Now.AddSeconds(7);
            }
            else // If we already accepted the mission, close the convo
            {
                Logging.Log("AgentInteraction: Mission [" + Cache.Instance.MissionName + "] already accepted");
                Logging.Log("AgentInteraction: Closing conversation");

                State = AgentInteractionState.CloseConversation;
                _nextAction = DateTime.Now.AddSeconds(7);
            }
        }

        private void AcceptMission()
        {
            var agentWindow = Cache.Instance.Agent.Window;
            if (agentWindow == null || !agentWindow.IsReady)
                return;

            var responses = agentWindow.AgentResponses;
            if (responses == null || responses.Count == 0)
                return;

            var accept = responses.FirstOrDefault(r => r.Text.Contains(Accept));
            if (accept == null)
                return;

            Logging.Log("AgentInteraction: Saying [Accept]");
            accept.Say();

            Logging.Log("AgentInteraction: Closing conversation");
            State = AgentInteractionState.CloseConversation;
            _nextAction = DateTime.Now.AddSeconds(7);
        }

        private void DeclineMission()
        {
            var agentWindow = Cache.Instance.Agent.Window;
            if (agentWindow == null || !agentWindow.IsReady)
                return;

            var responses = agentWindow.AgentResponses;
            if (responses == null || responses.Count == 0)
                return;

            var decline = responses.FirstOrDefault(r => r.Text.Contains(Decline));
            if (decline == null)
                return;

            // Decline and request a new mission
            Logging.Log("AgentInteraction: Saying [Decline]");
            decline.Say();

            Logging.Log("AgentInteraction: Replying to agent");
            State = AgentInteractionState.ReplyToAgent;
            _nextAction = DateTime.Now.AddSeconds(7);
        }

        public void CloseConversation()
        {
            var agentWindow = Cache.Instance.Agent.Window;
            if (agentWindow == null)
            {
                Logging.Log("AgentInteraction: Done");

                State = AgentInteractionState.Done;
                return;
            }

            agentWindow.Close();
        }

        public void ProcessState()
        {
            // Wait a bit before doing "things"
            if (DateTime.Now < _nextAction)
                return;

            switch (State)
            {
                case AgentInteractionState.Idle:
                case AgentInteractionState.Done:
                    break;

                case AgentInteractionState.StartConversation:
                    Cache.Instance.Agent.InteractWith();

                    Logging.Log("AgentInteraction: Waiting for conversation");
                    State = AgentInteractionState.WaitForConversation;
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