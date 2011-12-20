using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traveler.Actions
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
