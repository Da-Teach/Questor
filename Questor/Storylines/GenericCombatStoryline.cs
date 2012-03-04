namespace Questor.Storylines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::Questor.Modules;

    public class GenericCombatStoryline : IStoryline
    {
        private long _agentId;
        private List<Ammo> _neededAmmo;

        private AgentInteraction _agentInteraction;
        private Arm _arm;
        private Traveler _traveler;
        private MissionController _missionController;
        private Combat _combat;
        private Drones _drones;
        private Salvage _salvage;

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
            _missionController = new MissionController();

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
            if (_agentId != storyline.AgentId)
            {
                _neededAmmo.Clear();
                _agentId = storyline.AgentId;

                _agentInteraction.AgentId = _agentId;
                _agentInteraction.ForceAccept = true; // This makes agent interaction skip the offer-check
                _agentInteraction.State = AgentInteractionState.Idle;
                _agentInteraction.Purpose = AgentInteractionPurpose.AmmoCheck;

                _arm.AgentId = _agentId;
                _arm.State = ArmState.Idle;
                _arm.AmmoToLoad.Clear();

                _missionController.AgentId = _agentId;
                _missionController.State = MissionControllerState.Start;

                _combat.State = CombatState.CheckTargets;

                _drones.State = DroneState.WaitingForTargets;
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
            catch(Exception ex)
            {
                // Something went wrong!
                Logging.Log("GenericCombatStoryline: Something went wrong, blacklist this agent [" + ex.Message + "]");
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
            if (_agentInteraction.State == AgentInteractionState.Done)
                return true;

            if (_agentInteraction.Agent == null)
                throw new Exception("Invalid agent");

            // Start the conversation
            if (_agentInteraction.State == AgentInteractionState.Idle)
                _agentInteraction.State = AgentInteractionState.StartConversation;

            // Interact with the agent to find out what ammo we need
            _agentInteraction.ProcessState();

            if (_agentInteraction.State == AgentInteractionState.DeclineMission)
            {
                if (_agentInteraction.Agent.Window != null)
                    _agentInteraction.Agent.Window.Close();
                Logging.Log("GenericCombatStoryline: Mission offer is in a Low Security System"); //do storyline missions in lowsec get blacklisted by: "public StorylineState Arm(Storyline storyline)"?
                throw new Exception("Low security systems");

            }

            if (_agentInteraction.State == AgentInteractionState.Done)
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
            if (_arm.State == ArmState.Done)
                return true;

            if (_arm.State == ArmState.Idle)
                _arm.State = ArmState.Begin;

            _arm.ProcessState();

            if (_arm.State == ArmState.Done)
            {
                _arm.State = ArmState.Idle;
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
            _state = GenericCombatStorylineState.GotoMission;
            return StorylineState.AcceptMission;
        }

        /// <summary>
        ///   Do a mini-questor here (goto mission, execute mission, goto base)
        /// </summary>
        /// <returns></returns>
        public StorylineState ExecuteMission(Storyline storyline)
        {
            switch(_state)
            {
                case GenericCombatStorylineState.GotoMission:
                    var missionDestination = _traveler.Destination as MissionBookmarkDestination;
                    if (missionDestination == null || missionDestination.AgentId != storyline.AgentId) // We assume that this will always work "correctly" (tm)
                        _traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(storyline.AgentId, "Encounter"));

                    if (Cache.Instance.PriorityTargets.Any(pt => pt != null && pt.IsValid))
                    {
                        Logging.Log("GenericCombatStoryline: Priority targets found while traveling, engaging!");
                        _combat.ProcessState();
                    }

                    _traveler.ProcessState();
                    if (_traveler.State == TravelerState.AtDestination)
                    {
                        _state = GenericCombatStorylineState.ExecuteMission;
                        _traveler.Destination = null;
                    }
                    break;

                case GenericCombatStorylineState.ExecuteMission:
                    _combat.ProcessState();
                    _drones.ProcessState();
                    _salvage.ProcessState();
                    _missionController.ProcessState();

                    // If we are out of ammo, return to base, the mission will fail to complete and the bot will reload the ship
                    // and try the mission again
                    if (_combat.State == CombatState.OutOfAmmo)
                    {
                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();

                        Logging.Log("GenericCombatStoryline: Out of Ammo!");
                        return StorylineState.ReturnToAgent;
                    }

                    if (_missionController.State == MissionControllerState.Done)
                    {
                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();
                        return StorylineState.ReturnToAgent;
                    }

                    // If in error state, just go home and stop the bot
                    if (_missionController.State == MissionControllerState.Error)
                    {
                        // Clear looted containers
                        Cache.Instance.LootedContainers.Clear();

                        Logging.Log("MissionController: Error");
                        return StorylineState.ReturnToAgent;
                    }
                    break;
            }

            return StorylineState.ExecuteMission;
        }
    }
}
