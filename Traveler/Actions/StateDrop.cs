using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traveler.Actions
{
    public enum StateDrop
    {
        Idle,
        Done,
        Begin,
        OpenItemHangar,
        OpenCargo,
        MoveItems,
        AllItems,
        WaitForItems,
    }
}
