using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuestorManager.Actions
{
    public enum StateGrab
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
