namespace Traveler.Actions
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DirectEve;
    using DirectEve = global::Traveler.Common.DirectEve;
    using global::Traveler.Common;
    using global::Traveler.Domains;
    using global::Traveler.Module;


    class Grab
    {
        public StateGrab State { get; set; }

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
                case StateGrab.Idle:
                case StateGrab.Done:
                    break;

                case StateGrab.Begin:
                    State = StateGrab.OpenItemHangar;
                    break;

                case StateGrab.OpenItemHangar:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;

                    if ("Local Hangar" == Hangar)
                    {
                        // Is the hangar open?
                        if (_hangar.Window == null)
                        {
                            // No, command it to open
                            DirectEve.Instance.ExecuteCommand(DirectCmd.OpenHangarFloor);
                            
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
                            
                        }
                        if (!_hangar.IsReady)
                            break;
                    }
                    else if (Hangar != null)
                    {
                        if (_hangar.Window == null)
                        {
                            // No, command it to open
                            DirectEve.Instance.OpenCorporationHangar();
                            
                        }

                        if (!_hangar.IsReady)
                            break;
                    }

                        Logging.Log("Grab: Opening Hangar");

                        State = StateGrab.OpenCargo;
     
                    break;

                case StateGrab.OpenCargo:

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


                        Logging.Log("Grab: Opening Cargo Hold");

                        if (Item == 00)
                            State = StateGrab.AllItems;
                        else
                            State = StateGrab.MoveItems;

                    
                    break;

                case StateGrab.MoveItems:

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                        break;
                    if (Unit == 00)
                    {
                        var GrabItem = _hangar.Items.FirstOrDefault(i => (i.TypeId == Item));
                        if (GrabItem != null)
                        {
                            cargo.Add(GrabItem, GrabItem.Quantity);
                            Logging.Log("Grab: Moving all the items");
                            _lastAction = DateTime.Now;
                            State = StateGrab.WaitForItems;
                        }
                    }
                    else
                    {
                        var GrabItem = _hangar.Items.FirstOrDefault(i => (i.TypeId == Item));
                        if (GrabItem != null)
                        {
                            cargo.Add(GrabItem, Unit);
                            Logging.Log("Grab: Moving item");
                            _lastAction = DateTime.Now;
                            State = StateGrab.WaitForItems;
                        }
                    }

                    
                     break;


                case StateGrab.AllItems:

                     if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 2)
                         break;
                     var AllItem = _hangar.Items;
                     if (AllItem != null)
                     {
                      
                         cargo.Add(AllItem);
                         Logging.Log("Grab: Moving item");
                         _lastAction = DateTime.Now;
                         State = StateGrab.WaitForItems;
                     }


                     break;

                case StateGrab.WaitForItems:
                     // Wait 5 seconds after moving
                     if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                         break;


                     if (DirectEve.Instance.GetLockedItems().Count == 0)
                     {

                         Logging.Log("Grab: Done");
                         State = StateGrab.Done;
                         break;
                     }


                     break;

            }


        }



    }
}
