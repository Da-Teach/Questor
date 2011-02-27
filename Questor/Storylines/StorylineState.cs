namespace Questor.Storylines
{
    public enum StorylineState
    {
        Idle,
        Arm,
        GotoAgent,
        PreAcceptMission,
        AcceptMission,
        PostAcceptMission,
        CompleteMission,
        Done,
        BlacklistAgent,
        PostCompleteMission
    }
}
