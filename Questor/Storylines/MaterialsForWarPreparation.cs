namespace Questor.Storylines
{
    using System;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules;

    public class MaterialsForWarPreparation : IStoryline
    {
        private DateTime _nextAction;

        public StorylineState Arm()
        {
            if (_nextAction > DateTime.Now)
                return StorylineState.Arm; 
            
            var directEve = Cache.Instance.DirectEve;
            if (directEve.ActiveShip.GroupId == 31)
                return StorylineState.GotoAgent;

            var ships = directEve.GetShipHangar();
            if (ships.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("MaterialsForWarPreparation: Opening ship hangar");

                // No, command it to open
                Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenShipHangar);
                return StorylineState.Arm;
            }

            if (!ships.IsReady)
                return StorylineState.Arm;

            var item = ships.Items.FirstOrDefault(i => i.Quantity == -1 && i.GroupId == 31);
            if (item != null)
            {
                Logging.Log("MaterialsForWarPreparation: Switching to shuttle");

                _nextAction = DateTime.Now.AddSeconds(10);

                item.ActivateShip();
                return StorylineState.Arm;
            }
            else
            {
                Logging.Log("MaterialsForWarPreparation: No shuttle found, going in active ship");
                return StorylineState.GotoAgent;
            }
        }

        /// <summary>
        ///   Check if we have kernite in station
        /// </summary>
        /// <returns></returns>
        public StorylineState PreAcceptMission()
        {
            var directEve = Cache.Instance.DirectEve;
            if (_nextAction > DateTime.Now)
                return StorylineState.PreAcceptMission;

            var hangar = directEve.GetItemHangar();
            if (hangar.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("MaterialsForWarPreparation: Opening hangar floor");

                directEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                return StorylineState.PreAcceptMission;
            }

            if (!hangar.IsReady)
                return StorylineState.PreAcceptMission;

            if (hangar.Items.Where(i => i.TypeId == 20).Sum(i => i.Quantity) >= 8000)
            {
                Logging.Log("MaterialsForWarPreparation: We have [8000] kernite, accepting mission");

                return StorylineState.AcceptMission;
            }

            var marketWindow = directEve.Windows.OfType<DirectMarketWindow>().FirstOrDefault();
            if (marketWindow == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("MaterialsForWarPreparation: Opening market window");

                directEve.ExecuteCommand(DirectCmd.OpenMarket);
                return StorylineState.PreAcceptMission;
            }

            if (!marketWindow.IsReady)
                return StorylineState.PreAcceptMission;

            if (marketWindow.DetailTypeId != 20)
            {
                marketWindow.LoadTypeId(20);

                Logging.Log("MaterialsForWarPreparation: Loading kernite into market window");

                _nextAction = DateTime.Now.AddSeconds(5);
                return StorylineState.PreAcceptMission;
            }

            var type = Cache.Instance.InvTypesById[20];
            var maxPrice = type.MedianSell*2;

            var orders = marketWindow.SellOrders.Where(o => o.StationId == directEve.Session.StationId && o.Price < maxPrice);
            if (!orders.Any() || orders.Sum(o => o.VolumeRemaining) < 8000)
            {
                Logging.Log("MaterialsForWarPreparation: Not enough (reasonably priced) kernite available! Blacklisting agent for this Questor session!");

                return StorylineState.BlacklistAgent;
            }

            var neededQuantity = 8000 - (hangar.Items.Where(i => i.TypeId == 20).Sum(i => i.Quantity) ?? 0);
            if (neededQuantity > 0)
            {
                var order = orders.OrderBy(o => o.Price).FirstOrDefault();
                if (order != null)
                {
                    var remaining = Math.Min(neededQuantity, order.VolumeRemaining);
                    order.Buy(remaining, DirectOrderRange.Station);

                    Logging.Log("MaterialsForWarPreparation: Buying [" + remaining + "] kernite");

                    // Wait for the order to go through
                    _nextAction = DateTime.Now.AddSeconds(10);
                }
            }
            return StorylineState.PreAcceptMission;
        }

        /// <summary>
        ///   We have no combat/delivery part in this mission, just accept it
        /// </summary>
        /// <returns></returns>
        public StorylineState PostAcceptMission()
        {
            var directEve = Cache.Instance.DirectEve;
            var marketWindow = directEve.Windows.OfType<DirectMarketWindow>().FirstOrDefault();
            if (marketWindow != null)
                marketWindow.Close();

            return StorylineState.CompleteMission;
        }

        public StorylineState PostCompleteMission()
        {
            var directEve = Cache.Instance.DirectEve;
            if (_nextAction > DateTime.Now)
                return StorylineState.PostCompleteMission;

            var hangar = directEve.GetItemHangar();
            if (hangar.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("MaterialsForWarPreparation: Opening hangar floor");

                directEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                return StorylineState.PostCompleteMission;
            }

            if (!hangar.IsReady)
                return StorylineState.PostCompleteMission;

            if (!hangar.Items.Any(i => i.GroupId == 745))
                return StorylineState.Done;

            var cargo = directEve.GetShipsCargo();
            if (cargo.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("MaterialsForWarPreparation: Opening cargo");

                directEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                return StorylineState.PostCompleteMission;
            }

            if (!cargo.IsReady)
                return StorylineState.PostCompleteMission;

            if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
            {
                foreach (var item in hangar.Items.Where(i => i.GroupId == 745))
                {
                    Logging.Log("MaterialsForWarPreparation: Moving [" + item.Name + "][" + item.ItemId + "] to cargo");
                    cargo.Add(item.ItemId, item.Quantity ?? 1);
                }

                _nextAction = DateTime.Now.AddSeconds(10);
            }

            return StorylineState.PostCompleteMission;
        }
    }
}
