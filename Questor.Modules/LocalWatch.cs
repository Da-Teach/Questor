//NOT FINISH DON'T USE
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
                    if(DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    State = LocalWatchState.CheckLocal;
                    break;

                case LocalWatchState.CheckLocal:
                    Cache.Instance.Local_safe(0,0);
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
