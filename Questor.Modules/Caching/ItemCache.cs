//------------------------------------------------------------------------------
//  <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//    Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that
//    applies to this source code. (a copy can also be found at:
//    http://www.thehackerwithin.com/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------

namespace Questor.Modules.Caching
{
    using System.Collections.Generic;
    using DirectEve;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.Logging;

    /*
    public class ItemCache2
    {
        public ItemCache2(DirectItem item, bool cacheRefineOutput)
        {
            Id = item.ItemId;
            Name = item.TypeName;

            TypeId = item.TypeId;
            GroupId = item.GroupId;
            BasePrice = item.BasePrice;
            Volume = item.Volume;
            Capacity = item.Capacity;
            MarketGroupId = item.MarketGroupId;
            PortionSize = item.PortionSize;

            Quantity = item.Quantity;
            QuantitySold = 0;

            RefineOutput = new List<ItemCache2>();
            if (cacheRefineOutput)
            {
                foreach (DirectItem i in item.Materials)
                    RefineOutput.Add(new ItemCache2(i, false));
            }
        }

        public InvType InvType { get; set; }

        public long Id { get; private set; }
        public string Name { get; private set; }

        public int TypeId { get; private set; }
        public int GroupId { get; private set; }
        public double BasePrice { get; private set; }
        public double Volume { get; private set; }
        public double Capacity { get; private set; }
        public int MarketGroupId { get; private set; }
        public int PortionSize { get; private set; }

        public int Quantity { get; private set; }
        public int QuantitySold { get; set; }

        public double? StationBuy { get; set; }

        public List<ItemCache2> RefineOutput { get; private set; }
    }
     * */

    public class ItemCache
    {
        public ItemCache(DirectItem item, bool cacheRefineOutput)
        {
            BasePrice = item.BasePrice;
            Capacity = item.Capacity;
            MarketGroupId = item.MarketGroupId;
            PortionSize = item.PortionSize;
            QuantitySold = 0;
            RefineOutput = new List<ItemCache>();
            if (cacheRefineOutput)
            {
                foreach (DirectItem i in item.Materials)
                    RefineOutput.Add(new ItemCache(i, false));
            }
        }

        //public InvType InvType { get; set; }

        //public long Id { get; private set; }
        //public string Name { get; private set; }

        //public int TypeId { get; private set; }
        public int GroupId { get; private set; }

        public double BasePrice { get; private set; }

        //public double Volume { get; private set; }
        public double Capacity { get; private set; }

        public int MarketGroupId { get; private set; }

        public int PortionSize { get; private set; }

        //public int Quantity { get; private set; }
        public int QuantitySold { get; set; }

        public double? StationBuy { get; set; }

        public List<ItemCache> RefineOutput { get; private set; }

        private readonly DirectItem _directItem;

        public ItemCache(DirectItem item)
        {
            _directItem = item;
        }

        public DirectItem DirectItem
        {
            get { return _directItem; }
        }

        public long Id
        {
            get { return _directItem.ItemId; }
        }

        public int TypeId
        {
            get { return _directItem.TypeId; }
        }

        public int GroupID
        {
            get { return _directItem.GroupId; }
        }

        public int Quantity
        {
            get { return _directItem.Quantity; }
        }

        public bool IsContraband
        {
            get
            {
                if (GroupID == 313) return true; // Drugs
                if (GroupID == 282) return true; // Toxic Waste
                if (GroupID == 283) return true; // Slaves
                if (GroupID == 280) return true; // Small Arms
                if (GroupID == 284) return true; // Ectoplasm
                return false;
            }
        }

        public bool IsAliveandWontFitInContainers
        {
            get
            {
                if (TypeId == 41) return true;      // Garbage
                if (TypeId == 42) return true;      // Spiced Wine
                if (TypeId == 42) return true;      // Antibiotics
                if (TypeId == 44) return true;      // Enriched Uranium
                if (TypeId == 45) return true;      // Frozen Plant Seeds
                if (TypeId == 3673) return true;    // Wheat
                if (TypeId == 3699) return true;    // Quafe
                if (TypeId == 3715) return true;    // Frozen Food
                if (TypeId == 3717) return true;    // Dairy Products
                if (TypeId == 3721) return true;    // Slaves
                if (TypeId == 3723) return true;    // Slaver Hound
                if (TypeId == 3725) return true;    // Livestock
                if (TypeId == 3727) return true;    // Plutonium
                if (TypeId == 3729) return true;    // Toxic Waste
                if (TypeId == 3771) return true;    // Ectoplasm
                if (TypeId == 3773) return true;    // Hydrochloric Acid
                if (TypeId == 3775) return true;    // Viral Agent
                if (TypeId == 3777) return true;    // Long-limb Roes
                if (TypeId == 3779) return true;    // Biomass
                if (TypeId == 3804) return true;    // VIPs
                if (TypeId == 3806) return true;    // Refugees
                if (TypeId == 3808) return true;    // Prisoners
                //if (TypeId == 3810) return true;    // Marines **Common Mission Completion Item
                if (TypeId == 12865) return true;   // Quafe Ultra
                if (TypeId == 13267) return true;   // Janitor
                if (TypeId == 17765) return true;   // Exotic Dancers
                if (TypeId == 22208) return true;   // Prostitute
                if (TypeId == 22209) return true;   // Refugee
                if (TypeId == 22210) return true;   // Cloned SOE officer
                //if (TypeId == 25373) return true;   // Militants **Common Mission Completion Item
                // people (all the different kinds - ugh?)
                return false;
            }
        }

        public bool IsTypicalMissionCompletionItem
        {
            get
            {
                if (TypeId == 25373) return true;   // Militants
                if (TypeId == 3810) return true;    // Marines
                if (TypeId == 2076) return true;    // Gate Key
                if (TypeId == 28260) return true;   // Zbikoki's Hacker Card
                if (TypeId == 3814) return true;    // Reports
                return false;
            }
        }

        public bool IsOre
        {   // GroupIDs listed in this order: Plagioclase	Spodumain	Kernite	Hedbergite	Arkonor	Bistot	Pyroxeres	Crokite	Jaspet	Omber	Scordite	Gneiss	Veldspar	Hemorphite	Dark Ochre Ice
            get { return GroupID == 458 || GroupID == 461 || GroupID == 457 || GroupID == 454 || GroupID == 450 || GroupID == 451 || GroupID == 459 || GroupID == 452 || GroupID == 456 || GroupID == 469 || GroupID == 460 || GroupID == 467 || GroupID == 462 || GroupID == 455 || GroupID == 453 || GroupID == 465; }
        }

        public bool IsLowEndMineral
        {   // Tritanium, pyerite, mexalon
            get { return TypeId == 34 || TypeId == 35 || TypeId == 36; }
        }

        public bool IsHighEndMineral
        {   // isogen, nocxium, zydrine, megacyte
            get { return TypeId == 37 || TypeId == 38 || TypeId == 39 || TypeId == 40; }
        }

        public bool IsScrapMetal
        {
            get { return TypeId == 30497 || TypeId == 15331; }
        }

        public bool IsMissionItem
        {   //Zbikoki's Hacker Card 28260, Reports 3814, Gate Key 2076, Militants 25373, Marines 3810
            get { return TypeId == 28260 || TypeId == 3814 || TypeId == 2076 || TypeId == 25373 || TypeId == 3810; }
        }

        public bool IsLootForShipFitting
        {   //Named 100mn Afterburner, Named Target Painter (PWNAGE),
            // this needs attention // fix me
            get { return TypeId == 1 || TypeId == 1; }
        }

        public bool IsBookmark
        {
            get { return TypeId == 51; }
        }

        public string Name
        {
            get { return _directItem.TypeName; }
        }

        public double Volume
        {
            get { return _directItem.Volume; }
        }

        public double TotalVolume
        {
            get { return InvType.Volume * Quantity; }
        }

        public InvType InvType
        {
            get
            {
                // Create a new InvType if its unknown
                if (!Cache.Instance.InvTypesById.ContainsKey(TypeId))
                {
                    Logging.Log("ItemCache", "Unknown TypeID for [" + Name + "][" + TypeId + "]", Logging.orange);
                    Cache.Instance.InvTypesById[TypeId] = new Questor.Modules.Lookup.InvType(this);
                }

                return Cache.Instance.InvTypesById[TypeId];
            }
        }

        public double? IskPerM3
        {
            get
            {
                if (InvType.MaxBuy == null)
                {
                    if (InvType.MinSell == null)
                    {
                        return null;
                    }
                    else
                    {
                        return InvType.MedianSell / InvType.Volume;
                    }
                }
                else
                {
                    return InvType.MedianBuy / InvType.Volume;
                }
            }
        }

        public double? Value
        {
            get
            {
                if (InvType.MaxBuy == null)
                {
                    if (InvType.MinSell == null)
                    {
                        return null;
                    }
                    else
                    {
                        return InvType.MedianSell;
                    }
                }
                else
                {
                    return InvType.MedianBuy;
                }
            }
        }
    }
}