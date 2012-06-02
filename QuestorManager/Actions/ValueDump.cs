//------------------------------------------------------------------------------
//  <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//    Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that
//    applies to this source code. (a copy can also be found at:
//    http://www.thehackerwithin.com/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using DirectEve;
using System.Collections.Generic;
using Questor.Modules.Lookup;
using Questor.Modules.Logging;
using Questor.Modules.States;
using Questor.Modules.Caching;

namespace QuestorManager.Actions
{
    public class ValueDump
    {
        private QuestorManagerUI _form;
        readonly Random _random = new Random();

        private InvType _currentMineral;
        private ItemCache _currentItem;
        private DateTime _lastExecute = DateTime.MinValue;
        private bool _valueProcess; //false

        public string InvTypesPath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\InvTypes.xml";
            }
        }

        public ValueDump(QuestorManagerUI form1)
        {
            _form = form1;
        }

        public void ProcessState()
        {
            XDocument invIgnore = XDocument.Load(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\InvIgnore.xml"); //items to ignore
            DirectMarketWindow marketWindow = Cache.Instance.DirectEve.Windows.OfType<DirectMarketWindow>().FirstOrDefault();
            DirectContainer hangar = Cache.Instance.DirectEve.GetItemHangar();
            DirectMarketActionWindow sellWindow = Cache.Instance.DirectEve.Windows.OfType<DirectMarketActionWindow>().FirstOrDefault(w => w.IsSellAction);
            DirectReprocessingWindow reprorcessingWindow = Cache.Instance.DirectEve.Windows.OfType<DirectReprocessingWindow>().FirstOrDefault();
            bool block;

            int randomNumber = _random.Next(2, 4);

            switch (_States.CurrentValueDumpState)
            {
                case ValueDumpState.Idle:
                case ValueDumpState.Done:
                    break;

                case ValueDumpState.Begin:
                    if (_form.RefineCheckBox.Checked && _form.cbxSell.Checked)
                    {
                        _form.cbxSell.Checked = false;
                        _valueProcess = true;
                        _States.CurrentValueDumpState = ValueDumpState.GetItems;
                    }
                    else if (_form.RefineCheckBox.Checked && _valueProcess)
                    {
                        _form.RefineCheckBox.Checked = false;
                        _form.cbxSell.Checked = true;
                        _valueProcess = false;
                        _States.CurrentValueDumpState = ValueDumpState.GetItems;
                    }
                    else
                        _States.CurrentValueDumpState = ValueDumpState.GetItems;
                    break;

                case ValueDumpState.CheckMineralPrices:
                    _currentMineral = _form.InvTypesById.Values.FirstOrDefault(i => i.Id != 27029 && i.GroupId == 18 && i.LastUpdate < DateTime.Now.AddHours(-4));
                    if (_currentMineral == null)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > 5)
                        {
                            _States.CurrentValueDumpState = ValueDumpState.SaveMineralPrices;
                            if (marketWindow != null)
                                marketWindow.Close();
                        }
                    }
                    else
                        _States.CurrentValueDumpState = ValueDumpState.GetMineralPrice;

                    break;

                case ValueDumpState.GetMineralPrice:
                    if (marketWindow == null)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > 5)
                        {
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                            _lastExecute = DateTime.Now;
                        }

                        return;
                    }

                    if (marketWindow.DetailTypeId != _currentMineral.Id)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds < 5)
                            return;

                        Logging.Log("ValueDump", "Loading orders for " + _currentMineral.Name, Logging.white);

                        marketWindow.LoadTypeId(_currentMineral.Id);
                        _lastExecute = DateTime.Now;
                        return;
                    }

                    if (!marketWindow.BuyOrders.Any(o => o.StationId == Cache.Instance.DirectEve.Session.StationId))
                    {
                        _currentMineral.LastUpdate = DateTime.Now;

                        Logging.Log("ValueDump", "No orders found for " + _currentMineral.Name, Logging.white);
                        _States.CurrentValueDumpState = ValueDumpState.CheckMineralPrices;
                    }

                    // Take top 5 orders, average the buy price and consider that median-buy (it's not really median buy but its what we want)
                    _currentMineral.MedianBuy = marketWindow.BuyOrders.Where(o => o.StationId == Cache.Instance.DirectEve.Session.StationId).OrderByDescending(o => o.Price).Take(5).Average(o => o.Price);
                    _currentMineral.LastUpdate = DateTime.Now;
                    _States.CurrentValueDumpState = ValueDumpState.CheckMineralPrices;

                    Logging.Log("ValueDump", "Average price for " + _currentMineral.Name + " is " + _currentMineral.MedianBuy.Value.ToString("#,##0.00"), Logging.white);
                    break;

                case ValueDumpState.SaveMineralPrices:
                    Logging.Log("ValueDump", "Saving InvTypes.xml", Logging.white);

                    XDocument xdoc = new XDocument(new XElement("invtypes"));
                    foreach (InvType type in _form.InvTypesById.Values.OrderBy(i => i.Id))
                        if (xdoc.Root != null) xdoc.Root.Add(type.Save());
                    xdoc.Save(InvTypesPath);

                    _States.CurrentValueDumpState = ValueDumpState.Idle;
                    break;

                case ValueDumpState.GetItems:
                    if (hangar.Window == null)
                    {
                        // No, command it to open
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > 5)
                        {
                            Logging.Log("ValueDump", "Opening hangar", Logging.white);
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                            _lastExecute = DateTime.Now;
                        }

                        return;
                    }

                    if (!hangar.IsReady)
                        return;

                    Logging.Log("ValueDump", "Loading hangar items", Logging.white);

                    // Clear out the old
                    _form.Items.Clear();
                    List<DirectItem> hangarItems = hangar.Items;
                    if (hangarItems != null)
                        _form.Items.AddRange(hangarItems.Where(i => i.ItemId > 0 && i.MarketGroupId > 0 && i.Quantity > 0).Select(i => new ItemCache(i, _form.RefineCheckBox.Checked)));

                    _States.CurrentValueDumpState = ValueDumpState.UpdatePrices;
                    break;

                case ValueDumpState.UpdatePrices:
                    _form.ItemsToSell.Clear();
                    _form.ItemsToRefine.Clear();
                    _form.ItemsToSellUnsorted.Clear();

                    if (_form.cbxSell.Checked)
                    {
                        if (_form.cbxUndersell.Checked)
                            _form.ItemsToSellUnsorted.AddRange(_form.Items.Where(i => i.InvType != null));
                        else
                            _form.ItemsToSellUnsorted.AddRange(_form.Items.Where(i => i.InvType != null && i.InvType.MinSell.HasValue));

                        _form.ItemsToSell = _form.ItemsToSellUnsorted.OrderBy(i => i.Name).ToList();
                        _States.CurrentValueDumpState = ValueDumpState.NextItem;
                    }
                    else if (_form.RefineCheckBox.Checked)
                    {
                        _form.ItemsToSellUnsorted.AddRange(_form.Items.Where(i => i.InvType != null && i.InvType.MaxBuy.HasValue));
                        _form.ItemsToSell = _form.ItemsToSellUnsorted.OrderBy(i => i.Name).ToList();
                        _States.CurrentValueDumpState = ValueDumpState.NextItem;
                    }
                    else
                        _States.CurrentValueDumpState = ValueDumpState.Done;

                    break;

                case ValueDumpState.NextItem:

                    if (_form.ItemsToSell.Count == 0)
                    {
                        if (_form.ItemsToRefine.Count != 0)
                            _States.CurrentValueDumpState = ValueDumpState.RefineItems;
                        else
                            _States.CurrentValueDumpState = ValueDumpState.Done;
                        break;
                    }
                    block = false;
                    if (!_form.RefineCheckBox.Checked)
                        Logging.Log("ValueDump", _form.ItemsToSell.Count + " items left to sell", Logging.white);

                    _currentItem = _form.ItemsToSell[0];
                    _form.ItemsToSell.RemoveAt(0);

                    // Do not sell containers
                    if (_currentItem.GroupID == 448 || _currentItem.GroupID == 649)
                    {
                        Logging.Log("ValueDump", "Skipping " + _currentItem.Name, Logging.white);
                        break;
                    }
                    // Do not sell items in invignore.xml
                    if (invIgnore.Root != null)
                        foreach (XElement element in invIgnore.Root.Elements("invtype"))
                        {
                            if (_currentItem.TypeId == (int)element.Attribute("id"))
                            {
                                Logging.Log("ValueDump", "Skipping (block list) " + _currentItem.Name, Logging.white);
                                block = true;
                                break;
                            }
                        }
                    if (block)
                        break;

                    _States.CurrentValueDumpState = ValueDumpState.StartQuickSell;
                    break;

                case ValueDumpState.StartQuickSell:
                    if ((DateTime.Now.Subtract(_lastExecute).TotalSeconds < randomNumber) && _form.cbxSell.Checked)
                        break;
                    _lastExecute = DateTime.Now;

                    DirectItem directItem = hangar.Items.FirstOrDefault(i => i.ItemId == _currentItem.Id);
                    if (directItem == null)
                    {
                        Logging.Log("ValueDump", "Item " + _currentItem.Name + " no longer exists in the hanger", Logging.white);
                        break;
                    }

                    // Update Quantity
                    _currentItem.QuantitySold = _currentItem.Quantity - directItem.Quantity;

                    if (_form.cbxSell.Checked)
                    {
                        Logging.Log("ValueDump", "Starting QuickSell for " + _currentItem.Name, Logging.white);
                        if (!directItem.QuickSell())
                        {
                            _lastExecute = DateTime.Now.AddSeconds(-5);

                            Logging.Log("ValueDump", "QuickSell failed for " + _currentItem.Name + ", retrying in 5 seconds", Logging.white);
                            break;
                        }

                        _States.CurrentValueDumpState = ValueDumpState.WaitForSellWindow;
                    }
                    else
                    {
                        _States.CurrentValueDumpState = ValueDumpState.InspectRefinery;
                    }
                    break;

                case ValueDumpState.WaitForSellWindow:
                    if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != _currentItem.Id)
                        break;

                    // Mark as new execution
                    _lastExecute = DateTime.Now;

                    Logging.Log("ValueDump", "Inspecting sell order for " + _currentItem.Name, Logging.white);
                    _States.CurrentValueDumpState = ValueDumpState.InspectOrder;
                    break;

                case ValueDumpState.InspectOrder:
                    // Let the order window stay open for random number
                    if (DateTime.Now.Subtract(_lastExecute).TotalSeconds < randomNumber)
                        break;

                    if (sellWindow != null && (!sellWindow.OrderId.HasValue || !sellWindow.Price.HasValue || !sellWindow.RemainingVolume.HasValue))
                    {
                        Logging.Log("ValueDump", "No order available for " + _currentItem.Name, Logging.white);

                        sellWindow.Cancel();
                        _States.CurrentValueDumpState = ValueDumpState.WaitingToFinishQuickSell;
                        break;
                    }

                    if (sellWindow != null)
                    {
                        double price = sellWindow.Price.Value;
                        int quantity = (int)Math.Min(_currentItem.Quantity - _currentItem.QuantitySold, sellWindow.RemainingVolume.Value);
                        double totalPrice = quantity * price;

                        const string otherPrices = " ";

                        if (!_form.cbxUndersell.Checked)
                        {
                            if (_currentItem.InvType.MinSell != null)
                            {
                                double perc = _currentItem.InvType.MinSell.Value / price;
                                double total = _currentItem.InvType.MinSell.Value * _currentItem.Quantity;
                                // If percentage >= 130% and total price >= 1m isk then skip this item (we don't undersell)
                                if (perc >= 1.4 && ((total - totalPrice) >= 2000000))
                                {
                                    Logging.Log("ValueDump", "Not underselling item " + _currentItem.Name + " [Min sell price: " + _currentItem.InvType.MinSell.Value.ToString("#,##0.00") + "][Sell price: " + price.ToString("#,##0.00") + "][" + perc.ToString("0%") + "]", Logging.white);

                                    sellWindow.Cancel();
                                    _States.CurrentValueDumpState = ValueDumpState.WaitingToFinishQuickSell;
                                    break;
                                }
                            }
                        }

                        // Update quantity sold
                        _currentItem.QuantitySold += quantity;

                        // Update station price
                        if (!_currentItem.StationBuy.HasValue)
                            _currentItem.StationBuy = price;
                        _currentItem.StationBuy = (_currentItem.StationBuy + price) / 2;

                        Logging.Log("ValueDump", "Selling " + quantity + " of " + _currentItem.Name + " [Sell price: " + (price * quantity).ToString("#,##0.00") + "]" + otherPrices, Logging.white);
                    }
                    if (sellWindow != null) sellWindow.Accept();

                    // Re-queue to check again
                    if (_currentItem.QuantitySold < _currentItem.Quantity)
                        _form.ItemsToSell.Add(_currentItem);

                    _lastExecute = DateTime.Now;
                    _States.CurrentValueDumpState = ValueDumpState.WaitingToFinishQuickSell;
                    break;

                case ValueDumpState.InspectRefinery:

                    if (_currentItem.InvType.MaxBuy != null)
                    {
                        double priceR = _currentItem.InvType.MaxBuy.Value;
                        int quantityR = _currentItem.Quantity;
                        double totalPriceR = quantityR * priceR;
                        int portions = quantityR / _currentItem.PortionSize;
                        double refinePrice = _currentItem.RefineOutput.Any() ? _currentItem.RefineOutput.Sum(m => m.Quantity * m.InvType.MaxBuy ?? 0) * portions : 0;
                        refinePrice *= (double)_form.RefineEfficiencyInput.Value / 100;

                        if (refinePrice > totalPriceR || totalPriceR <= 1500000 || _currentItem.TypeId == 30497)
                        {
                            Logging.Log("ValueDump", "Refining gives a better price for item " + _currentItem.Name + " [Refine price: " + refinePrice.ToString("#,##0.00") + "][Sell price: " + totalPriceR.ToString("#,##0.00") + "]", Logging.white);
                            // Add it to the refine list
                            _form.ItemsToRefine.Add(_currentItem);
                        }
                    }
                    /*else
                    {
                        Logging.Log("Selling gives a better price for item " + _currentItem.Name + " [Refine price: " + refinePrice.ToString("#,##0.00") + "][Sell price: " + totalPrice_r.ToString("#,##0.00") + "]");
                    }*/

                    _lastExecute = DateTime.Now;
                    _States.CurrentValueDumpState = ValueDumpState.NextItem;

                    break;

                case ValueDumpState.WaitingToFinishQuickSell:
                    if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != _currentItem.Id)
                    {
                        DirectWindow modal = Cache.Instance.DirectEve.Windows.FirstOrDefault(w => w.IsModal);
                        if (modal != null)
                            modal.Close();

                        _States.CurrentValueDumpState = ValueDumpState.NextItem;
                        break;
                    }
                    break;

                case ValueDumpState.RefineItems:

                    if (reprorcessingWindow == null)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > randomNumber)
                        {
                            IEnumerable<DirectItem> refineItems = hangar.Items.Where(i => _form.ItemsToRefine.Any(r => r.Id == i.ItemId));
                            Cache.Instance.DirectEve.ReprocessStationItems(refineItems);

                            _lastExecute = DateTime.Now;
                        }
                        return;
                    }

                    if (reprorcessingWindow.NeedsQuote)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > randomNumber)
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
                        _lastExecute = DateTime.Now;
                        Logging.Log("Valuedump", "Waiting 17 second", Logging.white);
                        _States.CurrentValueDumpState = ValueDumpState.WaitingToBack;
                    }
                    break;

                case ValueDumpState.WaitingToBack:
                    if (DateTime.Now.Subtract(_lastExecute).TotalSeconds > 17 && _valueProcess)
                    {
                        if (_valueProcess)
                            _States.CurrentValueDumpState = ValueDumpState.Begin;
                        else
                            _States.CurrentValueDumpState = ValueDumpState.Done;
                    }
                    break;
            }
        }
    }
}