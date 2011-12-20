using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traveler.Actions
{
    public enum StateBuy
    {
        Idle,
        Done,
        Begin,
        OpenMarket,
        LoadItem,
        BuyItem,
        WaitForItems,
    }
}
