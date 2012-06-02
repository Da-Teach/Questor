// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Lookup
{
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Caching;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;

    public class CombatMissionSettings
    {
        private void LoadSpecificAmmo(IEnumerable<DamageType> damageTypes)
        {
            //AmmoToLoad.Clear();
            //AmmoToLoad.AddRange(Settings.Instance.Ammo.Where(a => damageTypes.Contains(a.DamageType)).Select(a => a.Clone()));
        }

        private void LoadCombatMissionXML(string html)
        {
            bool loadedAmmo = false;

            string missionName = Cache.Instance.FilterPath(Cache.Instance.Mission.Name);
            Cache.Instance.missionXmlPath = Path.Combine(Settings.Instance.MissionsPath, missionName + ".xml");

            Cache.Instance.MissionAmmo = new List<Ammo>();
            if (File.Exists(Cache.Instance.missionXmlPath))
            {
                Logging.Log("AgentInteraction", "Loading mission xml [" + missionName + "]", Logging.white);
                //
                // this loads the settings global to the mission, NOT individual pockets
                //
                try
                {
                    XDocument missionXml = XDocument.Load(Cache.Instance.missionXmlPath);
                    //load mission specific ammo and weapongroupid if specified in the mission xml
                    if (missionXml.Root != null)
                    {
                        XElement ammoTypes = missionXml.Root.Element("missionammo");
                        if (ammoTypes != null)
                            foreach (XElement ammo in ammoTypes.Elements("ammo"))
                                Cache.Instance.MissionAmmo.Add(new Ammo(ammo));

                        Cache.Instance.MissionWeaponGroupId = (int?)missionXml.Root.Element("weaponGroupId") ?? 0;
                        Cache.Instance.MissionUseDrones = (bool?)missionXml.Root.Element("useDrones");
                    }
                    //should this default to true?
                    //Cache.Instance.MissionDroneTypeID = (int?)missionXml.Root.Element("DroneTypeId") ?? Settings.Instance.DroneTypeId;
                    IEnumerable<DamageType> damageTypes = missionXml.XPathSelectElements("//damagetype").Select(
                            e => (DamageType)Enum.Parse(typeof(DamageType), (string)e, true)).ToList();
                    if (damageTypes.Any())
                    {
                        LoadSpecificAmmo(damageTypes.Distinct());
                        loadedAmmo = true;
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log("AgentInteraction", "Error parsing damage types for mission [" + Cache.Instance.Mission.Name + "], " + ex.Message, Logging.white);
                }
            }

            if (!loadedAmmo)
            {
                Logging.Log("AgentInteraction", "Detecting damage type for [" + missionName + "]", Logging.white);
                Cache.Instance.DamageType = GetMissionDamageType(html);
                LoadSpecificAmmo(new[] { Cache.Instance.DamageType });
            }
        }

        private DamageType GetMissionDamageType(string html)
        {
            // We are going to check damage types
            var logoRegex = new Regex("img src=\"factionlogo:(?<factionlogo>\\d+)");

            Match logoMatch = logoRegex.Match(html);
            if (logoMatch.Success)
            {
                var logo = logoMatch.Groups["factionlogo"].Value;

                // Load faction xml
                string factionsXML = Path.Combine(Settings.Instance.Path, "Factions.xml");
                try
                {
                    XDocument xml = XDocument.Load(factionsXML);
                    if (xml.Root != null)
                    {
                        XElement faction = xml.Root.Elements("faction").FirstOrDefault(f => (string)f.Attribute("logo") == logo);
                        if (faction != null)
                            return (DamageType)Enum.Parse(typeof(DamageType), (string)faction.Attribute("damagetype"));
                    }
                    else
                    {
                        Logging.Log("CombatMissionSettings", "ERROR! unable to read [" + factionsXML + "]  no root element named <faction> ERROR!", Logging.red);
                    }
                }
                catch (Exception ex)
                {
                  Logging.Log("CombatMissionSettings","ERROR! unable to find [" + factionsXML + "] ERROR! [" + ex.Message + "]",Logging.red);
                }
            }

            return DamageType.EM;
        }
    }
}