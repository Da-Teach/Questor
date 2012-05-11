namespace Questor.Modules
{
    using System;
    using System.Linq;

    public class LocalWatch
    {
        private LocalWatchState State { get; set; }
        private DateTime _lastAction;

        public void ProcessState()
        {

            switch(State)
            {
                case LocalWatchState.Start:
                    //checking local every 5 second
                    if(DateTime.Now.Subtract(_lastAction).TotalSeconds < (int)Time.LocalWatch_CheckLocalDelay_seconds)
                        break;

                    State = LocalWatchState.CheckLocal;
                    break;

                case LocalWatchState.CheckLocal:
                    //
                    // this ought to cache the name of the system, and the number of ppl in local (or similar)
                    // and only query everyone in local for standings changes if something has changed...
                    //
                    Cache.Instance.Local_safe(Settings.Instance.LocalBadStandingPilotsToTolerate,Settings.Instance.LocalBadStandingLevelToConsiderBad);
                    State = LocalWatchState.Done;
                    break;

                case LocalWatchState.Done:
                    _lastAction = DateTime.Now;
                    State = LocalWatchState.Start;
                    break;

                default:
                    // Next state
                    State = LocalWatchState.Start;
                    break;
            }
        }
    }
}
