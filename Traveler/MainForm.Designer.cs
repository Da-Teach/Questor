namespace Traveler
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.UpdateSearchResults = new System.Windows.Forms.Timer(this.components);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.cmbXML = new System.Windows.Forms.ComboBox();
            this.bttnSaveTask = new System.Windows.Forms.Button();
            this.chkPause = new System.Windows.Forms.CheckBox();
            this.bttnDelete = new System.Windows.Forms.Button();
            this.bttnDown = new System.Windows.Forms.Button();
            this.bttnUP = new System.Windows.Forms.Button();
            this.BttnStart = new System.Windows.Forms.Button();
            this.LstTask = new System.Windows.Forms.ListView();
            this.JobHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DescriptionHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.LblStatus = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.SearchResults = new System.Windows.Forms.ListView();
            this.NameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TypeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SearchLabel = new System.Windows.Forms.Label();
            this.RefreshBookmarksButton = new System.Windows.Forms.Button();
            this.SearchTextBox = new System.Windows.Forms.TextBox();
            this.BttnAddTraveler = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtNameCorp = new System.Windows.Forms.TextBox();
            this.rbttnCorp = new System.Windows.Forms.RadioButton();
            this.rbttnShip = new System.Windows.Forms.RadioButton();
            this.rbttnLocal = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.bttnTaskMakeShip = new System.Windows.Forms.Button();
            this.txtNameShip = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.bttnTaskAllItems = new System.Windows.Forms.Button();
            this.cmbAllMode = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BttnTaskForItem = new System.Windows.Forms.Button();
            this.LstItems = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cmbMode = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtUnit = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSearchItems = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.chkUpdateMineral = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.RefineEfficiencyInput = new System.Windows.Forms.NumericUpDown();
            this.RefineCheckBox = new System.Windows.Forms.CheckBox();
            this.cbxUndersell = new System.Windows.Forms.CheckBox();
            this.cbxSell = new System.Windows.Forms.CheckBox();
            this.tbTotalSold = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbTotalMedian = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.lvItems = new System.Windows.Forms.ListView();
            this.chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chQuantity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chQuantitySold = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chMedianBuy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chStationBuy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chTotalBuy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.bttnTaskValueDump = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RefineEfficiencyInput)).BeginInit();
            this.SuspendLayout();
            // 
            // UpdateSearchResults
            // 
            this.UpdateSearchResults.Enabled = true;
            this.UpdateSearchResults.Interval = 250;
            this.UpdateSearchResults.Tick += new System.EventHandler(this.UpdateSearchResultsTick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(1, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(656, 271);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cmbXML);
            this.tabPage1.Controls.Add(this.bttnSaveTask);
            this.tabPage1.Controls.Add(this.chkPause);
            this.tabPage1.Controls.Add(this.bttnDelete);
            this.tabPage1.Controls.Add(this.bttnDown);
            this.tabPage1.Controls.Add(this.bttnUP);
            this.tabPage1.Controls.Add(this.BttnStart);
            this.tabPage1.Controls.Add(this.LstTask);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.LblStatus);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(648, 245);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "General";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // cmbXML
            // 
            this.cmbXML.FormattingEnabled = true;
            this.cmbXML.Location = new System.Drawing.Point(385, 216);
            this.cmbXML.Name = "cmbXML";
            this.cmbXML.Size = new System.Drawing.Size(121, 21);
            this.cmbXML.TabIndex = 29;
            this.cmbXML.Text = "Select Jobs";
            this.cmbXML.SelectedIndexChanged += new System.EventHandler(this.cmbXML_SelectedIndexChanged);
            // 
            // bttnSaveTask
            // 
            this.bttnSaveTask.Location = new System.Drawing.Point(512, 216);
            this.bttnSaveTask.Name = "bttnSaveTask";
            this.bttnSaveTask.Size = new System.Drawing.Size(75, 23);
            this.bttnSaveTask.TabIndex = 28;
            this.bttnSaveTask.Text = "Save Task";
            this.bttnSaveTask.UseVisualStyleBackColor = true;
            this.bttnSaveTask.Click += new System.EventHandler(this.bttnSaveTask_Click);
            // 
            // chkPause
            // 
            this.chkPause.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPause.AutoSize = true;
            this.chkPause.Location = new System.Drawing.Point(91, 216);
            this.chkPause.Name = "chkPause";
            this.chkPause.Size = new System.Drawing.Size(47, 23);
            this.chkPause.TabIndex = 27;
            this.chkPause.Text = "Pause";
            this.chkPause.UseVisualStyleBackColor = true;
            this.chkPause.CheckedChanged += new System.EventHandler(this.chkPause_CheckedChanged);
            // 
            // bttnDelete
            // 
            this.bttnDelete.Location = new System.Drawing.Point(594, 138);
            this.bttnDelete.Name = "bttnDelete";
            this.bttnDelete.Size = new System.Drawing.Size(51, 23);
            this.bttnDelete.TabIndex = 26;
            this.bttnDelete.Text = "Delete";
            this.bttnDelete.UseVisualStyleBackColor = true;
            this.bttnDelete.Click += new System.EventHandler(this.bttnDelete_Click);
            // 
            // bttnDown
            // 
            this.bttnDown.Location = new System.Drawing.Point(594, 96);
            this.bttnDown.Name = "bttnDown";
            this.bttnDown.Size = new System.Drawing.Size(48, 23);
            this.bttnDown.TabIndex = 25;
            this.bttnDown.Text = "Down";
            this.bttnDown.UseVisualStyleBackColor = true;
            this.bttnDown.Click += new System.EventHandler(this.bttnDown_Click);
            // 
            // bttnUP
            // 
            this.bttnUP.Location = new System.Drawing.Point(594, 57);
            this.bttnUP.Name = "bttnUP";
            this.bttnUP.Size = new System.Drawing.Size(48, 23);
            this.bttnUP.TabIndex = 24;
            this.bttnUP.Text = "Up";
            this.bttnUP.UseVisualStyleBackColor = true;
            this.bttnUP.Click += new System.EventHandler(this.bttnUP_Click);
            // 
            // BttnStart
            // 
            this.BttnStart.Location = new System.Drawing.Point(10, 216);
            this.BttnStart.Name = "BttnStart";
            this.BttnStart.Size = new System.Drawing.Size(75, 23);
            this.BttnStart.TabIndex = 23;
            this.BttnStart.Text = "Start";
            this.BttnStart.UseVisualStyleBackColor = true;
            this.BttnStart.Click += new System.EventHandler(this.BttnStart_Click);
            // 
            // LstTask
            // 
            this.LstTask.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LstTask.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.JobHeader1,
            this.DescriptionHeader1,
            this.columnHeader4,
            this.columnHeader3});
            this.LstTask.FullRowSelect = true;
            this.LstTask.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.LstTask.HideSelection = false;
            this.LstTask.Location = new System.Drawing.Point(7, 25);
            this.LstTask.MultiSelect = false;
            this.LstTask.Name = "LstTask";
            this.LstTask.Size = new System.Drawing.Size(580, 185);
            this.LstTask.TabIndex = 22;
            this.LstTask.UseCompatibleStateImageBehavior = false;
            this.LstTask.View = System.Windows.Forms.View.Details;
            // 
            // JobHeader1
            // 
            this.JobHeader1.Text = "Job";
            // 
            // DescriptionHeader1
            // 
            this.DescriptionHeader1.Text = "Name";
            this.DescriptionHeader1.Width = 350;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Unit";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Hangar";
            this.columnHeader3.Width = 100;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Status";
            // 
            // LblStatus
            // 
            this.LblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LblStatus.AutoEllipsis = true;
            this.LblStatus.Location = new System.Drawing.Point(67, 4);
            this.LblStatus.Name = "LblStatus";
            this.LblStatus.Size = new System.Drawing.Size(412, 19);
            this.LblStatus.TabIndex = 20;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.SearchResults);
            this.tabPage2.Controls.Add(this.SearchLabel);
            this.tabPage2.Controls.Add(this.RefreshBookmarksButton);
            this.tabPage2.Controls.Add(this.SearchTextBox);
            this.tabPage2.Controls.Add(this.BttnAddTraveler);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(648, 245);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Traveler";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // SearchResults
            // 
            this.SearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameHeader,
            this.TypeHeader});
            this.SearchResults.FullRowSelect = true;
            this.SearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SearchResults.HideSelection = false;
            this.SearchResults.Location = new System.Drawing.Point(10, 35);
            this.SearchResults.MultiSelect = false;
            this.SearchResults.Name = "SearchResults";
            this.SearchResults.Size = new System.Drawing.Size(627, 173);
            this.SearchResults.TabIndex = 17;
            this.SearchResults.UseCompatibleStateImageBehavior = false;
            this.SearchResults.View = System.Windows.Forms.View.Details;
            // 
            // NameHeader
            // 
            this.NameHeader.Text = "Name";
            this.NameHeader.Width = 400;
            // 
            // TypeHeader
            // 
            this.TypeHeader.Text = "Type";
            this.TypeHeader.Width = 200;
            // 
            // SearchLabel
            // 
            this.SearchLabel.AutoSize = true;
            this.SearchLabel.Location = new System.Drawing.Point(7, 12);
            this.SearchLabel.Name = "SearchLabel";
            this.SearchLabel.Size = new System.Drawing.Size(81, 13);
            this.SearchLabel.TabIndex = 14;
            this.SearchLabel.Text = "Enter a location";
            // 
            // RefreshBookmarksButton
            // 
            this.RefreshBookmarksButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RefreshBookmarksButton.Location = new System.Drawing.Point(509, 214);
            this.RefreshBookmarksButton.Name = "RefreshBookmarksButton";
            this.RefreshBookmarksButton.Size = new System.Drawing.Size(128, 25);
            this.RefreshBookmarksButton.TabIndex = 15;
            this.RefreshBookmarksButton.Text = "Refresh Bookmarks";
            this.RefreshBookmarksButton.UseVisualStyleBackColor = true;
            // 
            // SearchTextBox
            // 
            this.SearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchTextBox.Location = new System.Drawing.Point(94, 9);
            this.SearchTextBox.Name = "SearchTextBox";
            this.SearchTextBox.Size = new System.Drawing.Size(543, 20);
            this.SearchTextBox.TabIndex = 16;
            this.SearchTextBox.TextChanged += new System.EventHandler(this.SearchTextBox_TextChanged);
            // 
            // BttnAddTraveler
            // 
            this.BttnAddTraveler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BttnAddTraveler.Location = new System.Drawing.Point(10, 213);
            this.BttnAddTraveler.Name = "BttnAddTraveler";
            this.BttnAddTraveler.Size = new System.Drawing.Size(75, 26);
            this.BttnAddTraveler.TabIndex = 13;
            this.BttnAddTraveler.Text = "Add Task";
            this.BttnAddTraveler.UseVisualStyleBackColor = true;
            this.BttnAddTraveler.Click += new System.EventHandler(this.BttnAddTraveler_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox4);
            this.tabPage3.Controls.Add(this.groupBox3);
            this.tabPage3.Controls.Add(this.groupBox2);
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(648, 245);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Action item";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtNameCorp);
            this.groupBox4.Controls.Add(this.rbttnCorp);
            this.groupBox4.Controls.Add(this.rbttnShip);
            this.groupBox4.Controls.Add(this.rbttnLocal);
            this.groupBox4.Location = new System.Drawing.Point(3, 6);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(457, 44);
            this.groupBox4.TabIndex = 15;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Select hangar";
            // 
            // txtNameCorp
            // 
            this.txtNameCorp.Enabled = false;
            this.txtNameCorp.Location = new System.Drawing.Point(332, 15);
            this.txtNameCorp.Name = "txtNameCorp";
            this.txtNameCorp.Size = new System.Drawing.Size(102, 20);
            this.txtNameCorp.TabIndex = 3;
            this.txtNameCorp.TextChanged += new System.EventHandler(this.txtNameCorp_TextChanged);
            // 
            // rbttnCorp
            // 
            this.rbttnCorp.AutoSize = true;
            this.rbttnCorp.Location = new System.Drawing.Point(241, 15);
            this.rbttnCorp.Name = "rbttnCorp";
            this.rbttnCorp.Size = new System.Drawing.Size(88, 17);
            this.rbttnCorp.TabIndex = 2;
            this.rbttnCorp.Text = "Corp Hangar:";
            this.rbttnCorp.UseVisualStyleBackColor = true;
            this.rbttnCorp.CheckedChanged += new System.EventHandler(this.rbttnCorp_CheckedChanged);
            // 
            // rbttnShip
            // 
            this.rbttnShip.AutoSize = true;
            this.rbttnShip.Location = new System.Drawing.Point(117, 15);
            this.rbttnShip.Name = "rbttnShip";
            this.rbttnShip.Size = new System.Drawing.Size(84, 17);
            this.rbttnShip.TabIndex = 1;
            this.rbttnShip.Text = "Ship Hangar";
            this.rbttnShip.UseVisualStyleBackColor = true;
            this.rbttnShip.CheckedChanged += new System.EventHandler(this.rbttnShip_CheckedChanged);
            // 
            // rbttnLocal
            // 
            this.rbttnLocal.AutoSize = true;
            this.rbttnLocal.Checked = true;
            this.rbttnLocal.Location = new System.Drawing.Point(6, 15);
            this.rbttnLocal.Name = "rbttnLocal";
            this.rbttnLocal.Size = new System.Drawing.Size(89, 17);
            this.rbttnLocal.TabIndex = 0;
            this.rbttnLocal.TabStop = true;
            this.rbttnLocal.Text = "Local Hangar";
            this.rbttnLocal.UseVisualStyleBackColor = true;
            this.rbttnLocal.CheckedChanged += new System.EventHandler(this.rbttnLocal_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.bttnTaskMakeShip);
            this.groupBox3.Controls.Add(this.txtNameShip);
            this.groupBox3.Location = new System.Drawing.Point(466, 120);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(175, 119);
            this.groupBox3.TabIndex = 14;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Make Ship";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 25);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Name Ship";
            // 
            // bttnTaskMakeShip
            // 
            this.bttnTaskMakeShip.Location = new System.Drawing.Point(6, 67);
            this.bttnTaskMakeShip.Name = "bttnTaskMakeShip";
            this.bttnTaskMakeShip.Size = new System.Drawing.Size(163, 24);
            this.bttnTaskMakeShip.TabIndex = 12;
            this.bttnTaskMakeShip.Text = "Add Task";
            this.bttnTaskMakeShip.UseVisualStyleBackColor = true;
            this.bttnTaskMakeShip.Click += new System.EventHandler(this.bttnTaskMakeShip_Click);
            // 
            // txtNameShip
            // 
            this.txtNameShip.Location = new System.Drawing.Point(6, 41);
            this.txtNameShip.Name = "txtNameShip";
            this.txtNameShip.Size = new System.Drawing.Size(163, 20);
            this.txtNameShip.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.bttnTaskAllItems);
            this.groupBox2.Controls.Add(this.cmbAllMode);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(466, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(176, 108);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "All Items";
            // 
            // bttnTaskAllItems
            // 
            this.bttnTaskAllItems.Location = new System.Drawing.Point(9, 76);
            this.bttnTaskAllItems.Name = "bttnTaskAllItems";
            this.bttnTaskAllItems.Size = new System.Drawing.Size(160, 24);
            this.bttnTaskAllItems.TabIndex = 11;
            this.bttnTaskAllItems.Text = "Add Task";
            this.bttnTaskAllItems.UseVisualStyleBackColor = true;
            this.bttnTaskAllItems.Click += new System.EventHandler(this.bttnTaskAllItems_Click);
            // 
            // cmbAllMode
            // 
            this.cmbAllMode.FormattingEnabled = true;
            this.cmbAllMode.Items.AddRange(new object[] {
            "Drop",
            "Grab"});
            this.cmbAllMode.Location = new System.Drawing.Point(9, 47);
            this.cmbAllMode.Name = "cmbAllMode";
            this.cmbAllMode.Size = new System.Drawing.Size(160, 21);
            this.cmbAllMode.TabIndex = 12;
            this.cmbAllMode.Text = "Select Mode";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Action All Items";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BttnTaskForItem);
            this.groupBox1.Controls.Add(this.LstItems);
            this.groupBox1.Controls.Add(this.cmbMode);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtUnit);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtSearchItems);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(2, 56);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(458, 183);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "For Item";
            // 
            // BttnTaskForItem
            // 
            this.BttnTaskForItem.Location = new System.Drawing.Point(333, 118);
            this.BttnTaskForItem.Name = "BttnTaskForItem";
            this.BttnTaskForItem.Size = new System.Drawing.Size(119, 24);
            this.BttnTaskForItem.TabIndex = 1;
            this.BttnTaskForItem.Text = "Add Task";
            this.BttnTaskForItem.UseVisualStyleBackColor = true;
            this.BttnTaskForItem.Click += new System.EventHandler(this.BttnTaskForItem_Click_1);
            // 
            // LstItems
            // 
            this.LstItems.CheckBoxes = true;
            this.LstItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.LstItems.FullRowSelect = true;
            this.LstItems.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.LstItems.Location = new System.Drawing.Point(10, 43);
            this.LstItems.Name = "LstItems";
            this.LstItems.Size = new System.Drawing.Size(317, 132);
            this.LstItems.TabIndex = 6;
            this.LstItems.UseCompatibleStateImageBehavior = false;
            this.LstItems.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 250;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "ID";
            // 
            // cmbMode
            // 
            this.cmbMode.FormattingEnabled = true;
            this.cmbMode.Items.AddRange(new object[] {
            "Drop",
            "Grab",
            "Buy",
            "Sell"});
            this.cmbMode.Location = new System.Drawing.Point(333, 43);
            this.cmbMode.Name = "cmbMode";
            this.cmbMode.Size = new System.Drawing.Size(119, 21);
            this.cmbMode.TabIndex = 3;
            this.cmbMode.Text = "Select Mode";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(380, 95);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "00 -> All units";
            // 
            // txtUnit
            // 
            this.txtUnit.Location = new System.Drawing.Point(333, 92);
            this.txtUnit.Name = "txtUnit";
            this.txtUnit.Size = new System.Drawing.Size(44, 20);
            this.txtUnit.TabIndex = 5;
            this.txtUnit.Text = "00";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(330, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Action Item";
            // 
            // txtSearchItems
            // 
            this.txtSearchItems.Location = new System.Drawing.Point(80, 15);
            this.txtSearchItems.Name = "txtSearchItems";
            this.txtSearchItems.Size = new System.Drawing.Size(247, 20);
            this.txtSearchItems.TabIndex = 7;
            this.txtSearchItems.TextChanged += new System.EventHandler(this.txtSearchItems_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(330, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Unit";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Search Item";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.chkUpdateMineral);
            this.tabPage4.Controls.Add(this.label8);
            this.tabPage4.Controls.Add(this.RefineEfficiencyInput);
            this.tabPage4.Controls.Add(this.RefineCheckBox);
            this.tabPage4.Controls.Add(this.cbxUndersell);
            this.tabPage4.Controls.Add(this.cbxSell);
            this.tabPage4.Controls.Add(this.tbTotalSold);
            this.tabPage4.Controls.Add(this.label9);
            this.tabPage4.Controls.Add(this.tbTotalMedian);
            this.tabPage4.Controls.Add(this.label10);
            this.tabPage4.Controls.Add(this.lvItems);
            this.tabPage4.Controls.Add(this.bttnTaskValueDump);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(648, 245);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "ValueDump";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // chkUpdateMineral
            // 
            this.chkUpdateMineral.AutoSize = true;
            this.chkUpdateMineral.Location = new System.Drawing.Point(286, 33);
            this.chkUpdateMineral.Name = "chkUpdateMineral";
            this.chkUpdateMineral.Size = new System.Drawing.Size(125, 17);
            this.chkUpdateMineral.TabIndex = 26;
            this.chkUpdateMineral.Text = "Update Mineral Price";
            this.chkUpdateMineral.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(470, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 13);
            this.label8.TabIndex = 25;
            this.label8.Text = "Refine efficiency:";
            // 
            // RefineEfficiencyInput
            // 
            this.RefineEfficiencyInput.Location = new System.Drawing.Point(565, 31);
            this.RefineEfficiencyInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.RefineEfficiencyInput.Name = "RefineEfficiencyInput";
            this.RefineEfficiencyInput.Size = new System.Drawing.Size(49, 20);
            this.RefineEfficiencyInput.TabIndex = 24;
            this.RefineEfficiencyInput.Value = new decimal(new int[] {
            95,
            0,
            0,
            0});
            // 
            // RefineCheckBox
            // 
            this.RefineCheckBox.AutoSize = true;
            this.RefineCheckBox.Location = new System.Drawing.Point(159, 34);
            this.RefineCheckBox.Name = "RefineCheckBox";
            this.RefineCheckBox.Size = new System.Drawing.Size(117, 17);
            this.RefineCheckBox.TabIndex = 23;
            this.RefineCheckBox.Text = "Check refine prices";
            this.RefineCheckBox.UseVisualStyleBackColor = true;
            // 
            // cbxUndersell
            // 
            this.cbxUndersell.AutoSize = true;
            this.cbxUndersell.Location = new System.Drawing.Point(83, 34);
            this.cbxUndersell.Name = "cbxUndersell";
            this.cbxUndersell.Size = new System.Drawing.Size(70, 17);
            this.cbxUndersell.TabIndex = 21;
            this.cbxUndersell.Text = "Undersell";
            this.cbxUndersell.UseVisualStyleBackColor = true;
            // 
            // cbxSell
            // 
            this.cbxSell.AutoSize = true;
            this.cbxSell.Location = new System.Drawing.Point(7, 34);
            this.cbxSell.Name = "cbxSell";
            this.cbxSell.Size = new System.Drawing.Size(70, 17);
            this.cbxSell.TabIndex = 20;
            this.cbxSell.Text = "Sell items";
            this.cbxSell.UseVisualStyleBackColor = true;
            // 
            // tbTotalSold
            // 
            this.tbTotalSold.Location = new System.Drawing.Point(502, 5);
            this.tbTotalSold.Name = "tbTotalSold";
            this.tbTotalSold.Size = new System.Drawing.Size(112, 20);
            this.tbTotalSold.TabIndex = 19;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(388, 10);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(108, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Amount (Station Sell):";
            // 
            // tbTotalMedian
            // 
            this.tbTotalMedian.Location = new System.Drawing.Point(286, 7);
            this.tbTotalMedian.Name = "tbTotalMedian";
            this.tbTotalMedian.Size = new System.Drawing.Size(96, 20);
            this.tbTotalMedian.TabIndex = 17;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(169, 10);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(111, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "Amount (Median Buy):";
            // 
            // lvItems
            // 
            this.lvItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lvItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chName,
            this.chQuantity,
            this.chQuantitySold,
            this.chMedianBuy,
            this.chStationBuy,
            this.chTotalBuy});
            this.lvItems.Location = new System.Drawing.Point(7, 57);
            this.lvItems.Name = "lvItems";
            this.lvItems.Size = new System.Drawing.Size(630, 179);
            this.lvItems.TabIndex = 15;
            this.lvItems.UseCompatibleStateImageBehavior = false;
            this.lvItems.View = System.Windows.Forms.View.Details;
            // 
            // chName
            // 
            this.chName.Text = "Name";
            this.chName.Width = 138;
            // 
            // chQuantity
            // 
            this.chQuantity.Text = "Quantity";
            this.chQuantity.Width = 54;
            // 
            // chQuantitySold
            // 
            this.chQuantitySold.Text = "Quantity Sold";
            this.chQuantitySold.Width = 108;
            // 
            // chMedianBuy
            // 
            this.chMedianBuy.Text = "Price (Median)";
            this.chMedianBuy.Width = 108;
            // 
            // chStationBuy
            // 
            this.chStationBuy.Text = "Price (Station)";
            this.chStationBuy.Width = 74;
            // 
            // chTotalBuy
            // 
            this.chTotalBuy.Text = "Total";
            // 
            // bttnTaskValueDump
            // 
            this.bttnTaskValueDump.Location = new System.Drawing.Point(7, 5);
            this.bttnTaskValueDump.Name = "bttnTaskValueDump";
            this.bttnTaskValueDump.Size = new System.Drawing.Size(75, 23);
            this.bttnTaskValueDump.TabIndex = 14;
            this.bttnTaskValueDump.Text = "Add Task";
            this.bttnTaskValueDump.UseVisualStyleBackColor = true;
            this.bttnTaskValueDump.Click += new System.EventHandler(this.bttnTaskValueDump_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 270);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.Text = "Questor Traveler/Transport";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RefineEfficiencyInput)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer UpdateSearchResults;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button BttnStart;
        private System.Windows.Forms.ListView LstTask;
        private System.Windows.Forms.ColumnHeader JobHeader1;
        private System.Windows.Forms.ColumnHeader DescriptionHeader1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label LblStatus;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListView SearchResults;
        private System.Windows.Forms.ColumnHeader NameHeader;
        private System.Windows.Forms.ColumnHeader TypeHeader;
        private System.Windows.Forms.Label SearchLabel;
        private System.Windows.Forms.Button RefreshBookmarksButton;
        private System.Windows.Forms.TextBox SearchTextBox;
        private System.Windows.Forms.Button BttnAddTraveler;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button bttnDelete;
        private System.Windows.Forms.Button bttnDown;
        private System.Windows.Forms.Button bttnUP;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BttnTaskForItem;
        private System.Windows.Forms.ListView LstItems;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ComboBox cmbMode;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtUnit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSearchItems;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button bttnTaskAllItems;
        private System.Windows.Forms.ComboBox cmbAllMode;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button bttnTaskMakeShip;
        private System.Windows.Forms.TextBox txtNameShip;
        private System.Windows.Forms.CheckBox chkPause;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtNameCorp;
        private System.Windows.Forms.RadioButton rbttnCorp;
        private System.Windows.Forms.RadioButton rbttnShip;
        private System.Windows.Forms.RadioButton rbttnLocal;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.NumericUpDown RefineEfficiencyInput;
        public System.Windows.Forms.CheckBox RefineCheckBox;
        public System.Windows.Forms.CheckBox cbxUndersell;
        public System.Windows.Forms.CheckBox cbxSell;
        private System.Windows.Forms.TextBox tbTotalSold;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox tbTotalMedian;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ListView lvItems;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chQuantity;
        private System.Windows.Forms.ColumnHeader chQuantitySold;
        private System.Windows.Forms.ColumnHeader chMedianBuy;
        private System.Windows.Forms.ColumnHeader chStationBuy;
        private System.Windows.Forms.ColumnHeader chTotalBuy;
        private System.Windows.Forms.Button bttnTaskValueDump;
        public System.Windows.Forms.CheckBox chkUpdateMineral;
        private System.Windows.Forms.Button bttnSaveTask;
        private System.Windows.Forms.ComboBox cmbXML;


    }
}

