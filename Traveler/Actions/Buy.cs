namespace Traveler.Actions
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DirectEve;
    using DirectEve = global::Traveler.Common.DirectEve;
    using global::Traveler.Common;
    using global::Traveler.Domains;
    using global::Traveler.Module;


    class Buy
    {
        public StateBuy State { get; set; }

        public int Item { get; set; }
        public int Unit { get; set; }

        private DateTime _lastAction;

        private bool ReturnBuy;

       
        
       

        public void ProcessState()
        {
            var marketWindow = DirectEve.Instance.Windows.OfType<DirectMarketWindow>().FirstOrDefault();
            
            switch (State)
            {
                case StateBuy.Idle:
                case StateBuy.Done:
                    break;

                case StateBuy.Begin:

                    // Close the market window if there is one
                    if (marketWindow != null)
                        marketWindow.Close();
                    State = StateBuy.OpenMarket;
                    break;

                case StateBuy.OpenMarket:

                    
                    // Close the market window if there is one
                    //if (marketWindow != null)
                    //    marketWindow.Close();

                     if (marketWindow == null)
                     {
                         DirectEve.Instance.ExecuteCommand(DirectCmd.OpenMarket);
                         break;
                     }


                     if (!marketWindow.IsReady)
                         break;


                    Logging.Log("Buy: Opening Market");
                    State = StateBuy.LoadItem;

                    break;

                case StateBuy.LoadItem:

                    _lastAction = DateTime.Now;

                    if (marketWindow.DetailTypeId != Item)
                    {
                        marketWindow.LoadTypeId(Item);
                        State = StateBuy.BuyItem;

                        break;
                    }
                    

                    break;

                case StateBuy.BuyItem:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                        
                        var orders = marketWindow.SellOrders.Where(o => o.StationId == DirectEve.Instance.Session.StationId);

                        var order = orders.OrderBy(o => o.Price).FirstOrDefault();
                        if (order != null)
                        {
                            // Calculate how much kernite we still need
                            if (order.VolumeEntered >= Unit)
                            {
                                order.Buy(Unit, DirectOrderRange.Station);
                                State = StateBuy.WaitForItems;
                            }
                            else
                            {
                                order.Buy(Unit, DirectOrderRange.Station);
                                 Unit = Unit - order.VolumeEntered;
                                 Logging.Log("Missing " + Convert.ToString(Unit) + " units");
                                ReturnBuy = true;
                                State = StateBuy.WaitForItems;
                            }

                        }

                    break;

                case StateBuy.WaitForItems:
                    // Wait 5 seconds after moving
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    // Close the market window if there is one
                    if (marketWindow != null)
                        marketWindow.Close();

                    if (ReturnBuy == true)
                    {
                        Logging.Log("Buy: Return Buy");
                        ReturnBuy = false;
                        State = StateBuy.OpenMarket;
                        break;
                    }

                        Logging.Log("Buy: Done");
                        State = StateBuy.Done;



                    break;

            }


        }



    }
}
