using System;
using Questor.Modules.Actions;
using Questor.Modules.Activities;
using Questor.Modules.Caching;
using Questor.Modules.Logging;
using Questor.Modules.States;
using System.Linq;
using DirectEve;

namespace Questor.Storylines
{
    public class GenericCourier : IStoryline
    {
        private DateTime _nextAction;
        private readonly Traveler _traveler;
        private GenericCourierStorylineState _state;

        public GenericCourier()
        {
            _traveler = new Traveler();
        }

        public StorylineState Arm(Storyline storyline)
        {
            if (_nextAction > DateTime.Now)
                return StorylineState.Arm;

            // Are we in an industrial?  Yes, goto the agent
            var directEve = Cache.Instance.DirectEve;
            if (directEve.ActiveShip.TypeId == 648 || directEve.ActiveShip.TypeId == 649 || directEve.ActiveShip.TypeId == 650 || directEve.ActiveShip.TypeId == 651 || directEve.ActiveShip.TypeId == 652 || directEve.ActiveShip.TypeId == 653 || directEve.ActiveShip.TypeId == 654 || directEve.ActiveShip.TypeId == 655 || directEve.ActiveShip.TypeId == 656 || directEve.ActiveShip.TypeId == 657 || directEve.ActiveShip.TypeId == 1944 || directEve.ActiveShip.TypeId == 19744)
                return StorylineState.GotoAgent;

            // Open the ship hangar
            var ships = directEve.GetShipHangar();
            if (ships.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("GenericCourier", "Opening ship hangar", Logging.white);

                // No, command it to open
                directEve.ExecuteCommand(DirectCmd.OpenShipHangar);
                return StorylineState.Arm;
            }

            // If the ship hangar is not ready then wait for it
            if (!ships.IsReady)
                return StorylineState.Arm;

            //  Look for an industrial
            var item = ships.Items.FirstOrDefault(i => i.Quantity == -1 && (i.TypeId == 648 || i.TypeId == 649 || i.TypeId == 650 || i.TypeId == 651 || i.TypeId == 652 || i.TypeId == 653 || i.TypeId == 654 || i.TypeId == 655 || i.TypeId == 656 || i.TypeId == 657 || i.TypeId == 1944 || i.TypeId == 19744));
            if (item != null)
            {
                Logging.Log("GenericCourier", "Switching to an industrial", Logging.white);

                _nextAction = DateTime.Now.AddSeconds(10);

                item.ActivateShip();
                return StorylineState.Arm;
            }
            else
            {
                Logging.Log("GenericCourier", "No industrial found, going in active ship", Logging.white);
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
            _state = GenericCourierStorylineState.GotoPickupLocation;

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
            var directEve = Cache.Instance.DirectEve;

            // Open the item hangar (should still be open)
            var hangar = directEve.GetItemHangar();
            if (hangar.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("GenericCourier", "Opening hangar floor", Logging.white);

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

                Logging.Log("GenericCourier", "Opening cargo", Logging.white);

                directEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                return false;
            }

            if (!cargo.IsReady)
                return false;

            // 314 == Giant Sealed Cargo Containers
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
                Logging.Log("GenericCourier", "Moving [" + item.TypeName + "][" + item.ItemId + "] to " + (pickup ? "cargo" : "hangar"), Logging.white);
                to.Add(item, item.Stacksize);
            }

            _nextAction = DateTime.Now.AddSeconds(10);
            return false;
        }

        /// <summary>
        ///   Goto the pickup location
        ///   Pickup the item
        ///   Goto drop off location
        ///   Drop the item
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
                case GenericCourierStorylineState.GotoPickupLocation:
                if (GotoMissionBookmark(Cache.Instance.CurrentStorylineAgentId, "Objective (Pick Up)"))
                        _state = GenericCourierStorylineState.PickupItem;
                    break;

                case GenericCourierStorylineState.PickupItem:
                    if (MoveItem(true))
                        _state = GenericCourierStorylineState.GotoDropOffLocation;
                    break;

                case GenericCourierStorylineState.GotoDropOffLocation:
                if (GotoMissionBookmark(Cache.Instance.CurrentStorylineAgentId, "Objective (Drop Off)"))
                        _state = GenericCourierStorylineState.DropOffItem;
                    break;

                case GenericCourierStorylineState.DropOffItem:
                    if (MoveItem(false))
                        return StorylineState.CompleteMission;
                    break;
            }

            return StorylineState.ExecuteMission;
        }
    }
}