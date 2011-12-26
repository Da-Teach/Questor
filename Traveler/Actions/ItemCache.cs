//------------------------------------------------------------------------------
//  <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//    Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that 
//    applies to this source code. (a copy can also be found at: 
//    http://www.thehackerwithin.com/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------
namespace Traveler.Actions
{
    using System.Collections.Generic;
    using DirectEve;

    public class ItemCache
    {
        public ItemCache(DirectItem item, bool cacheRefineOutput)
        {
            Id = item.ItemId;
            Name = item.TypeName;

            TypeId = item.TypeId;
            GroupId = item.GroupId;
            MarketGroupId = item.MarketGroupId;
            PortionSize = item.PortionSize;
            
            Quantity = item.Quantity;
            QuantitySold = 0;

            RefineOutput = new List<ItemCache>();
            if (cacheRefineOutput)
            {
                foreach (var i in item.Materials)
                    RefineOutput.Add(new ItemCache(i, false));
            }
        }

        public InvType InvType { get; set; }

        public long Id { get; private set; }
        public string Name { get; private set; }

        public int TypeId { get; private set; }
        public int GroupId { get; private set; }
        public int MarketGroupId { get; private set; }
        public int PortionSize { get; private set; }

        public int Quantity { get; private set; }
        public int QuantitySold { get; set; }

        public double? StationBuy { get; set; }

        public List<ItemCache> RefineOutput { get; private set; }
    }
}
