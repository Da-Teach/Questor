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
                case ArmState.Done:
                    break;

                case ArmState.Begin:
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
                                       ? Settings.Instance.CombatShipName
                                       : Settings.Instance.SalvageShipName;

                    if (!string.IsNullOrEmpty(shipName) && Cache.Instance.DirectEve.ActiveShip.GivenName != shipName)
                    {
                        if (DateTime.Now.Subtract(_lastAction).TotalSeconds > 15)
                        {
                            var ships = Cache.Instance.DirectEve.GetShipHangar().Items;
                            foreach (var ship in ships.Where(ship => ship.GivenName == shipName))
                            {
                                Logging.Log("Arm: Making [" + ship.GivenName + "] active");

                                ship.ActivateShip();
                                _lastAction = DateTime.Now;
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

                    if (State == ArmState.ActivateSalvageShip)
                    {
                        Logging.Log("Arm: Done");
                        State = ArmState.Done;
                        return;
                    }

                    _missionItemMoved = false;
                    Cache.Instance.RefreshMissionItems(AgentId);
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
                    else
                    {
                        Logging.Log("Arm: Moving items");
                        State = ArmState.MoveItems;
                    }
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
                    if (drone == null || !drone.Quantity.HasValue)
                    {
                        Logging.Log("Arm: Out of drones");
                        State = ArmState.NotEnoughAmmo;
                        break;
                    }

                    var neededDrones = Math.Floor((droneBay.Capacity - droneBay.UsedCapacity)/(drone.Volume ?? 5d));
                    if (neededDrones == 0)
                    {
                        Logging.Log("Arm: Moving items");
                        State = ArmState.MoveItems;
                        break;
                    }

                    // Move needed drones
                    droneBay.Add(drone.ItemId, (int) Math.Min(neededDrones, drone.Quantity.Value));
                    break;

                case ArmState.MoveItems:
                    var bringItem = Cache.Instance.BringMissionItem;
                    if (string.IsNullOrEmpty(bringItem))
                        _missionItemMoved = true;

                    if (!_missionItemMoved)
                    {
                        var missionItem = (corpHangar ?? itemHangar).Items.FirstOrDefault(i => (i.Name ?? string.Empty).ToLower() == bringItem);
                        if (missionItem == null)
                            missionItem = itemHangar.Items.FirstOrDefault(i => (i.Name ?? string.Empty).ToLower() == bringItem);

                        if (missionItem != null)
                        {
                            Logging.Log("Arm: Moving [" + missionItem.Name + "]");

                            cargo.Add(missionItem.ItemId, 1);
                            _missionItemMoved = true;
                            break;
                        }
                    }

                    var itemMoved = false;
                    foreach (var item in ammoHangar.Items.OrderBy(i => i.Quantity))
                    {
                        if (item.ItemId <= 0)
                            continue;

                        var ammo = AmmoToLoad.FirstOrDefault(a => a.TypeId == item.TypeId);
                        if (ammo == null)
                            continue;

                        Logging.Log("Arm: Moving [" + item.Name + "]");

                        var moveQuantity = Math.Min(item.Quantity ?? -1, ammo.Quantity);
                        moveQuantity = Math.Max(moveQuantity, 1);
                        cargo.Add(item.ItemId, moveQuantity);

                        ammo.Quantity -= moveQuantity;
                        if (ammo.Quantity <= 0)
                            AmmoToLoad.RemoveAll(a => a.TypeId == item.TypeId);

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
                        State = ArmState.Done;
                        break;
                    }

                    // Note, there's no unlock here as we *always* want our ammo!
                    break;
            }
        }
    }
}