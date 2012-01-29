using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuestorManager.Actions
{
    public enum StateDrop
    {
        Idle,
        Begin,
        OpenItemHangar,
        OpenCargo,
        MoveItems,
        AllItems,
        WaitForMove,
        StackItemsHangar,
        WaitForStacking,
        Done,
    }
}
