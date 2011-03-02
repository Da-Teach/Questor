namespace Questor.Storylines
{
    interface IStoryline
    {
        StorylineState Arm(Storyline storyline);
        StorylineState PreAcceptMission(Storyline storyline);
        StorylineState ExecuteMission(Storyline storyline);
    }
}
