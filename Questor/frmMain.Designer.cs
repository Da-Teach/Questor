namespace Questor
{
    partial class frmMain
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
            this.AutoStartCheckBox = new System.Windows.Forms.CheckBox();
            this.tUpdateUI = new System.Windows.Forms.Timer(this.components);
            this.DamageTypeComboBox = new System.Windows.Forms.ComboBox();
            this.lblDamageType = new System.Windows.Forms.Label();
            this.lblQuestorState = new System.Windows.Forms.Label();
            this.QuestorStateComboBox = new System.Windows.Forms.ComboBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.PauseCheckBox = new System.Windows.Forms.CheckBox();
            this.Disable3DCheckBox = new System.Windows.Forms.CheckBox();
            this.chkShowDetails = new System.Windows.Forms.CheckBox();
            this.lblMissionName = new System.Windows.Forms.Label();
            this.lblCurrentMissionInfo = new System.Windows.Forms.Label();
            this.lblPocketAction = new System.Windows.Forms.Label();
            this.lblCurrentPocketAction = new System.Windows.Forms.Label();
            this.buttonQuestormanager = new System.Windows.Forms.Button();
            this.buttonQuestorStatistics = new System.Windows.Forms.Button();
            this.buttonSettingsXML = new System.Windows.Forms.Button();
            this.buttonOpenMissionXML = new System.Windows.Forms.Button();
            this.buttonOpenLogDirectory = new System.Windows.Forms.Button();
            this.LiveScheduling = new System.Windows.Forms.TabPage();
            this.lblStartTime1 = new System.Windows.Forms.Label();
            this.lblMaxRunTime1 = new System.Windows.Forms.Label();
            this.dateTimePickerStartTime = new System.Windows.Forms.DateTimePicker();
            this.textBoxMaxRunTime = new System.Windows.Forms.TextBox();
            this.lblMaxRuntime2 = new System.Windows.Forms.Label();
            this.lblStopTime = new System.Windows.Forms.Label();
            this.dateTimePickerStopTime = new System.Windows.Forms.DateTimePicker();
            this.Console = new System.Windows.Forms.TabPage();
            this.txtExtConsole = new System.Windows.Forms.TextBox();
            this.txtComand = new System.Windows.Forms.TextBox();
            this.tabInterface1 = new System.Windows.Forms.TabControl();
            this.LiveScheduling.SuspendLayout();
            this.Console.SuspendLayout();
            this.tabInterface1.SuspendLayout();
            this.SuspendLayout();
            // 
            // AutoStartCheckBox
            // 
            this.AutoStartCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.AutoStartCheckBox.Location = new System.Drawing.Point(215, 4);
            this.AutoStartCheckBox.Name = "AutoStartCheckBox";
            this.AutoStartCheckBox.Size = new System.Drawing.Size(68, 23);
            this.AutoStartCheckBox.TabIndex = 2;
            this.AutoStartCheckBox.Text = "Autostart";
            this.AutoStartCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.AutoStartCheckBox.UseVisualStyleBackColor = true;
            this.AutoStartCheckBox.CheckedChanged += new System.EventHandler(this.AutoStartCheckBox_CheckedChanged);
            // 
            // tUpdateUI
            // 
            this.tUpdateUI.Enabled = true;
            this.tUpdateUI.Interval = 50;
            this.tUpdateUI.Tick += new System.EventHandler(this.tUpdateUI_Tick);
            // 
            // DamageTypeComboBox
            // 
            this.DamageTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DamageTypeComboBox.FormattingEnabled = true;
            this.DamageTypeComboBox.Location = new System.Drawing.Point(79, 30);
            this.DamageTypeComboBox.Name = "DamageTypeComboBox";
            this.DamageTypeComboBox.Size = new System.Drawing.Size(130, 21);
            this.DamageTypeComboBox.TabIndex = 4;
            this.DamageTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.DamageTypeComboBox_SelectedIndexChanged);
            this.DamageTypeComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DamageTypeComboBox_MouseWheel);
            // 
            // lblDamageType
            // 
            this.lblDamageType.AutoSize = true;
            this.lblDamageType.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDamageType.Location = new System.Drawing.Point(1, 34);
            this.lblDamageType.Name = "lblDamageType";
            this.lblDamageType.Size = new System.Drawing.Size(77, 13);
            this.lblDamageType.TabIndex = 90;
            this.lblDamageType.Text = "Damage Type:";
            // 
            // lblQuestorState
            // 
            this.lblQuestorState.AutoSize = true;
            this.lblQuestorState.Location = new System.Drawing.Point(3, 9);
            this.lblQuestorState.Name = "lblQuestorState";
            this.lblQuestorState.Size = new System.Drawing.Size(75, 13);
            this.lblQuestorState.TabIndex = 1;
            this.lblQuestorState.Text = "Questor State:";
            // 
            // QuestorStateComboBox
            // 
            this.QuestorStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.QuestorStateComboBox.FormattingEnabled = true;
            this.QuestorStateComboBox.Location = new System.Drawing.Point(79, 4);
            this.QuestorStateComboBox.Name = "QuestorStateComboBox";
            this.QuestorStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.QuestorStateComboBox.TabIndex = 1;
            this.QuestorStateComboBox.SelectedIndexChanged += new System.EventHandler(this.QuestorStateComboBox_SelectedIndexChanged);
            this.QuestorStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.QuestorStateComboBox_MouseWheel);
            // 
            // StartButton
            // 
            this.StartButton.AutoSize = true;
            this.StartButton.Location = new System.Drawing.Point(285, 4);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(68, 23);
            this.StartButton.TabIndex = 3;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // PauseCheckBox
            // 
            this.PauseCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.PauseCheckBox.Location = new System.Drawing.Point(285, 30);
            this.PauseCheckBox.Name = "PauseCheckBox";
            this.PauseCheckBox.Size = new System.Drawing.Size(68, 23);
            this.PauseCheckBox.TabIndex = 6;
            this.PauseCheckBox.Text = "Pause";
            this.PauseCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.PauseCheckBox.UseVisualStyleBackColor = true;
            this.PauseCheckBox.CheckedChanged += new System.EventHandler(this.PauseCheckBox_CheckedChanged);
            // 
            // Disable3DCheckBox
            // 
            this.Disable3DCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.Disable3DCheckBox.Location = new System.Drawing.Point(215, 30);
            this.Disable3DCheckBox.Name = "Disable3DCheckBox";
            this.Disable3DCheckBox.Size = new System.Drawing.Size(68, 23);
            this.Disable3DCheckBox.TabIndex = 5;
            this.Disable3DCheckBox.Text = "Disable 3D";
            this.Disable3DCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Disable3DCheckBox.UseVisualStyleBackColor = true;
            this.Disable3DCheckBox.CheckedChanged += new System.EventHandler(this.Disable3DCheckBox_CheckedChanged);
            // 
            // chkShowDetails
            // 
            this.chkShowDetails.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkShowDetails.Location = new System.Drawing.Point(270, 68);
            this.chkShowDetails.Name = "chkShowDetails";
            this.chkShowDetails.Size = new System.Drawing.Size(83, 23);
            this.chkShowDetails.TabIndex = 7;
            this.chkShowDetails.Text = "Show Details";
            this.chkShowDetails.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkShowDetails.UseVisualStyleBackColor = true;
            this.chkShowDetails.CheckedChanged += new System.EventHandler(this.chkShowConsole_CheckedChanged);
            // 
            // lblMissionName
            // 
            this.lblMissionName.AutoSize = true;
            this.lblMissionName.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblMissionName.Location = new System.Drawing.Point(2, 55);
            this.lblMissionName.Name = "lblMissionName";
            this.lblMissionName.Size = new System.Drawing.Size(76, 13);
            this.lblMissionName.TabIndex = 92;
            this.lblMissionName.Text = "Mission Name:";
            // 
            // lblCurrentMissionInfo
            // 
            this.lblCurrentMissionInfo.Location = new System.Drawing.Point(76, 55);
            this.lblCurrentMissionInfo.MaximumSize = new System.Drawing.Size(250, 13);
            this.lblCurrentMissionInfo.MinimumSize = new System.Drawing.Size(275, 13);
            this.lblCurrentMissionInfo.Name = "lblCurrentMissionInfo";
            this.lblCurrentMissionInfo.Size = new System.Drawing.Size(275, 13);
            this.lblCurrentMissionInfo.TabIndex = 93;
            this.lblCurrentMissionInfo.Text = "[ No Mission Selected Yet ]";
            // 
            // lblPocketAction
            // 
            this.lblPocketAction.AutoSize = true;
            this.lblPocketAction.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblPocketAction.Location = new System.Drawing.Point(1, 73);
            this.lblPocketAction.Name = "lblPocketAction";
            this.lblPocketAction.Size = new System.Drawing.Size(77, 13);
            this.lblPocketAction.TabIndex = 94;
            this.lblPocketAction.Text = "PocketAction: ";
            // 
            // lblCurrentPocketAction
            // 
            this.lblCurrentPocketAction.Location = new System.Drawing.Point(76, 73);
            this.lblCurrentPocketAction.MaximumSize = new System.Drawing.Size(180, 15);
            this.lblCurrentPocketAction.MinimumSize = new System.Drawing.Size(180, 15);
            this.lblCurrentPocketAction.Name = "lblCurrentPocketAction";
            this.lblCurrentPocketAction.Size = new System.Drawing.Size(180, 15);
            this.lblCurrentPocketAction.TabIndex = 95;
            this.lblCurrentPocketAction.Text = "[  ]";
            // 
            // buttonQuestormanager
            // 
            this.buttonQuestormanager.Location = new System.Drawing.Point(371, 4);
            this.buttonQuestormanager.Name = "buttonQuestormanager";
            this.buttonQuestormanager.Size = new System.Drawing.Size(109, 23);
            this.buttonQuestormanager.TabIndex = 107;
            this.buttonQuestormanager.Text = "QuestorManager";
            this.buttonQuestormanager.UseVisualStyleBackColor = true;
            this.buttonQuestormanager.Click += new System.EventHandler(this.buttonQuestormanager_Click);
            // 
            // buttonQuestorStatistics
            // 
            this.buttonQuestorStatistics.Location = new System.Drawing.Point(486, 4);
            this.buttonQuestorStatistics.Name = "buttonQuestorStatistics";
            this.buttonQuestorStatistics.Size = new System.Drawing.Size(109, 23);
            this.buttonQuestorStatistics.TabIndex = 108;
            this.buttonQuestorStatistics.Text = "QuestorStatistics";
            this.buttonQuestorStatistics.UseVisualStyleBackColor = true;
            this.buttonQuestorStatistics.Click += new System.EventHandler(this.buttonQuestorStatistics_Click);
            // 
            // buttonSettingsXML
            // 
            this.buttonSettingsXML.Location = new System.Drawing.Point(601, 4);
            this.buttonSettingsXML.Name = "buttonSettingsXML";
            this.buttonSettingsXML.Size = new System.Drawing.Size(109, 23);
            this.buttonSettingsXML.TabIndex = 110;
            this.buttonSettingsXML.Text = "QuestorSettings";
            this.buttonSettingsXML.UseVisualStyleBackColor = true;
            // 
            // buttonOpenMissionXML
            // 
            this.buttonOpenMissionXML.Location = new System.Drawing.Point(486, 30);
            this.buttonOpenMissionXML.Name = "buttonOpenMissionXML";
            this.buttonOpenMissionXML.Size = new System.Drawing.Size(224, 23);
            this.buttonOpenMissionXML.TabIndex = 118;
            this.buttonOpenMissionXML.Text = "Open Current Mission XML";
            this.buttonOpenMissionXML.UseVisualStyleBackColor = true;
            this.buttonOpenMissionXML.Click += new System.EventHandler(this.buttonOpenMissionXML_Click);
            // 
            // buttonOpenLogDirectory
            // 
            this.buttonOpenLogDirectory.Location = new System.Drawing.Point(371, 30);
            this.buttonOpenLogDirectory.Name = "buttonOpenLogDirectory";
            this.buttonOpenLogDirectory.Size = new System.Drawing.Size(109, 23);
            this.buttonOpenLogDirectory.TabIndex = 109;
            this.buttonOpenLogDirectory.Text = "Open Log Directory";
            this.buttonOpenLogDirectory.UseVisualStyleBackColor = true;
            this.buttonOpenLogDirectory.Click += new System.EventHandler(this.buttonOpenLogDirectory_Click);
            // 
            // LiveScheduling
            // 
            this.LiveScheduling.Controls.Add(this.dateTimePickerStopTime);
            this.LiveScheduling.Controls.Add(this.lblStopTime);
            this.LiveScheduling.Controls.Add(this.lblMaxRuntime2);
            this.LiveScheduling.Controls.Add(this.textBoxMaxRunTime);
            this.LiveScheduling.Controls.Add(this.dateTimePickerStartTime);
            this.LiveScheduling.Controls.Add(this.lblMaxRunTime1);
            this.LiveScheduling.Controls.Add(this.lblStartTime1);
            this.LiveScheduling.Location = new System.Drawing.Point(4, 22);
            this.LiveScheduling.Name = "LiveScheduling";
            this.LiveScheduling.Padding = new System.Windows.Forms.Padding(3);
            this.LiveScheduling.Size = new System.Drawing.Size(769, 276);
            this.LiveScheduling.TabIndex = 2;
            this.LiveScheduling.Text = "LiveScheduling";
            this.LiveScheduling.UseVisualStyleBackColor = true;
            // 
            // lblStartTime1
            // 
            this.lblStartTime1.AutoSize = true;
            this.lblStartTime1.Location = new System.Drawing.Point(302, 109);
            this.lblStartTime1.Name = "lblStartTime1";
            this.lblStartTime1.Size = new System.Drawing.Size(52, 13);
            this.lblStartTime1.TabIndex = 107;
            this.lblStartTime1.Text = "StartTime";
            // 
            // lblMaxRunTime1
            // 
            this.lblMaxRunTime1.AutoSize = true;
            this.lblMaxRunTime1.Location = new System.Drawing.Point(288, 155);
            this.lblMaxRunTime1.Name = "lblMaxRunTime1";
            this.lblMaxRunTime1.Size = new System.Drawing.Size(66, 13);
            this.lblMaxRunTime1.TabIndex = 108;
            this.lblMaxRunTime1.Text = "MaxRuntime";
            // 
            // dateTimePickerStartTime
            // 
            this.dateTimePickerStartTime.Enabled = false;
            this.dateTimePickerStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePickerStartTime.Location = new System.Drawing.Point(360, 105);
            this.dateTimePickerStartTime.Name = "dateTimePickerStartTime";
            this.dateTimePickerStartTime.ShowUpDown = true;
            this.dateTimePickerStartTime.Size = new System.Drawing.Size(121, 20);
            this.dateTimePickerStartTime.TabIndex = 109;
            // 
            // textBoxMaxRunTime
            // 
            this.textBoxMaxRunTime.Location = new System.Drawing.Point(360, 152);
            this.textBoxMaxRunTime.Name = "textBoxMaxRunTime";
            this.textBoxMaxRunTime.Size = new System.Drawing.Size(46, 20);
            this.textBoxMaxRunTime.TabIndex = 110;
            this.textBoxMaxRunTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblMaxRuntime2
            // 
            this.lblMaxRuntime2.AutoSize = true;
            this.lblMaxRuntime2.Location = new System.Drawing.Point(412, 155);
            this.lblMaxRuntime2.Name = "lblMaxRuntime2";
            this.lblMaxRuntime2.Size = new System.Drawing.Size(69, 13);
            this.lblMaxRuntime2.TabIndex = 111;
            this.lblMaxRuntime2.Text = "min from start";
            // 
            // lblStopTime
            // 
            this.lblStopTime.AutoSize = true;
            this.lblStopTime.Location = new System.Drawing.Point(302, 133);
            this.lblStopTime.Name = "lblStopTime";
            this.lblStopTime.Size = new System.Drawing.Size(52, 13);
            this.lblStopTime.TabIndex = 112;
            this.lblStopTime.Text = "StopTime";
            // 
            // dateTimePickerStopTime
            // 
            this.dateTimePickerStopTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePickerStopTime.Location = new System.Drawing.Point(360, 129);
            this.dateTimePickerStopTime.Name = "dateTimePickerStopTime";
            this.dateTimePickerStopTime.ShowUpDown = true;
            this.dateTimePickerStopTime.Size = new System.Drawing.Size(121, 20);
            this.dateTimePickerStopTime.TabIndex = 113;
            // 
            // Console
            // 
            this.Console.Controls.Add(this.txtComand);
            this.Console.Controls.Add(this.txtExtConsole);
            this.Console.Location = new System.Drawing.Point(4, 22);
            this.Console.Name = "Console";
            this.Console.Padding = new System.Windows.Forms.Padding(3);
            this.Console.Size = new System.Drawing.Size(769, 276);
            this.Console.TabIndex = 0;
            this.Console.Text = "Console";
            this.Console.UseVisualStyleBackColor = true;
            // 
            // txtExtConsole
            // 
            this.txtExtConsole.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExtConsole.Location = new System.Drawing.Point(0, 3);
            this.txtExtConsole.Multiline = true;
            this.txtExtConsole.Name = "txtExtConsole";
            this.txtExtConsole.ReadOnly = true;
            this.txtExtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExtConsole.Size = new System.Drawing.Size(773, 234);
            this.txtExtConsole.TabIndex = 25;
            // 
            // txtComand
            // 
            this.txtComand.Location = new System.Drawing.Point(3, 243);
            this.txtComand.Name = "txtComand";
            this.txtComand.Size = new System.Drawing.Size(760, 20);
            this.txtComand.TabIndex = 26;
            // 
            // tabInterface1
            // 
            this.tabInterface1.Controls.Add(this.Console);
            this.tabInterface1.Controls.Add(this.LiveScheduling);
            this.tabInterface1.Location = new System.Drawing.Point(4, 101);
            this.tabInterface1.Name = "tabInterface1";
            this.tabInterface1.SelectedIndex = 0;
            this.tabInterface1.Size = new System.Drawing.Size(777, 302);
            this.tabInterface1.TabIndex = 117;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.ClientSize = new System.Drawing.Size(356, 96);
            this.Controls.Add(this.buttonOpenMissionXML);
            this.Controls.Add(this.tabInterface1);
            this.Controls.Add(this.buttonSettingsXML);
            this.Controls.Add(this.buttonOpenLogDirectory);
            this.Controls.Add(this.buttonQuestorStatistics);
            this.Controls.Add(this.buttonQuestormanager);
            this.Controls.Add(this.lblCurrentPocketAction);
            this.Controls.Add(this.lblPocketAction);
            this.Controls.Add(this.lblCurrentMissionInfo);
            this.Controls.Add(this.lblMissionName);
            this.Controls.Add(this.chkShowDetails);
            this.Controls.Add(this.Disable3DCheckBox);
            this.Controls.Add(this.PauseCheckBox);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.QuestorStateComboBox);
            this.Controls.Add(this.lblQuestorState);
            this.Controls.Add(this.lblDamageType);
            this.Controls.Add(this.DamageTypeComboBox);
            this.Controls.Add(this.AutoStartCheckBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "Questor";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.LiveScheduling.ResumeLayout(false);
            this.LiveScheduling.PerformLayout();
            this.Console.ResumeLayout(false);
            this.Console.PerformLayout();
            this.tabInterface1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.CheckBox AutoStartCheckBox;
        private System.Windows.Forms.Timer tUpdateUI;
        private System.Windows.Forms.ComboBox DamageTypeComboBox;
        private System.Windows.Forms.Label lblDamageType;
        private System.Windows.Forms.Label lblQuestorState;
        private System.Windows.Forms.ComboBox QuestorStateComboBox;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.CheckBox PauseCheckBox;
        private System.Windows.Forms.CheckBox Disable3DCheckBox;
        //private System.Windows.Forms.Button chkTraveler;
        //private System.Windows.Forms.CheckBox Anomaly_chk;
        private System.Windows.Forms.CheckBox chkShowDetails;
        private System.Windows.Forms.Label lblMissionName;
        private System.Windows.Forms.Label lblCurrentMissionInfo;
        private System.Windows.Forms.Label lblPocketAction;
        private System.Windows.Forms.Label lblCurrentPocketAction;
        private System.Windows.Forms.Button buttonQuestormanager;
        private System.Windows.Forms.Button buttonQuestorStatistics;
        private System.Windows.Forms.Button buttonSettingsXML;
        private System.Windows.Forms.Button buttonOpenMissionXML;
        private System.Windows.Forms.Button buttonOpenLogDirectory;
        private System.Windows.Forms.TabPage LiveScheduling;
        private System.Windows.Forms.DateTimePicker dateTimePickerStopTime;
        private System.Windows.Forms.Label lblStopTime;
        private System.Windows.Forms.Label lblMaxRuntime2;
        private System.Windows.Forms.TextBox textBoxMaxRunTime;
        private System.Windows.Forms.DateTimePicker dateTimePickerStartTime;
        private System.Windows.Forms.Label lblMaxRunTime1;
        private System.Windows.Forms.Label lblStartTime1;
        private System.Windows.Forms.TabPage Console;
        private System.Windows.Forms.TextBox txtComand;
        private System.Windows.Forms.TextBox txtExtConsole;
        private System.Windows.Forms.TabControl tabInterface1;
    }
}

