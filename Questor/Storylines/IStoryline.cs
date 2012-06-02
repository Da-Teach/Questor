using Questor.Modules.States;

namespace Questor.Storylines
{
    public interface IStoryline
    {
        StorylineState Arm(Storyline storyline);

        StorylineState PreAcceptMission(Storyline storyline);

        StorylineState ExecuteMission(Storyline storyline);
    }
}