

namespace Questor.Storylines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using global::Questor.Behaviors;
    using global::Questor.Modules.Actions;
    using global::Questor.Modules.Activities;
    using global::Questor.Modules.BackgroundTasks;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Combat;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;

    public class GenericCombatStoryline : IStoryline
    {

        private long _agentId;
        private readonly List<Ammo> _neededAmmo;

        private readonly AgentInteraction _agentInteraction;
        private readonly Arm _arm;
        private readonly Traveler _traveler;
        private readonly CombatMissionCtrl _combatMissionCtrl;
        private readonly Combat _combat;
        private readonly Drones _drones;
        private readonly Salvage _salvage;
        private readonly Statistics _statistics;

        private GenericCombatStorylineState _state;

        public GenericCombatStorylineState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public GenericCombatStoryline()
        {
            _neededAmmo = new List<Ammo>();

            _agentInteraction = new AgentInteraction();
            _arm = new Arm();
            _traveler = new Traveler();
            _combat = new Combat();
            _drones = new Drones();
            _salvage = new Salvage();
            _statistics = new Statistics();
            _combatMissionCtrl = new CombatMissionCtrl();

            Settings.Instance.SettingsLoaded += ApplySettings;
        }

        /// <summary>
        ///   Apply settings to the salvager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplySettings(object sender, EventArgs e)
        {
            _salvage.Ammo = Settings.Instance.Ammo;
            _salvage.MaximumWreckTargets = Settings.Instance.MaximumWreckTargets;
            _salvage.ReserveCargoCapacity = Settings.Instance.ReserveCargoCapacity;
            _salvage.LootEverything = Settings.Instance.LootEverything;
        }

        /// <summary>
        ///   We check what ammo we need by convo'ing the agent and load the appropriate ammo
        /// </summary>
        /// <returns></returns>
        public StorylineState Arm(Storyline storyline)
        {
            if (_agentId != Cache.Instance.CurrentStorylineAgentId)
            {
                _neededAmmo.Clear();
                _agentId = Cache.Instance.CurrentStorylineAgentId;

                _agentInteraction.AgentId = _agentId;
                _agentInteraction.ForceAccept = true; // This makes agent interaction skip the offer-check
                _States.CurrentAgentInteractionState = AgentInteractionState.Idle;
                _agentInteraction.Purpose = AgentInteractionPurpose.AmmoCheck;

                _arm.AgentId = _agentId;
                _States.CurrentArmState = ArmState.Idle;
                _arm.AmmoToLoad.Clear();

                //Questor.AgentID = _agentId;

                _statistics.AgentID = _agentId;

                _combatMissionCtrl.AgentId = _agentId;
                _States.CurrentCombatMissionCtrlState = CombatMissionCtrlState.Start;

                _States.CurrentCombatState = CombatState.CheckTargets;

                _States.CurrentDroneState = DroneState.WaitingForTargets;
            }

            try
            {
                if (!Interact())
                    return StorylineState.Arm;

                if (!LoadAmmo())
                    return StorylineState.Arm;

                // We are done, reset agent id
                _agentId = 0;

                return StorylineState.GotoAgent;
            }
            catch (Exception ex)
            {
                // Something went wrong!
                Logging.Log("GenericCombatStoryline", "Something went wrong, blacklist this agent [" + ex.Message + "]", Logging.orange);
                return StorylineState.BlacklistAgent;
            }
        }

        /// <summary>
        ///   Interact with the agent so we know what ammo to bring
        /// </summary>
        /// <returns>True if interact is done</returns>
        private bool Interact()
        {
            // Are we done?
            if (_States.CurrentAgentInteractionState == AgentInteractionState.Done)
                return true;

            if (_agentInteraction.Agent == null)
                throw new Exception("Invalid agent");

            // Start the conversation
            if (_States.CurrentAgentInteractionState == AgentInteractionState.Idle)
                _States.CurrentAgentInteractionState = AgentInteractionState.StartConversation;

            // Interact with the agent to find out what ammo we need
            _agentInteraction.ProcessState();

            if (_States.CurrentAgentInteractionState == AgentInteractionState.DeclineMission)
            {
                if (_agentInteraction.Agent.Window != null)
                    _agentInteraction.Agent.Window.Close();
                Logging.Log("GenericCombatStoryline", "Mission offer is in a Low Security System", Logging.orange); //do storyline missions in lowsec get blacklisted by: "public StorylineState Arm(Storyline storyline)"?
                throw new Exception("Low security systems");
            }

            if (_States.CurrentAgentInteractionState == AgentInteractionState.Done)
            {
                _arm.AmmoToLoad.Clear();
                _arm.AmmoToLoad.AddRange(_agentInteraction.AmmoToLoad);
                return true;
            }

            return false;
        }

        /// <summary>
        ///   Load the appropriate ammo
        /// </summary>
        /// <returns></returns>
        private bool LoadAmmo()
        {
            if (_States.CurrentArmState == ArmState.Done)
                return true;

            if (_States.CurrentArmState == ArmState.Idle)
                _States.CurrentArmState = ArmState.Begin;

            _arm.ProcessState();

            if (_States.CurrentArmState == ArmState.Done)
            {
                _States.CurrentArmState = ArmState.Idle;
                return true;
            }

            return false;
        }

        /// <summary>
        ///   We have no pre-accept steps
        /// </summary>
        /// <returns></returns>
        public StorylineState PreAcceptMission(Storyline storyline)
        {
            // Not really a step is it? :)
            _state = GenericCombatStorylineState.WarpOutStation;
            return StorylineState.AcceptMission;
        }

        /// <summary>
        ///   Do a mini-questor here (goto mission, execute mission, goto base)
        /// </summary>
        /// <returns></returns>
        public StorylineState ExecuteMission(Storyline storyline)
        {
            switch (_state)
            {
                case GenericCombatStorylineState.WarpOutStation:
                    DirectBookmark warpOutBookMark = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkWarpOut ?? "").OrderByDescending(b => b.CreatedOn).FirstOrDefault(b => b.LocationId == Cache.Instance.DirectEve.Session.SolarSystemId);
                    long solarid = Cache.Instance.DirectEve.Session.SolarSystemId ?? -1;

                    if (warpOutBookMark == null)
                    {
                        Logging.Log("GenericCombatStoryline.WarpOut", "No Bookmark", Logging.orange);
                        if (_state == GenericCombatStorylineState.WarpOutStation)
                        {
                            _state = GenericCombatStorylineState.GotoMission;
                        }
                    }
                    else if (warpOutBookMark.LocationId == solarid)
                    {
                        if (_traveler.Destination == null)
                        {
                            Logging.Log("GenericCombatStoryline.WarpOut", "Warp at " + warpOutBookMark.Title, Logging.white);
                            _traveler.Destination = new BookmarkDestination(warpOutBookMark);
                            Cache.Instance.DoNotBreakInvul = true;
                        }

                        _traveler.ProcessState();
                        if (_States.CurrentTravelerState == TravelerState.AtDestination)
                        {
                            Logging.Log("GenericCombatStoryline.WarpOut", "Safe!", Logging.white);
                            Cache.Instance.DoNotBreakInvul = false;
                            if (_state == GenericCombatStorylineState.WarpOutStation)
                            {
                                _state = GenericCombatStorylineState.GotoMission;
                            }
                            _traveler.Destination = null;
                        }
                    }
                    else
                    {
                        Logging.Log("GenericCombatStoryline.WarpOut", "o Bookmark in System", Logging.white);
                        if (_state == GenericCombatStorylineState.WarpOutStation)
                        {
                            _state = GenericCombatStorylineState.GotoMission;
                        }
                    }
                    break;

                case GenericCombatStorylineState.GotoMission:
                    var missionDestination = _traveler.Destination as MissionBookmarkDestination;
                    //
                    // if we have no destination yet... OR if missionDestination.AgentId != storyline.CurrentStorylineAgentId
                    //
                    //if (missionDestination != null) Logging.Log("GenericCombatStoryline: missionDestination.AgentId [" + missionDestination.AgentId + "] " + "and storyline.CurrentStorylineAgentId [" + storyline.CurrentStorylineAgentId + "]");
                    //if (missionDestination == null) Logging.Log("GenericCombatStoryline: missionDestination.AgentId [ NULL ] " + "and storyline.CurrentStorylineAgentId [" + storyline.CurrentStorylineAgentId + "]");
                    if (missionDestination == null || missionDestination.AgentId != Cache.Instance.CurrentStorylineAgentId) // We assume that this will always work "correctly" (tm)
                    {
                        const string nameOfBookmark = "Encounter";
                        Logging.Log("GenericCombatStoryline", "Setting Destination to 1st bookmark from AgentID: [" + Cache.Instance.CurrentStorylineAgentId + "] with [" + nameOfBookmark + "] in the title", Logging.white);
                        _traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(Cache.Instance.CurrentStorylineAgentId, nameOfBookmark));
                    }

                    if (Cache.Instance.PriorityTargets.Any(pt => pt != null && pt.IsValid))
                    {
                        Logging.Log("GenericCombatStoryline", "Priority targets found while traveling, engaging!", Logging.white);
                        _combat.ProcessState();
                    }

                    _traveler.ProcessState();
                    if (_States.CurrentTravelerState == TravelerState.AtDestination)
                    {
                        _state = GenericCombatStorylineState.ExecuteMission;
                        _States.CurrentCombatState = CombatState.CheckTargets;
                        _traveler.Destination = null;
                    }
                    break;

                case GenericCombatStorylineState.ExecuteMission:
                    _combat.ProcessState();
                    _drones.ProcessState();
                    _salvage.ProcessState();
                    _combatMissionCtrl.ProcessState();

                    // If we are out of ammo, return to base, the mission will fail to complete and the bot will reload the ship
                    // and try the mission again
                    if (_States.CurrentCombatState == CombatState.OutOfAmmo)
                    {
                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();

                        Logging.Log("GenericCombatStoryline", "Out of Ammo!", Logging.orange);
                        return StorylineState.ReturnToAgent;
                    }

                    if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Done)
                    {
                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();
                        return StorylineState.ReturnToAgent;
                    }

                    // If in error state, just go home and stop the bot
                    if (_States.CurrentCombatMissionCtrlState == CombatMissionCtrlState.Error)
                    {
                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();

                        Logging.Log("MissionController", "Error", Logging.red);
                        return StorylineState.ReturnToAgent;
                    }
                    break;
            }

            return StorylineState.ExecuteMission;
        }
    }
}