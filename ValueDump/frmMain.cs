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
                InvTypesById.Add((int)element.Attribute("id"), new InvType(element));

            Items = new List<ItemCache>();
            ItemsToSell = new List<ItemCache>();

            DirectEve = new DirectEve();
            DirectEve.OnFrame += OnFrame;
        }

        private ItemCache _currentItem;
        private DateTime _lastExecute = DateTime.MinValue;

        private void OnFrame(object sender, EventArgs e)
        {
            if (State == ValueDumpState.Idle)
                return;

            var hangar = DirectEve.GetItemHangar();
            var sellWindow = DirectEve.Windows.OfType<DirectMarketActionWindow>().FirstOrDefault(w => w.IsSellAction);
            switch (State)
            {
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
                        Items.AddRange(hangarItems.Where(i => i.ItemId > 0 && i.MarketGroupId > 0 && i.Quantity > 0).Select(i => new ItemCache(i)));

                    State = ValueDumpState.UpdatePrices;
                    break;

                case ValueDumpState.UpdatePrices:
                    foreach (var item in Items)
                    {
                        InvType invType;
                        if (!InvTypesById.TryGetValue(item.TypeId, out invType))
                        {
                            Log("Unknown TypeId " + _currentItem.TypeId + " for " + _currentItem.Name);
                            continue;
                        }

                        item.InvType = invType;
                    }

                    State = ValueDumpState.Idle;
                    if (cbxSell.Checked)
                    {
                        // Copy the items to sell list
                        ItemsToSell.Clear();
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
                            Log("Not underselling item " + _currentItem.Name + " [" + _currentItem.InvType.MedianBuy.Value.ToString("#,##0.00") + "][" + price.ToString("#,##0.00") + "][" + perc.ToString("0%") + "]");

                            sellWindow.Cancel();
                            State = ValueDumpState.WaitingToFinishQuickSell;
                            break;
                        }
                    }

                    var quantity = (int)Math.Min(_currentItem.Quantity - _currentItem.QuantitySold, sellWindow.RemainingVolume.Value);

                    // Update quantity sold
                    _currentItem.QuantitySold += quantity;

                    // Update station price
                    if (!_currentItem.StationBuy.HasValue)
                        _currentItem.StationBuy = price;
                    _currentItem.StationBuy = (_currentItem.StationBuy + price)/2;

                    Log("Selling " + quantity + " of " + _currentItem.Name + " for " + (price * quantity).ToString("#,##0.00"));
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
    }
}
