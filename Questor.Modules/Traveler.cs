// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace Questor.Modules
{
    using System;
    using System.Linq;
    using DirectEve;

    public class Traveler
    {
        private TravelerDestination _destination;
        private DateTime _nextAction;

        public TravelerState State { get; set; }
        public DirectBookmark UndockBookmark { get; set; }

        public TravelerDestination Destination
        {
            get { return _destination; }
            set
            {
                _destination = value;
                State = TravelerState.Idle;
            }
        }

        /// <summary>
        ///   Navigate to a solar system
        /// </summary>
        /// <param name = "solarSystemId"></param>
        private void NagivateToBookmarkSystem(long solarSystemId)
        {
            if (_nextAction > DateTime.Now)
                return;

			var undockBookmark = UndockBookmark;
			UndockBookmark = undockBookmark;

            var destination = Cache.Instance.DirectEve.Navigation.GetDestinationPath();
            if (destination.Count == 0 || !destination.Any(d => d == solarSystemId))
            {
                // We do not have the destination set
                var location = Cache.Instance.DirectEve.Navigation.GetLocation(solarSystemId);
                if (location.IsValid)
                {
                    Logging.Log("Traveler: (traveler.cs) Setting destination to [" + location.Name + "]");
                    location.SetDestination();
                }
                else
                {
                    Logging.Log("Traveler: (traveler.cs) Error setting solar system destination [" + solarSystemId + "]");
                    State = TravelerState.Error;
                }

                return;
            }
            else
            {
                if (!Cache.Instance.InSpace)
                {
                    if (Cache.Instance.InStation)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdExitStation);
                        _nextAction = DateTime.Now.AddSeconds(25);
                    }

                    // We are not yet in space, wait for it
                    return;
                }

                // Find the first waypoint
                var waypoint = destination.First();

                // Get the name of the next system
                var locationName = Cache.Instance.DirectEve.Navigation.GetLocationName(waypoint);

                // Find the stargate associated with it
                var entities = Cache.Instance.EntitiesByName("Stargate (" + locationName + ")");
                if (entities.Count() == 0)
                {
                    // not found, that cant be true?!?!?!?!
                    Logging.Log("Traveler: (traveler.cs) Error [Stargate (" + locationName + ")] not found, most likely lag waiting 15 seconds.");
                    _nextAction = DateTime.Now.AddSeconds(15);
                    return;
                }

                // Warp to, approach or jump the stargate
                var entity = entities.First();
                if (entity.Distance < 2500)
                {
                    Logging.Log("Traveler: (traveler.cs) Jumping to [" + locationName + "]");
                    entity.Jump();

                    _nextAction = DateTime.Now.AddSeconds(15);
                }
                else if (entity.Distance < 150000)
                    entity.Approach();
                else
                {
                    Logging.Log("Traveler: (traveler.cs) Warping to [Stargate (" + locationName + ")]");
                    entity.WarpTo();

                    _nextAction = DateTime.Now.AddSeconds(5);
                }
            }
        }

        public void ProcessState()
        {
            switch (State)
            {
                case TravelerState.Idle:
                    State = TravelerState.Traveling;
                    break;

                case TravelerState.Traveling:
                    if (Destination == null)
                    {
                        State = TravelerState.Error;
                        break;
                    }

                    if (Destination.SolarSystemId != Cache.Instance.DirectEve.Session.SolarSystemId)
                        NagivateToBookmarkSystem(Destination.SolarSystemId);
                    else if (Destination.PerformFinalDestinationTask())
                        State = TravelerState.AtDestination;
                    break;

                default:
                    break;
            }
        }
    }
}