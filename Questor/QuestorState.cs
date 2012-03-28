// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace Questor
{
    public enum QuestorState
    {
        Idle,
        MissionStatistics,
        DelayedStart,
        Cleanup,
        Start,
        Switch,
        Arm,
        LocalWatch,
        WaitingforBadGuytoGoAway,
        WarpOutStation,
        GotoMission,
        ExecuteMission,
        CourierMission,
        DelayedGotoBase,
        GotoBase,
        CheckEVEStatus,
        CompleteMission,
        UnloadLoot,
        BeginAfterMissionSalvaging,
        GotoSalvageBookmark,
        SalvageUseGate,
        SalvageNextPocket,
        Salvage,
        CloseQuestor,
        GotoNearestStation,
        Error,
        Paused,
        Panic,
		Traveler,
        Scanning,
        Storyline,
        StorylinePanic,
        CombatHelper,
        CombatHelper_anomaly,
        SalvageOnly,
        SalvageOnlyBookmarks,
        GotoSalvageOnlyBookmark,
        Debug_Windows,
        Debug_CloseQuestor,
    }
}