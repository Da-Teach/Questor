using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ValueDump
{
    public enum ValueDumpState
    {
        Idle,
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