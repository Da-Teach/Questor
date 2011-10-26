namespace QuestorStatistics
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; false otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        ///Required method for Designer support. You can not
        /// edit the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.FBD = new System.Windows.Forms.FolderBrowserDialog();
            this.OFD = new System.Windows.Forms.OpenFileDialog();
            this.Tab1 = new System.Windows.Forms.TabControl();
            this.TabPage1 = new System.Windows.Forms.TabPage();
            this.Lst1 = new System.Windows.Forms.ListView();
            this.TabPage2 = new System.Windows.Forms.TabPage();
            this.lblDayAmmoValue = new System.Windows.Forms.Label();
            this.lblDayAmmoConsu = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lblDaylostDrones = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lbmediatotal = new System.Windows.Forms.Label();
            this.Label6 = new System.Windows.Forms.Label();
            this.LBGanacia = new System.Windows.Forms.Label();
            this.Label5 = new System.Windows.Forms.Label();
            this.CHDia = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.LBNmision = new System.Windows.Forms.Label();
            this.LBtotallp = new System.Windows.Forms.Label();
            this.LBtotalloot = new System.Windows.Forms.Label();
            this.lbTotalbounty = new System.Windows.Forms.Label();
            this.Label4 = new System.Windows.Forms.Label();
            this.Label3 = new System.Windows.Forms.Label();
            this.Label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.CmbDia = new System.Windows.Forms.ComboBox();
            this.TabPage3 = new System.Windows.Forms.TabPage();
            this.LbttMmedia = new System.Windows.Forms.Label();
            this.lbttMganancia = new System.Windows.Forms.Label();
            this.LbttMnmision = new System.Windows.Forms.Label();
            this.LbttMlp = new System.Windows.Forms.Label();
            this.LbttMloot = new System.Windows.Forms.Label();
            this.lbttMbounty = new System.Windows.Forms.Label();
            this.CHMes = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cmbMes = new System.Windows.Forms.ComboBox();
            this.Label7 = new System.Windows.Forms.Label();
            this.Label8 = new System.Windows.Forms.Label();
            this.Label9 = new System.Windows.Forms.Label();
            this.Label10 = new System.Windows.Forms.Label();
            this.Label11 = new System.Windows.Forms.Label();
            this.Label12 = new System.Windows.Forms.Label();
            this.TabPage4 = new System.Windows.Forms.TabPage();
            this.LstMision = new System.Windows.Forms.ListView();
            this.cmb1 = new System.Windows.Forms.ComboBox();
            this.lblMonthAmmovalue = new System.Windows.Forms.Label();
            this.lblMonthAmmoConsu = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.lblMonthLostDrones = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.Tab1.SuspendLayout();
            this.TabPage1.SuspendLayout();
            this.TabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CHDia)).BeginInit();
            this.TabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CHMes)).BeginInit();
            this.TabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // OFD
            // 
            this.OFD.FileName = "openFileDialog1";
            // 
            // Tab1
            // 
            this.Tab1.Controls.Add(this.TabPage1);
            this.Tab1.Controls.Add(this.TabPage2);
            this.Tab1.Controls.Add(this.TabPage3);
            this.Tab1.Controls.Add(this.TabPage4);
            this.Tab1.Location = new System.Drawing.Point(2, 38);
            this.Tab1.Name = "Tab1";
            this.Tab1.SelectedIndex = 0;
            this.Tab1.Size = new System.Drawing.Size(1005, 389);
            this.Tab1.TabIndex = 8;
            // 
            // TabPage1
            // 
            this.TabPage1.Controls.Add(this.Lst1);
            this.TabPage1.Location = new System.Drawing.Point(4, 22);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage1.Size = new System.Drawing.Size(997, 363);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "List";
            this.TabPage1.UseVisualStyleBackColor = true;
            // 
            // Lst1
            // 
            this.Lst1.Location = new System.Drawing.Point(3, 2);
            this.Lst1.Name = "Lst1";
            this.Lst1.Size = new System.Drawing.Size(988, 355);
            this.Lst1.TabIndex = 2;
            this.Lst1.UseCompatibleStateImageBehavior = false;
            this.Lst1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.Lst1_ColumnClick);
            // 
            // TabPage2
            // 
            this.TabPage2.Controls.Add(this.lblDayAmmoValue);
            this.TabPage2.Controls.Add(this.lblDayAmmoConsu);
            this.TabPage2.Controls.Add(this.label15);
            this.TabPage2.Controls.Add(this.label14);
            this.TabPage2.Controls.Add(this.lblDaylostDrones);
            this.TabPage2.Controls.Add(this.label13);
            this.TabPage2.Controls.Add(this.lbmediatotal);
            this.TabPage2.Controls.Add(this.Label6);
            this.TabPage2.Controls.Add(this.LBGanacia);
            this.TabPage2.Controls.Add(this.Label5);
            this.TabPage2.Controls.Add(this.CHDia);
            this.TabPage2.Controls.Add(this.LBNmision);
            this.TabPage2.Controls.Add(this.LBtotallp);
            this.TabPage2.Controls.Add(this.LBtotalloot);
            this.TabPage2.Controls.Add(this.lbTotalbounty);
            this.TabPage2.Controls.Add(this.Label4);
            this.TabPage2.Controls.Add(this.Label3);
            this.TabPage2.Controls.Add(this.Label2);
            this.TabPage2.Controls.Add(this.Label1);
            this.TabPage2.Controls.Add(this.CmbDia);
            this.TabPage2.Location = new System.Drawing.Point(4, 22);
            this.TabPage2.Name = "TabPage2";
            this.TabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage2.Size = new System.Drawing.Size(997, 363);
            this.TabPage2.TabIndex = 1;
            this.TabPage2.Text = "Day";
            this.TabPage2.UseVisualStyleBackColor = true;
            // 
            // lblDayAmmoValue
            // 
            this.lblDayAmmoValue.AutoSize = true;
            this.lblDayAmmoValue.Location = new System.Drawing.Point(89, 274);
            this.lblDayAmmoValue.Name = "lblDayAmmoValue";
            this.lblDayAmmoValue.Size = new System.Drawing.Size(0, 13);
            this.lblDayAmmoValue.TabIndex = 19;
            // 
            // lblDayAmmoConsu
            // 
            this.lblDayAmmoConsu.AutoSize = true;
            this.lblDayAmmoConsu.Location = new System.Drawing.Point(130, 242);
            this.lblDayAmmoConsu.Name = "lblDayAmmoConsu";
            this.lblDayAmmoConsu.Size = new System.Drawing.Size(0, 13);
            this.lblDayAmmoConsu.TabIndex = 18;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(3, 274);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(69, 13);
            this.label15.TabIndex = 17;
            this.label15.Text = "Ammo Value:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(3, 246);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(106, 13);
            this.label14.TabIndex = 16;
            this.label14.Text = "Ammo Consumption: ";
            // 
            // lblDaylostDrones
            // 
            this.lblDaylostDrones.AutoSize = true;
            this.lblDaylostDrones.Location = new System.Drawing.Point(89, 217);
            this.lblDaylostDrones.Name = "lblDaylostDrones";
            this.lblDaylostDrones.Size = new System.Drawing.Size(0, 13);
            this.lblDaylostDrones.TabIndex = 15;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(3, 217);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(70, 13);
            this.label13.TabIndex = 14;
            this.label13.Text = "Lost Drones: ";
            // 
            // lbmediatotal
            // 
            this.lbmediatotal.AutoSize = true;
            this.lbmediatotal.Location = new System.Drawing.Point(99, 185);
            this.lbmediatotal.Name = "lbmediatotal";
            this.lbmediatotal.Size = new System.Drawing.Size(0, 13);
            this.lbmediatotal.TabIndex = 13;
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Location = new System.Drawing.Point(3, 185);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(50, 13);
            this.Label6.TabIndex = 12;
            this.Label6.Text = "Average:";
            // 
            // LBGanacia
            // 
            this.LBGanacia.AutoSize = true;
            this.LBGanacia.Location = new System.Drawing.Point(99, 154);
            this.LBGanacia.Name = "LBGanacia";
            this.LBGanacia.Size = new System.Drawing.Size(0, 13);
            this.LBGanacia.TabIndex = 11;
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Location = new System.Drawing.Point(3, 154);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(60, 13);
            this.Label5.TabIndex = 10;
            this.Label5.Text = "Total profit:";
            // 
            // CHDia
            // 
            chartArea1.Name = "ChartArea1";
            this.CHDia.ChartAreas.Add(chartArea1);
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Name = "Legend1";
            this.CHDia.Legends.Add(legend1);
            this.CHDia.Location = new System.Drawing.Point(171, 9);
            this.CHDia.Name = "CHDia";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.IsXValueIndexed = true;
            series1.Legend = "Legend1";
            series1.Name = "Bounty Isk";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Legend = "Legend1";
            series2.Name = "Loot Isk";
            this.CHDia.Series.Add(series1);
            this.CHDia.Series.Add(series2);
            this.CHDia.Size = new System.Drawing.Size(826, 354);
            this.CHDia.TabIndex = 9;
            this.CHDia.Text = "Chart1";
            // 
            // LBNmision
            // 
            this.LBNmision.AutoSize = true;
            this.LBNmision.Location = new System.Drawing.Point(99, 124);
            this.LBNmision.Name = "LBNmision";
            this.LBNmision.Size = new System.Drawing.Size(0, 13);
            this.LBNmision.TabIndex = 8;
            // 
            // LBtotallp
            // 
            this.LBtotallp.AutoSize = true;
            this.LBtotallp.Location = new System.Drawing.Point(99, 90);
            this.LBtotallp.Name = "LBtotallp";
            this.LBtotallp.Size = new System.Drawing.Size(0, 13);
            this.LBtotallp.TabIndex = 7;
            // 
            // LBtotalloot
            // 
            this.LBtotalloot.AutoSize = true;
            this.LBtotalloot.Location = new System.Drawing.Point(99, 56);
            this.LBtotalloot.Name = "LBtotalloot";
            this.LBtotalloot.Size = new System.Drawing.Size(0, 13);
            this.LBtotalloot.TabIndex = 6;
            // 
            // lbTotalbounty
            // 
            this.lbTotalbounty.AutoSize = true;
            this.lbTotalbounty.Location = new System.Drawing.Point(99, 24);
            this.lbTotalbounty.Name = "lbTotalbounty";
            this.lbTotalbounty.Size = new System.Drawing.Size(0, 13);
            this.lbTotalbounty.TabIndex = 5;
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(3, 124);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(63, 13);
            this.Label4.TabIndex = 4;
            this.Label4.Text = "Nº Mission: ";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(3, 90);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(53, 13);
            this.Label3.TabIndex = 3;
            this.Label3.Text = "Total LP: ";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(3, 56);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(78, 13);
            this.Label2.TabIndex = 2;
            this.Label2.Text = "Total Loot Isk: ";
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(3, 24);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(90, 13);
            this.Label1.TabIndex = 1;
            this.Label1.Text = "Total Bounty Isk: ";
            // 
            // CmbDia
            // 
            this.CmbDia.FormattingEnabled = true;
            this.CmbDia.Location = new System.Drawing.Point(0, 0);
            this.CmbDia.Name = "CmbDia";
            this.CmbDia.Size = new System.Drawing.Size(93, 21);
            this.CmbDia.TabIndex = 0;
            this.CmbDia.SelectedIndexChanged += new System.EventHandler(this.CmbDia_SelectedIndexChanged_1);
            // 
            // TabPage3
            // 
            this.TabPage3.Controls.Add(this.lblMonthAmmovalue);
            this.TabPage3.Controls.Add(this.lblMonthAmmoConsu);
            this.TabPage3.Controls.Add(this.label18);
            this.TabPage3.Controls.Add(this.label19);
            this.TabPage3.Controls.Add(this.lblMonthLostDrones);
            this.TabPage3.Controls.Add(this.label21);
            this.TabPage3.Controls.Add(this.LbttMmedia);
            this.TabPage3.Controls.Add(this.lbttMganancia);
            this.TabPage3.Controls.Add(this.LbttMnmision);
            this.TabPage3.Controls.Add(this.LbttMlp);
            this.TabPage3.Controls.Add(this.LbttMloot);
            this.TabPage3.Controls.Add(this.lbttMbounty);
            this.TabPage3.Controls.Add(this.CHMes);
            this.TabPage3.Controls.Add(this.cmbMes);
            this.TabPage3.Controls.Add(this.Label7);
            this.TabPage3.Controls.Add(this.Label8);
            this.TabPage3.Controls.Add(this.Label9);
            this.TabPage3.Controls.Add(this.Label10);
            this.TabPage3.Controls.Add(this.Label11);
            this.TabPage3.Controls.Add(this.Label12);
            this.TabPage3.Location = new System.Drawing.Point(4, 22);
            this.TabPage3.Name = "TabPage3";
            this.TabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage3.Size = new System.Drawing.Size(997, 363);
            this.TabPage3.TabIndex = 2;
            this.TabPage3.Text = "Month";
            this.TabPage3.UseVisualStyleBackColor = true;
            // 
            // LbttMmedia
            // 
            this.LbttMmedia.AutoSize = true;
            this.LbttMmedia.Location = new System.Drawing.Point(99, 184);
            this.LbttMmedia.Name = "LbttMmedia";
            this.LbttMmedia.Size = new System.Drawing.Size(0, 13);
            this.LbttMmedia.TabIndex = 26;
            // 
            // lbttMganancia
            // 
            this.lbttMganancia.AutoSize = true;
            this.lbttMganancia.Location = new System.Drawing.Point(99, 153);
            this.lbttMganancia.Name = "lbttMganancia";
            this.lbttMganancia.Size = new System.Drawing.Size(0, 13);
            this.lbttMganancia.TabIndex = 25;
            // 
            // LbttMnmision
            // 
            this.LbttMnmision.AutoSize = true;
            this.LbttMnmision.Location = new System.Drawing.Point(99, 123);
            this.LbttMnmision.Name = "LbttMnmision";
            this.LbttMnmision.Size = new System.Drawing.Size(0, 13);
            this.LbttMnmision.TabIndex = 24;
            // 
            // LbttMlp
            // 
            this.LbttMlp.AutoSize = true;
            this.LbttMlp.Location = new System.Drawing.Point(99, 89);
            this.LbttMlp.Name = "LbttMlp";
            this.LbttMlp.Size = new System.Drawing.Size(0, 13);
            this.LbttMlp.TabIndex = 23;
            // 
            // LbttMloot
            // 
            this.LbttMloot.AutoSize = true;
            this.LbttMloot.Location = new System.Drawing.Point(99, 55);
            this.LbttMloot.Name = "LbttMloot";
            this.LbttMloot.Size = new System.Drawing.Size(0, 13);
            this.LbttMloot.TabIndex = 22;
            // 
            // lbttMbounty
            // 
            this.lbttMbounty.AutoSize = true;
            this.lbttMbounty.Location = new System.Drawing.Point(99, 23);
            this.lbttMbounty.Name = "lbttMbounty";
            this.lbttMbounty.Size = new System.Drawing.Size(0, 13);
            this.lbttMbounty.TabIndex = 21;
            // 
            // CHMes
            // 
            chartArea2.Name = "ChartArea1";
            this.CHMes.ChartAreas.Add(chartArea2);
            legend2.Alignment = System.Drawing.StringAlignment.Center;
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend2.Name = "Legend1";
            this.CHMes.Legends.Add(legend2);
            this.CHMes.Location = new System.Drawing.Point(167, 3);
            this.CHMes.Name = "CHMes";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Legend = "Legend1";
            series3.MarkerColor = System.Drawing.Color.Red;
            series3.Name = "ISK per Day";
            this.CHMes.Series.Add(series3);
            this.CHMes.Size = new System.Drawing.Size(830, 354);
            this.CHMes.TabIndex = 20;
            this.CHMes.Text = "Chart1";
            // 
            // cmbMes
            // 
            this.cmbMes.FormattingEnabled = true;
            this.cmbMes.Location = new System.Drawing.Point(0, -1);
            this.cmbMes.Name = "cmbMes";
            this.cmbMes.Size = new System.Drawing.Size(93, 21);
            this.cmbMes.TabIndex = 19;
            this.cmbMes.SelectedIndexChanged += new System.EventHandler(this.cmbMes_SelectedIndexChanged_1);
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Location = new System.Drawing.Point(3, 184);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(50, 13);
            this.Label7.TabIndex = 18;
            this.Label7.Text = "Average:";
            // 
            // Label8
            // 
            this.Label8.AutoSize = true;
            this.Label8.Location = new System.Drawing.Point(3, 153);
            this.Label8.Name = "Label8";
            this.Label8.Size = new System.Drawing.Size(60, 13);
            this.Label8.TabIndex = 17;
            this.Label8.Text = "Total profit:";
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Location = new System.Drawing.Point(3, 123);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(63, 13);
            this.Label9.TabIndex = 16;
            this.Label9.Text = "Nº Mission: ";
            // 
            // Label10
            // 
            this.Label10.AutoSize = true;
            this.Label10.Location = new System.Drawing.Point(3, 89);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(53, 13);
            this.Label10.TabIndex = 15;
            this.Label10.Text = "Total LP: ";
            // 
            // Label11
            // 
            this.Label11.AutoSize = true;
            this.Label11.Location = new System.Drawing.Point(3, 55);
            this.Label11.Name = "Label11";
            this.Label11.Size = new System.Drawing.Size(78, 13);
            this.Label11.TabIndex = 14;
            this.Label11.Text = "Total Loot Isk: ";
            // 
            // Label12
            // 
            this.Label12.AutoSize = true;
            this.Label12.Location = new System.Drawing.Point(3, 23);
            this.Label12.Name = "Label12";
            this.Label12.Size = new System.Drawing.Size(90, 13);
            this.Label12.TabIndex = 13;
            this.Label12.Text = "Total Bounty Isk: ";
            // 
            // TabPage4
            // 
            this.TabPage4.Controls.Add(this.LstMision);
            this.TabPage4.Location = new System.Drawing.Point(4, 22);
            this.TabPage4.Name = "TabPage4";
            this.TabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage4.Size = new System.Drawing.Size(997, 363);
            this.TabPage4.TabIndex = 3;
            this.TabPage4.Text = "Mission";
            this.TabPage4.UseVisualStyleBackColor = true;
            // 
            // LstMision
            // 
            this.LstMision.Location = new System.Drawing.Point(3, 6);
            this.LstMision.Name = "LstMision";
            this.LstMision.Size = new System.Drawing.Size(960, 351);
            this.LstMision.TabIndex = 0;
            this.LstMision.UseCompatibleStateImageBehavior = false;
            this.LstMision.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LstMision_ColumnClick);
            // 
            // cmb1
            // 
            this.cmb1.FormattingEnabled = true;
            this.cmb1.Location = new System.Drawing.Point(2, 2);
            this.cmb1.Name = "cmb1";
            this.cmb1.Size = new System.Drawing.Size(134, 21);
            this.cmb1.TabIndex = 7;
            this.cmb1.Text = "Select Char";
            this.cmb1.SelectedIndexChanged += new System.EventHandler(this.cmb1_SelectedIndexChanged);
            // 
            // lblMonthAmmovalue
            // 
            this.lblMonthAmmovalue.AutoSize = true;
            this.lblMonthAmmovalue.Location = new System.Drawing.Point(89, 267);
            this.lblMonthAmmovalue.Name = "lblMonthAmmovalue";
            this.lblMonthAmmovalue.Size = new System.Drawing.Size(0, 13);
            this.lblMonthAmmovalue.TabIndex = 32;
            // 
            // lblMonthAmmoConsu
            // 
            this.lblMonthAmmoConsu.AutoSize = true;
            this.lblMonthAmmoConsu.Location = new System.Drawing.Point(130, 239);
            this.lblMonthAmmoConsu.Name = "lblMonthAmmoConsu";
            this.lblMonthAmmoConsu.Size = new System.Drawing.Size(0, 13);
            this.lblMonthAmmoConsu.TabIndex = 31;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(3, 267);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(69, 13);
            this.label18.TabIndex = 30;
            this.label18.Text = "Ammo Value:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(3, 239);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(106, 13);
            this.label19.TabIndex = 29;
            this.label19.Text = "Ammo Consumption: ";
            // 
            // lblMonthLostDrones
            // 
            this.lblMonthLostDrones.AutoSize = true;
            this.lblMonthLostDrones.Location = new System.Drawing.Point(89, 210);
            this.lblMonthLostDrones.Name = "lblMonthLostDrones";
            this.lblMonthLostDrones.Size = new System.Drawing.Size(0, 13);
            this.lblMonthLostDrones.TabIndex = 28;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(3, 210);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(70, 13);
            this.label21.TabIndex = 27;
            this.label21.Text = "Lost Drones: ";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 430);
            this.Controls.Add(this.Tab1);
            this.Controls.Add(this.cmb1);
            this.Name = "FrmMain";
            this.Text = "Questor Statistics";
            this.Tab1.ResumeLayout(false);
            this.TabPage1.ResumeLayout(false);
            this.TabPage2.ResumeLayout(false);
            this.TabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CHDia)).EndInit();
            this.TabPage3.ResumeLayout(false);
            this.TabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CHMes)).EndInit();
            this.TabPage4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog FBD;
        private System.Windows.Forms.OpenFileDialog OFD;
        internal System.Windows.Forms.TabControl Tab1;
        internal System.Windows.Forms.TabPage TabPage1;
        internal System.Windows.Forms.ListView Lst1;
        internal System.Windows.Forms.TabPage TabPage2;
        internal System.Windows.Forms.Label lbmediatotal;
        internal System.Windows.Forms.Label Label6;
        internal System.Windows.Forms.Label LBGanacia;
        internal System.Windows.Forms.Label Label5;
        internal System.Windows.Forms.DataVisualization.Charting.Chart CHDia;
        internal System.Windows.Forms.Label LBNmision;
        internal System.Windows.Forms.Label LBtotallp;
        internal System.Windows.Forms.Label LBtotalloot;
        internal System.Windows.Forms.Label lbTotalbounty;
        internal System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.Label Label3;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.ComboBox CmbDia;
        internal System.Windows.Forms.TabPage TabPage3;
        internal System.Windows.Forms.Label LbttMmedia;
        internal System.Windows.Forms.Label lbttMganancia;
        internal System.Windows.Forms.Label LbttMnmision;
        internal System.Windows.Forms.Label LbttMlp;
        internal System.Windows.Forms.Label LbttMloot;
        internal System.Windows.Forms.Label lbttMbounty;
        internal System.Windows.Forms.DataVisualization.Charting.Chart CHMes;
        internal System.Windows.Forms.ComboBox cmbMes;
        internal System.Windows.Forms.Label Label7;
        internal System.Windows.Forms.Label Label8;
        internal System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.Label Label10;
        internal System.Windows.Forms.Label Label11;
        internal System.Windows.Forms.Label Label12;
        internal System.Windows.Forms.TabPage TabPage4;
        internal System.Windows.Forms.ListView LstMision;
        internal System.Windows.Forms.ComboBox cmb1;
        private System.Windows.Forms.Label lblDayAmmoValue;
        private System.Windows.Forms.Label lblDayAmmoConsu;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblDaylostDrones;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lblMonthAmmovalue;
        private System.Windows.Forms.Label lblMonthAmmoConsu;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label lblMonthLostDrones;
        private System.Windows.Forms.Label label21;
    }
}

