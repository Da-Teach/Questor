// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace Traveler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml.Linq;
    using System.IO;
    using DirectEve;
    using global::Traveler.Common;
    using global::Traveler.Domains;
    using global::Traveler.Module;
    using global::Traveler.Actions;
    using DirectEve = global::Traveler.Common.DirectEve;

    public partial class MainForm : Form
    {

        public State State { get; set; }

        private bool _changed;
        private bool Paused=false;
        private bool Start=false;


        private object _destination;
        private int _jumps;

        private object _previousDestination;
        private int _previousJumps;
        private List<DirectSolarSystem> _solarSystems;
        private List<DirectStation> _stations;
        private List<DirectBookmark> _bookmarks;
        private List<ListItems> _list { get; set; }

        private Traveler _traveler;
        private Grab _grab;
        private Drop _drop;
        private Buy _buy;
        private Sell _sell;
        private ListItems item;

        private string SelectHangar = "Local Hangar";



        public MainForm()
        {
            InitializeComponent();

            _traveler = new Traveler();
            _grab = new Grab();
            _drop = new Drop();
            _buy = new Buy();
            _sell = new Sell();
            _list = new List<ListItems>();
            

           
            var invTypes = XDocument.Load(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\InvTypes.xml");
           
            _list.Clear();
            foreach (var element in invTypes.Root.Elements("invtype"))
            {
                item = new ListItems();
                item.Id = (int)element.Attribute("id");
                item.name = (string)element.Attribute("name");
                _list.Add(item);
            }
            
            

            DirectEve.Instance.OnFrame += OnFrame;
        }

        private void InitializeTraveler()
        {
            if (_solarSystems == null)
            {
                _solarSystems = DirectEve.Instance.SolarSystems.Values.OrderBy(s => s.Name).ToList();
                _changed = true;
            }

            if (_stations == null)
            {
                _stations = DirectEve.Instance.Stations.Values.OrderBy(s => s.Name).ToList();
                _changed = true;
            }

            if (_bookmarks == null)
            {
                // Dirty hack to load all category id's (needed because categoryid is lazy-loaded by the bookmarks call)
                DirectEve.Instance.Bookmarks.All(b => b.CategoryId != 0);
                _bookmarks = DirectEve.Instance.Bookmarks.OrderBy(b => b.Title).ToList();
                _changed = true;
            }
        }

        public void OnFrame(object sender, EventArgs e)
        {

            if (!DirectEve.Instance.Session.IsReady)
                return;

            InitializeTraveler();

            if (Paused)
                return;

            switch (State)
            {

                case State.Idle:

                    if (Start)
                    {
                        Logging.Log("Traveler: Start");
                        State = State.NextAction;
                    }

                     break;

                case State.NextAction:

                     if (LstTask.Items.Count <= 0)
                     {
                         Logging.Log("Traveler: Finish");
                         LblStatus.Text = "Finish";
                         BttnStart.Text = "Start";
                         State = State.Idle;
                         Start = false;
                         break;
                     }


                     if ("Traveler" == LstTask.Items[0].Text)
                     {
                         _destination = LstTask.Items[0].Tag;
                         State = State.Traveler;
                         break;
                     }

                     if ("MakeShip" == LstTask.Items[0].Text)
                     {
                         LblStatus.Text = LstTask.Items[0].Text + ": Item:" + LstTask.Items[0].SubItems[1].Text;
                         State = State.MakeShip;
                         break;
                     }

                     if ("Drop" == LstTask.Items[0].Text)
                     {
                         LblStatus.Text = LstTask.Items[0].Text + ": Item:" + LstTask.Items[0].SubItems[1].Text;
                         State = State.Drop;
                         break;
                     }

                     if ("Grab" == LstTask.Items[0].Text)
                     {
                         LblStatus.Text = LstTask.Items[0].Text + ": Item:" + LstTask.Items[0].SubItems[1].Text;
                         State = State.Grab;
                         break;
                     }

                     if ("Buy" == LstTask.Items[0].Text)
                     {
                         LblStatus.Text = LstTask.Items[0].Text + ": Item:" + LstTask.Items[0].SubItems[1].Text;
                         State = State.Buy;
                         break;
                     }

                     if ("Sell" == LstTask.Items[0].Text)
                     {
                         LblStatus.Text = LstTask.Items[0].Text + ": Item:" + LstTask.Items[0].SubItems[1].Text;
                         State = State.Sell;
                         break;
                     }

                     break;

                case State.MakeShip:

                    var shipHangar = DirectEve.Instance.GetShipHangar();
                    if (shipHangar.Window == null)
                    {
                        // No, command it to open
                        DirectEve.Instance.ExecuteCommand(DirectCmd.OpenShipHangar);
                        break;
                    }

                    if (!shipHangar.IsReady)
                        break;

                    var ships = DirectEve.Instance.GetShipHangar().Items;
                    foreach (var ship in ships.Where(ship => ship.GivenName == txtNameShip.Text))
                    {
                        Logging.Log("MakeShip: Making [" + ship.GivenName + "] active");
                            
                        ship.ActivateShip();
                        LstTask.Items.Remove(LstTask.Items[0]);
                        State = State.NextAction;
                        break;
                    }


                     break;


                case State.Buy:

                     _buy.Item = Convert.ToInt32(LstTask.Items[0].Tag);
                     _buy.Unit = Convert.ToInt32(LstTask.Items[0].SubItems[2].Text);
                     

                     if (_buy.State == StateBuy.Idle)
                    {
                        Logging.Log("Buy: Begin");
                        _buy.State = StateBuy.Begin;
                    }

                     _buy.ProcessState();


                     if (_buy.State == StateBuy.Done)
                    {
                        _buy.State = StateBuy.Idle;
                        LstTask.Items.Remove(LstTask.Items[0]);
                        State = State.NextAction;
                    } 

                     break;

                case State.Sell:

                     _sell.Item = Convert.ToInt32(LstTask.Items[0].Tag);
                     _sell.Unit = Convert.ToInt32(LstTask.Items[0].SubItems[2].Text);

                     if (_sell.State == StateSell.Idle)
                    {
                        Logging.Log("Sell: Begin");
                        _sell.State = StateSell.Begin;
                    }

                     _sell.ProcessState();


                     if (_sell.State == StateSell.Done)
                    {
                        _sell.State = StateSell.Idle;
                        LstTask.Items.Remove(LstTask.Items[0]);
                        State = State.NextAction;
                    } 
                     break;

                case State.Drop:

                     _drop.Item = Convert.ToInt32(LstTask.Items[0].Tag);
                     _drop.Unit = Convert.ToInt32(LstTask.Items[0].SubItems[2].Text);
                     _drop.Hangar = LstTask.Items[0].SubItems[3].Text;

                     if (_drop.State == StateDrop.Idle)
                    {
                        Logging.Log("Drop: Begin");
                        _drop.State = StateDrop.Begin;

                    }

                     _drop.ProcessState();


                     if (_drop.State == StateDrop.Done)
                    {
                        _drop.State = StateDrop.Idle;
                        LstTask.Items.Remove(LstTask.Items[0]);
                        State = State.NextAction;
                    }   


                     break;

                case State.Grab:

                     _grab.Item = Convert.ToInt32(LstTask.Items[0].Tag);
                     _grab.Unit = Convert.ToInt32(LstTask.Items[0].SubItems[2].Text);
                     _grab.Hangar = LstTask.Items[0].SubItems[3].Text;


                     if (_grab.State == StateGrab.Idle)
                     {
                         Logging.Log("Grab: Begin");
                         _grab.State = StateGrab.Begin;

                     }

                     _grab.ProcessState();


                     if (_grab.State == StateGrab.Done)
                     {
                         _grab.State = StateGrab.Idle;
                         LstTask.Items.Remove(LstTask.Items[0]);
                         State = State.NextAction;
                     }     

                     break;

                case State.Traveler:

                     
                    // We are warping
                    if (DirectEve.Instance.Session.IsInSpace && DirectEve.Instance.ActiveShip.Entity != null && DirectEve.Instance.ActiveShip.Entity.IsWarping)
                        return;

                    var travelerDestination = _traveler.Destination;
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
                    _jumps = DirectEve.Instance.Navigation.GetDestinationPath().Count;

                    // Arrived at destination
                    if (_destination != null && _traveler.State == TravelerState.AtDestination)
                    {
                        Logging.Log("Arived at destination");

                        _traveler.Destination = null;
                        _destination = null;
                        LstTask.Items.Remove(LstTask.Items[0]);
                        State = State.NextAction;
                    }

                    // An error occured, reset traveler
                    if (_traveler.State == TravelerState.Error)
                    {
                        if (_traveler.Destination != null)
                            Logging.Log("Stopped traveling, Traveler threw an error...");

                        _destination = null;
                        _traveler.Destination = null;
                        Start = false;
                        State = State.Idle;
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

            var result = new List<ListViewItem>();
            foreach (var item in list)
            {
                var name = getTitle(item);
                if (string.IsNullOrEmpty(name))
                    continue;

                var found = search.All(t => name.IndexOf(t, StringComparison.OrdinalIgnoreCase) > -1);
                if (!found)
                    continue;

                var listViewItem = new ListViewItem(name);
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

                var name = "";
                if (_destination is DirectBookmark)
                    name = ((DirectBookmark) _destination).Title;
                if (_destination is DirectRegion)
                    name = ((DirectRegion) _destination).Name;
                if (_destination is DirectConstellation)
                    name = ((DirectConstellation) _destination).Name;
                if (_destination is DirectSolarSystem)
                    name = ((DirectSolarSystem) _destination).Name;
                if (_destination is DirectStation)
                    name = ((DirectStation) _destination).Name;

                if (!string.IsNullOrEmpty(name))
                    name = @"Traveling to " + name + " (" + _jumps + " jumps)";

                LblStatus.Text = name;
            }

            if (!_changed)
                return;
            _changed = false;

            var search = SearchTextBox.Text.Split(' ');

            SearchResults.BeginUpdate();
            try
            {
                SearchResults.Items.Clear();
                SearchResults.Items.AddRange(Filter(search, _bookmarks, b => b.Title, b => "Bookmark (" + ((Category) b.CategoryId) + ")"));
                SearchResults.Items.AddRange(Filter(search, _solarSystems, s => s.Name, b => "Solar System"));
                SearchResults.Items.AddRange(Filter(search, _stations, s => s.Name, b => "Station"));

                // Automaticly select the only item
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
                State = State.Idle;
                Start = true;
            }
            else
            {
                BttnStart.Text = "Start";
                State = State.Idle;
                Start = false;
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            _changed = true;
        }

        private void BttnAddTraveler_Click(object sender, EventArgs e)
        {
            var listItem = new ListViewItem("Traveler");
            listItem.SubItems.Add(SearchResults.SelectedItems[0].Text);
            listItem.Tag = SearchResults.SelectedItems[0].Tag;
            LstTask.Items.Add(listItem);
        }

        private void BttnTaskForItem_Click_1(object sender, EventArgs e)
        {
            foreach (ListViewItem item in LstItems.CheckedItems)
            {
                var listItem = new ListViewItem(cmbMode.Text);
                listItem.SubItems.Add(item.Text);
                listItem.Tag = item.SubItems[1].Text;
                listItem.SubItems.Add(txtUnit.Text);
                listItem.SubItems.Add(SelectHangar);
                LstTask.Items.Add(listItem);
            }
        }


        private void MoveListViewItem(ref ListView lv, bool moveUp)
        {
            string cache;
            int selIdx;

            selIdx = lv.SelectedItems[0].Index;
            if (moveUp)
            {
                // ignore moveup of row(0)
                if (selIdx == 0)
                    return;

                // move the subitems for the previous row
                // to cache to make room for the selected row
                for (int i = 0; i < lv.Items[selIdx].SubItems.Count; i++)
                {
                    cache = lv.Items[selIdx - 1].SubItems[i].Text;
                    lv.Items[selIdx - 1].SubItems[i].Text =
                      lv.Items[selIdx].SubItems[i].Text;
                    lv.Items[selIdx].SubItems[i].Text = cache;
                }
                lv.Items[selIdx - 1].Selected = true;
                lv.Refresh();
                lv.Focus();
            }
            else
            {
                // ignore movedown of last item
                if (selIdx == lv.Items.Count - 1)
                    return;
                // move the subitems for the next row
                // to cache so we can move the selected row down
                for (int i = 0; i < lv.Items[selIdx].SubItems.Count; i++)
                {
                    cache = lv.Items[selIdx + 1].SubItems[i].Text;
                    lv.Items[selIdx + 1].SubItems[i].Text =
                      lv.Items[selIdx].SubItems[i].Text;
                    lv.Items[selIdx].SubItems[i].Text = cache;
                }
                lv.Items[selIdx + 1].Selected = true;
                lv.Refresh();
                lv.Focus();
            }
        }

        private void bttnUP_Click(object sender, EventArgs e)
        {
            MoveListViewItem(ref LstTask ,true);
        }

        private void bttnDown_Click(object sender, EventArgs e)
        {
            MoveListViewItem(ref LstTask, false);
        }

        private void bttnDelete_Click(object sender, EventArgs e)
        {
            while (LstTask.SelectedItems.Count > 0)
            {
                    LstTask.Items.Remove(LstTask.SelectedItems[0]);
            }
        }

        private void txtSearchItems_TextChanged(object sender, EventArgs e)
        {

            LstItems.Items.Clear();

            if (txtSearchItems.Text.Length > 4)
            {
                var search = txtSearchItems.Text.Split(' ');
                foreach (var item in _list)
                {
                    var name = item.name;
                    if (string.IsNullOrEmpty(name))
                        continue;

                    var found = search.All(t => name.IndexOf(t, StringComparison.OrdinalIgnoreCase) > -1);
                    if (!found)
                        continue;

                    var listItem1 = new ListViewItem(item.name);
                    listItem1.SubItems.Add(Convert.ToString(item.Id));
                    LstItems.Items.Add(listItem1);
                }
            }
        }

        private void bttnTaskAllItems_Click(object sender, EventArgs e)
        {
            var listItem = new ListViewItem(cmbAllMode.Text);
            listItem.SubItems.Add("All items");
            listItem.Tag = 00;
            listItem.SubItems.Add("00");
            listItem.SubItems.Add(SelectHangar);
            LstTask.Items.Add(listItem);
        }

        private void bttnTaskMakeShip_Click(object sender, EventArgs e)
        {
            var listItem = new ListViewItem("MakeShip");
            listItem.SubItems.Add(txtNameShip.Text);
            LstTask.Items.Add(listItem);
        }

        private void chkPause_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPause.Checked == true)
                Paused = true;
            if (chkPause.Checked == false)
                Paused = false;
        }

        private void rbttnLocal_CheckedChanged(object sender, EventArgs e)
        {
            if (rbttnLocal.Checked == true)
                SelectHangar = rbttnLocal.Text;
        }

        private void rbttnShip_CheckedChanged(object sender, EventArgs e)
        {
            if (rbttnShip.Checked == true)
                SelectHangar = rbttnShip.Text;
        }

        private void rbttnCorp_CheckedChanged(object sender, EventArgs e)
        {
            if (rbttnCorp.Checked == true)
            {
                txtNameCorp.Enabled = true;
                SelectHangar = txtNameCorp.Text;
            }
            else if (rbttnCorp.Checked == false)
            {
                txtNameCorp.Enabled = false;
            }
        }

        private void txtNameCorp_TextChanged(object sender, EventArgs e)
        {
            SelectHangar = txtNameCorp.Text;
        }

    }
}