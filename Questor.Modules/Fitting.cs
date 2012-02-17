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

    public class FactionFitting
    {
        public FactionFitting()
        {
        }

        public FactionFitting(XElement factionfitting)
        {
            Faction = (string)factionfitting.Attribute("faction") ?? "";
            Fitting = (string)factionfitting.Attribute("fitting") ?? "";
            Settings.Instance.DroneTypeId = (int?)factionfitting.Attribute("dronetype") ?? Settings.Instance.DroneTypeId;
        }

        public string Faction { get; private set; }
        public string Fitting { get; private set; }
    }

    public class MissionFitting
    {
        public MissionFitting()
        {
        }

        public MissionFitting(XElement missionfitting)
        {
            Mission = (string)missionfitting.Attribute("mission") ?? "";
            Faction = (string)missionfitting.Attribute("faction") ?? "Default";
            Fitting = (string)missionfitting.Attribute("fitting") ?? "";
            Ship = (string)missionfitting.Attribute("ship") ?? "";
        }

        public string Mission { get; private set; }
        public string Faction { get; private set; }
        public string Fitting { get; private set; }
        public string Ship { get; private set; }
    }
}