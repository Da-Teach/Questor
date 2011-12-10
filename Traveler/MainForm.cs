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
    using DirectEve;
    using Traveler.Common;
    using Traveler.Domains;
    using Traveler.Module;
    using DirectEve = Traveler.Common.DirectEve;

    public partial class MainForm : Form
    {
        private List<DirectBookmark> _bookmarks;
        private bool _changed;

        private object _destination;
        private int _jumps;

        private object _previousDestination;
        private int _previousJumps;
        private List<DirectSolarSystem> _solarSystems;
        private List<DirectStation> _stations;

        private Traveler _traveler;

        public MainForm()
        {
            InitializeComponent();

            _traveler = new Traveler();

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
            }

            // An error occured, reset traveler
            if (_traveler.State == TravelerState.Error)
            {
                if (_traveler.Destination != null)
                    Logging.Log("Stopped traveling, Traveler threw an error...");

                _destination = null;
                _traveler.Destination = null;
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

                DestinationLabel.Text = name;
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

        private void SearchTextBoxChanged(object sender, EventArgs e)
        {
            _changed = true;
        }

        private void ActionButtonClick(object sender, EventArgs e)
        {
            if (SearchResults.SelectedItems.Count == 0)
            {
                DestinationLabel.Text = "";
                _destination = null;
                return;
            }

            // Set the destination
            _destination = SearchResults.SelectedItems[0].Tag;
        }
    }
}