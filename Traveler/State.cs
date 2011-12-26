using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traveler
{
    public enum State
    {
        Idle,
        NextAction,
        Traveler,
        ValueDump,
        MakeShip,
        Drop,
        Grab,
        Buy,
        Sell,
    }
}
