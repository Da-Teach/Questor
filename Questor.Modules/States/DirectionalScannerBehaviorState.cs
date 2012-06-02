
namespace Questor.Modules.States
{
    public enum DirectionalScannerBehaviorState
    {
        Default,
        Idle,
        LocalWatch,
        DelayedGotoBase,
        GotoBase,
        GotoNearestStation,
        Error,
        Paused,
        Panic,
        Traveler,
        PVPDirectionalScanOnGrid,
        PVPDirectionalScanHalfanAU, 
        PVPDirectionalScan1AU,
        PVPDirectionalScan5AU,
        PVPDirectionalScan10AU,
        PVPDirectionalScan15AU,
        PVPDirectionalScan20AU,
        PVPDirectionalScan50AU,
        PVEDirectionalScanOnGrid,
        PVEDirectionalScanHalfanAU,
        PVEDirectionalScan1AU,
        PVEDirectionalScan5AU,
        PVEDirectionalScan10AU,
        PVEDirectionalScan15AU,
        PVEDirectionalScan20AU,
        PVEDirectionalScan50AU,
        LogCombatTargets, 
        LogDroneTargets,
        LogStationEntities,
        LogStargateEntities,
        LogAsteroidBelts,
    }
}
