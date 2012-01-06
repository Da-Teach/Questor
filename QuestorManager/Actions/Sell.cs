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


    class Sell
    {
        public StateSell State { get; set; }

        public int Item { get; set; }
        public int Unit { get; set; }

        private DateTime _lastAction;




        public void ProcessState()
        {
            var marketWindow = DirectEve.Instance.Windows.OfType<DirectMarketWindow>().FirstOrDefault();
            var hangar = DirectEve.Instance.GetItemHangar();
            var sellWindow = DirectEve.Instance.Windows.OfType<DirectMarketActionWindow>().FirstOrDefault(w => w.IsSellAction);

            switch (State)
            {
                case StateSell.Idle:
                case StateSell.Done:
                    break;

                case StateSell.Begin:
                    State = StateSell.StartQuickSell;
                    break;

                case StateSell.StartQuickSell:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 1)
                        break;
                    _lastAction = DateTime.Now;

                    if (hangar.Window == null)
                    {
                        // No, command it to open
                        DirectEve.Instance.ExecuteCommand(DirectCmd.OpenHangarFloor);
                        break;
                    }

                    if (!hangar.IsReady)
                        break;

                    var directItem = hangar.Items.FirstOrDefault(i => (i.TypeId == Item));
                    if (directItem == null)
                    {
                        Logging.Log("Sell: Item " + Item + " no longer exists in the hanger");
                        break;
                    }

                    // Update Quantity
                    if (Unit == 00)
                       Unit = directItem.Quantity;
                    

                       
                    Logging.Log("Sell: Starting QuickSell for " + Item);
                    if (!directItem.QuickSell())
                    {
                        _lastAction = DateTime.Now.AddSeconds(-5);

                        Logging.Log("Sell: QuickSell failed for " + Item + ", retrying in 5 seconds");
                        break;
                    }

                    State = StateSell.WaitForSellWindow;
                    break;

                case StateSell.WaitForSellWindow:


                    //if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != Item)
                    //    break;

                    // Mark as new execution
                    _lastAction = DateTime.Now;

                    Logging.Log("Sell: Inspecting sell order for " + Item);
                    State = StateSell.InspectOrder;
                    break;

                case StateSell.InspectOrder:
                    // Let the order window stay open for 2 seconds
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    if (!sellWindow.OrderId.HasValue || !sellWindow.Price.HasValue || !sellWindow.RemainingVolume.HasValue)
                    {
                        Logging.Log("Sell: No order available for " + Item);

                        sellWindow.Cancel();
                        State = StateSell.WaitingToFinishQuickSell;
                        break;
                    }

                    var price = sellWindow.Price.Value;

                    Logging.Log("Sell: Selling " + Unit + " of " + Item + " [Sell price: " + (price * Unit).ToString("#,##0.00") + "]");
                   
                    sellWindow.Accept();


                    _lastAction = DateTime.Now;
                    State = StateSell.WaitingToFinishQuickSell;
                    break;

                case StateSell.WaitingToFinishQuickSell:
                    if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != Item)
                    {
                        var modal = DirectEve.Instance.Windows.FirstOrDefault(w => w.IsModal);
                        if (modal != null)
                            modal.Close();

                        State = StateSell.Done;
                        break;
                    }
                    break;

            }


        }



    }
}
