namespace Questor.Modules
{
    using System;
    using System.Linq;
    using DirectEve;

    public class CourierMission
    {
        private DateTime _nextCourierAction;
        private Traveler _traveler;
        public CourierMissionState State { get; set; }

        /// <summary>
        ///   Arm does nothing but get into a (assembled) shuttle
        /// </summary>
        /// <returns></returns>
        /// 
        public CourierMission()
        {
            _traveler = new Traveler();
        }

        private bool GotoMissionBookmark(long agentId, string title)
        {
            var destination = _traveler.Destination as MissionBookmarkDestination;
            if (destination == null || destination.AgentId != agentId || !destination.Title.StartsWith(title))
                _traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(agentId, title));

            _traveler.ProcessState();

            if (_traveler.State == TravelerState.AtDestination)
            {
                Logging.Log("CourierMission: Final Destination");
                _traveler.Destination = null;
                return true;
            }

            return false;
        }

        private bool MoveItem(bool pickup)
        {
            var directEve = Cache.Instance.DirectEve;

            // Open the item hangar (should still be open)
            var hangar = directEve.GetItemHangar();
            if (hangar.Window == null)
            {
                _nextCourierAction = DateTime.Now.AddSeconds(8);
                Logging.Log("CourierMissionState: Opening hangar floor");
                directEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                return false;
            }

            // Wait for it to become ready
            if (!hangar.IsReady)
                return false;

            var cargo = directEve.GetShipsCargo();
            if (cargo.Window == null)
            {
                _nextCourierAction = DateTime.Now.AddSeconds(8);
                Logging.Log("CourierMissionState: Opening cargo");
                directEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                return false;
            }

            if (!cargo.IsReady)
                return false;

            string missionItem = "Encoded Data Chip";
            Logging.Log("CourierMission: mission item is: " + missionItem);
            DirectContainer from = pickup ? hangar : cargo;
            DirectContainer to = pickup ? cargo : hangar;

            // We moved the item
            if (to.Items.Any(i => i.TypeName == missionItem))
                return true;

            if (directEve.GetLockedItems().Count != 0)
                return false;

            // Move items
            foreach (var item in from.Items.Where(i => i.TypeName == missionItem))
            {
                Logging.Log("CourierMissionState: Moving [" + item.TypeName + "][" + item.ItemId + "] to " + (pickup ? "cargo" : "hangar"));
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
        /// <param name="storyline"></param>
        /// <returns></returns>
        public void ProcessState()
        {
            switch (State)
            {
                case CourierMissionState.Idle:
                    break;

                case CourierMissionState.GotoPickupLocation:
                    if (GotoMissionBookmark(Cache.Instance.AgentId, "Objective (Pick Up)"))
                        State = CourierMissionState.PickupItem;
                    break;

                case CourierMissionState.PickupItem:
                    if (MoveItem(true))
                        State = CourierMissionState.GotoDropOffLocation;
                    break;

                case CourierMissionState.GotoDropOffLocation:
                    if (GotoMissionBookmark(Cache.Instance.AgentId, "Objective (Drop Off)"))
                        State = CourierMissionState.DropOffItem;
                    break;

                case CourierMissionState.DropOffItem:
                    if (MoveItem(false))
                        State = CourierMissionState.Done;
                    break;

                case CourierMissionState.Done:
                    Logging.Log("CourierMissionState: Done");
                    break;
            }

        }
    }
}
