namespace Questor.Storylines
{
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules;

    public class Storyline
    {
        public StorylineState State { get; set; }

        private long _agentId;
        private IStoryline _storyline;
        private Dictionary<string, IStoryline> _storylines;
        private List<long> _agentBlacklist;

        private Combat _combat;
        private Traveler _traveler;
        private AgentInteraction _agentInteraction;

        public Storyline()
        {
            _combat = new Combat();
            _traveler = new Traveler();
            _agentInteraction = new AgentInteraction();

            _agentBlacklist = new List<long>();

            _storylines = new Dictionary<string, IStoryline>();
            _storylines.Add("Materials For War Preparation", new MaterialsForWarPreparation());
        }

        public void Reset()
        {
            State = StorylineState.Idle;
            _agentId = 0;
            _storyline = null;
            _agentInteraction.State = AgentInteractionState.Idle;
        }

        private DirectAgentMission Mission
        {
            get
            {
                IEnumerable<DirectAgentMission> missions = Cache.Instance.DirectEve.AgentMissions;
                if (_agentId != 0)
                    return missions.FirstOrDefault(m => m.AgentId == _agentId);

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
                return;
            }

            _agentId = mission.AgentId;
            var agent = Cache.Instance.DirectEve.GetAgentById(_agentId);
            if (agent == null)
            {
                Logging.Log("Storyline: Unknown agent [" + _agentId + "]");

                State = StorylineState.Done;
                return;
            }

            Logging.Log("Storyline: Going to do [" + mission.Name + "] for agent [" + agent.Name + "]");

            State = StorylineState.Arm;
            _storyline = _storylines[Cache.Instance.FilterPath(mission.Name)];
        }

        private void GotoAgentState()
        {
            var agent = Cache.Instance.DirectEve.GetAgentById(_agentId);
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
                State = StorylineState.PreAcceptMission;

            if (Settings.Instance.DebugStates)
                Logging.Log("Traveler.State = " + _traveler.State);
        }

        public void ProcessState()
        {
            switch (State)
            {
                case StorylineState.Idle:
                    IdleState();
                    break;

                case StorylineState.Arm:
                    State = _storyline.Arm();
                    break;

                case StorylineState.GotoAgent:
                    GotoAgentState();
                    break;

                case StorylineState.PreAcceptMission:
                    State = _storyline.PreAcceptMission();
                    break;

                case StorylineState.AcceptMission:
                    if (_agentInteraction.State == AgentInteractionState.Idle)
                    {
                        Logging.Log("AgentInteraction: Start conversation [Start Mission]");

                        _agentInteraction.State = AgentInteractionState.StartConversation;
                        _agentInteraction.Purpose = AgentInteractionPurpose.StartMission;
                        _agentInteraction.AgentId = _agentId;
                        _agentInteraction.ForceAccept = true;
                    }

                    _agentInteraction.ProcessState();

                    if (Settings.Instance.DebugStates)
                        Logging.Log("AgentInteraction.State = " + _agentInteraction.State);

                    if (_agentInteraction.State == AgentInteractionState.Done)
                    {
                        _agentInteraction.State = AgentInteractionState.Idle;
                        State = StorylineState.PostAcceptMission;
                    }
                    break;
                
                case StorylineState.PostAcceptMission:
                    State = _storyline.PostAcceptMission();
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
                        State = StorylineState.PostCompleteMission;
                    }
                    break;

                case StorylineState.PostCompleteMission:
                    State = _storyline.PostCompleteMission();
                    break;

                case StorylineState.BlacklistAgent:
                    _agentBlacklist.Add(_agentId);
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
    }
}
