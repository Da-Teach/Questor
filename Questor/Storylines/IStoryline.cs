namespace Questor.Storylines
{
    interface IStoryline
    {
        StorylineState Arm();
        StorylineState PreAcceptMission();
        StorylineState PostAcceptMission();
        StorylineState PostCompleteMission();
    }
}
