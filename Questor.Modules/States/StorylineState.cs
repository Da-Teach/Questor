namespace Questor.Modules.States
{
    public enum StorylineState
    {
        Idle,
        Arm,
        GotoAgent,
        PreAcceptMission,
        AcceptMission,
        ExecuteMission,
        CompleteMission,
        Done,
        BlacklistAgent,
        BringSpoilsOfWar,
        ReturnToAgent
    }
}