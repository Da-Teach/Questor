using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traveler.Actions
{
        public enum StateBuyLPI
        {
            Idle,
            Begin,
            OpenItemHangar,
            OpenLpStore,
            FindOffer,
            CheckPetition,
            OpenMarket,
            BuyItems,
            AcceptOffer,
            Quatity,
            Done,
        }
}
