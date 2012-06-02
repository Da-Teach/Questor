
namespace Questor.Modules.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.States;

    public class Grab
    {
        public int Item { get; set; }
        public int Unit { get; set; }
        public string Hangar { get; set; }
        private double freeCargoCapacity;

        private DateTime _lastAction;

        public void ProcessState()
        {
            DirectContainer _hangar = null;

            DirectContainer cargo = Cache.Instance.DirectEve.GetShipsCargo();

            if ("Local Hangar" == Hangar)
                _hangar = Cache.Instance.DirectEve.GetItemHangar();
            else if ("Ship Hangar" == Hangar)
                _hangar = Cache.Instance.DirectEve.GetShipHangar();
            else
                _hangar = Cache.Instance.DirectEve.GetCorporationHangar(Hangar);

            switch (_States.CurrentGrabState)
            {
                case GrabState.Idle:
                case GrabState.Done:
                    break;

                case GrabState.Begin:
                    _States.CurrentGrabState = GrabState.OpenItemHangar;
                    break;

                case GrabState.OpenItemHangar:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    if ("Local Hangar" == Hangar)
                    {
                        // Is the hangar open?
                        if (_hangar.Window == null)
                        {
                            // No, command it to open
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                        }
                        if (!_hangar.IsReady)
                            break;
                    }
                    else if ("Ship Hangar" == Hangar)
                    {
                        if (_hangar.Window == null)
                        {
                            // No, command it to open
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenShipHangar);
                        }
                        if (!_hangar.IsReady)
                            break;
                    }
                    else if (Hangar != null)
                    {
                        if (_hangar.Window == null)
                        {
                            // No, command it to open
                            //Cache.Instance.DirectEve.OpenCorporationHangar();
                        }

                        if (!_hangar.IsReady)
                            break;
                    }

                    Logging.Log("Grab", "Opening Hangar", Logging.white);

                    _States.CurrentGrabState = GrabState.OpenCargo;

                    break;

                case GrabState.OpenCargo:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;
                    // Is cargo open?
                    if (cargo.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                        break;
                    }

                    if (!cargo.IsReady)
                        break;

                    Logging.Log("Grab", "Opening Cargo Hold", Logging.white);

                    freeCargoCapacity = cargo.Capacity - cargo.UsedCapacity;

                    _States.CurrentGrabState = Item == 00 ? GrabState.AllItems : GrabState.MoveItems;

                    break;

                case GrabState.MoveItems:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;
                    if (Unit == 00)
                    {
                        DirectItem GrabItem = _hangar.Items.FirstOrDefault(i => (i.TypeId == Item));
                        if (GrabItem != null)
                        {
                            double totalVolum = GrabItem.Quantity * GrabItem.Volume;
                            if (freeCargoCapacity >= totalVolum)
                            {
                                cargo.Add(GrabItem, GrabItem.Quantity);
                                freeCargoCapacity -= totalVolum;
                                Logging.Log("Grab", "Moving all the items", Logging.white);
                                _lastAction = DateTime.Now;
                                _States.CurrentGrabState = GrabState.WaitForItems;
                            }
                            else
                            {
                                _States.CurrentGrabState = GrabState.Done;
                                Logging.Log("Grab", "No load capacity", Logging.white);
                            }
                        }
                    }
                    else
                    {
                        DirectItem GrabItem = _hangar.Items.FirstOrDefault(i => (i.TypeId == Item));
                        if (GrabItem != null)
                        {
                            double totalVolum = Unit * GrabItem.Volume;
                            if (freeCargoCapacity >= totalVolum)
                            {
                                cargo.Add(GrabItem, Unit);
                                freeCargoCapacity -= totalVolum;
                                Logging.Log("Grab", "Moving item", Logging.white);
                                _lastAction = DateTime.Now;
                                _States.CurrentGrabState = GrabState.WaitForItems;
                            }
                            else
                            {
                                _States.CurrentGrabState = GrabState.Done;
                                Logging.Log("Grab", "No load capacity", Logging.white);
                            }
                        }
                    }

                    break;

                case GrabState.AllItems:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    List<DirectItem> AllItem = _hangar.Items;
                    if (AllItem != null)
                    {
                        foreach (DirectItem item in AllItem)
                        {
                            double totalVolum = item.Quantity * item.Volume;

                            if (freeCargoCapacity >= totalVolum)
                            {
                                cargo.Add(item);
                                freeCargoCapacity -= totalVolum;
                            }
                        }
                        Logging.Log("Grab", "Moving items", Logging.white);
                        _lastAction = DateTime.Now;
                        _States.CurrentGrabState = GrabState.WaitForItems;
                    }

                    break;

                case GrabState.WaitForItems:
                    // Wait 5 seconds after moving
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        Logging.Log("Grab", "Done", Logging.white);
                        _States.CurrentGrabState = GrabState.Done;
                        break;
                    }

                    break;
            }
        }
    }
}