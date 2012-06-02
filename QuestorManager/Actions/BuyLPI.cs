using System;
using System.Collections.Generic;
using System.Linq;
using DirectEve;
using Questor.Modules.Caching;
using Questor.Modules.Logging;
using Questor.Modules.States;

namespace QuestorManager.Actions
{
    public class BuyLPI
    {
        public int Item { get; set; }

        public int Unit { get; set; }

        private QuestorManagerUI _form;

        private static DateTime _lastAction;
        private static DateTime _loyaltyPointTimeout;
        private static long _lastLoyaltyPoints;
        private int _requiredUnit;
        private int _requiredItemId;

        public BuyLPI(QuestorManagerUI form1)
        {
            _form = form1;
        }

        public void ProcessState()
        {
            DirectContainer hangar = Cache.Instance.DirectEve.GetItemHangar();
            DirectContainer shiphangar = Cache.Instance.DirectEve.GetShipHangar();
            DirectLoyaltyPointStoreWindow lpstore =
                Cache.Instance.DirectEve.Windows.OfType<DirectLoyaltyPointStoreWindow>().FirstOrDefault();
            DirectMarketWindow marketWindow =
                Cache.Instance.DirectEve.Windows.OfType<DirectMarketWindow>().FirstOrDefault();

            if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 1)
                return;
            _lastAction = DateTime.Now;

            switch (_States.CurrentBuyLPIState)
            {
                case BuyLPIState.Idle:
                case BuyLPIState.Done:
                    break;

                case BuyLPIState.Begin:

                    /*
                    if(marketWindow != null)
                        marketWindow.Close();

                    if(lpstore != null)
                        lpstore.Close();*/

                    _States.CurrentBuyLPIState = BuyLPIState.OpenItemHangar;

                    break;

                case BuyLPIState.OpenItemHangar:

                    if (!hangar.IsReady)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                        Logging.Log("BuyLPI", "Opening item hangar", Logging.white);
                    }
                    _States.CurrentBuyLPIState = BuyLPIState.OpenLpStore;

                    break;

                case BuyLPIState.OpenLpStore:

                    if (lpstore == null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenLpstore);
                        Logging.Log("BuyLPI", "Opening loyalty point store", Logging.white);
                    }
                    _States.CurrentBuyLPIState = BuyLPIState.FindOffer;

                    break;

                case BuyLPIState.FindOffer:

                    if (lpstore != null)
                    {
                        DirectLoyaltyPointOffer offer = lpstore.Offers.FirstOrDefault(o => o.TypeId == Item);

                        // Wait for the amount of LP to change
                        if (_lastLoyaltyPoints == lpstore.LoyaltyPoints)
                            break;

                        // Do not expect it to be 0 (probably means its reloading)
                        if (lpstore.LoyaltyPoints == 0)
                        {
                            if (_loyaltyPointTimeout < DateTime.Now)
                            {
                                Logging.Log("BuyLPI", "It seems we have no loyalty points left", Logging.white);

                                _States.CurrentBuyLPIState = BuyLPIState.Done;
                            }
                            break;
                        }

                        _lastLoyaltyPoints = lpstore.LoyaltyPoints;

                        // Find the offer
                        if (offer == null)
                        {
                            Logging.Log("BuyLPI", "Can't find offer with type name/id: " + Item + "!", Logging.white);

                            _States.CurrentBuyLPIState = BuyLPIState.Done;
                            break;
                        }
                    }

                    _States.CurrentBuyLPIState = BuyLPIState.CheckPetition;

                    break;

                case BuyLPIState.CheckPetition:

                    if (lpstore != null)
                    {
                        DirectLoyaltyPointOffer offer1 = lpstore.Offers.FirstOrDefault(o => o.TypeId == Item);

                        // Check LP
                        if (offer1 != null && _lastLoyaltyPoints < offer1.LoyaltyPointCost)
                        {
                            Logging.Log("BuyLPI", "Not enough loyalty points left", Logging.white);

                            _States.CurrentBuyLPIState = BuyLPIState.Done;
                            break;
                        }

                        // Check ISK
                        if (offer1 != null && Cache.Instance.DirectEve.Me.Wealth < offer1.IskCost)
                        {
                            Logging.Log("BuyLPI", "Not enough ISK left", Logging.white);

                            _States.CurrentBuyLPIState = BuyLPIState.Done;
                            break;
                        }

                        // Check items
                        if (offer1 != null)
                            foreach (DirectLoyaltyPointOfferRequiredItem requiredItem in offer1.RequiredItems)
                            {
                                DirectItem ship = shiphangar.Items.FirstOrDefault(i => i.TypeId == requiredItem.TypeId);
                                DirectItem item = hangar.Items.FirstOrDefault(i => i.TypeId == requiredItem.TypeId);
                                if (item == null || item.Quantity < requiredItem.Quantity)
                                {
                                    if (ship == null || ship.Quantity < requiredItem.Quantity)
                                    {
                                        Logging.Log("BuyLPI", "Missing [" + requiredItem.Quantity + "] x [" +
                                                    requiredItem.TypeName + "]", Logging.white);

                                        //if(!_form.chkBuyItems.Checked)
                                        //{
                                        //    Logging.Log("BuyLPI: Done, do not buy item");
                                        //    States.CurrentBuyLPIState = BuyLPIState.Done;
                                        //    break;
                                        //}

                                        Logging.Log("BuyLPI", "Are buying the item [" + requiredItem.TypeName + "]", Logging.white);
                                        _requiredUnit = Convert.ToInt32(requiredItem.Quantity);
                                        _requiredItemId = requiredItem.TypeId;
                                        _States.CurrentBuyLPIState = BuyLPIState.OpenMarket;
                                        return;
                                    }
                                }
                            }
                    }

                    _States.CurrentBuyLPIState = BuyLPIState.AcceptOffer;

                    break;

                case BuyLPIState.OpenMarket:

                    if (marketWindow == null)
                    {
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                        break;
                    }

                    if (!marketWindow.IsReady)
                        break;

                    _States.CurrentBuyLPIState = BuyLPIState.BuyItems;

                    break;

                case BuyLPIState.BuyItems:

                    Logging.Log("BuyLPI", "Opening Market", Logging.white);

                    if (marketWindow != null && marketWindow.DetailTypeId != _requiredItemId)
                    {
                        marketWindow.LoadTypeId(_requiredItemId);
                        break;
                    }

                    if (marketWindow != null)
                    {
                        IEnumerable<DirectOrder> orders =
                            marketWindow.SellOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId);

                        DirectOrder order = orders.OrderBy(o => o.Price).FirstOrDefault();

                        if (order == null)
                        {
                            Logging.Log("BuyLPI", "No orders", Logging.white);
                            _States.CurrentBuyLPIState = BuyLPIState.Done;
                            break;
                        }

                        order.Buy(_requiredUnit, DirectOrderRange.Station);
                    }

                    Logging.Log("BuyLPI", "Buy Item", Logging.white);

                    _States.CurrentBuyLPIState = BuyLPIState.CheckPetition;

                    break;

                case BuyLPIState.AcceptOffer:

                    if (lpstore != null)
                    {
                        DirectLoyaltyPointOffer offer2 = lpstore.Offers.FirstOrDefault(o => o.TypeId == Item);

                        if (offer2 != null)
                        {
                            Logging.Log("BuyLPI", "Accepting [" + offer2.TypeName + "]", Logging.white);
                            offer2.AcceptOffer();
                        }
                    }
                    _States.CurrentBuyLPIState = BuyLPIState.Quatity;

                    break;

                case BuyLPIState.Quatity:

                    _loyaltyPointTimeout = DateTime.Now.AddSeconds(1);

                    Unit = Unit - 1;
                    if (Unit <= 0)
                    {
                        Logging.Log("BuyLPI", "Quantity limit reached", Logging.white);

                        _States.CurrentBuyLPIState = BuyLPIState.Done;
                        break;
                    }

                    _States.CurrentBuyLPIState = BuyLPIState.Begin;

                    break;
            }
        }
    }
}