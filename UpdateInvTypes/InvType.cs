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
            LastUpdate = (DateTime?)element.Attribute("lastupdate");
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
            if (MinSell.HasValue && MinSell.Value > 0)
                element.SetAttributeValue("minsell", MinSell.Value.ToString("0.00", CultureInfo.InvariantCulture));
            if (MaxBuy.HasValue && MaxBuy.Value > 0)
                element.SetAttributeValue("maxbuy", MaxBuy.Value.ToString("0.00", CultureInfo.InvariantCulture));

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
        public double? MinSell { get; set; }
        public double? MaxBuy { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
