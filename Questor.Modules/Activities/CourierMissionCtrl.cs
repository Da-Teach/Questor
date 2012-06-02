
namespace Questor.Modules.Activities
{
    using System;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules.Actions;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.States;
    using global::Questor.Modules.Caching;

    public class CourierMissionCtrl
    {
        private DateTime _nextCourierAction;
        private readonly Traveler _traveler;

        /// <summary>
        ///   Arm does nothing but get into a (assembled) shuttle
        /// </summary>
        /// <returns></returns>
        ///
        public CourierMissionCtrl()
        {
            _traveler = new Traveler();
        }

        private bool GotoMissionBookmark(long agentId, string title)
        {
            var destination = _traveler.Destination as MissionBookmarkDestination;
            if (destination == null || destination.AgentId != agentId || !destination.Title.StartsWith(title))
                _traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(agentId, title));

            _traveler.ProcessState();

            if (_States.CurrentTravelerState == TravelerState.AtDestination)
            {
                if (destination != null)
                    Logging.Log("CourierMissionCtrl", "Arrived at Mission Bookmark Destination [ " + destination.Title + " ]", Logging.white);
                else
                {
                    Logging.Log("CourierMissionCtrl", "destination is null", Logging.white); //how would this occur exactly?
                }
                _traveler.Destination = null;
                return true;
            }

            return false;
        }

        private bool MoveItem(bool pickup)
        {
            DirectEve directEve = Cache.Instance.DirectEve;

            // Open the item hangar (should still be open)
            if (!Cache.Instance.OpenItemsHangar("CourierMissionCtrl")) return false;

            if (!Cache.Instance.OpenCargoHold("CourierMissionCtrl")) return false;

            const string missionItem = "Encoded Data Chip";
            Logging.Log("CourierMissionCtrl", "mission item is: " + missionItem, Logging.white);
            DirectContainer from = pickup ? Cache.Instance.ItemHangar : Cache.Instance.CargoHold;
            DirectContainer to = pickup ? Cache.Instance.CargoHold : Cache.Instance.ItemHangar;

            // We moved the item
            if (to.Items.Any(i => i.TypeName == missionItem))
                return true;

            if (directEve.GetLockedItems().Count != 0)
                return false;

            // Move items
            foreach (DirectItem item in from.Items.Where(i => i.TypeName == missionItem))
            {
                Logging.Log("CourierMissionCtrl", "Moving [" + item.TypeName + "][" + item.ItemId + "] to " + (pickup ? "cargo" : "hangar"), Logging.white);
                to.Add(item);
            }
            _nextCourierAction = DateTime.Now.AddSeconds(8);
            return false;
        }

        /// <summary>
        ///   Goto the pickup location
        ///   Pickup the item
        ///   Goto drop off location
        ///   Drop the item
        ///   Goto Agent
        ///   Complete mission
        /// </summary>
        /// <returns></returns>
        public void ProcessState()
        {
            switch (_States.CurrentCourierMissionCtrlState)
            {
                case CourierMissionCtrlState.Idle:
                    break;

                case CourierMissionCtrlState.GotoPickupLocation:
                    //cache.instance.agentid cannot be used for storyline missions! you must pass the correct agentID to this module if you wish to extend it to do storyline missions
                    if (GotoMissionBookmark(Cache.Instance.AgentId, "Objective (Pick Up)"))
                        _States.CurrentCourierMissionCtrlState = CourierMissionCtrlState.PickupItem;
                    break;

                case CourierMissionCtrlState.PickupItem:
                    if (MoveItem(true))
                        _States.CurrentCourierMissionCtrlState = CourierMissionCtrlState.GotoDropOffLocation;
                    break;

                case CourierMissionCtrlState.GotoDropOffLocation:
                    //cache.instance.agentid cannot be used for storyline missions! you must pass the correct agentID to this module if you wish to extend it to do storyline missions
                    if (GotoMissionBookmark(Cache.Instance.AgentId, "Objective (Drop Off)"))
                        _States.CurrentCourierMissionCtrlState = CourierMissionCtrlState.DropOffItem;
                    break;

                case CourierMissionCtrlState.DropOffItem:
                    if (MoveItem(false))
                        _States.CurrentCourierMissionCtrlState = CourierMissionCtrlState.Done;
                    break;

                case CourierMissionCtrlState.Done:
                    Logging.Log("CourierMissionCtrl", "Done", Logging.white);
                    break;
            }
        }
    }
}