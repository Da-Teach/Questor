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
        private DateTime _nextTravelerAction;

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
            if (_nextTravelerAction > DateTime.Now)
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
                    Logging.Log("Traveler: Setting destination to [" + location.Name + "]");
                    location.SetDestination();
                }
                else
                {
                    Logging.Log("Traveler: Error setting solar system destination [" + solarSystemId + "]");
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
                        _nextTravelerAction = DateTime.Now.AddSeconds((int)Time.TravelerExitStationAmIInSpaceYet_seconds);
                    }

                    // We are not yet in space, wait for it
                    return;
                }

                // Find the first waypoint
                var waypoint = destination.First();

                // Get the name of the next systems
                var locationName = Cache.Instance.DirectEve.Navigation.GetLocationName(waypoint);

                // Find the stargate associated with it
                var entities = Cache.Instance.EntitiesByName(locationName).Where(e => e.GroupId == (int)Group.Stargate);
                if (entities.Count() == 0)
                {
                    // not found, that cant be true?!?!?!?!
                    Logging.Log("Traveler: Error [Stargate (" + locationName + ")] not found, most likely lag waiting 15 seconds.");
                    _nextTravelerAction = DateTime.Now.AddSeconds((int)Time.TravelerNoStargatesFoundRetryDelay_seconds);
                    return;
                }

                // Warp to, approach or jump the stargate
                var entity = entities.First();
                if (entity.Distance < (int)Distance.DecloakRange)
                {
                    Logging.Log("Traveler: Jumping to [" + locationName + "]");
                    entity.Jump();

                    _nextTravelerAction = DateTime.Now.AddSeconds((int)Time.TravelerJumpedGateNextCommandDelay_seconds);
                }
                else if (entity.Distance < (int)Distance.WarptoDistance)
                    entity.Approach(); //you could use a negative approach distance here but ultimately that is a bad idea.. Id like to go toward the entity without approaching it so we would end up inside the docking ring (eventually)
                   
                else
                {
                    Logging.Log("Traveler: Warping to [Stargate (" + locationName + ")]");
                    entity.WarpTo();
                    _nextTravelerAction = DateTime.Now.AddSeconds((int)Time.TravelerInWarpedNextCommandDelay_seconds);
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