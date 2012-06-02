// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Globalization;
using Questor.Modules.Caching;

namespace Questor.Modules.Lookup
{
    public class InvType
    {
        public static List<string> Minerals = new List<string>()
                                               {
                                                  "Morphite",
                                                  "Megacyte",
                                                  "Zydrine",
                                                  "Nocxium",
                                                  "Isogen",
                                                  "Mexallon",
                                                  "Pyerite",
                                                  "Tritanium"
                                               };

        public InvType(XElement element)
        {
            Id = (int)element.Attribute("id");
            Name = (string)element.Attribute("name");
            GroupId = (int)element.Attribute("groupid");
            BasePrice = (double)element.Attribute("baseprice");
            Volume = (double)element.Attribute("volume");
            Capacity = (double)element.Attribute("capacity");
            PortionSize = (double)element.Attribute("portionsize");
            MedianBuy = (double?)element.Attribute("medianbuy");
            MedianSell = (double?)element.Attribute("mediansell");
            MedianAll = (double?)element.Attribute("medianall");
            MinSell = (double?)element.Attribute("minsell");
            MaxBuy = (double?)element.Attribute("maxbuy");
            ReprocessValue = (double?)element.Attribute("reprocess");
            LastUpdate = (DateTime?)element.Attribute("lastupdate");
            Reprocess = new Dictionary<string, double?>();
            foreach (string m in Minerals)
                Reprocess.Add(m, (double?)element.Attribute(m));
        }

        public InvType(ItemCache item)
        {
            Id = item.TypeId;
            Name = item.Name;
            GroupId = item.GroupID;
            Volume = item.Volume;
            BasePrice = item.BasePrice;
            Capacity = item.Capacity;
            PortionSize = item.PortionSize;
            LastUpdate = DateTime.MinValue;
            Reprocess = new Dictionary<string, double?>();
            foreach (string m in Minerals)
                Reprocess.Add(m, null);
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int GroupId { get; set; }

        public double BasePrice { get; set; }

        public double Volume { get; set; }

        public double Capacity { get; set; }

        public double PortionSize { get; set; }

        public double? MedianSell { get; set; }

        public double? MedianBuy { get; set; }

        public double? MedianAll { get; set; }

        public double? MinSell { get; set; }

        public double? MaxBuy { get; set; }

        public double? ReprocessValue { get; set; }

        public DateTime? LastUpdate { get; set; }

        public Dictionary<string, double?> Reprocess { get; set; }

        public XElement Save()
        {
            XElement element = new XElement("invtype");
            element.SetAttributeValue("id", Id);
            element.SetAttributeValue("name", Name);
            element.SetAttributeValue("groupid", GroupId);
            element.SetAttributeValue("baseprice", BasePrice);
            element.SetAttributeValue("volume", Volume);
            element.SetAttributeValue("capacity", Capacity);
            element.SetAttributeValue("portionsize", PortionSize);
            if (MedianBuy.HasValue && MedianBuy.Value > 0)
                element.SetAttributeValue("medianbuy", MedianBuy.Value.ToString("0.00", CultureInfo.InvariantCulture));
            if (MedianSell.HasValue && MedianSell.Value > 0)
                element.SetAttributeValue("mediansell", MedianSell.Value.ToString("0.00", CultureInfo.InvariantCulture));
            if (MedianAll.HasValue && MedianAll.Value > 0)
                element.SetAttributeValue("medianall", MedianAll.Value.ToString("0.00", CultureInfo.InvariantCulture));
            if (ReprocessValue.HasValue && ReprocessValue.Value > 0)
                element.SetAttributeValue("reprocess", ReprocessValue.Value.ToString("0.00", CultureInfo.InvariantCulture));
            foreach (string m in Minerals)
                if (Reprocess[m].HasValue && Reprocess[m] > 0)
                {
                    var d = Reprocess[m];
                    if (d != null) element.SetAttributeValue(m, d.Value);
                }

            if (MinSell.HasValue && MinSell.Value > 0)
                element.SetAttributeValue("minsell", MinSell.Value.ToString("0.00", CultureInfo.InvariantCulture));
            if (MaxBuy.HasValue && MaxBuy.Value > 0)
                element.SetAttributeValue("maxbuy", MaxBuy.Value.ToString("0.00", CultureInfo.InvariantCulture));
            element.SetAttributeValue("lastupdate", LastUpdate);
            return element;
        }
    }
}