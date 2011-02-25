//------------------------------------------------------------------------------
//  <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//    Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that 
//    applies to this source code. (a copy can also be found at: 
//    http://www.thehackerwithin.com/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------
namespace Questor
{
    using DirectEve;

    public class ItemCache
    {
        public ItemCache(DirectItem item)
        {
            Id = item.ItemId;
            Name = item.Name;

            TypeId = item.TypeId ?? -1;
            GroupId = item.GroupId ?? -1;
            MarketGroupId = item.MarketGroupId ?? -1;
            
            Quantity = item.Quantity ?? -1;
            QuantitySold = 0;
        }

        public InvType InvType { get; set; }

        public long Id { get; private set; }
        public string Name { get; private set; }

        public int TypeId { get; private set; }
        public int GroupId { get; private set; }
        public int MarketGroupId { get; private set; }

        public int Quantity { get; private set; }
        public int QuantitySold { get; set; }

        public double? StationBuy { get; set; }
    }
}
