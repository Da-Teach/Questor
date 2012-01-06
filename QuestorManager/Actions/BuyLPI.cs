
namespace QuestorManager.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DirectEve;
    using DirectEve = global::QuestorManager.Common.DirectEve;
    using global::QuestorManager.Common;
    using global::QuestorManager.Domains;
    using global::QuestorManager.Module;


    class BuyLPI
    {
        public StateBuyLPI State { get; set; }

        public int Item { get; set; }
        public int Unit { get; set; }

        private MainForm _form;

        private static DateTime _lastAction;
        private static DateTime _loyaltyPointTimeout;
        private static long _lastLoyaltyPoints;
        private int requiredUnit;
        private int requiredItemId;



        public BuyLPI(MainForm form1)
        {
            _form = form1;
        }

        public void ProcessState()
        {

            var hangar = DirectEve.Instance.GetItemHangar();
            var shiphangar = DirectEve.Instance.GetShipHangar();
            var lpstore = DirectEve.Instance.Windows.OfType<DirectLoyaltyPointStoreWindow>().FirstOrDefault();
            var marketWindow = DirectEve.Instance.Windows.OfType<DirectMarketWindow>().FirstOrDefault();
            
            

            if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                return;
            _lastAction = DateTime.Now;

            switch (State)
            {
                case StateBuyLPI.Idle:
                case StateBuyLPI.Done:
                    break;


                case StateBuyLPI.Begin:


                    if (marketWindow != null)
                        marketWindow.Close();

                    if (lpstore != null)
                        lpstore.Close();


                    State = StateBuyLPI.OpenItemHangar;

                    break;

                case StateBuyLPI.OpenItemHangar:

                    if (!hangar.IsReady)
                    {
                       DirectEve.Instance.ExecuteCommand(DirectCmd.OpenHangarFloor);
                       Logging.Log("BuyLPI: Opening item hangar");
                    }
                    State = StateBuyLPI.OpenLpStore;

                    break;

                case StateBuyLPI.OpenLpStore:

                    

                    if (lpstore == null)
                    {
                        DirectEve.Instance.ExecuteCommand(DirectCmd.OpenLpstore);
                        Logging.Log("BuyLPI: Opening loyalty point store");
                    }
                    State = StateBuyLPI.FindOffer;

                    break;

                case StateBuyLPI.FindOffer:

                    var offer = lpstore.Offers.FirstOrDefault(o => o.TypeId == Item);

                    // Wait for the amount of LP to change
                    if (_lastLoyaltyPoints == lpstore.LoyaltyPoints)
                        break;

                    // Do not expect it to be 0 (probably means its reloading)
                    if (lpstore.LoyaltyPoints == 0)
                    {
                        if (_loyaltyPointTimeout < DateTime.Now)
                        {
                            Logging.Log("BuyLPI: It seems we have no loyalty points left");

                            State = StateBuyLPI.Done;
                        }
                        break;
                    }

                    _lastLoyaltyPoints = lpstore.LoyaltyPoints;

                    // Find the offer
                    if (offer == null)
                    {
                        Logging.Log("BuyLPI: Can't find offer with type name/id: {0}!", Item);

                        State = StateBuyLPI.Done;
                        break;
                    }

                    State = StateBuyLPI.CheckPetition;


                    break;

                case StateBuyLPI.CheckPetition:

                    var offer1 = lpstore.Offers.FirstOrDefault(o => o.TypeId == Item);

                    // Check LP
                    if (_lastLoyaltyPoints < offer1.LoyaltyPointCost)
                    {
                        Logging.Log("BuyLPI: Not enough loyalty points left");

                        State = StateBuyLPI.Done;
                        break;
                    }

                    // Check ISK
                    if (DirectEve.Instance.Me.Wealth < offer1.IskCost)
                    {
                        Logging.Log("BuyLPI: Not enough ISK left");

                        State = StateBuyLPI.Done;
                        break;
                    }

                    // Check items
                    foreach (var requiredItem in offer1.RequiredItems)
                    {

                        var ship = shiphangar.Items.FirstOrDefault(i => i.TypeId == requiredItem.TypeId);
                        var item = hangar.Items.FirstOrDefault(i => i.TypeId == requiredItem.TypeId);
                        if (item == null || item.Quantity < requiredItem.Quantity)
                        {
                            if (ship == null || ship.Quantity < requiredItem.Quantity)
                            {
                                Logging.Log("BuyLPI: Missing {0}x {1}", requiredItem.Quantity, requiredItem.TypeName);

                                if (!_form.chkBuyItems.Checked)
                                {
                                    Logging.Log("BuyLPI: Done, do not buy item");
                                    State = StateBuyLPI.Done;
                                    break;
                                }

                                Logging.Log("BuyLPI: Are buying the item " + requiredItem.TypeName);
                                requiredUnit = Convert.ToInt32(requiredItem.Quantity);
                                requiredItemId = requiredItem.TypeId;
                                State = StateBuyLPI.OpenMarket;
                                return;
                            }
                        }
                    }

                    State = StateBuyLPI.AcceptOffer;

                    break;


                case StateBuyLPI.OpenMarket:

                     if (marketWindow == null)
                     {
                        DirectEve.Instance.ExecuteCommand(DirectCmd.OpenMarket);
                        break;
                     }

                     if (!marketWindow.IsReady)
                         break;

                     State = StateBuyLPI.BuyItems;

                    break;

                case StateBuyLPI.BuyItems:

                    Logging.Log("BuyLPI: Opening Market");

                    if (marketWindow.DetailTypeId != requiredItemId)
                    {
                        marketWindow.LoadTypeId(requiredItemId);
                        break;
                    }

                    var orders = marketWindow.SellOrders.Where(o => o.StationId == DirectEve.Instance.Session.StationId);

                    var order = orders.OrderBy(o => o.Price).FirstOrDefault();

                    if (order == null)
                    {
                        Logging.Log("BuyLPI: No orders");
                      State = StateBuyLPI.Done;
                      break;
                    }

                    order.Buy(requiredUnit, DirectOrderRange.Station);

                    Logging.Log("BuyLPI: Buy Item");


                    State = StateBuyLPI.CheckPetition;

                    break;

                case StateBuyLPI.AcceptOffer:

                    var offer2 = lpstore.Offers.FirstOrDefault(o => o.TypeId == Item);

                    Logging.Log("BuyLPI: Accepting {0}", offer2.TypeName);
                     offer2.AcceptOffer();
                     State = StateBuyLPI.Quatity;

                    break;

                case StateBuyLPI.Quatity:

                    _loyaltyPointTimeout = DateTime.Now.AddSeconds(10);

                        Unit = Unit - 1;
                        if (Unit <= 0)
                        {
                            Logging.Log("BuyLPI: Quantity limit reached");

                            State = StateBuyLPI.Done;
                            break;
                        }

                        State = StateBuyLPI.Begin;
                   
                    break;


            }
        }

    }
}
