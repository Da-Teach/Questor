namespace Questor.Storylines
{
    using System;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules;

    public class MaterialsForWarPreparation : IStoryline
    {
        private DateTime _nextAction;

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

                Logging.Log("MaterialsForWarPreparation: Opening ship hangar");

                // No, command it to open
                Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenShipHangar);
                return StorylineState.Arm;
            }

            // If the ship hangar is not ready then wait for it
            if (!ships.IsReady)
                return StorylineState.Arm;
            
            //  Look for a shuttle
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
        public StorylineState PreAcceptMission(Storyline storyline)
        {
            var directEve = Cache.Instance.DirectEve;
            if (_nextAction > DateTime.Now)
                return StorylineState.PreAcceptMission;


            // the ore and ore quantity can be stored in the char.xml this is to facility mission levels other than 4.
            //The defaults are for level 4 so it will not break for those people that dont include these in their settings file
            //  Level 1         <MaterialsForWarOreID>1230</MaterialsForWarOreID>
            //                  <MaterialsForWarOreQty>999</MaterialsForWarOreQty>
            //  Level 4         <MaterialsForWarOreID>20</MaterialsForWarOreID>
            //                  <MaterialsForWarOreQty>8000</MaterialsForWarOreQty>


            var oreid = Settings.Instance.MaterialsForWarOreID; //1230;
            var orequantity = Settings.Instance.MaterialsForWarOreQty; //999

            // Open the item hangar
            var hangar = directEve.GetItemHangar();
            if (hangar.Window == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("MaterialsForWarPreparation: Opening hangar floor");

                directEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                return StorylineState.PreAcceptMission;
            }

            // Wait for the item hangar to get ready
            if (!hangar.IsReady)
                return StorylineState.PreAcceptMission;

            // Is there a market window?
            var marketWindow = directEve.Windows.OfType<DirectMarketWindow>().FirstOrDefault();

            // Do we have the ore we need.

            if (hangar.Items.Where(i => i.TypeId == oreid).Sum(i => i.Quantity) >= orequantity) 
            {
                Logging.Log(String.Format("MaterialsForWarPreparation: We have [{0}] {1}, accepting mission", hangar.Items.Where(i => i.TypeId == oreid).Sum(i => i.Quantity).ToString(), hangar.Items.Where(i => i.TypeId == oreid).FirstOrDefault().TypeName));

                // Close the market window if there is one
                if (marketWindow != null)
                    marketWindow.Close();

                return StorylineState.AcceptMission;
            }

            // We do not have enough ore, open the market window
            if (marketWindow == null)
            {
                _nextAction = DateTime.Now.AddSeconds(10);

                Logging.Log("MaterialsForWarPreparation: Opening market window");

                directEve.ExecuteCommand(DirectCmd.OpenMarket);
                return StorylineState.PreAcceptMission;
            }

            // Wait for the window to become ready (this includes loading the ore info)
            if (!marketWindow.IsReady)
                return StorylineState.PreAcceptMission;

            // Are we currently viewing ore orders?
            if (marketWindow.DetailTypeId != oreid)
            {
                // No, load the kernite orders
                marketWindow.LoadTypeId(oreid);


                Logging.Log("MaterialsForWarPreparation: Loading market window");

                _nextAction = DateTime.Now.AddSeconds(5);
                return StorylineState.PreAcceptMission;
            }

            // Get the median sell price
            var type = Cache.Instance.InvTypesById[20];
            var maxPrice = type.MedianSell*2;

            // Do we have orders that sell enough ore for the mission?
            var orders = marketWindow.SellOrders.Where(o => o.StationId == directEve.Session.StationId && o.Price < maxPrice);
            if (!orders.Any() || orders.Sum(o => o.VolumeRemaining) < orequantity)
            {
                Logging.Log("MaterialsForWarPreparation: Not enough (reasonably priced) ore available! Blacklisting agent for this Questor session!");

                // Close the market window
                marketWindow.Close();

                // No, black list the agent in this Questor session (note we will never decline storylines!)
                return StorylineState.BlacklistAgent;
            }

            // How much kernite do we still need?
            var neededQuantity = orequantity - hangar.Items.Where(i => i.TypeId == oreid).Sum(i => i.Quantity);
            if (neededQuantity > 0)
            {
                // Get the first order
                var order = orders.OrderBy(o => o.Price).FirstOrDefault();
                if (order != null)
                {
                    // Calculate how much ore we still need
                    var remaining = Math.Min(neededQuantity, order.VolumeRemaining);
                    order.Buy(remaining, DirectOrderRange.Station);

                    Logging.Log("MaterialsForWarPreparation: Buying [" + remaining + "] ore");

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
        public StorylineState PostAcceptMission(Storyline storyline)
        {
            // Close the market window (if its open)
            var directEve = Cache.Instance.DirectEve;

            return StorylineState.CompleteMission;
        }

        /// <summary>
        ///   We have no execute mission code
        /// </summary>
        /// <returns></returns>
        public StorylineState ExecuteMission(Storyline storyline)
        {
            return StorylineState.CompleteMission;
        }
    }
}
