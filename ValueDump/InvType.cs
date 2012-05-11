using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;

namespace Questor
{
    public class InvType
    {
        public static List<string> Minerals = new List<string>() { "Morphite", "Megacyte", "Zydrine", "Nocxium", "Isogen", "Mexallon", "Pyerite", "Tritanium" };

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
            ReprocessValue = (double?)element.Attribute("reprocess");
            LastUpdate = (DateTime?)element.Attribute("lastupdate");
            Reprocess = new Dictionary<string, double?>();
            foreach (var m in Minerals)
                Reprocess.Add(m, (double?)element.Attribute(m));
        }

        public InvType(ItemCache item)
        {
            Id = item.TypeId;
            Name = item.Name;
            GroupId = item.GroupId;
            Volume = item.Volume;
            BasePrice = item.BasePrice;
            Capacity = item.Capacity;
            PortionSize = item.PortionSize;
            LastUpdate = DateTime.MinValue;
            Reprocess = new Dictionary<string, double?>();
            foreach (var m in Minerals)
                Reprocess.Add(m, null);
        }

        public XElement Save()
        {
            var element = new XElement("invtype");
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
            foreach (var m in Minerals)
                if (Reprocess[m].HasValue && Reprocess[m] > 0)
                    element.SetAttributeValue(m, Reprocess[m].Value);

            element.SetAttributeValue("lastupdate", LastUpdate);
            return element;
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
        public double? ReprocessValue { get; set; }
        public DateTime? LastUpdate { get; set; }
        public Dictionary<string, double?> Reprocess { get; set; }
    }
}
