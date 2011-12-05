// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace Questor.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using System.Xml.Linq;
    using System.IO;

    public class Arm
    {
        private DateTime _lastAction;
        private bool _missionItemMoved;

        public Arm()
        {
            AmmoToLoad = new List<Ammo>();
        }

        // Bleh, dont want this here :(
        public long AgentId { get; set; }

        public ArmState State { get; set; }
        public List<Ammo> AmmoToLoad { get; private set; }

        public bool DefaultFittingChecked = false; //flag to check for the correct default fitting before using the fitting manager
        public bool DefaultFittingFound = true; //Did we find the default fitting?
        public bool TryMissionShip = true;  // Used in the event we can't find the ship specified in the missionfittings
        public bool UseMissionShip = false; // Were we successful in activating the mission specific ship?

        public void ProcessState()
        {
            var cargo = Cache.Instance.DirectEve.GetShipsCargo();
            var droneBay = Cache.Instance.DirectEve.GetShipsDroneBay();
            var itemHangar = Cache.Instance.DirectEve.GetItemHangar();
            var shipHangar = Cache.Instance.DirectEve.GetShipHangar();

            DirectContainer corpHangar = null;
            if (!string.IsNullOrEmpty(Settings.Instance.AmmoHangar))
                corpHangar = Cache.Instance.DirectEve.GetCorporationHangar(Settings.Instance.AmmoHangar);

            // Select the correct ammo hangar
            var ammoHangar = corpHangar ?? itemHangar;
            switch (State)
            {
                case ArmState.Idle:

                    break;
                case ArmState.Done:
                    break;

                case ArmState.Begin:
                    //DefaultFittingChecked = false; //flag to check for the correct default fitting before using the fitting manager
                    //DefaultFittingFound = true; //Did we find the default fitting?
                    Cache.Instance.ArmLoadedCache = false;
                    TryMissionShip = true;  // Used in the event we can't find the ship specified in the missionfittings
                    UseMissionShip = false; // Were we successful in activating the mission specific ship?
                    State = ArmState.OpenShipHangar;
                    break;

                case ArmState.OpenShipHangar:
                case ArmState.SwitchToSalvageShip:
                    // Is the ship hangar open?
                    if (shipHangar.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenShipHangar);
                        break;
                    }

                    if (!shipHangar.IsReady)
                        break;

                    if (State == ArmState.OpenShipHangar)
                    {
                        Logging.Log("Arm: Activating combat ship");
                        State = ArmState.ActivateCombatShip;
                    }
                    else
                    {
                        Logging.Log("Arm: Activating salvage ship");
                        State = ArmState.ActivateSalvageShip;
                    }
                    break;

                case ArmState.ActivateCombatShip:
                case ArmState.ActivateSalvageShip:
                    var shipName = State == ArmState.ActivateCombatShip
                                       ? Settings.Instance.CombatShipName.ToLower()
                                       : Settings.Instance.SalvageShipName.ToLower();

                    if (!Cache.Instance.ArmLoadedCache)
                    {
                        _missionItemMoved = false;
                        Cache.Instance.RefreshMissionItems(AgentId);
                        Cache.Instance.ArmLoadedCache = true;
                    }

                    // If we've got a mission-specific ship defined, switch to it
                    if ((State == ArmState.ActivateCombatShip) && !(Cache.Instance.MissionShip == "" || Cache.Instance.MissionShip == null) && TryMissionShip)
                        shipName = Cache.Instance.MissionShip.ToLower();

                    if (Settings.Instance.CombatShipName.ToLower() == shipName) // if the mission specific ship is our default combat ship, no need to do anything special
                        TryMissionShip = false;

                    if ((!string.IsNullOrEmpty(shipName) && Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != shipName))
                    {
                        if (DateTime.Now.Subtract(_lastAction).TotalSeconds > 15)
                        {
                            var ships = Cache.Instance.DirectEve.GetShipHangar().Items;
                            foreach (var ship in ships.Where(ship => ship.GivenName.ToLower() == shipName))
                            {
                                Logging.Log("Arm: Making [" + ship.GivenName + "] active");

                                ship.ActivateShip();
                                _lastAction = DateTime.Now;
                                if (TryMissionShip)
                                    UseMissionShip = true;
                                return;
                            }

                            if (TryMissionShip && !UseMissionShip)
                            {
                                Logging.Log("Arm: Unable to find the ship specified in the missionfitting.  Using default combat ship and default fitting.");
                                TryMissionShip = false;
                                Cache.Instance.Fitting = Cache.Instance.DefaultFitting;
                                return;
                            }

                            State = ArmState.NotEnoughAmmo;
                            Logging.Log("Arm: Found the following ships:");
                            foreach (var ship in ships)
                                Logging.Log("Arm: [" + ship.GivenName + "]");
                            Logging.Log("Arm: Could not find [" + shipName + "] ship!");
                            return;
                        }
                        return;
                    }
                    else if (TryMissionShip)
                        UseMissionShip = true;

                    if (State == ArmState.ActivateSalvageShip)
                    {
                        Logging.Log("Arm: Done");
                        State = ArmState.Done;
                        return;
                    }

                    //_missionItemMoved = false;
                    //Cache.Instance.RefreshMissionItems(AgentId);
                    if (AmmoToLoad.Count == 0 && string.IsNullOrEmpty(Cache.Instance.BringMissionItem))
                    {
                        Logging.Log("Arm: Done");
                        State = ArmState.Done;
                    }
                    else
                    {
                        Logging.Log("Arm: Opening item hangar");
                        State = ArmState.OpenItemHangar;
                    }
                    break;

                case ArmState.OpenItemHangar:
                    // Is the hangar open?
                    if (itemHangar.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                        break;
                    }

                    if (!itemHangar.IsReady)
                        break;

                    if (corpHangar != null)
                    {
                        Logging.Log("Arm: Opening corporation hangar");
                        State = ArmState.OpenCorpHangar;
                    }
                    else
                    {
                        Logging.Log("Arm: Opening ship's cargo");
                        State = ArmState.OpenCargo;
                    }
                    break;

                case ArmState.OpenCorpHangar:
                    // Is the hangar open?
                    if (corpHangar != null)
                    {
                        if (corpHangar.Window == null)
                        {
                            // No, command it to open
                            Cache.Instance.DirectEve.OpenCorporationHangar();
                            break;
                        }

                        if (!corpHangar.IsReady)
                            break;
                    }

                    Logging.Log("Arm: Opening ship's cargo");
                    State = ArmState.OpenCargo;
                    break;

                case ArmState.OpenCargo:
                    // Is cargo open?
                    if (cargo.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                        break;
                    }

                    if (!cargo.IsReady)
                        break;

                    if (Settings.Instance.UseDrones && Settings.Instance.DroneTypeId > 0)
                    {
                        Logging.Log("Arm: Opening ship's drone bay");
                        State = ArmState.OpenDroneBay;
                    }
                    else if ((Settings.Instance.FittingsDefined && DefaultFittingFound) && !(UseMissionShip && !(Cache.Instance.ChangeMissionShipFittings)))
                    {
                        Logging.Log("Arm: Fitting");
                        State = ArmState.OpenFittingWindow;
                    }
                    else
                        State = ArmState.MoveItems;
                    break;

                case ArmState.OpenFittingWindow:
                    //let's check first if we need to change fitting at all
                    Logging.Log("Arm: Fitting: " + Cache.Instance.Fitting + " - currentFit: " + Cache.Instance.currentFit);
                    if (Cache.Instance.Fitting.Equals(Cache.Instance.currentFit))
                    {
                        Logging.Log("Arm: Current fit is correct - no change necessary");
                        State = ArmState.MoveItems;
                    }
                    else
                    {
                        Cache.Instance.DirectEve.OpenFitingManager();
                        State = ArmState.WaitForFittingWindow;
                    }
                    break;
                case ArmState.WaitForFittingWindow:

                    var fittingMgr = Cache.Instance.DirectEve.Windows.OfType<DirectFittingManagerWindow>().FirstOrDefault();
                    //open it again ?
                    if (fittingMgr == null)
                    {
                        Logging.Log("Arm: Opening fitting manager");
                        Cache.Instance.DirectEve.OpenFitingManager();
                    }
                    //check if it's ready
                    else if (fittingMgr.IsReady)
                    {
                        State = ArmState.ChoseFitting;
                    }
                    break;
                case ArmState.ChoseFitting:
                    fittingMgr = Cache.Instance.DirectEve.Windows.OfType<DirectFittingManagerWindow>().FirstOrDefault();
                    bool found = false;
                    if (!DefaultFittingChecked)
                    {
                        DefaultFittingChecked = true;
                        Logging.Log("Arm: Looking for Default Fitting " + Cache.Instance.DefaultFitting);
                        foreach (var fitting in fittingMgr.Fittings)
                        {
                            //ok found it
                            if (Cache.Instance.DefaultFitting.ToLower().Equals(fitting.Name.ToLower()))
                            {
                                found = true;
                                Logging.Log("Arm: Found Default Fitting " + fitting.Name);
                            }
                        }
                        if (!found)
                        {
                            Logging.Log("Arm: Error! Couldn't find Default Fitting.  Disabling fitting manager.");
                            DefaultFittingFound = false;
                            Settings.Instance.FittingsDefined = false;
                            State = ArmState.MoveItems;
                            break;
                        }
                        found = false;
                    }
                    Logging.Log("Arm: Looking for fitting " + Cache.Instance.Fitting);
                    foreach (var fitting in fittingMgr.Fittings)
                    {
                        //ok found it
                        var ship = Cache.Instance.DirectEve.ActiveShip;
                        if (Cache.Instance.Fitting.ToLower().Equals(fitting.Name.ToLower()) && fitting.ShipTypeId == ship.TypeId)
                        {
                            Logging.Log("Arm: Found fitting " + fitting.Name);
                            //switch to the requested fitting for the current mission
                            fitting.Fit();
                            _lastAction = DateTime.Now;
                            Cache.Instance.currentFit = fitting.Name;
                            State = ArmState.WaitForFitting;
                            found = true;
                            break;
                        }

                    }
                    //if we didn't find it, we'll set currentfit to default
                    //this should provide backwards compatibility without trying to fit always
                    if (!found)
                    {
                        if (UseMissionShip)
                        {
                            Logging.Log("Arm: Couldn't find fitting for this ship typeid.  Using current fitting.");
                            State = ArmState.MoveItems;
                            break;
                        }
                        else
                        {
                            Logging.Log("Arm: Couldn't find fitting - switching to default");
                            Cache.Instance.Fitting = Cache.Instance.DefaultFitting;
                            break;
                        }
                    }
                    State = ArmState.MoveItems;
                    fittingMgr.Close();
                    break;

                case ArmState.WaitForFitting:
                    //let's wait 10 seconds
                    if (DateTime.Now.Subtract(_lastAction).TotalMilliseconds > 10000 &&
                        Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        //we should be done fitting, proceed to the next state
                        State = ArmState.MoveItems;
                        fittingMgr = Cache.Instance.DirectEve.Windows.OfType<DirectFittingManagerWindow>().FirstOrDefault();
                        fittingMgr.Close();
                        Logging.Log("Arm: Done fitting");
                    }
                    else Logging.Log("Arm: Waiting for fitting. time elapsed = " + DateTime.Now.Subtract(_lastAction).TotalMilliseconds + " locked items = " + Cache.Instance.DirectEve.GetLockedItems().Count);
                    break;

                case ArmState.OpenDroneBay:
                    // Is cargo open?
                    if (droneBay.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenDroneBayOfActiveShip);
                        break;
                    }
 
                    if (!droneBay.IsReady)
                        break;
 
                    Logging.Log("Arm: Moving drones");
                    State = ArmState.MoveDrones;
                    break;
 
                case ArmState.MoveDrones:
                    var drone = ammoHangar.Items.FirstOrDefault(i => i.TypeId == Settings.Instance.DroneTypeId);
                    if (drone == null || drone.Stacksize < 1)
                    {
                        Logging.Log("Arm: Out of drones");
                        State = ArmState.NotEnoughAmmo;
                        break;
                    }
 
                    var neededDrones = Math.Floor((droneBay.Capacity - droneBay.UsedCapacity)/drone.Volume);
                    Logging.Log("neededDrones: " + neededDrones);
                    if (neededDrones == 0 && ((Settings.Instance.FittingsDefined && DefaultFittingFound) && !(UseMissionShip && !(Cache.Instance.ChangeMissionShipFittings))))
                    {
                        Logging.Log("Arm: Fitting");
                        State = ArmState.OpenFittingWindow;
                        break;
                    }
                    else if (neededDrones == 0)
                    {
                        State = ArmState.MoveItems;
                        break;
                    }

                    // Move needed drones
                    droneBay.Add(drone, (int)Math.Min(neededDrones, drone.Stacksize));
                    break;

                case ArmState.MoveItems:
                    var bringItem = Cache.Instance.BringMissionItem;
                    if (string.IsNullOrEmpty(bringItem))
                        _missionItemMoved = true;

                    if (!_missionItemMoved)
                    {
                        var missionItem = (corpHangar ?? itemHangar).Items.FirstOrDefault(i => (i.TypeName ?? string.Empty).ToLower() == bringItem);
                        if (missionItem == null)
                            missionItem = itemHangar.Items.FirstOrDefault(i => (i.TypeName ?? string.Empty).ToLower() == bringItem);

                        if (missionItem != null)
                        {
                            Logging.Log("Arm: Moving [" + missionItem.TypeName + "]");

                            cargo.Add(missionItem, 1);
                            _missionItemMoved = true;
                            break;
                        }
                    }

                    var itemMoved = false;
                    if (Cache.Instance.missionAmmo.Count() != 0)
                    {
                        AmmoToLoad = new List<Ammo>(Cache.Instance.missionAmmo);

                    }
                    foreach (var item in ammoHangar.Items.OrderBy(i => i.Quantity))
                    {
                        if (item.ItemId <= 0)
                            continue;

                        var ammo = AmmoToLoad.FirstOrDefault(a => a.TypeId == item.TypeId);
                        if (ammo == null)
                            continue;

                        Logging.Log("Arm: Moving [" + item.TypeName + "]");

                        var moveQuantity = Math.Min(item.Quantity, ammo.Quantity);
                        moveQuantity = Math.Max(moveQuantity, 1);
                        cargo.Add(item, moveQuantity);

                        ammo.Quantity -= moveQuantity;
                        if (ammo.Quantity <= 0)
                        {
                            Cache.Instance.missionAmmo.RemoveAll(a => a.TypeId == item.TypeId);
                            AmmoToLoad.RemoveAll(a => a.TypeId == item.TypeId);
                        }
                        itemMoved = true;
                        break;
                    }

                    if (AmmoToLoad.Count == 0 && _missionItemMoved)
                    {
                        _lastAction = DateTime.Now;

                        Logging.Log("Arm: Waiting for items");
                        State = ArmState.WaitForItems;
                    }
                    else if (!itemMoved)
                    {
                        if (AmmoToLoad.Count > 0)
                            foreach (var ammo in AmmoToLoad)
                                Logging.Log("Arm: Missing ammo with TypeId [" + ammo.TypeId + "]");

                        if (!_missionItemMoved)
                            Logging.Log("Arm: Missing mission item [" + bringItem + "]");

                        State = ArmState.NotEnoughAmmo;
                    }
                    break;

                case ArmState.WaitForItems:
                    // Wait 5 seconds after moving
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (cargo.Items.Count == 0)
                        break;

                    if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        // Close the drone bay, its not required in space.
                        if (droneBay.IsReady)
                            droneBay.Window.Close();

                        Logging.Log("Arm: Done");

                        //reload the ammo setting for combat
                        try
                        {
                            var mission = Cache.Instance.DirectEve.AgentMissions.FirstOrDefault(m => m.AgentId == AgentId);
                            if (mission == null)
                                return;

                            var missionName = Cache.Instance.FilterPath(mission.Name);
                            var missionXmlPath = Path.Combine(Settings.Instance.MissionsPath, missionName + ".xml");
                            var missionXml = XDocument.Load(missionXmlPath);
                            Cache.Instance.missionAmmo = new List<Ammo>();
                            var ammoTypes = missionXml.Root.Element("missionammo");
                            if (ammoTypes != null)
                                foreach (var ammo in ammoTypes.Elements("ammo"))
                                    Cache.Instance.missionAmmo.Add(new Ammo(ammo));
                        }
                        catch (Exception e)
                        {
                            Cache.Instance.missionAmmo = new List<Ammo>();
                        }

                        State = ArmState.Done;
                        break;
                    }

                    // Note, there's no unlock here as we *always* want our ammo!
                    break;
            }
        }
    }
}