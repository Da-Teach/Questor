using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace QuestorSettings
{
    public partial class Form1 : Form
    {
        string LocalRuta = Application.StartupPath;
        

        public Form1()
        {

            InitializeComponent();
            Refres();
        }

        public object Refres()
        {
            cmbXML.Items.Clear();
            string LocalRuta = Application.StartupPath;
            System.IO.DirectoryInfo o = new System.IO.DirectoryInfo(LocalRuta);
            System.IO.FileInfo[] myfiles = null;

            myfiles = o.GetFiles("*.xml");
            for (int y = 0; y <= myfiles.Length - 1; y++)
            {
                cmbXML.Items.Add(myfiles[y].Name);
            }

            return true;
        }

        private void cmbXML_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblmessage.Text = "";
            string fic = LocalRuta + "\\" + cmbXML.SelectedItem.ToString();
            ReadXML(fic);

        }

        private void ReadXML(string File)
        {
            var xml = XDocument.Load(File).Root;

            cmbautoStart.Text = (string)xml.Element("autoStart");
            txtrandomDelay.Text = (string)xml.Element("randomDelay");
            txtminimumDelay.Text = (string)xml.Element("minimumDelay");
            txtwindowXPosition.Text = (string)xml.Element("windowXPosition");
            txtwindowYPosition.Text = (string)xml.Element("windowYPosition");
            txtbookmarkWarpOut.Text = (string)xml.Element("bookmarkWarpOut");
            cmbsaveLog.Text = (string)xml.Element("saveLog");
            txtmaxLineConsole.Text = (string)xml.Element("maxLineConsole");

            cmbenableStorylines.Text = (string)xml.Element("enableStorylines");
            txtagentName.Text = (string)xml.Element("agentName");
            txtmissionsPath.Text = (string)xml.Element("missionsPath");
            cmbwaitDecline.Text = (string)xml.Element("waitDecline");
            txtminStandings.Text = (string)xml.Element("minStandings");
            cmblootEverything.Text = (string)xml.Element("lootEverything");

            txtfactionblacklist.Text = "";
            var factionblacklist = xml.Element("factionblacklist");
            if (factionblacklist != null)
                foreach (var faction in factionblacklist.Elements("faction"))
                    txtfactionblacklist.Text += ((string)faction) + "\r\n";

            txtblacklist.Text = "";
            var blacklist = xml.Element("blacklist");
            if (blacklist != null)
                foreach (var mission in blacklist.Elements("mission"))
                    txtblacklist.Text += ((string)mission) + "\r\n";


            txtcombatShipName.Text = (string)xml.Element("combatShipName");
            txtmaximumHighValueTargets.Text = (string)xml.Element("maximumHighValueTargets");
            txtmaximumLowValueTargets.Text = (string)xml.Element("maximumLowValueTargets");
            txtorbitDistance.Text = (string)xml.Element("orbitDistance") ?? "";
            txtminimumAmmoCharges.Text = (string)xml.Element("minimumAmmoCharges");
            txtweaponGroupId.Text = (string)xml.Element("weaponGroupId");
            txtreserveCargoCapacity.Text = (string)xml.Element("reserveCargoCapacity");
            cmbspeedTank.Text = (string)xml.Element("speedTank") ?? "";
            txtmaximumWreckTargets.Text = (string)xml.Element("maximumWreckTargets");
            txtminimumPropulsionModuleDistance.Text = (string)xml.Element("minimumPropulsionModuleDistance");
            txtminimumPropulsionModuleCapacitor.Text = (string)xml.Element("minimumPropulsionModuleCapacitor");

            var i = -1;
            var ammoTypes = xml.Element("ammoTypes");
            DGV.Rows.Clear();
            if (ammoTypes != null)
                foreach (var ammo in ammoTypes.Elements("ammoType"))
                {
                    i = i + 1;
                    DGV.Rows.Add();
                    DGV.Rows[i].Cells["Column1"].Value = (string)ammo.Attribute("typeId");
                    DGV.Rows[i].Cells["Column2"].Value = (string)ammo.Attribute("damageType");
                    DGV.Rows[i].Cells["Column3"].Value = (string)ammo.Attribute("range");
                    DGV.Rows[i].Cells["Column4"].Value = (string)ammo.Attribute("quantity");
                }

            var q = -1;
            var factionfittings = xml.Element("factionfittings");
            DGV1.Rows.Clear();
            if (factionfittings != null)
                foreach (var fittings in factionfittings.Elements("factionfitting"))
                {
                    q = q + 1;
                    DGV1.Rows.Add();
                    DGV1.Rows[q].Cells["Column5"].Value = (string)fittings.Attribute("faction");
                    DGV1.Rows[q].Cells["Column6"].Value = (string)fittings.Attribute("fitting");
                }

            txtsalvageShipName.Text = (string)xml.Element("salvageShipName");
            txtlootHangar.Text = (string)xml.Element("lootHangar");
            txtammoHangar.Text = (string)xml.Element("ammoHangar");
            cmbcreateSalvageBookmarks.Text = (string)xml.Element("createSalvageBookmarks");
            txtbookmarkPrefix.Text = (string)xml.Element("bookmarkPrefix");
            txtminimumWreckCount.Text = (string)xml.Element("minimumWreckCount");
            cmbafterMissionSalvaging.Text = (string)xml.Element("afterMissionSalvaging");
            cmbunloadLootAtStation.Text = (string)xml.Element("unloadLootAtStation");


            txtactivateRepairModules.Text = (string)xml.Element("activateRepairModules");
            txtdeactivateRepairModules.Text = (string)xml.Element("deactivateRepairModules");
            txtminimumShieldPct.Text = (string)xml.Element("minimumShieldPct");
            txtminimumArmorPct.Text = (string)xml.Element("minimumArmorPct");
            txtminimumCapacitorPct.Text = (string)xml.Element("minimumCapacitorPct");
            txtsafeShieldPct.Text = (string)xml.Element("safeShieldPct");
            txtsafeArmorPct.Text = (string)xml.Element("safeArmorPct");
            txtsafeCapacitorPct.Text = (string)xml.Element("safeCapacitorPct");

            cmbuseDrones.Text = (string)xml.Element("useDrones");
            txtdroneTypeId.Text = (string)xml.Element("droneTypeId");
            txtdroneControlRange.Text = (string)xml.Element("droneControlRange");
            txtdroneMinimumShieldPct.Text = (string)xml.Element("droneMinimumShieldPct");
            txtdroneMinimumArmorPct.Text = (string)xml.Element("droneMinimumArmorPct");
            txtdroneMinimumCapacitorPct.Text = (string)xml.Element("droneMinimumCapacitorPct");
            txtdroneRecallShieldPct.Text = (string)xml.Element("droneRecallShieldPct");
            txtdroneRecallArmorPct.Text = (string)xml.Element("droneRecallArmorPct");
            txtdroneRecallCapacitorPct.Text = (string)xml.Element("droneRecallCapacitorPct");
            txtlongRangeDroneRecallShieldPct.Text = (string)xml.Element("longRangeDroneRecallShieldPct");
            txtlongRangeDroneRecallArmorPct.Text = (string)xml.Element("longRangeDroneRecallArmorPct");
            txtlongRangeDroneRecallCapacitorPct.Text = (string)xml.Element("longRangeDroneRecallCapacitorPct");


            txtinvasionMinimumDelay.Text = (string)xml.Element("invasionMinimumDelay");
            txtinvasionRandomDelay.Text = (string)xml.Element("invasionRandomDelay");
            txtfrigateInvasionLimit.Text = (string)xml.Element("frigateInvasionLimit");
            txtcruiserInvasionLimit.Text = (string)xml.Element("cruiserInvasionLimit");
            txtbattlecruiserInvasionLimit.Text = (string)xml.Element("battlecruiserInvasionLimit");
            txtbattleshipInvasionLimit.Text = (string)xml.Element("battleshipInvasionLimit");


        }

        private void cmdSave_Click(object sender, EventArgs e)
        {

            string fic = LocalRuta + "\\" + cmbXML.Text;
            string strXml = "<Settings>";

            //Settings general
            strXml += "<!--Settings General-->";
            strXml += "<autoStart>" + cmbautoStart.Text + "</autoStart>";
            strXml += "<randomDelay>" + txtrandomDelay.Text + "</randomDelay>";
            strXml += "<minimumDelay>" + txtminimumDelay.Text + "</minimumDelay>";
            strXml += "<windowXPosition>" + txtwindowXPosition.Text + "</windowXPosition>";
            strXml += "<windowYPosition>" + txtwindowYPosition.Text + "</windowYPosition>";
            strXml += "<bookmarkWarpOut>" + txtbookmarkWarpOut.Text + "</bookmarkWarpOut>";
            strXml += "<saveLog>" + cmbsaveLog.Text + "</saveLog>";
            strXml += "<maxLineConsole>" + txtmaxLineConsole.Text + "</maxLineConsole>";

            //Settings Mission
            strXml += "<!--Settings Mission-->";
            strXml += "<enableStorylines>" + cmbenableStorylines.Text + "</enableStorylines>";
            strXml += "<agentName>" + txtagentName.Text + "</agentName>";
            strXml += "<missionsPath>" + txtmissionsPath.Text + "</missionsPath>";
            strXml += "<waitDecline>" + cmbwaitDecline.Text + "</waitDecline>";
            strXml += "<minStandings>" + txtminStandings.Text + "</minStandings>";
            strXml += "<lootEverything>" + cmblootEverything.Text + "</lootEverything>";

            strXml += "<factionblacklist>";
            foreach (string linea in txtfactionblacklist.Lines)
                strXml += "<faction>" + linea + "</faction>";
            strXml += "</factionblacklist>";

            strXml += "<blacklist>";
            foreach (string linea in txtblacklist.Lines)
                strXml += "<mission>" + linea + "</mission>";
            strXml += "</blacklist>";


            //Settings Combat
            strXml += "<!--Settings Combat-->";
            strXml += "<combatShipName>" + txtcombatShipName.Text + "</combatShipName>";
            strXml += "<maximumHighValueTargets>" + txtmaximumHighValueTargets.Text + "</maximumHighValueTargets>";
            strXml += "<maximumLowValueTargets>" + txtmaximumLowValueTargets.Text + "</maximumLowValueTargets>";
            strXml += "<orbitDistance>" + txtorbitDistance.Text + "</orbitDistance>";
            strXml += "<minimumAmmoCharges>" + txtminimumAmmoCharges.Text + "</minimumAmmoCharges>";
            strXml += "<weaponGroupId>" + txtweaponGroupId.Text + "</weaponGroupId>";
            strXml += "<reserveCargoCapacity>" + txtreserveCargoCapacity.Text + "</reserveCargoCapacity>";
            strXml += "<speedTank>" + cmbspeedTank.Text + "</speedTank>";
            strXml += "<maximumWreckTargets>" + txtmaximumWreckTargets.Text + "</maximumWreckTargets>";
            strXml += "<minimumPropulsionModuleDistance>" + txtminimumPropulsionModuleDistance.Text + "</minimumPropulsionModuleDistance>";
            strXml += "<minimumPropulsionModuleCapacitor>" + txtminimumPropulsionModuleCapacitor.Text + "</minimumPropulsionModuleCapacitor>";

            strXml += "<ammoTypes>";
            for (int o = 0; o < DGV.RowCount - 1; o++)
                strXml += "<ammoType typeId='" + Convert.ToString(DGV.Rows[o].Cells["Column1"].Value) + "' damageType='" + Convert.ToString(DGV.Rows[o].Cells["Column2"].Value) + "' range='" + Convert.ToString(DGV.Rows[o].Cells["Column3"].Value) + "' quantity='" + Convert.ToString(DGV.Rows[o].Cells["Column4"].Value) + "' />";
            strXml += "</ammoTypes>";

            strXml += "<factionfittings>";
            for (int o = 0; o < DGV1.RowCount - 1; o++)
                strXml += "<factionfitting faction='" + Convert.ToString(DGV1.Rows[o].Cells["Column6"].Value) + "' fitting='" + Convert.ToString(DGV1.Rows[o].Cells["Column5"].Value) + "' />";
            strXml += "</factionfittings>";


            //Settings Salvage
            strXml += "<!--Settings Salvage-->";
            strXml += "<salvageShipName>" + txtsalvageShipName.Text + "</salvageShipName>";
            strXml += "<lootHangar>" + txtlootHangar.Text + "</lootHangar>";
            strXml += "<ammoHangar>" + txtammoHangar.Text + "</ammoHangar>";
            strXml += "<createSalvageBookmarks>" + cmbcreateSalvageBookmarks.Text + "</createSalvageBookmarks>";
            strXml += "<bookmarkPrefix>" + txtbookmarkPrefix.Text + "</bookmarkPrefix>";
            strXml += "<minimumWreckCount>" + txtminimumWreckCount.Text + "</minimumWreckCount>";
            strXml += "<afterMissionSalvaging>" + cmbafterMissionSalvaging.Text + "</afterMissionSalvaging>";
            strXml += "<unloadLootAtStation>" + cmbunloadLootAtStation.Text + "</unloadLootAtStation>";


            //Settings Defense
            strXml += "<!--Settings Defense-->";
            strXml += "<activateRepairModules>" + txtactivateRepairModules.Text + "</activateRepairModules>";
            strXml += "<deactivateRepairModules>" + txtdeactivateRepairModules.Text + "</deactivateRepairModules>";
            strXml += "<minimumShieldPct>" + txtminimumShieldPct.Text + "</minimumShieldPct>";
            strXml += "<minimumArmorPct>" + txtminimumArmorPct.Text + "</minimumArmorPct>";
            strXml += "<minimumCapacitorPct>" + txtminimumCapacitorPct.Text + "</minimumCapacitorPct>";
            strXml += "<safeShieldPct>" + txtsafeShieldPct.Text + "</safeShieldPct>";
            strXml += "<safeArmorPct>" + txtsafeArmorPct.Text + "</safeArmorPct>";
            strXml += "<safeCapacitorPct>" + txtsafeCapacitorPct.Text + "</safeCapacitorPct>";


            //Settings Drones
            strXml += "<!--Settings Drones-->";
            strXml += "<useDrones>" + cmbuseDrones.Text + "</useDrones>";
            strXml += "<droneTypeId>" + txtdroneTypeId.Text + "</droneTypeId>";
            strXml += "<droneControlRange>" + txtdroneControlRange.Text + "</droneControlRange>";
            strXml += "<droneMinimumShieldPct>" + txtdroneMinimumShieldPct.Text + "</droneMinimumShieldPct>";
            strXml += "<droneMinimumArmorPct>" + txtdroneMinimumArmorPct.Text + "</droneMinimumArmorPct>";
            strXml += "<droneMinimumCapacitorPct>" + txtdroneMinimumCapacitorPct.Text + "</droneMinimumCapacitorPct>";
            strXml += "<droneRecallShieldPct>" + txtdroneRecallShieldPct.Text + "</droneRecallShieldPct>";
            strXml += "<droneRecallArmorPct>" + txtdroneRecallArmorPct.Text + "</droneRecallArmorPct>";
            strXml += "<droneRecallCapacitorPct>" + txtdroneRecallCapacitorPct.Text + "</droneRecallCapacitorPct>";
            strXml += "<longRangeDroneRecallShieldPct>" + txtlongRangeDroneRecallShieldPct.Text + "</longRangeDroneRecallShieldPct>";
            strXml += "<longRangeDroneRecallArmorPct>" + txtlongRangeDroneRecallArmorPct.Text + "</longRangeDroneRecallArmorPct>";
            strXml += "<longRangeDroneRecallCapacitorPct>" + txtlongRangeDroneRecallCapacitorPct.Text + "</longRangeDroneRecallCapacitorPct>";


            //Settings Invasion
            strXml += "<!--Settings Invasion-->";
            strXml += "<invasionRandomDelay>" + txtinvasionRandomDelay.Text + "</invasionRandomDelay>";
            strXml += "<invasionMinimumDelay>" + txtinvasionMinimumDelay.Text + "</invasionMinimumDelay>";
            strXml += "<frigateInvasionLimit>" + txtfrigateInvasionLimit.Text + "</frigateInvasionLimit>";
            strXml += "<cruiserInvasionLimit>" + txtcruiserInvasionLimit.Text + "</cruiserInvasionLimit>";
            strXml += "<battlecruiserInvasionLimit>" + txtbattlecruiserInvasionLimit.Text + "</battlecruiserInvasionLimit>";
            strXml += "<battleshipInvasionLimit>" + txtbattleshipInvasionLimit.Text + "</battleshipInvasionLimit>";


            strXml += "</Settings>";

            XElement xml = XElement.Parse(strXml);
            XDocument FileXml = new XDocument(xml);
            FileXml.Save(fic);
            lblmessage.ForeColor = System.Drawing.ColorTranslator.FromHtml("Green");
            lblmessage.Text = "Save as " + cmbXML.Text;

        }

        private void cmbuseDrones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ("false" == cmbuseDrones.SelectedItem.ToString())
                txtdroneTypeId.Text = "0";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Auto start when Questor is loaded -->");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Auto start when Questor is loaded -->");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Random delay between missions -->");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("  <!-- X Position of Questor Window -->");
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("  <!-- Y Position of Questor Window -->");
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Create Bookmark to the output line from the station to more than 500km -->");
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- The agent that you are running missions for -->");
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- missionsPath is the sub-directory where the mission xml's are located -->");
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Enable storyline botting (experimental) -->");
        }

        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Wait for mission decline timer to expire before declining again -->");
        }

        private void linkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Minimum standings to decline missions while the decline timer is active -->");
        }

        private void linkLabel12_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(@"<!-- Loot everything or only mission loot, if turned off empty loot-actions are ignored 
       and the action is finished as soon as the mission item is in the ship's cargo, if turned
       on then the bot will loot all wrecks/cans before finishing the loot-action -->");
        }

        private void linkLabel13_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- List of factions which the bot will try to avoid -->");
        }

        private void linkLabel14_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- List of missions which the bot will always decline -->");
        }

        private void linkLabel15_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Ship used for combat missions, empty means current ship -->");
        }

        private void linkLabel16_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Maximum number of targets per value -->");
        }

        private void linkLabel17_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Maximum number of targets per value -->");
        }

        private void linkLabel18_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- The range used by SpeedTank -->");
        }

        private void linkLabel19_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Weapon group (508 = Siege, 506 = Cruise, 771 = HAM, etc) -->");
        }

        private void linkLabel20_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(@"<!-- If the weapon has less then minimumAmmoCharges, the weapon is forced to reload 
       before attacking a new target -->");
        }

        private void linkLabel21_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Amount of cargo capacity to reserve, should be at least 80m3 if you are salvaging -->");
        }

        private void linkLabel22_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- When SpeedTank is enabled, Combat-module will try to orbit ships at orbitDistance -->");
        }

        private void linkLabel23_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- maximum number of wreck targets, at least 1 is required for salvager or tractor beam to work -->");
        }

        private void linkLabel24_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(@"<!-- When defining ammo here, do NOT define them by the damage that the ammo does 
       but define them based on the ammo you want to use when the bot faces rats weak 
       to the damageType, each damage type HAS to be present at least once! -->");
        }

        private void linkLabel26_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Propulsion mod settings, do not activate AB/MWD unless these settings are matched -->");
        }

        private void linkLabel27_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Propulsion mod settings, do not activate AB/MWD unless these settings are matched -->");
        }

        private void linkLabel25_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- List of Faction fittings -->");
        }

        private void linkLabel28_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Ship used for after mission salvaging, empty means current ship -->");
        }

        private void linkLabel29_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Corporation hangar to use for loot (empty is personal hangar) -->");
        }

        private void linkLabel30_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Corporation hangar to use for ammo (empty is personal hangar) -->");
        }

        private void linkLabel31_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(@"  <!-- Create salvage bookmarks once a pocket has been cleared and there are wrecks/cans left
                                        Note: All salvage bookmarks will be in this naming format:  BookmarkPrefix [time]
                                        Note: It will check all salvage bookmarks to see if the current spot has to be bookmarked 
                                        Warning: The bot could become slow with a lot of salvage-bookmarks, make sure that
                                        after mission salvaging is enabled or that you manually delete bookmarks! -->");
        }

        private void linkLabel32_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- All salvage bookmarks will be prefixed by this string, default: Salvage: -->");
        }

        private void linkLabel33_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Minimum amount of wrecks / unlooted containers needed for a bookmark to be created -->");
        }

        private void linkLabel34_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(@"  <!-- After-mission salvaging, this will cause the bot to visit all salvage bookmarks 
                                        with the salvage ship and pickup loot / salvage wrecks 
                                        Note: After mission salvaging will *only* take place if there are *no* accepted missions left ! -->");
        }

        private void linkLabel35_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(@"  <!-- Unload loot at station when doing after-mission salvaging (*only* when doing after-mission salvaging)
                                     Note: If this is disabled then the bot will use the isk/m3 algorithm used in missions to dump low-value loot -->");
        }

        private void linkLabel36_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Activate shield booster or armor rep when shields/armor drop below this percentage -->");
        }

        private void linkLabel37_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Deactivate shield booster or armor rep when shields/armor are above this percentage -->");
        }

        private void linkLabel38_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Minimum amount of shields before the bot panics and warps to safety -->");
        }

        private void linkLabel39_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Minimum amount of armor before the bot panics and warps to safety -->");
        }

        private void linkLabel40_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Minimum amount of capacitor before the bot panics and warps to safety -->");
        }

        private void linkLabel41_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Amount of shield that is seen as 'safe' to return to the mission -->");
        }

        private void linkLabel42_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Amount of armor that is seen as 'safe' to return to the mission -->");
        }

        private void linkLabel43_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Amount of capacitor that is seen as 'safe' to return to the mission -->");
        }

        private void linkLabel44_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Use drones, disable on ships without a drone bay! -->");
        }

        private void linkLabel45_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Drone type id to load (set to 0 to not reload drones) -->");
        }

        private void linkLabel46_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Maximum drone control range -->");
        }

        private void linkLabel47_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Do not launch drones if below this shield percentage -->");
        }

        private void linkLabel48_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Do not launch drones if below this armor percentage -->");
        }

        private void linkLabel49_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Do not launch drones if below this capacitor percentage -->");
        }

        private void linkLabel50_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Recall drones if below this shield percentage -->");
        }

        private void linkLabel51_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Recall drones if below this armor percentage -->");
        }

        private void linkLabel52_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Recall drones if below this capacitor percentage -->");
        }

        private void linkLabel53_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Recall drones if below this shield percentage and further away then half the drone control range -->");
        }

        private void linkLabel54_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Recall drones if below this armor percentage and further away then half the drone control range -->");
        }

        private void linkLabel55_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Recall drones if below this capacitor percentage and further away then half the drone control range -->");
        }

        private void linkLabel56_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Invasion resume delay -->");
        }

        private void linkLabel57_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Invasion resume delay -->");
        }

        private void linkLabel58_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Invasion limits, panic if these number of ships enter your mission pocket -->");
        }

        private void linkLabel59_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Invasion limits, panic if these number of ships enter your mission pocket -->");
        }

        private void linkLabel60_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Invasion limits, panic if these number of ships enter your mission pocket -->");
        }

        private void linkLabel61_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Invasion limits, panic if these number of ships enter your mission pocket -->");
        }

        private void linkLabel62_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Save Log Console -->");
        }

        private void linkLabel63_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("<!-- Maximum number of lines in the external console -->");
        }

    }
}
