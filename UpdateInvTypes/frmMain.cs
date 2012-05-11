using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UpdateInvTypes
{
    using System.IO;
    using System.Xml.Linq;
    using Questor;

    public partial class frmMain : Form
    {
        private bool _doUpdate;
        private bool _updating;
        private List<InvType> _invTypes;
        
        public string InvTypesPath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\InvTypes.xml";
            }
        }

        public frmMain()
        {
            InitializeComponent();

            _invTypes = new List<InvType>();
            
            var invTypes = XDocument.Load(InvTypesPath);
            foreach (var element in invTypes.Root.Elements("invtype"))
                _invTypes.Add(new InvType(element));

            Progress.Step = 50;
            Progress.Value = 0;
            Progress.Minimum = 0;
            Progress.Maximum = _invTypes.Count;
        }

        private void Update_Click(object sender, EventArgs e)
        {
            _doUpdate ^= true;
            UpdateButton.Text = _doUpdate ? "Stop" : "Update";

            if (!_doUpdate)
            {
                var xdoc = new XDocument(new XElement("invtypes"));
                foreach (var type in _invTypes)
                    xdoc.Root.Add(type.Save());
                xdoc.Save(InvTypesPath);
            }
        }

        private void tUpdate_Tick(object sender, EventArgs e)
        {
            // This is what you get if your too bored to setup an actual thread and do UI-invoke shit
            if (!_doUpdate)
                return;

            if (_updating)
                return;

            _updating = true;
            try
            {
                var types = _invTypes.Skip(Progress.Value).Take(Progress.Step);
                try
                {
                    var needUpdating = types.Where(type => !type.LastUpdate.HasValue || DateTime.Now.Subtract(type.LastUpdate.Value).TotalDays > 4 );
                    if (chkfast.Checked)
                        needUpdating = types.Where(type => !type.LastUpdate.HasValue || DateTime.Now.Subtract(type.LastUpdate.Value).TotalMinutes > 2);

                    if (needUpdating.Count() == 0)
                        return;

                    var queryString = string.Join("&", types.Select(type => "typeid=" + type.Id).ToArray());
                    queryString += "&usesystem=30000142"; //jita

                    var url = "http://api.eve-central.com/api/marketstat?" + queryString;
                    try
                    {
                        var prices = XDocument.Load(url);

                        if ((string)prices.Root.Attribute("method") != "marketstat_xml")
                            throw new Exception("Invalid XML method");

                        foreach (var type in prices.Root.Element("marketstat").Elements("type"))
                        {
                            var id = (int)type.Attribute("id");
                            var invType = types.Single(t => t.Id == id);

                            var all = type.Element("all");
                            if (all != null)
                                invType.MedianAll = (double?)all.Element("median");

                            var buy = type.Element("buy");
                            if (buy != null)
                            {
                                invType.MedianBuy = (double?)buy.Element("median");
                                invType.MaxBuy = (double?)buy.Element("max");
                            }

                            var sell = type.Element("sell");
                            if (sell != null)
                            {
                                invType.MedianSell = (double?)sell.Element("median");
                                invType.MinSell = (double?)sell.Element("min");
                            }

                            invType.LastUpdate = DateTime.Now;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                finally
                {
                    Progress.Value += types.Count();

                    if (Progress.Value >= _invTypes.Count - 1)
                    {
                        _doUpdate = false;

                        var xdoc = new XDocument(new XElement("invtypes"));
                        foreach (var type in _invTypes)
                            xdoc.Root.Add(type.Save());
                        xdoc.Save(InvTypesPath);

                        UpdateButton.Text = _doUpdate ? "Stop" : "Update";
                    }
                }
            }
            finally
            {
                _updating = false;
            }
        }
    }
}
