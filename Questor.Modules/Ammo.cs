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

    public class Ammo
    {
        public Ammo()
        {
        }

        public Ammo(XElement ammo)
        {
            TypeId = (int) ammo.Attribute("typeId");
            DamageType = (DamageType) Enum.Parse(typeof (DamageType), (string) ammo.Attribute("damageType"));
            Range = (int) ammo.Attribute("range");
            Quantity = (int) ammo.Attribute("quantity");
        }

        public int TypeId { get; private set; }
        public DamageType DamageType { get; private set; }
        public int Range { get; private set; }
        public int Quantity { get; set; }

        public Ammo Clone()
        {
            var ammo = new Ammo();
            ammo.TypeId = TypeId;
            ammo.DamageType = DamageType;
            ammo.Range = Range;
            ammo.Quantity = Quantity;
            return ammo;
        }
    }
}