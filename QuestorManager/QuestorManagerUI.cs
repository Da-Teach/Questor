// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace QuestorManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using System.IO;
    using System.Reflection;
    using DirectEve;
    using LavishScriptAPI;
    using Questor.Modules.Caching;
    using Questor.Modules.Logging;
    using Questor.Modules.Lookup;
    using Questor.Modules.States;
    using Questor.Modules.BackgroundTasks;
    using Questor.Modules.Actions;
    using Questor.Modules.Activities;
    using Actions;

    public partial class QuestorManagerUI : Form
    {
        public QuestormanagerState State { get; set; }

        private bool _changed; //false
        private bool _paused; //false
        private bool _start; //false
        private bool _lpstoreRe; //false
        private bool _requiredCom; //false

        private object _destination;
        private object _extrDestination;
        private int _jumps;
        private readonly DirectEve _directEve;

        private object _previousDestination;
        private int _previousJumps;
        private List<DirectSolarSystem> _solarSystems;
        private List<DirectStation> _stations;
        private List<DirectBookmark> _bookmarks;

        private List<ListItems> List { get; set; }

        public List<ItemCache> Items { get; set; }

        public List<ItemCache> ItemsToSellUnsorted { get; set; }

        public List<ItemCache> ItemsToSell { get; set; }

        public List<ItemCache> ItemsToRefine { get; set; }

        public Dictionary<int, InvType> InvTypesById { get; set; }

        private DateTime _lastPulse;

        private readonly Traveler _traveler;
        private readonly Grab _grab;
        private readonly Drop _drop;
        private readonly Buy _buy;
        private readonly Sell _sell;
        private readonly ValueDump _valuedump;
        private readonly BuyLPI _buylpi;
        private readonly ListItems _item;

        private DateTime _lastAction;

        public string CharacterName { get; set; }

        private string _selectHangar = "Local Hangar";

        public QuestorManagerUI()
        {
            InitializeComponent();

            _traveler = new Traveler();
            _grab = new Grab();
            _drop = new Drop();
            _buy = new Buy();
            _sell = new Sell();
            _valuedump = new ValueDump(this);
            //_valuedump = new ValueDump();
            _buylpi = new BuyLPI(this);
            //_buylpi = new BuyLPI();
            List = new List<ListItems>();
            Items = new List<ItemCache>();
            ItemsToSell = new List<ItemCache>();
            ItemsToSellUnsorted = new List<ItemCache>();
            ItemsToRefine = new List<ItemCache>();
            //Cache.Instance.DirectEve = _directEve;
            _directEve = new DirectEve();
            Cache.Instance.DirectEve = _directEve;
            XDocument invTypes = XDocument.Load(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\InvTypes.xml");

            InvTypesById = new Dictionary<int, InvType>();
            if (invTypes.Root != null)
                foreach (XElement element in invTypes.Root.Elements("invtype"))
                    InvTypesById.Add((int)element.Attribute("id"), new InvType(element));

            List.Clear();
            if (invTypes.Root != null)
                foreach (XElement element in invTypes.Root.Elements("invtype"))
                {
                    _item = new ListItems();
                    _item.Id = (int)element.Attribute("id");
                    _item.name = (string)element.Attribute("name");
                    List.Add(_item);
                }

            RefreshJobs();
            _directEve.OnFrame += OnFrame;
        }

        private void InitializeTraveler()
        {
            if (_solarSystems == null)
            {
                _solarSystems = Cache.Instance.DirectEve.SolarSystems.Values.OrderBy(s => s.Name).ToList();
                _changed = true;
            }

            if (_stations == null)
            {
                _stations = Cache.Instance.DirectEve.Stations.Values.OrderBy(s => s.Name).ToList();
                _changed = true;
            }

            if (_bookmarks == null)
            {
                // Dirty hack to load all category id's (needed because categoryid is lazy-loaded by the bookmarks call)
                Cache.Instance.DirectEve.Bookmarks.All(b => b.CategoryId != 0);
                _bookmarks = Cache.Instance.DirectEve.Bookmarks.OrderBy(b => b.Title).ToList();
                _changed = true;
            }
        }

        public void OnFrame(object sender, EventArgs e)
        {
            Cache.Instance.LastFrame = DateTime.Now;

            // Only pulse state changes every 1.5s
            if (DateTime.Now.Subtract(_lastPulse).TotalMilliseconds < (int)Time.QuestorPulse_milliseconds) //default: 1500ms
                return;
            _lastPulse = DateTime.Now;

            // Session is not ready yet, do not continue
            if (!Cache.Instance.DirectEve.Session.IsReady)
                return;

            if (Cache.Instance.DirectEve.Session.IsReady)
                Cache.Instance.LastSessionIsReady = DateTime.Now;

            // We are not in space or station, don't do shit yet!
            if (!Cache.Instance.InSpace && !Cache.Instance.InStation)
            {
                Cache.Instance.NextInSpaceorInStation = DateTime.Now.AddSeconds(12);
                Cache.Instance.LastSessionChange = DateTime.Now;
                return;
            }

            if (DateTime.Now < Cache.Instance.NextInSpaceorInStation)
                return;

            // New frame, invalidate old cache
            Cache.Instance.InvalidateCache();

            // Update settings (settings only load if character name changed)
            if (!Settings.Instance.Defaultsettingsloaded)
            {
                Settings.Instance.LoadSettings();
            }
            CharacterName = Cache.Instance.DirectEve.Me.Name;

            // Check 3D rendering
            if (Cache.Instance.DirectEve.Session.IsInSpace &&
                Cache.Instance.DirectEve.Rendering3D != !Settings.Instance.Disable3D)
                Cache.Instance.DirectEve.Rendering3D = !Settings.Instance.Disable3D;

            if (DateTime.Now.Subtract(Cache.Instance.LastupdateofSessionRunningTime).TotalSeconds <
                (int)Time.SessionRunningTimeUpdate_seconds)
            {
                Cache.Instance.SessionRunningTime =
                    (int)DateTime.Now.Subtract(Cache.Instance.QuestorStarted_DateTime).TotalMinutes;
                Cache.Instance.LastupdateofSessionRunningTime = DateTime.Now;
            }

            InitializeTraveler();

            if (_lpstoreRe)
                ResfreshLPI();

            if (_requiredCom)
                Required();

            Text = "Questor Manager [" + Cache.Instance.DirectEve.Me.Name + "]";

            if (_paused)
                return;

            switch (State)
            {
                case QuestormanagerState.Idle:

                    if (_start)
                    {
                        Logging.Log("QuestorManager", "Start", Logging.white);
                        State = QuestormanagerState.NextAction;
                    }

                    break;

                case QuestormanagerState.NextAction:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 3)
                        break;

                    if (LstTask.Items.Count <= 0)
                    {
                        Logging.Log("QuestorManager", "Finish", Logging.white);
                        LblStatus.Text = "Finish";
                        BttnStart.Text = "Start";
                        State = QuestormanagerState.Idle;
                        _start = false;
                        break;
                    }

                    if ("QuestorManager" == LstTask.Items[0].Text)
                    {
                        _destination = LstTask.Items[0].Tag;
                        State = QuestormanagerState.Traveler;
                        break;
                    }

                    if ("CmdLine" == LstTask.Items[0].Text)
                    {
                        LblStatus.Text = LstTask.Items[0].Text + ":-:" + LstTask.Items[0].SubItems[1].Text;
                        State = QuestormanagerState.CmdLine;
                        break;
                    }

                    if ("BuyLPI" == LstTask.Items[0].Text)
                    {
                        LblStatus.Text = LstTask.Items[0].Text + ":-:" + LstTask.Items[0].SubItems[1].Text;
                        State = QuestormanagerState.BuyLPI;
                        break;
                    }

                    if ("ValueDump" == LstTask.Items[0].Text)
                    {
                        LblStatus.Text = LstTask.Items[0].Text + ":-:" + LstTask.Items[0].SubItems[1].Text;
                        State = QuestormanagerState.ValueDump;
                        break;
                    }

                    if ("MakeShip" == LstTask.Items[0].Text)
                    {
                        LblStatus.Text = LstTask.Items[0].Text + ":-:" + LstTask.Items[0].SubItems[1].Text;
                        State = QuestormanagerState.MakeShip;
                        break;
                    }

                    if ("Drop" == LstTask.Items[0].Text)
                    {
                        LblStatus.Text = LstTask.Items[0].Text + ":-:" + LstTask.Items[0].SubItems[1].Text;
                        State = QuestormanagerState.Drop;
                        break;
                    }

                    if ("Grab" == LstTask.Items[0].Text)
                    {
                        LblStatus.Text = LstTask.Items[0].Text + ":-:" + LstTask.Items[0].SubItems[1].Text;
                        State = QuestormanagerState.Grab;
                        break;
                    }

                    if ("Buy" == LstTask.Items[0].Text)
                    {
                        LblStatus.Text = LstTask.Items[0].Text + ":-:" + LstTask.Items[0].SubItems[1].Text;
                        State = QuestormanagerState.Buy;
                        break;
                    }

                    if ("Sell" == LstTask.Items[0].Text)
                    {
                        LblStatus.Text = LstTask.Items[0].Text + ":-:" + LstTask.Items[0].SubItems[1].Text;
                        State = QuestormanagerState.Sell;
                        break;
                    }

                    break;

                case QuestormanagerState.CmdLine:

                    if (Settings.Instance.UseInnerspace)
                    {
                        Logging.Log("QuestorManager", "CmdLine: " + LstTask.Items[0].SubItems[1].Text, Logging.white);
                        LavishScript.ExecuteCommand(LstTask.Items[0].SubItems[1].Text);
                    }
                    else
                    {
                        Logging.Log("QuestorManager", "CmdLine: " + LstTask.Items[0].SubItems[1].Text, Logging.white);
                        Logging.Log("QuestorManager", "CmdLine: Error: command skipped: UseInnerspace is false", Logging.white);
                    }
                    LstTask.Items.Remove(LstTask.Items[0]);
                    _lastAction = DateTime.Now;
                    State = QuestormanagerState.NextAction;

                    break;

                case QuestormanagerState.BuyLPI:

                    if (_States.CurrentBuyLPIState == BuyLPIState.Idle)
                    {
                        _buylpi.Item = Convert.ToInt32(LstTask.Items[0].Tag);
                        _buylpi.Unit = Convert.ToInt32(LstTask.Items[0].SubItems[2].Text);
                        Logging.Log("QuestorManager", "BuyLPI: Begin", Logging.white);
                        _States.CurrentBuyLPIState = BuyLPIState.Begin;
                    }

                    _buylpi.ProcessState();

                    if (_States.CurrentBuyLPIState == BuyLPIState.Done)
                    {
                        Logging.Log("QuestorManager", "BuyLPI: Done", Logging.white);
                        _States.CurrentBuyLPIState = BuyLPIState.Idle;
                        LstTask.Items.Remove(LstTask.Items[0]);
                        _lastAction = DateTime.Now;
                        State = QuestormanagerState.NextAction;
                    }

                    break;

                case QuestormanagerState.ValueDump:

                    if (chkUpdateMineral.Checked)
                    {
                        chkUpdateMineral.Checked = false;
                        _States.CurrentValueDumpState = ValueDumpState.CheckMineralPrices;
                    }

                    if (_States.CurrentValueDumpState == ValueDumpState.Idle)
                    {
                        Logging.Log("QuestorManager", "ValueDump: Begin", Logging.white);
                        _States.CurrentValueDumpState = ValueDumpState.Begin;
                    }

                    _valuedump.ProcessState();

                    if (_States.CurrentValueDumpState == ValueDumpState.Done)
                    {
                        Logging.Log("QuestorManager", "ValueDump: Done", Logging.white);
                        _States.CurrentValueDumpState = ValueDumpState.Idle;
                        ProcessItems();
                        LstTask.Items.Remove(LstTask.Items[0]);
                        _lastAction = DateTime.Now;
                        State = QuestormanagerState.NextAction;
                    }

                    break;

                case QuestormanagerState.MakeShip:

                    if (!Cache.Instance.OpenShipsHangar("QuestorManager")) break;

                    if (DateTime.Now > _lastAction)
                    {
                        List<DirectItem> ships = Cache.Instance.DirectEve.GetShipHangar().Items;
                        foreach (DirectItem ship in ships.Where(ship => ship.GivenName != null && ship.GivenName == txtNameShip.Text))
                        {
                            Logging.Log("QuestorManager", "MakeShip: Making [" + ship.GivenName + "] active", Logging.white);

                            ship.ActivateShip();
                            LstTask.Items.Remove(LstTask.Items[0]);
                            _lastAction = DateTime.Now;
                            State = QuestormanagerState.NextAction;
                            break;
                        }
                    }

                    break;

                case QuestormanagerState.Buy:

                    if (_States.CurrentBuyState == BuyState.Idle)
                    {
                        _buy.Item = Convert.ToInt32(LstTask.Items[0].Tag);
                        _buy.Unit = Convert.ToInt32(LstTask.Items[0].SubItems[2].Text);
                        Logging.Log("QuestorManager", "Buy: Begin", Logging.white);
                        _States.CurrentBuyState = BuyState.Begin;
                    }

                    _buy.ProcessState();

                    if (_States.CurrentBuyState == BuyState.Done)
                    {
                        Logging.Log("QuestorManager", "Buy: Done", Logging.white);
                        _States.CurrentBuyState = BuyState.Idle;
                        LstTask.Items.Remove(LstTask.Items[0]);
                        _lastAction = DateTime.Now;
                        State = QuestormanagerState.NextAction;
                    }

                    break;

                case QuestormanagerState.Sell:

                    _sell.Item = Convert.ToInt32(LstTask.Items[0].Tag);
                    _sell.Unit = Convert.ToInt32(LstTask.Items[0].SubItems[2].Text);

                    if (_States.CurrentSellState == SellState.Idle)
                    {
                        Logging.Log("QuestorManager", "Sell: Begin", Logging.white);
                        _States.CurrentSellState = SellState.Begin;
                    }

                    _sell.ProcessState();

                    if (_States.CurrentSellState == SellState.Done)
                    {
                        Logging.Log("QuestorManager", "Sell: Done", Logging.white);
                        _States.CurrentSellState = SellState.Idle;
                        LstTask.Items.Remove(LstTask.Items[0]);
                        _lastAction = DateTime.Now;
                        State = QuestormanagerState.NextAction;
                    }
                    break;

                case QuestormanagerState.Drop:

                    _drop.Item = Convert.ToInt32(LstTask.Items[0].Tag);
                    _drop.Unit = Convert.ToInt32(LstTask.Items[0].SubItems[2].Text);
                    _drop.Hangar = LstTask.Items[0].SubItems[3].Text;

                    if (_States.CurrentDropState == DropState.Idle)
                    {
                        Logging.Log("QuestorManager", "Drop: Begin", Logging.white);
                        _States.CurrentDropState = DropState.Begin;
                    }

                    _drop.ProcessState();

                    if (_States.CurrentDropState == DropState.Done)
                    {
                        Logging.Log("QuestorManager", "Drop: Done", Logging.white);
                        _States.CurrentDropState = DropState.Idle;
                        LstTask.Items.Remove(LstTask.Items[0]);
                        _lastAction = DateTime.Now;
                        State = QuestormanagerState.NextAction;
                    }

                    break;

                case QuestormanagerState.Grab:

                    _grab.Item = Convert.ToInt32(LstTask.Items[0].Tag);
                    _grab.Unit = Convert.ToInt32(LstTask.Items[0].SubItems[2].Text);
                    _grab.Hangar = LstTask.Items[0].SubItems[3].Text;

                    if (_States.CurrentGrabState == GrabState.Idle)
                    {
                        Logging.Log("QuestorManager", "Grab: Begin", Logging.white);
                        _States.CurrentGrabState = GrabState.Begin;
                    }

                    _grab.ProcessState();

                    if (_States.CurrentGrabState == GrabState.Done)
                    {
                        Logging.Log("QuestorManager", "Grab: Done", Logging.white);
                        _States.CurrentGrabState = GrabState.Idle;
                        LstTask.Items.Remove(LstTask.Items[0]);
                        _lastAction = DateTime.Now;
                        State = QuestormanagerState.NextAction;
                    }

                    break;

                case QuestormanagerState.Traveler:

                    // We are warping
                    if (Cache.Instance.DirectEve.Session.IsInSpace && Cache.Instance.DirectEve.ActiveShip.Entity != null && Cache.Instance.DirectEve.ActiveShip.Entity.IsWarping)
                        return;

                    TravelerDestination travelerDestination = _traveler.Destination;
                    if (_destination == null)
                        travelerDestination = null;

                    if (_destination is DirectBookmark)
                    {
                        if (!(travelerDestination is BookmarkDestination) || (travelerDestination as BookmarkDestination).BookmarkId != (_destination as DirectBookmark).BookmarkId)
                            travelerDestination = new BookmarkDestination(_destination as DirectBookmark);
                    }

                    if (_destination is DirectSolarSystem)
                    {
                        if (!(travelerDestination is SolarSystemDestination) || (travelerDestination as SolarSystemDestination).SolarSystemId != (_destination as DirectSolarSystem).Id)
                            travelerDestination = new SolarSystemDestination((_destination as DirectSolarSystem).Id);
                    }

                    if (_destination is DirectStation)
                    {
                        if (!(travelerDestination is StationDestination) || (travelerDestination as StationDestination).StationId != (_destination as DirectStation).Id)
                            travelerDestination = new StationDestination((_destination as DirectStation).Id);
                    }

                    // Check to see if destination changed, since changing it will set the traveler to Idle
                    if (_traveler.Destination != travelerDestination)
                        _traveler.Destination = travelerDestination;

                    _traveler.ProcessState();

                    // Record number of jumps
                    _jumps = Cache.Instance.DirectEve.Navigation.GetDestinationPath().Count;

                    // Arrived at destination
                    if (_destination != null && _States.CurrentTravelerState == TravelerState.AtDestination)
                    {
                        Logging.Log("QuestorManager", "Arrived at destination", Logging.white);

                        _traveler.Destination = null;
                        _destination = null;
                        LstTask.Items.Remove(LstTask.Items[0]);
                        _lastAction = DateTime.Now;
                        State = QuestormanagerState.NextAction;
                    }

                    // An error occurred, reset traveler
                    if (_States.CurrentTravelerState == TravelerState.Error)
                    {
                        if (_traveler.Destination != null)
                            Logging.Log("QuestorManager", "Stopped traveling, QuestorManager threw an error...", Logging.white);

                        _destination = null;
                        _traveler.Destination = null;
                        _start = false;
                        State = QuestormanagerState.Idle;
                    }
                    break;
            }
        }

        private void RefreshBookmarksClick(object sender, EventArgs e)
        {
            _bookmarks = null;
        }

        private ListViewItem[] Filter<T>(IEnumerable<string> search, IEnumerable<T> list, Func<T, string> getTitle, Func<T, string> getType)
        {
            if (list == null)
                return new ListViewItem[0];

            List<ListViewItem> result = new List<ListViewItem>();
            foreach (T item in list)
            {
                string name = getTitle(item);
                if (string.IsNullOrEmpty(name))
                    continue;

                bool found = search != null && search.All(t => name.IndexOf(t, StringComparison.OrdinalIgnoreCase) > -1);
                if (!found)
                    continue;

                ListViewItem listViewItem = new ListViewItem(name);
                listViewItem.SubItems.Add(getType(item));
                listViewItem.Tag = item;
                result.Add(listViewItem);
            }
            return result.ToArray();
        }

        private void UpdateSearchResultsTick(object sender, EventArgs e)
        {
            if (_previousDestination != _destination || _jumps != _previousJumps)
            {
                _previousDestination = _destination;
                _previousJumps = _jumps;

                string name = string.Empty;
                if (_destination is DirectBookmark)
                    name = ((DirectBookmark)_destination).Title;
                if (_destination is DirectRegion)
                    name = ((DirectRegion)_destination).Name;
                if (_destination is DirectConstellation)
                    name = ((DirectConstellation)_destination).Name;
                if (_destination is DirectSolarSystem)
                    name = ((DirectSolarSystem)_destination).Name;
                if (_destination is DirectStation)
                    name = ((DirectStation)_destination).Name;

                if (!string.IsNullOrEmpty(name))
                    name = @"Traveling to " + name + " (" + _jumps + " jumps)";

                LblStatus.Text = name;
            }

            if (!_changed)
                return;
            _changed = false;

            string[] search = SearchTextBox.Text.Split(' ');

            SearchResults.BeginUpdate();
            try
            {
                SearchResults.Items.Clear();
                SearchResults.Items.AddRange(Filter(search, _bookmarks, b => b.Title, b => "Bookmark (" + ((CategoryID)b.CategoryId) + ")"));
                SearchResults.Items.AddRange(Filter(search, _solarSystems, s => s.Name, b => "Solar System"));
                SearchResults.Items.AddRange(Filter(search, _stations, s => s.Name, b => "Station"));

                // Automatically select the only item
                if (SearchResults.Items.Count == 1)
                    SearchResults.Items[0].Selected = true;
            }
            finally
            {
                SearchResults.EndUpdate();
            }
        }

        private void BttnStart_Click(object sender, EventArgs e)
        {
            if (BttnStart.Text == "Start")
            {
                BttnStart.Text = "Stop";
                State = QuestormanagerState.Idle;
                _start = true;
            }
            else
            {
                BttnStart.Text = "Start";
                State = QuestormanagerState.Idle;
                _States.CurrentBuyState = BuyState.Idle;
                _States.CurrentDropState = DropState.Idle;
                _States.CurrentGrabState = GrabState.Idle;
                _States.CurrentSellState = SellState.Idle;
                _States.CurrentValueDumpState = ValueDumpState.Idle;
                _States.CurrentBuyLPIState = BuyLPIState.Idle;
                _start = false;
            }
        }

        private void SearchTextBoxTextChanged(object sender, EventArgs e)
        {
            _changed = true;
        }

        private void BttnAddTraveler_Click(object sender, EventArgs e)
        {
            ListViewItem listItem = new ListViewItem("QuestorManager");
            listItem.SubItems.Add(SearchResults.SelectedItems[0].Text);
            listItem.Tag = SearchResults.SelectedItems[0].Tag;
            listItem.SubItems.Add(" ");
            listItem.SubItems.Add(" ");
            LstTask.Items.Add(listItem);
        }

        private void BttnTaskForItemClick1(object sender, EventArgs e)
        {
            if (cmbMode.Text == "Select Mode")
                return;

            foreach (ListViewItem item in LstItems.CheckedItems)
            {
                ListViewItem listItem = new ListViewItem(cmbMode.Text);
                listItem.SubItems.Add(item.Text);
                listItem.Tag = item.SubItems[1].Text;
                listItem.SubItems.Add(txtUnit.Text);
                listItem.SubItems.Add(_selectHangar);
                LstTask.Items.Add(listItem);
            }
        }

        private void MoveListViewItem(ref ListView lv, bool moveUp)
        {
            string cache;

            int selIdx = lv.SelectedItems[0].Index;
            if (moveUp)
            {
                // ignore moveup of row(0)
                if (selIdx == 0)
                    return;
                if (_start)
                    if (selIdx == 1)
                        return;
                // move the subitems for the previous row
                // to cache to make room for the selected row
                for (int i = 0; i < lv.Items[selIdx].SubItems.Count; i++)
                {
                    cache = lv.Items[selIdx - 1].SubItems[i].Text;
                    lv.Items[selIdx - 1].SubItems[i].Text = lv.Items[selIdx].SubItems[i].Text;
                    lv.Items[selIdx].SubItems[i].Text = cache;
                }
                object cache1 = lv.Items[selIdx - 1].Tag;
                lv.Items[selIdx - 1].Tag = lv.Items[selIdx].Tag;
                lv.Items[selIdx].Tag = cache1;

                lv.Items[selIdx - 1].Selected = true;
                lv.Refresh();
                lv.Focus();
            }
            else
            {
                // ignore movedown of last item
                if (selIdx == lv.Items.Count - 1)
                    return;
                if (_start)
                    if (selIdx == 0)
                        return;
                // move the subitems for the next row
                // to cache so we can move the selected row down
                for (int i = 0; i < lv.Items[selIdx].SubItems.Count; i++)
                {
                    cache = lv.Items[selIdx + 1].SubItems[i].Text;
                    lv.Items[selIdx + 1].SubItems[i].Text = lv.Items[selIdx].SubItems[i].Text;
                    lv.Items[selIdx].SubItems[i].Text = cache;
                }
                object cache1 = lv.Items[selIdx + 1].Tag;
                lv.Items[selIdx + 1].Tag = lv.Items[selIdx].Tag;
                lv.Items[selIdx].Tag = cache1;

                lv.Items[selIdx + 1].Selected = true;
                lv.Refresh();
                lv.Focus();
            }
        }

        private void BttnUpClick(object sender, EventArgs e)
        {
            MoveListViewItem(ref LstTask, true);
        }

        private void BttnDownClick(object sender, EventArgs e)
        {
            MoveListViewItem(ref LstTask, false);
        }

        private void BttnDeleteClick(object sender, EventArgs e)
        {
            if (_start)
                if (LstTask.SelectedItems[0].Index == 0)
                    return;

            while (LstTask.SelectedItems.Count > 0)
            {
                LstTask.Items.Remove(LstTask.SelectedItems[0]);
            }
        }

        private void TxtSearchItemsTextChanged(object sender, EventArgs e)
        {
            LstItems.Items.Clear();

            if (txtSearchItems.Text.Length > 4)
            {
                string[] search = txtSearchItems.Text.Split(' ');
                foreach (ListItems item in List)
                {
                    string name = item.name;
                    if (string.IsNullOrEmpty(name))
                        continue;

                    bool found = search.All(t => name.IndexOf(t, StringComparison.OrdinalIgnoreCase) > -1);
                    if (!found)
                        continue;

                    ListViewItem listItem1 = new ListViewItem(item.name);
                    listItem1.SubItems.Add(Convert.ToString(item.Id));
                    LstItems.Items.Add(listItem1);
                }
            }
        }

        private void BttnTaskAllItemsClick(object sender, EventArgs e)
        {
            if (cmbAllMode.Text == "Select Mode")
                return;

            ListViewItem listItem = new ListViewItem(cmbAllMode.Text);
            listItem.SubItems.Add("All items");
            listItem.Tag = 00;
            listItem.SubItems.Add("00");
            listItem.SubItems.Add(_selectHangar);
            LstTask.Items.Add(listItem);
        }

        private void BttnTaskMakeShipClick(object sender, EventArgs e)
        {
            if (txtNameShip.Text == "")
                return;

            ListViewItem listItem = new ListViewItem("MakeShip");
            listItem.SubItems.Add(txtNameShip.Text);
            listItem.SubItems.Add(" ");
            listItem.SubItems.Add(" ");
            LstTask.Items.Add(listItem);
        }

        private void ChkPauseCheckedChanged(object sender, EventArgs e)
        {
            if (chkPause.Checked == true)
                _paused = true;
            if (chkPause.Checked == false)
                _paused = false;
        }

        private void RbttnLocalCheckedChanged(object sender, EventArgs e)
        {
            if (rbttnLocal.Checked == true)
                _selectHangar = rbttnLocal.Text;
        }

        private void RbttnShipCheckedChanged(object sender, EventArgs e)
        {
            if (rbttnShip.Checked == true)
                _selectHangar = rbttnShip.Text;
        }

        private void RbttnCorpCheckedChanged(object sender, EventArgs e)
        {
            if (rbttnCorp.Checked == true)
            {
                txtNameCorp.Enabled = true;
                _selectHangar = txtNameCorp.Text;
            }
            else if (rbttnCorp.Checked == false)
            {
                txtNameCorp.Enabled = false;
            }
        }

        private void TxtNameCorpTextChanged(object sender, EventArgs e)
        {
            _selectHangar = txtNameCorp.Text;
        }

        private void ProcessItems()
        {
            lvItems.Items.Clear();
            foreach (ItemCache item in Items.Where(i => i.InvType != null).OrderByDescending(i => i.InvType.MedianBuy * i.Quantity))
            {
                ListViewItem listItem = new ListViewItem(item.Name);
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
                tbTotalSold.Text = string.Format("{0:#,##0}", Items.Sum(i => i.StationBuy * i.QuantitySold));
            }
            else
            {
                tbTotalMedian.Text = string.Format("{0:#,##0}", Items.Where(i => i.InvType != null).Sum(i => i.InvType.MedianBuy * i.Quantity));
                tbTotalSold.Text = "";
            }
        }

        private void FrmMainFormClosed(object sender, FormClosedEventArgs e)
        {
            _directEve.Dispose();
        }

        private void UpdateMineralPricesButton_Click(object sender, EventArgs e)
        {
            _States.CurrentValueDumpState = ValueDumpState.CheckMineralPrices;
        }

        private void LvItemsColumnClick(object sender, ColumnClickEventArgs e)
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

        private void BttnTaskValueDumpClick(object sender, EventArgs e)
        {
            ListViewItem listItem = new ListViewItem("ValueDump");
            listItem.SubItems.Add("All Items");
            listItem.SubItems.Add(" ");
            listItem.SubItems.Add(" ");
            LstTask.Items.Add(listItem);
        }

        private void BttnSaveTaskClick(object sender, EventArgs e)
        {
            if (cmbXML.Text == "Select Jobs")
            {
                MessageBox.Show("Write name to save");
                return;
            }

            string fic = Path.Combine(Settings.Instance.Path, cmbXML.Text + ".jobs");
            string strXml = "<Jobs>";

            for (int o = 0; o < LstTask.Items.Count; o++)
                strXml += "<Job typeJob='" + LstTask.Items[o].SubItems[0].Text + "' Name='" + LstTask.Items[o].SubItems[1].Text + "' Unit='" + LstTask.Items[o].SubItems[2].Text + "' Hangar='" + LstTask.Items[o].SubItems[3].Text + "' Tag='" + LstTask.Items[o].Tag + "' />";

            strXml += "</Jobs>";

            XElement xml = XElement.Parse(strXml);
            XDocument fileXml = new XDocument(xml);
            fileXml.Save(fic);

            RefreshJobs();
        }

        private void RefreshJobs()
        {
            cmbXML.Items.Clear();

            DirectoryInfo o = new System.IO.DirectoryInfo(Settings.Instance.Path);
            FileInfo[] myfiles = null;

            myfiles = o.GetFiles("*.jobs");
            for (int y = 0; y <= myfiles.Length - 1; y++)
            {
                string[] file = myfiles[y].Name.Split('.');
                cmbXML.Items.Add(file[0]);
            }
        }

        private void ExtractTraveler(string nameDestination)
        {
            if (_extrDestination == null)
            {
                foreach (DirectStation item in _stations)
                {
                    if (nameDestination == item.Name)
                        _extrDestination = item;
                }
            }
            else if (_extrDestination == null)
            {
                foreach (DirectSolarSystem item in _solarSystems)
                {
                    if (nameDestination == item.Name)
                        _extrDestination = item;
                }
            }
            else if (_extrDestination == null)
            {
                foreach (DirectBookmark item in _bookmarks)
                {
                    if (nameDestination == item.Title)
                        _extrDestination = item;
                }
            }
        }

        private void ReadXML(string fic)
        {
            XElement xml = XDocument.Load(fic).Root;

            LstTask.Items.Clear();
            if (xml != null)
            {
                foreach (XElement job in xml.Elements("Job"))
                {
                    ListViewItem listItem = new ListViewItem((string)job.Attribute("typeJob"));
                    listItem.SubItems.Add((string)job.Attribute("Name"));
                    listItem.SubItems.Add((string)job.Attribute("Unit"));
                    listItem.SubItems.Add((string)job.Attribute("Hangar"));
                    if (((string)job.Attribute("typeJob")) == "QuestorManager")
                    {
                        ExtractTraveler(((string)job.Attribute("Name")));
                        listItem.Tag = _extrDestination;
                        _extrDestination = null;
                    }
                    else
                        listItem.Tag = (string)job.Attribute("Tag");

                    LstTask.Items.Add(listItem);
                }
            }
        }

        private void CmbXMLSelectedIndexChanged(object sender, EventArgs e)
        {
            string fic = Path.Combine(Settings.Instance.Path, cmbXML.Text + ".jobs");
            ReadXML(fic);
        }

        public void ResfreshLPI()
        {
            _lpstoreRe = false;
            DirectLoyaltyPointStoreWindow lpstore = _directEve.Windows.OfType<DirectLoyaltyPointStoreWindow>().FirstOrDefault();
            if (lpstore == null)
            {
                Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenLpstore);

                return;
            }

            lstbuyLPI.Items.Clear();

            string[] search = txtSearchLPI.Text.Split(' ');
            foreach (DirectLoyaltyPointOffer offer in lpstore.Offers)
            {
                string name = offer.TypeName;
                if (string.IsNullOrEmpty(name))
                    continue;

                bool found = search.All(t => name.IndexOf(t, StringComparison.OrdinalIgnoreCase) > -1);
                if (!found)
                    continue;

                ListViewItem listItem = new ListViewItem(offer.TypeName);
                listItem.SubItems.Add(Convert.ToString(offer.TypeId));
                lstbuyLPI.Items.Add(listItem);
            }
        }

        public void Required()
        {
            DirectLoyaltyPointStoreWindow lpstore = Cache.Instance.DirectEve.Windows.OfType<DirectLoyaltyPointStoreWindow>().FirstOrDefault();
            XDocument invTypes = XDocument.Load(Path.GetDirectoryName(Settings.Instance.Path) + "\\InvTypes.xml");
            if (invTypes.Root != null)
            {
                IEnumerable<XElement> invType = invTypes.Root.Elements("invtype").ToList();

                if (lpstore == null)
                {
                    Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenLpstore);

                    return;
                }

                foreach (DirectLoyaltyPointOffer offer in lpstore.Offers)
                {
                    if (offer.TypeName == lstbuyLPI.SelectedItems[0].Text)
                    {
                        double totalISK = 0;
                        lstItemsRequiered.Items.Clear();

                        if (offer.RequiredItems.Count > 0)
                        {
                            foreach (DirectLoyaltyPointOfferRequiredItem requiredItem in offer.RequiredItems)
                            {
                                foreach (XElement item in invType)
                                {
                                    if ((string)item.Attribute("name") == requiredItem.TypeName)
                                    {
                                        double medianbuy = (double?)item.Attribute("medianbuy") ?? 0;
                                        ListViewItem listItemRequired = new ListViewItem(requiredItem.TypeName);
                                        listItemRequired.SubItems.Add(Convert.ToString(requiredItem.Quantity));
                                        listItemRequired.SubItems.Add(string.Format("{0:#,#0.00}", medianbuy));
                                        lstItemsRequiered.Items.Add(listItemRequired);
                                        totalISK = totalISK + (Convert.ToDouble(requiredItem.Quantity) * medianbuy);
                                    }
                                }
                            }
                        }

                        lblitemisk.Text = string.Format("{0:#,#0.00}", totalISK);
                        totalISK = totalISK + Convert.ToDouble(offer.IskCost);
                        lbliskLPI.Text = string.Format("{0:#,#0.00}", offer.IskCost);
                        lblTotal.Text = string.Format("{0:#,#0.00}", totalISK);
                        lblLP.Text = string.Format("{0:#,#}", offer.LoyaltyPointCost);
                    }
                }
            }
            _requiredCom = false;
        }

        private void BttnRefreshLPIClick(object sender, EventArgs e)
        {
            _lpstoreRe = true;
        }

        private void LstbuyLPISelectedIndexChanged(object sender, EventArgs e)
        {
            _requiredCom = true;
        }

        private void BttnTaskLPIClick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lstbuyLPI.CheckedItems)
            {
                ListViewItem listItem = new ListViewItem("BuyLPI");
                listItem.SubItems.Add(item.Text);
                listItem.Tag = item.SubItems[1].Text;
                listItem.SubItems.Add(txtUnitLPI.Text);
                listItem.SubItems.Add(" ");
                LstTask.Items.Add(listItem);
            }
        }

        private void BttnTaskLineCmdClick(object sender, EventArgs e)
        {
            ListViewItem listItem = new ListViewItem("CmdLine");
            listItem.SubItems.Add(txtCmdLine.Text);
            listItem.SubItems.Add(" ");
            listItem.SubItems.Add(" ");
            LstTask.Items.Add(listItem);
        }

        private void TxtSearchLPIKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                _lpstoreRe = true;
            }
        }

        private void SearchResults_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}