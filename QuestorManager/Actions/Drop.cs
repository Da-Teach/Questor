namespace QuestorManager.Actions
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DirectEve;
    using DirectEve = global::QuestorManager.Common.DirectEve;
    using global::QuestorManager.Common;
    using global::QuestorManager.Domains;
    using global::QuestorManager.Module;
    using global::QuestorManager;



    class Drop
    {
        public StateDrop State { get; set; }

        public int Item { get; set; }
        public int Unit { get; set; }
        public string Hangar { get; set; }



        private DateTime _lastAction;



        public void ProcessState()
        {
            DirectContainer _hangar = null;

            var cargo = DirectEve.Instance.GetShipsCargo();

            if ("Local Hangar" == Hangar)
                 _hangar = DirectEve.Instance.GetItemHangar();
            else if ("Ship Hangar" == Hangar)
                 _hangar = DirectEve.Instance.GetShipHangar();
            else
                 _hangar = DirectEve.Instance.GetCorporationHangar(Hangar);


            switch (State)
            {
                case StateDrop.Idle:
                case StateDrop.Done:
                    break;

                case StateDrop.Begin:
                    State = StateDrop.OpenItemHangar;
                    break;

                case StateDrop.OpenItemHangar:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    if ("Local Hangar" == Hangar)
                    {
                        // Is the hangar open?
                        if (_hangar.Window == null)
                        {
                            // No, command it to open
                            DirectEve.Instance.ExecuteCommand(DirectCmd.OpenHangarFloor);
                            break;
                        }
                        if (!_hangar.IsReady)
                            break;
            
                    }
                    else if ("Ship Hangar" == Hangar)
                    {
                        if (_hangar.Window == null)
                        {
                            // No, command it to open
                            DirectEve.Instance.ExecuteCommand(DirectCmd.OpenShipHangar);
                            break;
                        }
                        if (!_hangar.IsReady)
                            break;
                    }
                    else 
                    {
                        if (_hangar.Window == null)
                            {
                                // No, command it to open
                                DirectEve.Instance.OpenCorporationHangar();
                                break;
                            }

                        if (!_hangar.IsReady)
                            break;
                    }

                    Logging.Log("Drop: Opening Hangar");
                    State = StateDrop.OpenCargo;
                    break;

                case StateDrop.OpenCargo:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;
                    // Is cargo open?
                    if (cargo.Window == null)
                    {
                        // No, command it to open
                        DirectEve.Instance.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                        break;
                    }

                    if (!cargo.IsReady)
                        break;

                    Logging.Log("Drop: Opening Cargo Hold");
                    if (Item == 00)
                        State = StateDrop.AllItems;
                    else
                        State = StateDrop.MoveItems;

                    break;

                case StateDrop.MoveItems:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    if (Unit == 00)
                    {
                        var DropItem = cargo.Items.FirstOrDefault(i => (i.TypeId == Item));
                        if (DropItem != null)
                        {
                            _hangar.Add(DropItem, DropItem.Quantity);
                            Logging.Log("Drop: Moving all the items");
                            _lastAction = DateTime.Now;
                            State = StateDrop.WaitForMove;
                        }
                    }
                    else
                    {
                        var DropItem = cargo.Items.FirstOrDefault(i => (i.TypeId == Item));
                        if (DropItem != null)
                        {
                            _hangar.Add(DropItem, Unit);
                            Logging.Log("Drop: Moving item");
                            _lastAction = DateTime.Now;
                            State = StateDrop.WaitForMove;
                        }
                    }

                    break;

                case StateDrop.AllItems:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                        var AllItem = cargo.Items;
                        if (AllItem != null)
                        {
                            _hangar.Add(AllItem);
                            Logging.Log("Drop: Moving item");
                            _lastAction = DateTime.Now;
                            State = StateDrop.WaitForMove;
                        }


                    break;

                case StateDrop.WaitForMove:
                    if (cargo.Items.Count != 0)
                    {
                        _lastAction = DateTime.Now;
                        break;
                    }

                    // Wait 5 seconds after moving
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (DirectEve.Instance.GetLockedItems().Count == 0)
                    {
                        
                        State = StateDrop.StackItemsHangar;
                        break;
                    }

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds > 120)
                    {
                        Logging.Log("Drop: Moving items timed out, clearing item locks");
                        DirectEve.Instance.UnlockItems();

                        
                        State = StateDrop.StackItemsHangar;
                        break;
                    }
                    break;

                case StateDrop.StackItemsHangar:
                    // Dont stack until 5 seconds after the cargo has cleared
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    // Stack everything
                    if (_hangar != null)
                    {
                        Logging.Log("Drop: Stacking items");
                        _hangar.StackAll();
                        _lastAction = DateTime.Now;
                    }

                    

                    State = StateDrop.WaitForStacking;
                    break;


                case StateDrop.WaitForStacking:
                    // Wait 5 seconds after stacking
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (DirectEve.Instance.GetLockedItems().Count == 0)
                    {
                        Logging.Log("Drop: Done");
                        State = StateDrop.Done;
                        break;
                    }

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds > 120)
                    {
                        Logging.Log("Drop: Stacking items timed out, clearing item locks");
                        DirectEve.Instance.UnlockItems();

                        Logging.Log("Drop: Done");
                        State = StateDrop.Done;
                        break;
                    }
                    break;

                   

            }


        }



    }
}
