namespace Questor.Storylines
{
    using System;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules;

    public class TransactionDataDelivery : IStoryline
    {
        private DateTime _nextAction;
        private Traveler _traveler;
        private TransactionDataDeliveryState _state;

        public TransactionDataDelivery()
        {
            _traveler = new Traveler();
        }

        /// <summary>
        ///   Arm does nothing but get into a (assembled) shuttle
        /// </summary>
        /// <returns></returns>
        public StorylineState Arm(Storyline storyline)
        {
            if (_nextAction > DateTime.Now)
                return StorylineState.Arm;

            // Are we in a shuttle?  Yes, goto the agent
            var directEve = Cache.Instance.DirectEve;
            if (directEve.ActiveShip.GroupId == 31)
                return StorylineState.GotoAgent;

            // Open the ship hangar
            var ships = directEve.GetShipHangar();
            if (ships.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("TransactionDataDelivery: Opening ship hangar");

                // No, command it to open
                directEve.ExecuteCommand(DirectCmd.OpenShipHangar);
                return StorylineState.Arm;
            }

            // If the ship hangar is not ready then wait for it
            if (!ships.IsReady)
                return StorylineState.Arm;

            //  Look for a shuttle
            var item = ships.Items.FirstOrDefault(i => i.Quantity == -1 && i.GroupId == 31);
            if (item != null)
            {
                Logging.Log("TransactionDataDelivery: Switching to shuttle");

                _nextAction = DateTime.Now.AddSeconds(10);

                item.ActivateShip();
                return StorylineState.Arm;
            }
            else
            {
                Logging.Log("TransactionDataDelivery: No shuttle found, going in active ship");
                return StorylineState.GotoAgent;
            }
        }

        /// <summary>
        ///   There are no pre-accept actions
        /// </summary>
        /// <param name="storyline"></param>
        /// <returns></returns>
        public StorylineState PreAcceptMission(Storyline storyline)
        {
            _state = TransactionDataDeliveryState.GotoPickupLocation;
            
            _traveler.State = TravelerState.Idle;
            _traveler.Destination = null;

            return StorylineState.AcceptMission;
        }

        private bool GotoMissionBookmark(long agentId, string title)
        {
            var destination = _traveler.Destination as MissionBookmarkDestination;
            if (destination == null || destination.AgentId != agentId || !destination.Title.StartsWith(title))
                _traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(agentId, title));

            _traveler.ProcessState();

            if (_traveler.State == TravelerState.AtDestination)
            {
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
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("TransactionDataDelivery: Opening hangar floor");

                directEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                return false;
            }

            // Wait for it to become ready
            if (!hangar.IsReady)
                return false;

            var cargo = directEve.GetShipsCargo();
            if (cargo.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("TransactionDataDelivery: Opening cargo");

                directEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                return false;
            }

            if (!cargo.IsReady)
                return false;

            // 314 == Transaction And Salary Logs (all different versions)
            const int groupId = 314;
            DirectContainer from = pickup ? hangar : cargo;
            DirectContainer to = pickup ? cargo : hangar;

            // We moved the item
            if (to.Items.Any(i => i.GroupId == groupId))
                return true;
            
            if (directEve.GetLockedItems().Count != 0)
                return false;

            // Move items
            foreach (var item in from.Items.Where(i => i.GroupId == groupId))
            {
                Logging.Log("TransactionDataDelivery: Moving [" + item.Name + "][" + item.ItemId + "] to " + (pickup ? "cargo" : "hangar"));
                to.Add(item.ItemId, item.Quantity ?? 1);
            }

            _nextAction = DateTime.Now.AddSeconds(10);
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
        public StorylineState ExecuteMission(Storyline storyline)
        {
            if (_nextAction > DateTime.Now)
                return StorylineState.ExecuteMission; 
            
            switch (_state)
            {
                case TransactionDataDeliveryState.GotoPickupLocation:
                    if (GotoMissionBookmark(storyline.AgentId, "Objective (Pick Up)"))
                        _state = TransactionDataDeliveryState.PickupItem;
                    break;

                case TransactionDataDeliveryState.PickupItem:
                    if (MoveItem(true))
                        _state = TransactionDataDeliveryState.GotoDropOffLocation;
                    break;

                case TransactionDataDeliveryState.GotoDropOffLocation:
                    if (GotoMissionBookmark(storyline.AgentId, "Objective (Drop Off)"))
                        _state = TransactionDataDeliveryState.DropOffItem;
                    break;

                case TransactionDataDeliveryState.DropOffItem:
                    if (MoveItem(false))
                        return StorylineState.ReturnToAgent;
                    break;
            }

            return StorylineState.ExecuteMission;
        }
    }
}
