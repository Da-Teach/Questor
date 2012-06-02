namespace UpdateInvTypes
{
    using System.IO;
    using System.Xml.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.Logging;

    public partial class UpdateInvTypesUI : Form
    {
        private bool _doUpdate;
        private bool _updating;
        private readonly List<InvType> _invTypes;

        public string InvTypesPath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\InvTypes.xml";
            }
        }

        public UpdateInvTypesUI()
        {
            InitializeComponent();

            _invTypes = new List<InvType>();

            XDocument invTypes = XDocument.Load(InvTypesPath);
            if (invTypes.Root != null)
                foreach (XElement element in invTypes.Root.Elements("invtype"))
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
                XDocument xdoc = new XDocument(new XElement("invtypes"));
                foreach (InvType type in _invTypes)
                    if (xdoc.Root != null) xdoc.Root.Add(type.Save());
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
                IEnumerable<InvType> types = _invTypes.Skip(Progress.Value).Take(Progress.Step).ToList();
                try
                {
                    IEnumerable<InvType> needUpdating = types.Where(type => !type.LastUpdate.HasValue || DateTime.Now.Subtract(type.LastUpdate.Value).TotalDays > 4).ToList();
                    if (chkfast.Checked)
                        needUpdating = types.Where(type => !type.LastUpdate.HasValue || DateTime.Now.Subtract(type.LastUpdate.Value).TotalMinutes > 2);

                    if (!needUpdating.Any())
                        return;

                    string queryString = string.Join("&", types.Select(type => "typeid=" + type.Id).ToArray());
                    queryString += "&usesystem=30000142"; //jita

                    string url = "http://api.eve-central.com/api/marketstat?" + queryString;
                    try
                    {
                        XDocument prices = XDocument.Load(url);

                        if (prices.Root != null && (string)prices.Root.Attribute("method") != "marketstat_xml")
                        {
                            Logging.Log("UpdateInvTypes", "Invalid XML Method", Logging.red);
                            throw new Exception("Invalid XML method");
                        }

                        if (prices.Root != null)
                            foreach (XElement type in prices.Root.Element("marketstat").Elements("type"))
                            {
                                int id = (int)type.Attribute("id");
                                InvType invType = types.Single(t => t.Id == id);

                                XElement all = type.Element("all");
                                if (all != null)
                                    invType.MedianAll = (double?)all.Element("median");

                                XElement buy = type.Element("buy");
                                if (buy != null)
                                {
                                    invType.MedianBuy = (double?)buy.Element("median");
                                    invType.MaxBuy = (double?)buy.Element("max");
                                }

                                XElement sell = type.Element("sell");
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
                        Logging.Log("UpdateInvTypes", "Invalid XML Method in marketstat_xml [" + ex.Message + "]", Logging.red);
                        return;
                    }
                }
                finally
                {
                    Progress.Value += types.Count();

                    if (Progress.Value >= _invTypes.Count - 1)
                    {
                        _doUpdate = false;

                        XDocument xdoc = new XDocument(new XElement("invtypes"));
                        foreach (InvType type in _invTypes)
                            if (xdoc.Root != null) xdoc.Root.Add(type.Save());
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