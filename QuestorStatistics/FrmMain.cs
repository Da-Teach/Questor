
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Text.RegularExpressions;
   
namespace QuestorStatistics
{

    public partial class FrmMain : Form
    {
        

        string Localpath = Application.StartupPath+"//log//";

        //-------------------------------------------------------------------------
        // Start the Form and look for the files .Statistics.log in the root
        // the two Listview are configured
        //-------------------------------------------------------------------------

        public FrmMain()
        {
            InitializeComponent();

            System.IO.DirectoryInfo o = new System.IO.DirectoryInfo(Localpath);
            System.IO.FileInfo[] myfiles = null;

            myfiles = o.GetFiles("*.CustomDatedStatistics.log");
            for (int y = 0; y <= myfiles.Length - 1; y++)
            {
                cmb1.Items.Add(myfiles[y].Name);
            }

            var _with1 = Lst1;
            _with1.View = View.Details;
            _with1.FullRowSelect = true;
            _with1.GridLines = true;
            _with1.LabelEdit = false;
            _with1.Columns.Clear();
            _with1.Items.Clear();

            _with1.Columns.Add("", 0, HorizontalAlignment.Right);
            _with1.Columns.Add("Date", 145, HorizontalAlignment.Left);
            _with1.Columns.Add("Mission", 190, HorizontalAlignment.Left);
            _with1.Columns.Add("Time", 40, HorizontalAlignment.Right);
            _with1.Columns.Add("Isk Bounty", 100, HorizontalAlignment.Right);
            _with1.Columns.Add("Bounty/Min", 80, HorizontalAlignment.Right);
            _with1.Columns.Add("Isk Loot", 100, HorizontalAlignment.Right);
            _with1.Columns.Add("Loot/Min", 60, HorizontalAlignment.Right);
            _with1.Columns.Add("LP", 80, HorizontalAlignment.Right);
            _with1.Columns.Add("LP/Min", 60, HorizontalAlignment.Right);
            _with1.Columns.Add("Total ISK/Min", 80, HorizontalAlignment.Right);
            _with1.Columns.Add("Lost Drones", 80, HorizontalAlignment.Right);
            _with1.Columns.Add("Ammo Consumption", 80, HorizontalAlignment.Right);
            _with1.Columns.Add("Ammo Value", 80, HorizontalAlignment.Right);
            _with1.Columns.Add("Panics", 80, HorizontalAlignment.Right);
            _with1.Columns.Add("Lowest Shield", 80, HorizontalAlignment.Right);
            _with1.Columns.Add("Lowest Armor", 80, HorizontalAlignment.Right);
            _with1.Columns.Add("Lowest Cap", 80, HorizontalAlignment.Right);
            _with1.Columns.Add("Repair Cycles", 80, HorizontalAlignment.Right);


            var _with2 = LstMision;
            _with2.View = View.Details;
            _with2.FullRowSelect = true;
            _with2.GridLines = true;
            _with2.LabelEdit = false;
            _with2.Columns.Clear();
            _with2.Items.Clear();

            _with2.Columns.Add("", 0, HorizontalAlignment.Right);
            _with2.Columns.Add("Mission", 240, HorizontalAlignment.Left);
            _with2.Columns.Add("Repet %", 60, HorizontalAlignment.Right);
            _with2.Columns.Add("Time", 100, HorizontalAlignment.Right);
            _with2.Columns.Add("Media Isk Bounty", 100, HorizontalAlignment.Right);
            _with2.Columns.Add("Media Isk Loot", 100, HorizontalAlignment.Right);
            _with2.Columns.Add("Media LP", 100, HorizontalAlignment.Right);
            _with2.Columns.Add("Media ISK/Min", 100, HorizontalAlignment.Right);
            _with2.Columns.Add("Lost Drones", 100, HorizontalAlignment.Right);
            _with2.Columns.Add("Media Ammo Consumption", 100, HorizontalAlignment.Right);
            _with2.Columns.Add("Media Ammo Value", 100, HorizontalAlignment.Right);
            return;
        }

        //-------------------------------
        // Open the selected log
        //-------------------------------
        private void cmb1_SelectedIndexChanged(object sender, EventArgs e)
        {

            Lst1.View = View.Details;
            Lst1.FullRowSelect = true;
            Lst1.GridLines = true;
            Lst1.LabelEdit = false;
            Lst1.Items.Clear();
            string fic = Localpath + "\\" + cmb1.SelectedItem.ToString();
            dynamic Data = null;
            bool Heading = false;
            bool AmmoStat = true;
            System.IO.StreamReader sr = new System.IO.StreamReader(fic);
            Heading = true;
            Cleardata();


            while (!(sr.Peek() == -1))
            {
                Data = Strings.Split(sr.ReadLine(), ";");

                if (Heading)
                {
                    Heading = false;

                }
                else
                {
                    ListViewItem item = new ListViewItem("");
                    if (Data[6] == "")
                    {
                        AmmoStat = false;
                    }
      
                    var _with3 = item;
                    _with3.SubItems.Add(Data[0]);
                    _with3.SubItems.Add(Data[1]);
                    _with3.SubItems.Add(Data[2]);
                    _with3.SubItems.Add(Strings.Format(Convert.ToDouble(Data[3]), "###,###,##0"));
                    _with3.SubItems.Add(Strings.Format(Convert.ToDouble(Data[3]) / Convert.ToDouble(Data[2]), "###,###,##0"));
                    _with3.SubItems.Add(Strings.Format(Convert.ToDouble(Data[4]), "###,###,##0"));
                    _with3.SubItems.Add(Strings.Format(Convert.ToDouble(Data[4]) / Convert.ToDouble(Data[2]), "###,###,##0"));
                    _with3.SubItems.Add(Strings.Format(Convert.ToDouble(Data[5]), "###,###,##0"));
                    _with3.SubItems.Add(Strings.Format(Convert.ToDouble(Data[5]) / Convert.ToDouble(Data[2]), "###,###,##0"));
                    _with3.SubItems.Add(Strings.Format((Convert.ToDouble(Data[3]) + Convert.ToDouble(Data[4])) / Convert.ToDouble(Data[2]), "###,###,##0"));
                    if (AmmoStat)
                    {
                        _with3.SubItems.Add(Strings.Format(Convert.ToDouble(Data[6]), "###,###,##0"));
                        _with3.SubItems.Add(Strings.Format(Convert.ToDouble(Data[7]), "###,###,##0"));
                        _with3.SubItems.Add(Strings.Format(Convert.ToDouble(Data[8]), "###,###,##0"));
                    }
                    else
                    {
                        _with3.SubItems.Add(Strings.Format(Convert.ToDouble("0"), "###,###,##0"));
                        _with3.SubItems.Add(Strings.Format(Convert.ToDouble("0"), "###,###,##0"));
                        _with3.SubItems.Add(Strings.Format(Convert.ToDouble("0"), "###,###,##0"));
                    }
                    AmmoStat = true;

                    _with3.SubItems.Add(Data[9]);
                    _with3.SubItems.Add(Data[10]);
                    _with3.SubItems.Add(Data[11]);
                    _with3.SubItems.Add(Data[12]);
                    _with3.SubItems.Add(Data[13]);
                    _with3.SubItems.Add(Data[14]);

                    Lst1.Items.Add(item);
  
                }
            }
            sr.Close();
            Preload();
            StatsMision();
        }

        //----------------------------
        // Day Selector
        //----------------------------
        private void CmbDay_SelectedIndexChanged_1(object sender, System.EventArgs e)
        {
         
         SortListUP();
         DayDataCalculated(CmbDia.SelectedItem.ToString());
         ChartoftheDay(CmbDia.SelectedItem.ToString());
        }

        //----------------------------
        // Month Selector
        //----------------------------
        private void cmbMes_SelectedIndexChanged_1(object sender, System.EventArgs e)
        {
           SortListUP();
           Mes(cmbMes.SelectedItem.ToString());
           ChartOfMonth(cmbMes.SelectedItem.ToString());
        }


        //---------------------------
        // Create the Chart of the Month
        //---------------------------
        public object ChartOfMonth(string Seleccion)
        {
     
            double ProfitTotal = 0;
            string SelectedDay = null;
            string AntDia = "";
            SortListDown();

            CHMes.Series[0].Points.Clear();
            for (var i = 0; i <= Lst1.Items.Count - 1; i++)
            {
                if (Seleccion == Strings.Mid(Lst1.Items[i].SubItems[1].Text, 4, 7))
                {
                    SelectedDay = Strings.Left(Lst1.Items[i].SubItems[1].Text, 10);

                    for (var e = 0; e <= Lst1.Items.Count - 1; e++)
                    {
                       
                        if (SelectedDay == Strings.Left(Lst1.Items[e].SubItems[1].Text, 10))
                        {
                            ProfitTotal = ProfitTotal + Convert.ToDouble(Lst1.Items[e].SubItems[4].Text) + Convert.ToDouble(Lst1.Items[e].SubItems[6].Text);
                        }
                    }

                    if (AntDia != Strings.Left(Lst1.Items[i].SubItems[1].Text, 10))
                    {
                        CHMes.Series[0].Points.AddXY(Strings.Left(Lst1.Items[i].SubItems[1].Text, 10), ProfitTotal);
                        AntDia = Strings.Left(Lst1.Items[i].SubItems[1].Text, 10);
                    }
                    ProfitTotal = 0;
                }
            }
            SortListUP();
            return true;
        }


        //-------------------------------
        // We calculate the data of the Month
        //-------------------------------
        public object Mes(string Seleccion)
        {

            double TotalBounty = 0;
            double TotalLoot = 0;
            double Nmission = 0;
            double TotalLP = 0;
            double LostDrones = 0;
            double AmmoConsumption = 0;
            double AmmoValue = 0;
            var cont1 = 0;
            var cont2 = 0;
            var var1 = "";

            for (var i = 0; i <= Lst1.Items.Count - 1; i++)
            {
                if (Seleccion == Strings.Mid(Lst1.Items[i].SubItems[1].Text, 4, 7))
                {
                    TotalBounty += Convert.ToDouble(Lst1.Items[i].SubItems[4].Text);
                    TotalLoot += Convert.ToDouble(Lst1.Items[i].SubItems[6].Text);
                    TotalLP += Convert.ToDouble(Lst1.Items[i].SubItems[8].Text);
                    LostDrones += Convert.ToDouble(Lst1.Items[i].SubItems[11].Text);
                    AmmoConsumption += Convert.ToDouble(Lst1.Items[i].SubItems[12].Text);
                    AmmoValue += Convert.ToDouble(Lst1.Items[i].SubItems[13].Text);
                    Nmission = Nmission + 1;
                    cont1 = cont1 + 1;
                }
                if (Seleccion == Strings.Mid(Lst1.Items[i].SubItems[1].Text, 4, 7))
                {
                    if (var1 != Strings.Left(Lst1.Items[i].SubItems[1].Text, 10))
                    {
                        var1 = Strings.Left(Lst1.Items[i].SubItems[1].Text, 10);
                        cont2 = cont2 + 1;
                    }
                }
            }
            
            lbttMbounty.Text = Strings.Format(TotalBounty, "###,###,###");
            LbttMloot.Text = Strings.Format(TotalLoot, "###,###,###");
            LbttMlp.Text = Strings.Format(TotalLP, "###,###,###");
            LbttMnmision.Text = Strings.Format(Nmission, "###,###,###");
            lbttMganancia.Text = Strings.Format(TotalBounty + TotalLoot, "###,###,###,###");
            LbttMmedia.Text = Strings.Format((TotalBounty + TotalLoot) / cont2, "###,###,###,###");
            lblMonthLostDrones.Text = Strings.Format(LostDrones, "###,###,###");
            lblMonthAmmoConsu.Text = Strings.Format(AmmoConsumption, "###,###,###");
            lblMonthAmmovalue.Text = Strings.Format(AmmoValue, "###,###,###");

            return true;
        }


        //---------------------------
        // Create the Chart of the Day
        //---------------------------
        public object ChartoftheDay(string Seleccion)
        {
            SortListDown();
            CHDia.Series[0].Points.Clear();
            for (var i = 0; i <= Lst1.Items.Count - 1; i++)
            {
                if (Seleccion == Strings.Left(Lst1.Items[i].SubItems[1].Text, 10))
                {
                    CHDia.Series[0].Points.AddXY(Lst1.Items[i].SubItems[1].Text, Lst1.Items[i].SubItems[4].Text);
                }
            }
            CHDia.Series[1].Points.Clear();
            for (var i = 0; i <= Lst1.Items.Count - 1; i++)
            {
                if (Seleccion == Strings.Left(Lst1.Items[i].SubItems[1].Text, 10))
                {
                    CHDia.Series[1].Points.AddXY(Lst1.Items[i].SubItems[1].Text, Lst1.Items[i].SubItems[6].Text);
                }
            }
            SortListUP();
            return true;
        }


        //-------------------------------
        // Day data calculated
        //-------------------------------
        public object DayDataCalculated(string Seleccion)
        {

            double TotalBounty = 0;
            double TotalLoot = 0;
            double LostDrones = 0;
            double AmmoConsumption = 0;
            double AmmoValue = 0;
            var Nmission = 0;
            double TotalLP = 0;
            var cont1 = 0;

            for (var i = 0; i <= Lst1.Items.Count - 1; i++)
            {
                if (Seleccion == Strings.Left(Lst1.Items[i].SubItems[1].Text, 10))
                {
                    TotalBounty += Convert.ToDouble(Lst1.Items[i].SubItems[4].Text);
                    TotalLoot += Convert.ToDouble(Lst1.Items[i].SubItems[6].Text);
                    TotalLP += Convert.ToDouble(Lst1.Items[i].SubItems[8].Text);
                    LostDrones += Convert.ToDouble(Lst1.Items[i].SubItems[11].Text);
                    AmmoConsumption += Convert.ToDouble(Lst1.Items[i].SubItems[12].Text);
                    AmmoValue += Convert.ToDouble(Lst1.Items[i].SubItems[13].Text);
                    Nmission = Nmission + 1;
                    cont1 = cont1 + 1;
                }
            }
            
            lbTotalbounty.Text = Strings.Format(TotalBounty, "###,###,###");
            LBtotalloot.Text = Strings.Format(TotalLoot, "###,###,###");
            LBtotallp.Text = Strings.Format(TotalLP, "###,###,###");
            LBNmision.Text = Strings.Format(Nmission, "###,###,###");
            LBProfit.Text = Strings.Format(TotalBounty + TotalLoot, "###,###,###");
            lbmediatotal.Text = Strings.Format((TotalBounty + TotalLoot) / cont1, "###,###,###");
            lblDaylostDrones.Text = Strings.Format(LostDrones, "###,###,###");
            lblDayAmmoConsu.Text = Strings.Format(AmmoConsumption, "###,###,###");
            lblDayAmmoValue.Text = Strings.Format(AmmoValue, "###,###,###");
            return true;
        }


        //-------------------------------
        // Mission statistics
        //-------------------------------
        public object StatsMision()
        {
            string Mision = null;
            double Time = 0;
            double Bounty = 0;
            double Loot = 0;
            double Lp = 0;
            double Iskmedia = 0;
            double Nmision = 0;
            double LostDrones = 0;
            double AmmoConsumption = 0;
            double AmmoValue = 0;
            double Tmision = Lst1.Items.Count;
            bool var1 = false;
            LstMision.Items.Clear();

            for (var i = 0; i <= Lst1.Items.Count - 1; i++)
            {
                Mision = Lst1.Items[i].SubItems[2].Text;
                Nmision = 0;
                 Time = 0;
                 Bounty = 0;
                 Loot = 0;
                 Lp = 0;
                 Iskmedia = 0;
                 LostDrones = 0;
                 AmmoConsumption = 0;
                 AmmoValue = 0;
                for (var e = 0; e <= Lst1.Items.Count - 1; e++)
                {
                    if (Mision == Lst1.Items[e].SubItems[2].Text)
                    {
                        Nmision = Nmision + 1;
                        Time += Convert.ToDouble(Lst1.Items[e].SubItems[3].Text);
                        Bounty += Convert.ToDouble(Lst1.Items[e].SubItems[4].Text);
                        Loot += Convert.ToDouble(Lst1.Items[e].SubItems[6].Text);
                        Lp += Convert.ToDouble(Lst1.Items[e].SubItems[8].Text);
                        Iskmedia += Convert.ToDouble(Lst1.Items[e].SubItems[10].Text);
                        LostDrones += Convert.ToDouble(Lst1.Items[i].SubItems[11].Text);
                        AmmoConsumption += Convert.ToDouble(Lst1.Items[i].SubItems[12].Text);
                        AmmoValue += Convert.ToDouble(Lst1.Items[i].SubItems[13].Text);
                    }
                }

                for (var o = 0; o <= LstMision.Items.Count - 1; o++)
                {
                    if (Mision == LstMision.Items[o].SubItems[1].Text)
                    {
                        var1 = true;
                        break;
                    }
                    else
                    {
                        var1 = false;
                    }
                }
                if (var1 == false)
                {
                    ListViewItem item1 = new ListViewItem("");
                    var _with1 = item1;
                    _with1.SubItems.Add(Mision);
                    _with1.SubItems.Add(Strings.Format(Convert.ToDouble((Nmision / Tmision) * 100), "0.#"));
                    _with1.SubItems.Add(Strings.Format(Convert.ToDouble(Time / Nmision), "###,###,##0"));
                    _with1.SubItems.Add(Strings.Format(Convert.ToDouble(Bounty / Nmision), "###,###,##0"));
                    _with1.SubItems.Add(Strings.Format(Convert.ToDouble(Loot / Nmision), "###,###,##0"));
                    _with1.SubItems.Add(Strings.Format(Convert.ToDouble(Lp / Nmision), "###,###,##0"));
                    _with1.SubItems.Add(Strings.Format(Convert.ToDouble(Iskmedia / Nmision), "###,###,##0"));
                    _with1.SubItems.Add(Strings.Format(Convert.ToDouble(LostDrones / Nmision), "###,###,##0"));
                    _with1.SubItems.Add(Strings.Format(Convert.ToDouble(AmmoConsumption / Nmision), "###,###,##0"));
                    _with1.SubItems.Add(Strings.Format(Convert.ToDouble(AmmoValue / Nmision), "###,###,##0"));
                    LstMision.Items.Add(item1);
                    var1 = true;
                }
            }
            PutMissions();
            return true;

        }


        //--------------------------------
        // Load data in the combox
        //--------------------------------
        public object Preload()
        {
            string SelectDia = null;
            string SelectMes = null;
            string var1 = "";

            SortListUP();
            CmbDia.Items.Clear();
            for (var i = 0; i <= Lst1.Items.Count - 1; i++)
            {
                SelectDia = Lst1.Items[i].SubItems[1].Text;

                SelectDia = Strings.Left(SelectDia, 10);


                if (var1 != SelectDia)
                {
                    CmbDia.Items.Add(SelectDia);
                    var1 = SelectDia;
                }
            }

            cmbMes.Items.Clear();
            for (var i = 0; i <= Lst1.Items.Count - 1; i++)
            {
                SelectMes = Lst1.Items[i].SubItems[1].Text;

                SelectMes = Strings.Mid(SelectMes, 4, 7);


                if (var1 != SelectMes)
                {
                    cmbMes.Items.Add(SelectMes);
                    var1 = SelectMes;
                }
            }

            return true;
        }

        //----------------------------------------
        // We organize the listview in descending
        //----------------------------------------

        public object SortListUP()
        {

            ListViewColumnSort oCompare = new ListViewColumnSort();
            oCompare.Sorting = SortOrder.Descending;
            Lst1.Sorting = oCompare.Sorting;
            oCompare.ColumnIndex = 1;
            oCompare.CompararPor = ListViewColumnSort.TipoCompare.Fecha;
            Lst1.ListViewItemSorter = oCompare;
            return true;
        }

        //----------------------------------------
        // We organize the listview in ascending
        //----------------------------------------
        public  object SortListDown()
        {

            ListViewColumnSort oCompare = new ListViewColumnSort();
            oCompare.Sorting = SortOrder.Ascending;
            Lst1.Sorting = oCompare.Sorting;
            oCompare.ColumnIndex = 1;
            oCompare.CompararPor = ListViewColumnSort.TipoCompare.Fecha;
            Lst1.ListViewItemSorter = oCompare;
            return true;
        }

        //----------------------------------
        // Put mission list
        //----------------------------------
        public object PutMissions()
        {
            ListViewColumnSort oCompare = new ListViewColumnSort();
            oCompare.Sorting = SortOrder.Descending;
            LstMision.Sorting = oCompare.Sorting;
            oCompare.ColumnIndex = 2;
            oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
            LstMision.ListViewItemSorter = oCompare;
            return true;
        }

        //-------------------------------
        // Clear Data
        //-------------------------------
        public object Cleardata()
        {
            Lst1.ResetText();
            Lst1.Items.Clear();
            LstMision.ResetText();
            LstMision.Items.Clear();
            CHDia.Series[0].Points.Clear();
            CHDia.Series[1].Points.Clear();
            CHMes.Series[0].Points.Clear();
            CmbDia.Text = "";
            cmbMes.Text = "";
            lbttMbounty.Text = "";
            lbttMganancia.Text = "";
            LbttMloot.Text = "";
            LbttMlp.Text = "";
            LbttMmedia.Text = "";
            LbttMnmision.Text = "";
            lbTotalbounty.Text = "";
            LBtotalloot.Text = "";
            LBtotallp.Text = "";
            LBNmision.Text = "";
            LBProfit.Text = "";
            lbmediatotal.Text = "";
            lblDayAmmoConsu.Text = "";
            lblDayAmmoValue.Text = "";
            lblDaylostDrones.Text = "";
            lblMonthAmmoConsu.Text = "";
            lblMonthAmmovalue.Text = "";
            lblMonthLostDrones.Text = "";
            return true;
        }


        //----------------------------------
        // Organized by column list
        //----------------------------------
        private void Lst1_ColumnClick(object o, ColumnClickEventArgs e)
        {

            ListViewColumnSort oCompare = new ListViewColumnSort(e.Column);

            if (Lst1.Sorting == SortOrder.Ascending)
                oCompare.Sorting = SortOrder.Descending;
            else
                oCompare.Sorting = SortOrder.Ascending;
            Lst1.Sorting = oCompare.Sorting;
            oCompare.ColumnIndex = e.Column;

            switch (e.Column)
            {
                case 1:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Fecha;
                    break;
                case 2:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Cadena;
                    break;
                case 3:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 4:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 5:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 6:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 7:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 8:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 9:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 10:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 11:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 12:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 13:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
            }

            Lst1.ListViewItemSorter = oCompare;

        }

        //----------------------------------
        // Organizing tasks by column
        //----------------------------------
        private void LstMision_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            ListViewColumnSort oCompare = new ListViewColumnSort();

            if (LstMision.Sorting == SortOrder.Ascending)
                oCompare.Sorting = SortOrder.Descending;
            else
                oCompare.Sorting = SortOrder.Ascending;
            LstMision.Sorting = oCompare.Sorting;
            oCompare.ColumnIndex = e.Column;

            switch (e.Column)
            {
                case 1:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Cadena;
                    break;
                case 2:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 3:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 4:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 5:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 6:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 7:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 8:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 9:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;
                case 10:
                    oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero;
                    break;

            }
            LstMision.ListViewItemSorter = oCompare;

        }

        private void FrmMain_Load(object sender, EventArgs e)
        {

        }
    }
}
