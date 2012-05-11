using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuestorManager.Actions
{
    public enum StateSell
    {
        Idle,
        Done,
        Begin,
        StartQuickSell,
        WaitForSellWindow,
        InspectOrder,
        WaitingToFinishQuickSell,
    }
}
