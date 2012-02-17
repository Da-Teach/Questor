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
        DelayedStart,
        Cleanup,
        Start,
        Switch,
        Arm,
        LocalWatch,
        WaitingBadGuyGoAway,
        WarpOutStation,
        GotoMission,
        ExecuteMission,
        DelayedGotoBase,
        GotoBase,
        CloseQuestorCmdQuitGame,
        CloseQuestorCmdLogOff,
        CompleteMission,
        UnloadLoot,
        BeginAfterMissionSalvaging,
        GotoSalvageBookmark,
        SalvageUseGate,
        SalvageNextPocket,
        Salvage,
        ScoopStep1SetupBookmarkLocation,
        ScoopStep2GotoScoopBookmark,
        ScoopStep3WaitForWrecks,
        GotoNearestStation,
        Error,
        Paused,
        Debug_WindowNames,
        Debug_WindowCaptions,
        Debug_WindowTypes,
        Panic,
        SalvageOnly,
        SalvageOnlyBookmarks,
        GotoSalvageOnlyBookmark,
        StorylinePanic,
        CombatHelper,
        CombatHelper_anomaly,
		Traveler,
        Scanning,
        Storyline
    }
}