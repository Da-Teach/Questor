
namespace Questor.Modules.BackgroundTasks
{
    using System;
    //using System.Linq;
    using Questor.Modules.Caching;
    using Questor.Modules.Lookup;
    using Questor.Modules.States;

    public class LocalWatch
    {
        private DateTime _lastAction;

        public void ProcessState()
        {
            switch (_States.CurrentLocalWatchState)
            {
                case LocalWatchState.Idle:
                    //checking local every 5 second
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < (int)Time.CheckLocalDelay_seconds)
                        break;

                    _States.CurrentLocalWatchState = LocalWatchState.CheckLocal;
                    break;

                case LocalWatchState.CheckLocal:
                    //
                    // this ought to cache the name of the system, and the number of ppl in local (or similar)
                    // and only query everyone in local for standings changes if something has changed...
                    //
                    Cache.Instance.LocalSafe(Settings.Instance.LocalBadStandingPilotsToTolerate, Settings.Instance.LocalBadStandingLevelToConsiderBad);

                    _lastAction = DateTime.Now;
                    _States.CurrentLocalWatchState = LocalWatchState.Idle;
                    break;

                default:
                    // Next state
                    _States.CurrentLocalWatchState = LocalWatchState.Idle;
                    break;
            }
        }
    }
}