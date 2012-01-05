//------------------------------------------------------------------------------
//  <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//    Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that 
//    applies to this source code. (a copy can also be found at: 
//    http://www.thehackerwithin.com/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------

namespace ValueDump
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using Questor;
    using System.Xml.Linq;
    using System.IO;
    using LavishScriptAPI;
    using DirectEve;

    public partial class frmMain : Form
    {
        private Dictionary<int, InvType> InvTypesById { get; set; }
        private List<ItemCache> Items { get; set; }
        private List<ItemCache> ItemsToSell { get; set; }
        private List<ItemCache> ItemsToRefine { get; set; }
        private ValueDumpState State { get; set; }
        private DirectEve DirectEve { get; set; }

        public string InvTypesPath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\InvTypes.xml";
            }
        }

        public void Log(string line)
        {
            InnerSpaceAPI.InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line));
        }

        public frmMain()
        {
            InitializeComponent();

            InvTypesById = new Dictionary<int, InvType>();
            var invTypes = XDocument.Load(InvTypesPath);
            foreach (var element in invTypes.Root.Elements("invtype"))
                InvTypesById.Add((int) element.Attribute("id"), new InvType(element));

            Items = new List<ItemCache>();
            ItemsToSell = new List<ItemCache>();
            ItemsToRefine = new List<ItemCache>();

            DirectEve = new DirectEve();
            DirectEve.OnFrame += OnFrame;
        }

        private InvType _currentMineral;
        private ItemCache _currentItem;
        private DateTime _lastExecute = DateTime.MinValue;

        private void OnFrame(object sender, EventArgs e)
        {
            if (State == ValueDumpState.Idle)
                return;

            var marketWindow = DirectEve.Windows.OfType<DirectMarketWindow>().FirstOrDefault();
            var hangar = DirectEve.GetItemHangar();
            var sellWindow = DirectEve.Windows.OfType<DirectMarketActionWindow>().FirstOrDefault(w => w.IsSellAction);
            var reprorcessingWindow = DirectEve.Windows.OfType<DirectReprocessingWindow>().FirstOrDefault();
            switch (State)
            {
                case ValueDumpState.CheckMineralPrices:
                    _currentMineral = InvTypesById.Values.FirstOrDefault(i => i.Id != 27029 && i.GroupId == 18 && i.LastUpdate < DateTime.Now.AddHours(-4));
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
                            DirectEve.ExecuteCommand(DirectCmd.OpenMarket);
                            _lastExecute = DateTime.Now;
                        }

                        return;
                    }

                    if (marketWindow.DetailTypeId != _currentMineral.Id)
                    {
                        if (DateTime.Now.Subtract(_lastExecute).TotalSeconds < 5)
                            return;

                        Log("Loading orders for " + _currentMineral.Name);

                        marketWindow.LoadTypeId(_currentMineral.Id);
                        _lastExecute = DateTime.Now;
                        return;
                    }

                    if (!marketWindow.BuyOrders.Any(o => o.StationId == DirectEve.Session.StationId))
                    {
                        _currentMineral.LastUpdate = DateTime.Now;

                        Log("No orders found for " + _currentMineral.Name);
                        State = ValueDumpState.CheckMineralPrices;
                    }

                    // Take top 5 orders, average the buy price and consider that median-buy (it's not really median buy but its what we want)
                    _currentMineral.MedianBuy = marketWindow.BuyOrders.Where(o => o.StationId == DirectEve.Session.StationId).OrderByDescending(o => o.Price).Take(5).Average(o => o.Price);
                    _currentMineral.LastUpdate = DateTime.Now;
                    State = ValueDumpState.CheckMineralPrices;

                    Log("Average price for " + _currentMineral.Name + " is " + _currentMineral.MedianBuy.Value.ToString("#,##0.00"));
                    break;

                case ValueDumpState.SaveMineralPrices:
                    Log("Saving InvItems.xml");

                    var xdoc = new XDocument(new XElement("invtypes"));
                    foreach (var type in InvTypesById.Values.OrderBy(i => i.Id))
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
                            Log("Opening hangar");
                            DirectEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                            _lastExecute = DateTime.Now;
                        }

                        return;
                    }

                    if (!hangar.IsReady)
                        return;

                    Log("Loading hangar items");

                    // Clear out the old
                    Items.Clear();
                    var hangarItems = hangar.Items;
                    if (hangarItems != null)
                        Items.AddRange(hangarItems.Where(i => i.ItemId > 0 && i.MarketGroupId > 0 && i.Quantity > 0).Select(i => new ItemCache(i, RefineCheckBox.Checked)));

                    State = ValueDumpState.UpdatePrices;
                    break;

                case ValueDumpState.UpdatePrices:
                    foreach (var item in Items)
                    {
                        InvType invType;
                        if (!InvTypesById.TryGetValue(item.TypeId, out invType))
                        {
                            Log("Unknown TypeId " + item.TypeId + " for " + item.Name);
                            continue;
                        }

                        item.InvType = invType;
                        foreach(var material in item.RefineOutput)
                        {
                            if (!InvTypesById.TryGetValue(material.TypeId, out invType))
                            {
                                Log("Unknown TypeId " + material.TypeId + " for " + material.Name);
                                continue;
                            }

                            material.InvType = invType;
                        }
                    }

                    State = ValueDumpState.Idle;
                    if (cbxSell.Checked)
                    {
                        // Copy the items to sell list
                        ItemsToSell.Clear();
                        ItemsToRefine.Clear();
                        if (cbxUndersell.Checked)
                            ItemsToSell.AddRange(Items.Where(i => i.InvType != null));
                        else
                            ItemsToSell.AddRange(Items.Where(i => i.InvType != null && i.InvType.MedianBuy.HasValue));
                        
                        State = ValueDumpState.NextItem;
                    }
                    break;

                case ValueDumpState.NextItem:
                    if (ItemsToSell.Count == 0)
                    {
                        if (ItemsToRefine.Count != 0)
                            State = ValueDumpState.RefineItems;
                        else
                            State = ValueDumpState.Idle;
                        break;
                    }

                    Log(ItemsToSell.Count + " items left to sell");

                    _currentItem = ItemsToSell[0];
                    ItemsToSell.RemoveAt(0);

                    // Dont sell containers
                    if (_currentItem.GroupId == 448 || _currentItem.GroupId == 649)
                    {
                        Log("Skipping " + _currentItem.Name);
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
                        Log("Item " + _currentItem.Name + " no longer exists in the hanger");
                        break;
                    }

                    // Update Quantity
                    _currentItem.QuantitySold = _currentItem.Quantity - directItem.Quantity;
                    
                    Log("Starting QuickSell for " + _currentItem.Name);
                    if (!directItem.QuickSell())
                    {
                        _lastExecute = DateTime.Now.AddSeconds(-5);

                        Log("QuickSell failed for " + _currentItem.Name + ", retrying in 5 seconds");
                        break;
                    }

                    State = ValueDumpState.WaitForSellWindow;
                    break;

                case ValueDumpState.WaitForSellWindow:
                    if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != _currentItem.Id)
                        break;

                    // Mark as new execution
                    _lastExecute = DateTime.Now;

                    Log("Inspecting sell order for " + _currentItem.Name);
                    State = ValueDumpState.InspectOrder;
                    break;

                case ValueDumpState.InspectOrder:
                    // Let the order window stay open for 2 seconds
                    if (DateTime.Now.Subtract(_lastExecute).TotalSeconds < 2)
                        break;

                    if (!sellWindow.OrderId.HasValue || !sellWindow.Price.HasValue || !sellWindow.RemainingVolume.HasValue)
                    {
                        Log("No order available for " + _currentItem.Name);

                        sellWindow.Cancel();
                        State = ValueDumpState.WaitingToFinishQuickSell;
                        break;
                    }

                    var price = sellWindow.Price.Value;
                    var quantity = (int)Math.Min(_currentItem.Quantity - _currentItem.QuantitySold, sellWindow.RemainingVolume.Value);
                    var totalPrice = quantity*price;

                    string otherPrices = " ";
                    if (_currentItem.InvType.MedianBuy.HasValue)
                        otherPrices += "[Median buy price: " + (_currentItem.InvType.MedianBuy.Value * quantity).ToString("#,##0.00") + "]";
                    else
                        otherPrices += "[No median buy price]";

                    if (RefineCheckBox.Checked)
                    {
                        var portions = quantity/_currentItem.PortionSize;
                        var refinePrice = _currentItem.RefineOutput.Any() ? _currentItem.RefineOutput.Sum(m => m.Quantity*m.InvType.MedianBuy ?? 0)*portions : 0;
                        refinePrice *= (double)RefineEfficiencyInput.Value / 100;

                        otherPrices += "[Refine price: " + refinePrice.ToString("#,##0.00") + "]";

                        if (refinePrice > totalPrice)
                        {
                            Log("Refining gives a better price for item " + _currentItem.Name + " [Refine price: " + refinePrice.ToString("#,##0.00") + "][Sell price: " + totalPrice.ToString("#,##0.00") + "]");

                            // Add it to the refine list
                            ItemsToRefine.Add(_currentItem);

                            sellWindow.Cancel();
                            State = ValueDumpState.WaitingToFinishQuickSell;
                            break;
                        }
                    }
                    
                    if (!cbxUndersell.Checked)
                    {
                        if (!_currentItem.InvType.MedianBuy.HasValue)
                        {
                            Log("No historical price available for " + _currentItem.Name);

                            sellWindow.Cancel();
                            State = ValueDumpState.WaitingToFinishQuickSell;
                            break;
                        }

                        var perc = price/_currentItem.InvType.MedianBuy.Value;
                        var total = _currentItem.InvType.MedianBuy.Value*_currentItem.Quantity;
                        // If percentage < 85% and total price > 1m isk then skip this item (we don't undersell)
                        if (perc < 0.85 && total > 1000000)
                        {
                            Log("Not underselling item " + _currentItem.Name + " [Median buy price: " + _currentItem.InvType.MedianBuy.Value.ToString("#,##0.00") + "][Sell price: " + price.ToString("#,##0.00") + "][" + perc.ToString("0%") + "]");

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
                    _currentItem.StationBuy = (_currentItem.StationBuy + price)/2;

                    Log("Selling " + quantity + " of " + _currentItem.Name + " [Sell price: " + (price * quantity).ToString("#,##0.00") + "]" + otherPrices);
                    sellWindow.Accept();

                    // Requeue to check again
                    if (_currentItem.QuantitySold < _currentItem.Quantity)
                        ItemsToSell.Add(_currentItem);

                    _lastExecute = DateTime.Now;
                    State = ValueDumpState.WaitingToFinishQuickSell;
                    break;

                case ValueDumpState.WaitingToFinishQuickSell:
                    if (sellWindow == null || !sellWindow.IsReady || sellWindow.Item.ItemId != _currentItem.Id)
                    {
                        var modal = DirectEve.Windows.FirstOrDefault(w => w.IsModal);
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
                            var refineItems = hangar.Items.Where(i => ItemsToRefine.Any(r => r.Id == i.ItemId));
                            DirectEve.ReprocessStationItems(refineItems);

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
                        State = ValueDumpState.Idle;
                    }
                    break;
            }

        }

        private void btnHangar_Click(object sender, EventArgs e)
        {
            State = ValueDumpState.GetItems;
            ProcessItems();
        }

        private void ProcessItems()
        {
            // Wait for the items to load
            Log("Waiting for items");
            while (State != ValueDumpState.Idle)
            {
                System.Threading.Thread.Sleep(50);
                Application.DoEvents();
            }

            lvItems.Items.Clear();
            foreach (var item in Items.Where(i => i.InvType != null).OrderByDescending(i => i.InvType.MedianBuy * i.Quantity))
            {
                var listItem = new ListViewItem(item.Name);
                listItem.SubItems.Add(string.Format("{0:#,##0}", item.Quantity));
                listItem.SubItems.Add(string.Format("{0:#,##0}", item.QuantitySold));
                listItem.SubItems.Add(string.Format("{0:#,##0}", item.InvType.MedianBuy));
                listItem.SubItems.Add(string.Format("{0:#,##0}", item.StationBuy));

                if (cbxSell.Checked)
                    listItem.SubItems.Add(string.Format("{0:#,##0}", item.StationBuy * item.QuantitySold));
                else
                    listItem.SubItems.Add(string.Format("{0:#,##0}", item.InvType.MedianBuy * item.Quantity));

                lvItems.Items.Add(listItem);
            }

            if (cbxSell.Checked)
            {
                tbTotalMedian.Text = string.Format("{0:#,##0}", Items.Where(i => i.InvType != null).Sum(i => i.InvType.MedianBuy * i.QuantitySold));
                tbTotalSold.Text = string.Format("{0:#,##0}", Items.Sum(i => i.StationBuy*i.QuantitySold));
            }
            else
            {
                tbTotalMedian.Text = string.Format("{0:#,##0}", Items.Where(i => i.InvType != null).Sum(i => i.InvType.MedianBuy * i.Quantity));
                tbTotalSold.Text = "";
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            DirectEve.Dispose();
            DirectEve = null;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            State = ValueDumpState.Idle;
        }

        private void UpdateMineralPricesButton_Click(object sender, EventArgs e)
        {
            State = ValueDumpState.CheckMineralPrices;
        }

        private void lvItems_ColumnClick(object sender, ColumnClickEventArgs e)
        {

            ListViewColumnSort oCompare = new ListViewColumnSort();

            if (lvItems.Sorting == SortOrder.Ascending)
                oCompare.Sorting = SortOrder.Descending;
            else
                oCompare.Sorting = SortOrder.Ascending;
            lvItems.Sorting = oCompare.Sorting;
            oCompare.ColumnIndex = e.Column;

            switch (e.Column)
            {
                case 1:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Cadena;
                    break;
                case 2:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 3:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 4:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 5:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 6:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;

            }

            lvItems.ListViewItemSorter = oCompare;

        }
    }
}
