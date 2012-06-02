
namespace Questor.Storylines
{
    using System;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules.Actions;
    using global::Questor.Modules.Activities;
    using global::Questor.Modules.States;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;

    public class TransactionDataDelivery : IStoryline
    {
        private DateTime _nextAction;
        private readonly Traveler _traveler;
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

            // Are we in a shuttle?  Yes, go to the agent
            DirectEve directEve = Cache.Instance.DirectEve;
            if (directEve.ActiveShip.GroupId == 31)
                return StorylineState.GotoAgent;

            // Open the ship hangar
            if (!Cache.Instance.OpenShipsHangar("TransactionDataDelivery")) return StorylineState.Arm;

            //  Look for a shuttle
            DirectItem item = Cache.Instance.ShipHangar.Items.FirstOrDefault(i => i.Quantity == -1 && i.GroupId == 31);
            if (item != null)
            {
                Logging.Log("TransactionDataDelivery", "Switching to shuttle", Logging.white);

                _nextAction = DateTime.Now.AddSeconds(10);

                item.ActivateShip();
                return StorylineState.Arm;
            }
            else
            {
                Logging.Log("TransactionDataDelivery", "No shuttle found, going in active ship", Logging.orange);
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

            _States.CurrentTravelerState = TravelerState.Idle;
            _traveler.Destination = null;

            return StorylineState.AcceptMission;
        }

        private bool GotoMissionBookmark(long agentId, string title)
        {
            var destination = _traveler.Destination as MissionBookmarkDestination;
            if (destination == null || destination.AgentId != agentId || !destination.Title.ToLower().StartsWith(title.ToLower()))
                _traveler.Destination = new MissionBookmarkDestination(Cache.Instance.GetMissionBookmark(agentId, title));

            _traveler.ProcessState();

            if (_States.CurrentTravelerState == TravelerState.AtDestination)
            {
                _traveler.Destination = null;
                return true;
            }

            return false;
        }

        private bool MoveItem(bool pickup)
        {
            DirectEve directEve = Cache.Instance.DirectEve;

            // Open the item hangar (should still be open)
            if (!Cache.Instance.OpenItemsHangar("TransactionDataDelivery")) return false;

            if (!Cache.Instance.OpenCargoHold("TransactionDataDelivery")) return false;

            // 314 == Transaction And Salary Logs (all different versions)
            const int groupId = 314;
            DirectContainer from = pickup ? Cache.Instance.ItemHangar : Cache.Instance.CargoHold;
            DirectContainer to = pickup ? Cache.Instance.CargoHold : Cache.Instance.ItemHangar;

            // We moved the item
            if (to.Items.Any(i => i.GroupId == groupId))
                return true;

            if (directEve.GetLockedItems().Count != 0)
                return false;

            // Move items
            foreach (DirectItem item in from.Items.Where(i => i.GroupId == groupId))
            {
                Logging.Log("TransactionDataDelivery", "Moving [" + item.TypeName + "][" + item.ItemId + "] to " + (pickup ? "cargo" : "hangar"), Logging.white);
                to.Add(item);
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
                    if (GotoMissionBookmark(Cache.Instance.CurrentStorylineAgentId, "Objective (Pick Up)"))
                        _state = TransactionDataDeliveryState.PickupItem;
                    break;

                case TransactionDataDeliveryState.PickupItem:
                    if (MoveItem(true))
                        _state = TransactionDataDeliveryState.GotoDropOffLocation;
                    break;

                case TransactionDataDeliveryState.GotoDropOffLocation:
                    if (GotoMissionBookmark(Cache.Instance.CurrentStorylineAgentId, "Objective (Drop Off)"))
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