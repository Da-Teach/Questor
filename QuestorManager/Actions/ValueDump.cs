//------------------------------------------------------------------------------
//  <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//    Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that 
//    applies to this source code. (a copy can also be found at: 
//    http://www.thehackerwithin.com/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------

namespace QuestorManager.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using System.IO;
    using LavishScriptAPI;
    using DirectEve;
    using DirectEve = global::QuestorManager.Common.DirectEve;
    using global::QuestorManager.Common;
    using global::QuestorManager.Domains;
    using global::QuestorManager.Module;

    class ValueDump
    {

        public ValueDumpState State { get; set; }

        private MainForm _form;

        private InvType _currentMineral;
        private ItemCache _currentItem;
        private DateTime _lastExecute = DateTime.MinValue;

 

        public string InvTypesPath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\InvTypes.xml";
            }
        }

        public ValueDump(MainForm form1)
        {
            _form = form1;
        }

        public void ProcessState()
        {


            var marketWindow = DirectEve.Instance.Windows.OfType<DirectMarketWindow>().FirstOrDefault();
            var hangar = DirectEve.Instance.GetItemHangar();
            var sellWindow = DirectEve.Instance.Windows.OfType<DirectMarketActionWindow>().FirstOrDefault(w => w.IsSellAction);
            var reprorcessingWindow = DirectEve.Instance.Windows.OfType<DirectReprocessingWindow>().FirstOrDefault();

            switch (State)
            {

                case ValueDumpState.Idle:
                case ValueDumpState.Done:
                    break;

                case ValueDumpState.Begin:
                    State = ValueDumpState.GetItems;
                    break;

                case ValueDumpState.CheckMineralPrices:
                    _currentMineral = _form.InvTypesById.Values.FirstOrDefault(i => i.Id != 27029 && i.GroupId == 18 && i.LastUpdate < DateTime.Now.AddHours(-4));
                    if (_currentMineral == null)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > 5)
                        {
                            State = ValueDumpState.SaveMineralPrices;
                            if (marketWindow != null)
                                marketWindow.Close();
                        }
                    }
                    else
                        State = ValueDumpState.GetMineralPrice;
                    
                    break;

                case ValueDumpState.GetMineralPrice:
                    if (marketWindow == null)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > 5)
                        {
                            DirectEve.Instance.ExecuteCommand(DirectCmd.OpenMarket);
                            _lastExecute = DateTime.Now;
                        }

                        return;
                    }

                    if (marketWindow.DetailTypeId != _currentMineral.Id)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds < 5)
                            return;

                        Logging.Log("ValueDump: Loading orders for " + _currentMineral.Name);

                        marketWindow.LoadTypeId(_currentMineral.Id);
                        _lastExecute = DateTime.Now;
                        return;
                    }

                    if (!marketWindow.BuyOrders.Any(o => o.StationId == DirectEve.Instance.Session.StationId))
                    {
                        _currentMineral.LastUpdate = DateTime.Now;

                        Logging.Log("ValueDump: No orders found for " + _currentMineral.Name);
                        State = ValueDumpState.CheckMineralPrices;
                    }

                    // Take top 5 orders, average the buy price and consider that median-buy (it's not really median buy but its what we want)
                    _currentMineral.MedianBuy = marketWindow.BuyOrders.Where(o => o.StationId == DirectEve.Instance.Session.StationId).OrderByDescending(o => o.Price).Take(5).Average(o => o.Price);
                    _currentMineral.LastUpdate = DateTime.Now;
                    State = ValueDumpState.CheckMineralPrices;

                    Logging.Log("ValueDump: Average price for " + _currentMineral.Name + " is " + _currentMineral.MedianBuy.Value.ToString("#,##0.00"));
                    break;

                case ValueDumpState.SaveMineralPrices:
                    Logging.Log("ValueDump: Saving InvItems.xml");

                    var xdoc = new XDocument(new XElement("invtypes"));
                    foreach (var type in _form.InvTypesById.Values.OrderBy(i => i.Id))
                        xdoc.Root.Add(type.Save());
                    xdoc.Save(InvTypesPath);

                    
                    State = ValueDumpState.Idle;
                    break;

                case ValueDumpState.GetItems:


                    if (hangar.Window == null)
                    {
                        // No, command it to open
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > 5)
                        {
                            Logging.Log("ValueDump: Opening hangar");
                            DirectEve.Instance.ExecuteCommand(DirectCmd.OpenHangarFloor);
                            _lastExecute = DateTime.Now;
                        }

                        return;
                    }

                    if (!hangar.IsReady)
                        return;

                    Logging.Log("ValueDump: Loading hangar items");

                    // Clear out the old
                    _form.Items.Clear();
                    var hangarItems = hangar.Items;
                    if (hangarItems != null)
                        _form.Items.AddRange(hangarItems.Where(i => i.ItemId > 0 && i.MarketGroupId > 0 && i.Quantity > 0).Select(i => new ItemCache(i, _form.RefineCheckBox.Checked)));


                    State = ValueDumpState.UpdatePrices;
                    break;

                case ValueDumpState.UpdatePrices:
                    foreach (var item in _form.Items)
                    {
                        InvType invType;
                        if (!_form.InvTypesById.TryGetValue(item.TypeId, out invType))
                        {
                            Logging.Log("ValueDump: Unknown TypeId " + item.TypeId + " for " + item.Name);
                            continue;
                        }

                        item.InvType = invType;
                        foreach (var material in item.RefineOutput)
                        {
                            if (!_form.InvTypesById.TryGetValue(material.TypeId, out invType))
                            {
                                Logging.Log("ValueDump: Unknown TypeId " + material.TypeId + " for " + material.Name);
                                continue;
                            }

                            material.InvType = invType;
                        }
                    }


                    if (_form.cbxSell.Checked)
                    {
                        // Copy the items to sell list
                        _form.ItemsToSell.Clear();
                        _form.ItemsToRefine.Clear();
                        if (_form.cbxUndersell.Checked)
                            _form.ItemsToSell.AddRange(_form.Items.Where(i => i.InvType != null));
                        else
                            _form.ItemsToSell.AddRange(_form.Items.Where(i => i.InvType != null && i.InvType.MedianBuy.HasValue));

                        State = ValueDumpState.NextItem;
                    }
                    else
                        State = ValueDumpState.Done;

                    break;

                case ValueDumpState.NextItem:

                    if (_form.ItemsToSell.Count == 0)
                    {
                        if (_form.ItemsToRefine.Count != 0)
                            State = ValueDumpState.RefineItems;
                        else
                            State = ValueDumpState.Done;
                        break;
                    }

                    Logging.Log("ValueDump: " + _form.ItemsToSell.Count + " items left to sell");

                    _currentItem = _form.ItemsToSell[0];
                    _form.ItemsToSell.RemoveAt(0);

                    // Dont sell containers
                    if (_currentItem.GroupId == 448 || _currentItem.GroupId == 649)
                    {
                        Logging.Log("ValueDump: Skipping " + _currentItem.Name);
                        break;
                    }

                    State = ValueDumpState.StartQuickSell;
                    break;

                case ValueDumpState.StartQuickSell:
                    if (DateTime.Now.Subtract(_lastExecute).TotalSeconds < 1)
                        break;
                    _lastExecute = DateTime.Now;

                    var directItem = hangar.Items.FirstOrDefault(i => i.ItemId == _currentItem.Id);
                    if (directItem == null)
                    {
                        Logging.Log("ValueDump: Item " + _currentItem.Name + " no longer exists in the hanger");
                        break;
                    }

                    // Update Quantity
                    _currentItem.QuantitySold = _currentItem.Quantity - directItem.Quantity;

                    Logging.Log("ValueDump: Starting QuickSell for " + _currentItem.Name);
                    if (!directItem.QuickSell())
                    {
                        _lastExecute = DateTime.Now.AddSeconds(-5);

                        Logging.Log("ValueDump: QuickSell failed for " + _currentItem.Name + ", retrying in 5 seconds");
                        break;
                    }

                    State = ValueDumpState.WaitForSellWindow;
                    break;

                case ValueDumpState.WaitForSellWindow:
                    if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != _currentItem.Id)
                        break;

                    // Mark as new execution
                    _lastExecute = DateTime.Now;

                    Logging.Log("ValueDump: Inspecting sell order for " + _currentItem.Name);
                    State = ValueDumpState.InspectOrder;
                    break;

                case ValueDumpState.InspectOrder:
                    // Let the order window stay open for 2 seconds
                    if (DateTime.Now.Subtract(_lastExecute).TotalSeconds < 2)
                        break;

                    if (!sellWindow.OrderId.HasValue || !sellWindow.Price.HasValue || !sellWindow.RemainingVolume.HasValue)
                    {
                        Logging.Log("ValueDump: No order available for " + _currentItem.Name);

                        sellWindow.Cancel();
                        State = ValueDumpState.WaitingToFinishQuickSell;
                        break;
                    }

                    var price = sellWindow.Price.Value;
                    var quantity = (int)Math.Min(_currentItem.Quantity - _currentItem.QuantitySold, sellWindow.RemainingVolume.Value);
                    var totalPrice = quantity * price;

                    string otherPrices = " ";
                    if (_currentItem.InvType.MedianBuy.HasValue)
                        otherPrices += "[Median buy price: " + (_currentItem.InvType.MedianBuy.Value * quantity).ToString("#,##0.00") + "]";
                    else
                        otherPrices += "[No median buy price]";

                    if (_form.RefineCheckBox.Checked)
                    {
                        var portions = quantity / _currentItem.PortionSize;
                        var refinePrice = _currentItem.RefineOutput.Any() ? _currentItem.RefineOutput.Sum(m => m.Quantity * m.InvType.MedianBuy ?? 0) * portions : 0;
                        refinePrice *= (double)_form.RefineEfficiencyInput.Value / 100;

                        otherPrices += "[Refine price: " + refinePrice.ToString("#,##0.00") + "]";

                        if (refinePrice > totalPrice)
                        {
                            Logging.Log("ValueDump: Refining gives a better price for item " + _currentItem.Name + " [Refine price: " + refinePrice.ToString("#,##0.00") + "][Sell price: " + totalPrice.ToString("#,##0.00") + "]");

                            // Add it to the refine list
                            _form.ItemsToRefine.Add(_currentItem);

                            sellWindow.Cancel();
                            State = ValueDumpState.WaitingToFinishQuickSell;
                            break;
                        }
                    }

                    if (!_form.cbxUndersell.Checked)
                    {
                        if (!_currentItem.InvType.MedianBuy.HasValue)
                        {
                            Logging.Log("ValueDump: No historical price available for " + _currentItem.Name);

                            sellWindow.Cancel();
                            State = ValueDumpState.WaitingToFinishQuickSell;
                            break;
                        }

                        var perc = price / _currentItem.InvType.MedianBuy.Value;
                        var total = _currentItem.InvType.MedianBuy.Value * _currentItem.Quantity;
                        // If percentage < 85% and total price > 1m isk then skip this item (we don't undersell)
                        if (perc < 0.85 && total > 1000000)
                        {
                            Logging.Log("ValueDump: Not underselling item " + _currentItem.Name + " [Median buy price: " + _currentItem.InvType.MedianBuy.Value.ToString("#,##0.00") + "][Sell price: " + price.ToString("#,##0.00") + "][" + perc.ToString("0%") + "]");

                            sellWindow.Cancel();
                            State = ValueDumpState.WaitingToFinishQuickSell;
                            break;
                        }
                    }

                    // Update quantity sold
                    _currentItem.QuantitySold += quantity;

                    // Update station price
                    if (!_currentItem.StationBuy.HasValue)
                        _currentItem.StationBuy = price;
                    _currentItem.StationBuy = (_currentItem.StationBuy + price) / 2;

                    Logging.Log("ValueDump: Selling " + quantity + " of " + _currentItem.Name + " [Sell price: " + (price * quantity).ToString("#,##0.00") + "]" + otherPrices);
                    sellWindow.Accept();

                    // Requeue to check again
                    if (_currentItem.QuantitySold < _currentItem.Quantity)
                        _form.ItemsToSell.Add(_currentItem);

                    _lastExecute = DateTime.Now;
                    State = ValueDumpState.WaitingToFinishQuickSell;
                    break;

                case ValueDumpState.WaitingToFinishQuickSell:
                    if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != _currentItem.Id)
                    {
                        var modal = DirectEve.Instance.Windows.FirstOrDefault(w => w.IsModal);
                        if (modal != null)
                            modal.Close();

                        State = ValueDumpState.NextItem;
                        break;
                    }
                    break;

                case ValueDumpState.RefineItems:

                    if (reprorcessingWindow == null)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > 5)
                        {

                            var refineItems = hangar.Items.Where(i => _form.ItemsToRefine.Any(r => r.Id == i.ItemId));
                            DirectEve.Instance.ReprocessStationItems(refineItems);

                            _lastExecute = DateTime.Now;
                        }
                        return;
                    }

                    if (reprorcessingWindow.NeedsQuote)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > 5)
                        {
                            reprorcessingWindow.GetQuotes();
                            _lastExecute = DateTime.Now;
                        }

                        return;
                    }

                    // Wait till we have a quote
                    if (reprorcessingWindow.Quotes.Count == 0)
                    {
                        _lastExecute = DateTime.Now;
                        return;
                    }

                    // Wait another 5 seconds to view the quote and then reprocess the stuff
                    if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > 5)
                    {
                        // TODO: We should wait for the items to appear in our hangar and then sell them...
                        reprorcessingWindow.Reprocess();
                        State = ValueDumpState.Done;
                    }
                    break;
            }

        }

    }
}
