using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traveler.Actions
{
    public enum ValueDumpState
    {
        Idle,
        Begin,
        Done,
        GetItems,
        UpdatePrices,
        NextItem,
        StartQuickSell,
        WaitForSellWindow,
        InspectOrder,
        WaitingToFinishQuickSell,
        CheckMineralPrices,
        GetMineralPrice,
        RefineItems,
        SaveMineralPrices
    }
}
