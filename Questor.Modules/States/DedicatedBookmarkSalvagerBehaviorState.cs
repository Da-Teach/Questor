
namespace Questor.Modules.States
{
    public enum DedicatedBookmarkSalvagerBehaviorState
    {
        Default,
        Idle,
        MissionStatistics,
        DelayedStart,
        Cleanup,
        Start,
        Arm,
        LocalWatch,
        WaitingforBadGuytoGoAway,
        WarpOutStation,
        DelayedGotoBase,
        GotoBase,
        UnloadLoot,
        BeginAfterMissionSalvaging,
        GotoSalvageBookmark,
        SalvageUseGate,
        SalvageNextPocket,
        Salvage,
        GotoNearestStation,
        Error,
        Paused,
        Panic,
        Traveler,
    }
}