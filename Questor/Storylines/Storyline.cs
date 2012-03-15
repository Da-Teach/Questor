namespace Questor.Storylines
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules;

    public class Storyline
    {
        public StorylineState State { get; set; }

        public long AgentId { get; private set; }
        
        private IStoryline _storyline;
        private Dictionary<string, IStoryline> _storylines;
        private List<long> _agentBlacklist;

        private Combat _combat;
        private Traveler _traveler;
        private AgentInteraction _agentInteraction;

        private DateTime _nextAction;

        public Storyline()
        {
            _combat = new Combat();
            _traveler = new Traveler();
            _agentInteraction = new AgentInteraction();

            _agentBlacklist = new List<long>();

            _storylines = new Dictionary<string, IStoryline>();
            //_storylines.Add("__", new GenericCombatStoryline());
            // broken in crucible 1.5 (yes/no dialog needs directeve support)
            //_storylines.Add("Materials For War Preparation", new MaterialsForWarPreparation());
            _storylines.Add("Shipyard Theft", new GenericCombatStoryline());
            _storylines.Add("Evolution", new GenericCombatStoryline());
            _storylines.Add("Record Cleaning", new GenericCombatStoryline());
            _storylines.Add("Covering Your Tracks", new GenericCombatStoryline());
            _storylines.Add("Crowd Control", new GenericCombatStoryline());
            _storylines.Add("A Force to Be Reckoned With", new GenericCombatStoryline());
            _storylines.Add("Kidnappers Strike - Ambush In The Dark (1 of 10)", new GenericCombatStoryline());
            _storylines.Add("Kidnappers Strike - The Kidnapping (3 of 10)", new GenericCombatStoryline());
            _storylines.Add("Kidnappers Strike - Incriminating Evidence (5 of 10)", new GenericCombatStoryline());
            _storylines.Add("Kidnappers Strike - The Secret Meeting (7 of 10)", new GenericCombatStoryline());
            _storylines.Add("Kidnappers Strike - Defend the Civilian Convoy (8 of 10)", new GenericCombatStoryline());
            _storylines.Add("Kidnappers Strike - Retrieve the Prisoners (9 of 10)", new GenericCombatStoryline());
            _storylines.Add("Kidnappers Strike - The Final Battle (10 of 10)", new GenericCombatStoryline());
            _storylines.Add("Whispers in the Dark - First Contact (1 of 4)", new GenericCombatStoryline());
            _storylines.Add("Whispers in the Dark - Lay and Pray (2 of 4)", new GenericCombatStoryline());
            _storylines.Add("Whispers in the Dark - The Outpost (4 of 4)", new GenericCombatStoryline());
            _storylines.Add("Transaction Data Delivery", new TransactionDataDelivery());
            _storylines.Add("Innocents in the Crossfire", new GenericCombatStoryline());
			_storylines.Add("Patient Zero", new GenericCombatStoryline());
            _storylines.Add("Soothe the Salvage Beast", new GenericCombatStoryline());
            _storylines.Add("Forgotten Outpost", new GenericCombatStoryline());
            _storylines.Add("Stem the Flow", new GenericCombatStoryline());
			_storylines.Add("Quota Season", new GenericCombatStoryline());
			//_storylines.Add("Matriarch", new GenericCombatStoryline());
            //_storylines.Add("Diplomatic Incident", new GenericCombatStoryline());
			_storylines.Add("Nine Tenths of the Wormhole", new GenericCombatStoryline());
		}
            //these work but are against other factions that I generally like to avoid
			//_storylines.Add("The Blood of Angry Men", new GenericCombatStoryline());  //amarr faction
			//_storylines.Add("Amarrian Excavators", new GenericCombatStoryline()); 	//amarr faction
			
        
        public void Reset()
        {
            State = StorylineState.Idle;
            AgentId = 0;
            _storyline = null;
            _agentInteraction.State = AgentInteractionState.Idle;
            _traveler.State = TravelerState.Idle;
            _traveler.Destination = null;
        }

        private DirectAgentMission Mission
        {
            get
            {
                IEnumerable<DirectAgentMission> missions = Cache.Instance.DirectEve.AgentMissions;
                if (AgentId != 0)
                    return missions.FirstOrDefault(m => m.AgentId == AgentId);

                missions = missions.Where(m => !_agentBlacklist.Contains(m.AgentId));
                missions = missions.Where(m => m.Important);
                missions = missions.Where(m => _storylines.ContainsKey(Cache.Instance.FilterPath(m.Name)));
                missions = missions.Where(m => !Settings.Instance.Blacklist.Any(b => b.ToLower() == Cache.Instance.FilterPath(m.Name).ToLower()));
                return missions.FirstOrDefault();
            }
        }

        private void IdleState()
        {
            var mission = Mission;
            if (mission == null)
            {
                State = StorylineState.Done;
                Cache.Instance.MissionName = "";
                return;
            }

            AgentId = mission.AgentId;
            var agent = Cache.Instance.DirectEve.GetAgentById(AgentId);
            if (agent == null)
            {
                Logging.Log("Storyline: Unknown agent [" + AgentId + "]");

                State = StorylineState.Done;
                return;
            }

            Logging.Log("Storyline: Going to do [" + mission.Name + "] for agent [" + agent.Name + "]");
            Cache.Instance.MissionName = mission.Name;

            State = StorylineState.Arm;
            _storyline = _storylines[Cache.Instance.FilterPath(mission.Name)];
        }

        private void GotoAgent(StorylineState nextState)
        {
            var agent = Cache.Instance.DirectEve.GetAgentById(AgentId);
            if (agent == null)
            {
                State = StorylineState.Done;
                return;
            }

            var baseDestination = _traveler.Destination as StationDestination;
            if (baseDestination == null || baseDestination.StationId != agent.StationId)
                _traveler.Destination = new StationDestination(agent.SolarSystemId, agent.StationId, Cache.Instance.DirectEve.GetLocationName(agent.StationId));

            if (Cache.Instance.PriorityTargets.Any(pt => pt != null && pt.IsValid))
            {
                Logging.Log("GotoBase: Priority targets found, engaging!");
                _combat.ProcessState();
            }

            _traveler.ProcessState();
            if (_traveler.State == TravelerState.AtDestination)
            {
                State = nextState;
                _traveler.Destination = null;
            }

            if (Settings.Instance.DebugStates)
                Logging.Log("Traveler.State = " + _traveler.State);
        }

        private void BringSpoilsOfWar()
        {
            var directEve = Cache.Instance.DirectEve;
            if (_nextAction > DateTime.Now)
                return;

            // Open the item hangar (should still be open)
            var hangar = directEve.GetItemHangar();
            if (hangar.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("MaterialsForWarPreparation: Opening hangar floor");

                directEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                return;
            }

            // Wait for it to become ready
            if (!hangar.IsReady)
                return;

            // Do we have any implants?
            if (!hangar.Items.Any(i => i.GroupId >= 738 && i.GroupId <= 750))
            {
                State = StorylineState.Done;
                return;
            }

            // Yes, open the ships cargo
            var cargo = directEve.GetShipsCargo();
            if (cargo.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("MaterialsForWarPreparation: Opening cargo");

                directEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                return;
            }

            if (!cargo.IsReady)
                return;

            // If we aren't moving items
            if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
            {
                // Move all the implants to the cargo bay
                foreach (var item in hangar.Items.Where(i => i.GroupId >= 738 && i.GroupId <= 750))
                {
                    if (cargo.Capacity - cargo.UsedCapacity - (item.Volume * item.Quantity) < 0)
                    {
                        Logging.Log("Storyline: We are full, not moving anything else");
                        State = StorylineState.Done;
                        return;
                    }

                    Logging.Log("Storyline: Moving [" + item.TypeName + "][" + item.ItemId + "] to cargo");
                    cargo.Add(item, item.Quantity);
                }

                _nextAction = DateTime.Now.AddSeconds(10);
            }

            return;
        }

        public void ProcessState()
        {
            switch (State)
            {
                case StorylineState.Idle:
                    IdleState();
                    break;

                case StorylineState.Arm:
                    State = _storyline.Arm(this);
                    break;

                case StorylineState.GotoAgent:
                    GotoAgent(StorylineState.PreAcceptMission);
                    break;

                case StorylineState.PreAcceptMission:
                    State = _storyline.PreAcceptMission(this);
                    break;

                case StorylineState.AcceptMission:
                    if (_agentInteraction.State == AgentInteractionState.Idle)
                    {
                        Logging.Log("AgentInteraction: Start conversation [Start Mission]");

                        _agentInteraction.State = AgentInteractionState.StartConversation;
                        _agentInteraction.Purpose = AgentInteractionPurpose.StartMission;
                        _agentInteraction.AgentId = AgentId;
                        _agentInteraction.ForceAccept = true;
                    }

                    _agentInteraction.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("AgentInteraction.State = " + _agentInteraction.State);

                    if (_agentInteraction.State == AgentInteractionState.Done)
                    {
                        _agentInteraction.State = AgentInteractionState.Idle;
                        // If theres no mission anymore then we're done (we declined it)
                        State = Mission == null ? StorylineState.Done : StorylineState.ExecuteMission;
                    }
                    break;
                
                case StorylineState.ExecuteMission:
                    State = _storyline.ExecuteMission(this);
                    break;

                case StorylineState.ReturnToAgent:
                    GotoAgent(StorylineState.CompleteMission);
                    break;

                case StorylineState.CompleteMission:
                    if (_agentInteraction.State == AgentInteractionState.Idle)
                    {
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
                        State = StorylineState.BringSpoilsOfWar;
                    }
                    break;

                case StorylineState.BringSpoilsOfWar:
                    BringSpoilsOfWar();
                    break;

                case StorylineState.BlacklistAgent:
                    _agentBlacklist.Add(AgentId);
                    State = StorylineState.Done;
                    break;

                case StorylineState.Done:
                    break;
            }
        }

        public bool HasStoryline()
        {
            // Do we have a registered storyline?
            return Mission != null;
        }

        public IStoryline StorylineHandler
        {
            get { return _storyline; }
        }
    }
}
