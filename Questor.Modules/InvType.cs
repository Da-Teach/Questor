// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace Questor.Modules
{
    using System;
    using System.Xml.Linq;

    public class InvType
    {
        public InvType(XElement element)
        {
            Id = (int) element.Attribute("id");
            Name = (string) element.Attribute("name");
            GroupId = (int) element.Attribute("groupid");
            BasePrice = (double) element.Attribute("baseprice");
            Volume = (double) element.Attribute("volume");
            Capacity = (double) element.Attribute("capacity");
            PortionSize = (double) element.Attribute("portionsize");
            MedianBuy = (double?) element.Attribute("medianbuy");
            MedianSell = (double?) element.Attribute("mediansell");
            MedianAll = (double?) element.Attribute("medianall");
            LastUpdate = (DateTime?) element.Attribute("lastupdate");
        }

        public InvType(ItemCache item)
        {
            Id = item.TypeId;
            Name = item.Name;
            GroupId = item.GroupID;
            Volume = item.Volume;
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
        public DateTime? LastUpdate { get; set; }

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
            element.SetAttributeValue("medianbuy", MedianBuy);
            element.SetAttributeValue("mediansell", MedianSell);
            element.SetAttributeValue("medianall", MedianAll);
            element.SetAttributeValue("lastupdate", LastUpdate);
            return element;
        }
    }
}