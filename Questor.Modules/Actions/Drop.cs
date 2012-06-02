
namespace Questor.Modules.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.States;

    public class Drop
    {
        public int Item { get; set; }
        public int Unit { get; set; }
        public string Hangar { get; set; }

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

            switch (_States.CurrentDropState)
            {
                case DropState.Idle:
                case DropState.Done:
                    break;

                case DropState.Begin:
                    _States.CurrentDropState = DropState.OpenItemHangar;
                    break;

                case DropState.OpenItemHangar:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    if ("Local Hangar" == Hangar)
                    {
                        // Is the hangar open?
                        if (_hangar.Window == null)
                        {
                            // No, command it to open
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                            break;
                        }
                        if (!_hangar.Window.IsReady)
                            break;
                    }
                    else if ("Ship Hangar" == Hangar)
                    {
                        if (_hangar.Window == null)
                        {
                            // No, command it to open
                            Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenShipHangar);
                            break;
                        }
                        if (!_hangar.Window.IsReady)
                            break;
                    }
                    else
                    {
                        if (_hangar.Window == null)
                        {
                            // No, command it to open
                            //Cache.Instance.DirectEve.OpenCorporationHangar();
                            break;
                        }

                        if (!_hangar.Window.IsReady)
                            break;
                    }

                    Logging.Log("Drop", "Opening Hangar", Logging.white);
                    _States.CurrentDropState = DropState.OpenCargo;
                    break;

                case DropState.OpenCargo:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;
                    // Is cargo open?
                    if (cargo.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                        break;
                    }

                    if (!cargo.Window.IsReady)
                        break;

                    Logging.Log("Drop", "Opening Cargo Hold", Logging.white);
                    _States.CurrentDropState = Item == 00 ? DropState.AllItems : DropState.MoveItems;

                    break;

                case DropState.MoveItems:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    if (Unit == 00)
                    {
                        DirectItem DropItem = cargo.Items.FirstOrDefault(i => (i.TypeId == Item));
                        if (DropItem != null)
                        {
                            _hangar.Add(DropItem, DropItem.Quantity);
                            Logging.Log("Drop", "Moving all the items", Logging.white);
                            _lastAction = DateTime.Now;
                            _States.CurrentDropState = DropState.WaitForMove;
                        }
                    }
                    else
                    {
                        DirectItem DropItem = cargo.Items.FirstOrDefault(i => (i.TypeId == Item));
                        if (DropItem != null)
                        {
                            _hangar.Add(DropItem, Unit);
                            Logging.Log("Drop", "Moving item", Logging.white);
                            _lastAction = DateTime.Now;
                            _States.CurrentDropState = DropState.WaitForMove;
                        }
                    }

                    break;

                case DropState.AllItems:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    List<DirectItem> AllItem = cargo.Items;
                    if (AllItem != null)
                    {
                        _hangar.Add(AllItem);
                        Logging.Log("Drop", "Moving item", Logging.white);
                        _lastAction = DateTime.Now;
                        _States.CurrentDropState = DropState.WaitForMove;
                    }

                    break;

                case DropState.WaitForMove:
                    if (cargo.Items.Count != 0)
                    {
                        _lastAction = DateTime.Now;
                        break;
                    }

                    // Wait 5 seconds after moving
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        _States.CurrentDropState = DropState.StackItemsHangar;
                        break;
                    }

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds > 120)
                    {
                        Logging.Log("Drop", "Moving items timed out, clearing item locks", Logging.white);
                        Cache.Instance.DirectEve.UnlockItems();

                        _States.CurrentDropState = DropState.StackItemsHangar;
                        break;
                    }
                    break;

                case DropState.StackItemsHangar:
                    // Do not stack until 5 seconds after the cargo has cleared
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    // Stack everything
                    if (_hangar != null && _hangar.Window.IsReady)
                    {
                        Logging.Log("Drop", "Stacking items", Logging.white);
                        _hangar.StackAll();
                        _lastAction = DateTime.Now;
                        _States.CurrentDropState = DropState.WaitForStacking;
                    }
                    break;

                case DropState.WaitForStacking:
                    // Wait 5 seconds after stacking
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        Logging.Log("Drop", "Done", Logging.white);
                        _States.CurrentDropState = DropState.Done;
                        break;
                    }

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds > 120)
                    {
                        Logging.Log("Drop", "Stacking items timed out, clearing item locks", Logging.white);
                        Cache.Instance.DirectEve.UnlockItems();

                        Logging.Log("Drop", "Done", Logging.white);
                        _States.CurrentDropState = DropState.Done;
                        break;
                    }
                    break;
            }
        }
    }
}