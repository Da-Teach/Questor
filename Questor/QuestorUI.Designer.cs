namespace Questor
{
    partial class QuestorfrmMain
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
        //public class QuestorMode
        //{
        //    public string Name { get; set; }
        //    public string Value { get; set; }
        //}
        //public IList<QuestorMode> QuestorModeList = new List<QuestorMode>();

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
            this.PauseCheckBox = new System.Windows.Forms.CheckBox();
            this.Disable3DCheckBox = new System.Windows.Forms.CheckBox();
            this.chkShowDetails = new System.Windows.Forms.CheckBox();
            this.lblMissionName = new System.Windows.Forms.Label();
            this.lblCurrentMissionInfo = new System.Windows.Forms.Label();
            this.lblPocketAction = new System.Windows.Forms.Label();
            this.lblCurrentPocketAction = new System.Windows.Forms.Label();
            this.buttonQuestorStatistics = new System.Windows.Forms.Button();
            this.buttonQuestorSettings = new System.Windows.Forms.Button();
            this.buttonOpenMissionXML = new System.Windows.Forms.Button();
            this.buttonOpenLogDirectory = new System.Windows.Forms.Button();
            this.Console = new System.Windows.Forms.TabPage();
            this.txtComand = new System.Windows.Forms.TextBox();
            this.txtExtConsole = new System.Windows.Forms.RichTextBox();
            this.Tabs = new System.Windows.Forms.TabControl();
            this.States = new System.Windows.Forms.TabPage();
            this.label19 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label18 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.SalvageStateComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.LocalWatchStateComboBox = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.CleanupStateComboBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.DronesStateComboBox = new System.Windows.Forms.ComboBox();
            this.CombatStateComboBox = new System.Windows.Forms.ComboBox();
            this.PanicStateComboBox = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.AgentInteractionStateComboBox = new System.Windows.Forms.ComboBox();
            this.TravelerStateComboBox = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.UnloadStateComboBox = new System.Windows.Forms.ComboBox();
            this.ArmStateComboBox = new System.Windows.Forms.ComboBox();
            this.StorylineStateComboBox = new System.Windows.Forms.ComboBox();
            this.CombatMissionCtrlStateComboBox = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Schedule = new System.Windows.Forms.TabPage();
            this.Targets = new System.Windows.Forms.TabPage();
            this.lblDistance3 = new System.Windows.Forms.Label();
            this.lblDistanceDroneTarget = new System.Windows.Forms.Label();
            this.lblDistanceWeaponsTarget = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.CurrentPriorityTargetsData = new System.Windows.Forms.Label();
            this.CurrentPocketNumberData = new System.Windows.Forms.Label();
            this.MissionPocketNumberlbl = new System.Windows.Forms.Label();
            this.CurrentMissionActionData = new System.Windows.Forms.Label();
            this.CurrentWeaponsTargetData = new System.Windows.Forms.Label();
            this.CurrentDroneTargetData = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.EWar = new System.Windows.Forms.TabPage();
            this.dataEntitiesTargetPaintingMe = new System.Windows.Forms.Label();
            this.lblTargetPaintingMe = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.lblTitleEWar = new System.Windows.Forms.Label();
            this.dataEntitiesDampening = new System.Windows.Forms.Label();
            this.lblDampeningMe = new System.Windows.Forms.Label();
            this.dataEntitiesTrackingDisruptingMe = new System.Windows.Forms.Label();
            this.lblTrackingDisruptingMe = new System.Windows.Forms.Label();
            this.dataEntitiesNeutralizingMe = new System.Windows.Forms.Label();
            this.lblWebbingMe = new System.Windows.Forms.Label();
            this.dataEntitiesWebbingMe = new System.Windows.Forms.Label();
            this.lblNeutralizingMe = new System.Windows.Forms.Label();
            this.dataEntitiesJammingMe = new System.Windows.Forms.Label();
            this.lblJammingMe = new System.Windows.Forms.Label();
            this.dataEntitiesWarpDisruptingMe = new System.Windows.Forms.Label();
            this.lblWarpDisrupingMe = new System.Windows.Forms.Label();
            this.BehaviorComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.QuestorStateComboBox = new System.Windows.Forms.ComboBox();
            this.QuestorStatelbl = new System.Windows.Forms.Label();
            this.buttonOpenSchedulesXML = new System.Windows.Forms.Button();
            this.buttonOpenCharacterXML = new System.Windows.Forms.Button();
            this.buttonQuestormanager = new System.Windows.Forms.Button();
            this.label25 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.Console.SuspendLayout();
            this.Tabs.SuspendLayout();
            this.States.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.Targets.SuspendLayout();
            this.EWar.SuspendLayout();
            this.SuspendLayout();
            // 
            // AutoStartCheckBox
            // 
            this.AutoStartCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.AutoStartCheckBox.Location = new System.Drawing.Point(222, 28);
            this.AutoStartCheckBox.Name = "AutoStartCheckBox";
            this.AutoStartCheckBox.Size = new System.Drawing.Size(65, 23);
            this.AutoStartCheckBox.TabIndex = 2;
            this.AutoStartCheckBox.Text = "Autostart";
            this.AutoStartCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.AutoStartCheckBox.UseVisualStyleBackColor = true;
            this.AutoStartCheckBox.CheckedChanged += new System.EventHandler(this.AutoStartCheckBoxCheckedChanged);
            // 
            // tUpdateUI
            // 
            this.tUpdateUI.Enabled = true;
            this.tUpdateUI.Interval = 50;
            this.tUpdateUI.Tick += new System.EventHandler(this.UpdateUiTick);
            // 
            // DamageTypeComboBox
            // 
            this.DamageTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DamageTypeComboBox.FormattingEnabled = true;
            this.DamageTypeComboBox.Location = new System.Drawing.Point(288, 3);
            this.DamageTypeComboBox.Name = "DamageTypeComboBox";
            this.DamageTypeComboBox.Size = new System.Drawing.Size(65, 21);
            this.DamageTypeComboBox.TabIndex = 4;
            this.DamageTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.DamageTypeComboBoxSelectedIndexChanged);
            this.DamageTypeComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // lblDamageType
            // 
            this.lblDamageType.AutoSize = true;
            this.lblDamageType.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblDamageType.Location = new System.Drawing.Point(232, 6);
            this.lblDamageType.Name = "lblDamageType";
            this.lblDamageType.Size = new System.Drawing.Size(50, 13);
            this.lblDamageType.TabIndex = 90;
            this.lblDamageType.Text = "Damage:";
            // 
            // PauseCheckBox
            // 
            this.PauseCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.PauseCheckBox.Location = new System.Drawing.Point(288, 28);
            this.PauseCheckBox.Name = "PauseCheckBox";
            this.PauseCheckBox.Size = new System.Drawing.Size(65, 23);
            this.PauseCheckBox.TabIndex = 6;
            this.PauseCheckBox.Text = "Pause";
            this.PauseCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.PauseCheckBox.UseVisualStyleBackColor = true;
            this.PauseCheckBox.CheckedChanged += new System.EventHandler(this.PauseCheckBoxCheckedChanged);
            // 
            // Disable3DCheckBox
            // 
            this.Disable3DCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.Disable3DCheckBox.Location = new System.Drawing.Point(367, 1);
            this.Disable3DCheckBox.Name = "Disable3DCheckBox";
            this.Disable3DCheckBox.Size = new System.Drawing.Size(154, 23);
            this.Disable3DCheckBox.TabIndex = 5;
            this.Disable3DCheckBox.Text = "Disable 3D";
            this.Disable3DCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Disable3DCheckBox.UseVisualStyleBackColor = true;
            this.Disable3DCheckBox.CheckedChanged += new System.EventHandler(this.Disable3DCheckBoxCheckedChanged);
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
            this.chkShowDetails.CheckedChanged += new System.EventHandler(this.ChkShowConsoleCheckedChanged);
            // 
            // lblMissionName
            // 
            this.lblMissionName.AutoSize = true;
            this.lblMissionName.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblMissionName.Location = new System.Drawing.Point(2, 55);
            this.lblMissionName.Name = "lblMissionName";
            this.lblMissionName.Size = new System.Drawing.Size(0, 13);
            this.lblMissionName.TabIndex = 92;
            // 
            // lblCurrentMissionInfo
            // 
            this.lblCurrentMissionInfo.Location = new System.Drawing.Point(3, 55);
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
            this.lblPocketAction.Size = new System.Drawing.Size(0, 13);
            this.lblPocketAction.TabIndex = 94;
            // 
            // lblCurrentPocketAction
            // 
            this.lblCurrentPocketAction.Location = new System.Drawing.Point(2, 73);
            this.lblCurrentPocketAction.MaximumSize = new System.Drawing.Size(180, 15);
            this.lblCurrentPocketAction.MinimumSize = new System.Drawing.Size(180, 15);
            this.lblCurrentPocketAction.Name = "lblCurrentPocketAction";
            this.lblCurrentPocketAction.Size = new System.Drawing.Size(180, 15);
            this.lblCurrentPocketAction.TabIndex = 95;
            // 
            // buttonQuestorStatistics
            // 
            this.buttonQuestorStatistics.Location = new System.Drawing.Point(527, 1);
            this.buttonQuestorStatistics.Name = "buttonQuestorStatistics";
            this.buttonQuestorStatistics.Size = new System.Drawing.Size(156, 23);
            this.buttonQuestorStatistics.TabIndex = 108;
            this.buttonQuestorStatistics.Text = "QuestorStatistics";
            this.buttonQuestorStatistics.UseVisualStyleBackColor = true;
            this.buttonQuestorStatistics.Click += new System.EventHandler(this.ButtonQuestorStatisticsClick);
            // 
            // buttonQuestorSettings
            // 
            this.buttonQuestorSettings.Location = new System.Drawing.Point(527, 28);
            this.buttonQuestorSettings.Name = "buttonQuestorSettings";
            this.buttonQuestorSettings.Size = new System.Drawing.Size(156, 23);
            this.buttonQuestorSettings.TabIndex = 110;
            this.buttonQuestorSettings.Text = "QuestorSettings";
            this.buttonQuestorSettings.UseVisualStyleBackColor = true;
            this.buttonQuestorSettings.Click += new System.EventHandler(this.ButtonQuestorSettingsXMLClick);
            // 
            // buttonOpenMissionXML
            // 
            this.buttonOpenMissionXML.Location = new System.Drawing.Point(367, 57);
            this.buttonOpenMissionXML.Name = "buttonOpenMissionXML";
            this.buttonOpenMissionXML.Size = new System.Drawing.Size(154, 23);
            this.buttonOpenMissionXML.TabIndex = 118;
            this.buttonOpenMissionXML.Text = "Open Current Mission XML";
            this.buttonOpenMissionXML.UseVisualStyleBackColor = true;
            this.buttonOpenMissionXML.Click += new System.EventHandler(this.ButtonOpenMissionXmlClick);
            // 
            // buttonOpenLogDirectory
            // 
            this.buttonOpenLogDirectory.Location = new System.Drawing.Point(367, 28);
            this.buttonOpenLogDirectory.Name = "buttonOpenLogDirectory";
            this.buttonOpenLogDirectory.Size = new System.Drawing.Size(154, 23);
            this.buttonOpenLogDirectory.TabIndex = 109;
            this.buttonOpenLogDirectory.Text = "Log Directory";
            this.buttonOpenLogDirectory.UseVisualStyleBackColor = true;
            this.buttonOpenLogDirectory.Click += new System.EventHandler(this.ButtonOpenLogDirectoryClick);
            // 
            // Console
            // 
            this.Console.Controls.Add(this.txtComand);
            this.Console.Controls.Add(this.txtExtConsole);
            this.Console.Location = new System.Drawing.Point(4, 22);
            this.Console.Name = "Console";
            this.Console.Padding = new System.Windows.Forms.Padding(3);
            this.Console.Size = new System.Drawing.Size(687, 276);
            this.Console.TabIndex = 0;
            this.Console.Text = "Console";
            this.Console.UseVisualStyleBackColor = true;
            // 
            // txtComand
            // 
            this.txtComand.Location = new System.Drawing.Point(3, 243);
            this.txtComand.Name = "txtComand";
            this.txtComand.Size = new System.Drawing.Size(685, 20);
            this.txtComand.TabIndex = 26;
            // 
            // txtExtConsole
            // 
            this.txtExtConsole.BackColor = System.Drawing.SystemColors.Control;
            this.txtExtConsole.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtExtConsole.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExtConsole.Location = new System.Drawing.Point(0, 3);
            this.txtExtConsole.Multiline = true;
            this.txtExtConsole.Name = "txtExtConsole";
            this.txtExtConsole.ReadOnly = true;
            this.txtExtConsole.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtExtConsole.Size = new System.Drawing.Size(688, 234);
            this.txtExtConsole.TabIndex = 25;
            this.txtExtConsole.TextChanged += new System.EventHandler(this.TxtExtConsoleTextChanged);
            // 
            // Tabs
            // 
            this.Tabs.Controls.Add(this.Console);
            this.Tabs.Controls.Add(this.States);
            this.Tabs.Controls.Add(this.Schedule);
            this.Tabs.Controls.Add(this.Targets);
            this.Tabs.Controls.Add(this.EWar);
            this.Tabs.Location = new System.Drawing.Point(4, 101);
            this.Tabs.Name = "Tabs";
            this.Tabs.SelectedIndex = 0;
            this.Tabs.Size = new System.Drawing.Size(695, 302);
            this.Tabs.TabIndex = 117;
            // 
            // States
            // 
            this.States.Controls.Add(this.label19);
            this.States.Controls.Add(this.label11);
            this.States.Controls.Add(this.panel2);
            this.States.Controls.Add(this.panel1);
            this.States.Location = new System.Drawing.Point(4, 22);
            this.States.Name = "States";
            this.States.Padding = new System.Windows.Forms.Padding(3);
            this.States.Size = new System.Drawing.Size(687, 276);
            this.States.TabIndex = 1;
            this.States.Text = "States";
            this.States.UseVisualStyleBackColor = true;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(88, 258);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(400, 13);
            this.label19.TabIndex = 168;
            this.label19.Text = "it is a very bad idea to change these states unless you understand what will happ" +
                "en";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(88, 3);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(400, 13);
            this.label11.TabIndex = 167;
            this.label11.Text = "it is a very bad idea to change these states unless you understand what will happ" +
                "en";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label18);
            this.panel2.Controls.Add(this.label16);
            this.panel2.Controls.Add(this.label15);
            this.panel2.Controls.Add(this.SalvageStateComboBox);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.LocalWatchStateComboBox);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.CleanupStateComboBox);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.DronesStateComboBox);
            this.panel2.Controls.Add(this.CombatStateComboBox);
            this.panel2.Controls.Add(this.PanicStateComboBox);
            this.panel2.Location = new System.Drawing.Point(9, 19);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(257, 236);
            this.panel2.TabIndex = 154;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(11, 140);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(96, 13);
            this.label18.TabIndex = 166;
            this.label18.Text = "LocalWatch State:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(30, 170);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(77, 13);
            this.label16.TabIndex = 165;
            this.label16.Text = "Salvage State:";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(42, 19);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(65, 13);
            this.label15.TabIndex = 164;
            this.label15.Text = "Panic State:";
            // 
            // SalvageStateComboBox
            // 
            this.SalvageStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SalvageStateComboBox.FormattingEnabled = true;
            this.SalvageStateComboBox.Location = new System.Drawing.Point(113, 167);
            this.SalvageStateComboBox.Name = "SalvageStateComboBox";
            this.SalvageStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.SalvageStateComboBox.TabIndex = 161;
            this.SalvageStateComboBox.SelectedIndexChanged += new System.EventHandler(this.SalvageStateComboBoxSelectedIndexChanged);
            this.SalvageStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(35, 79);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(72, 13);
            this.label10.TabIndex = 160;
            this.label10.Text = "Drones State:";
            // 
            // LocalWatchStateComboBox
            // 
            this.LocalWatchStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LocalWatchStateComboBox.FormattingEnabled = true;
            this.LocalWatchStateComboBox.Location = new System.Drawing.Point(113, 137);
            this.LocalWatchStateComboBox.Name = "LocalWatchStateComboBox";
            this.LocalWatchStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.LocalWatchStateComboBox.TabIndex = 159;
            this.LocalWatchStateComboBox.SelectedIndexChanged += new System.EventHandler(this.LocalWatchStateComboBoxSelectedIndexChanged);
            this.LocalWatchStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(33, 49);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(74, 13);
            this.label9.TabIndex = 158;
            this.label9.Text = "Combat State:";
            // 
            // CleanupStateComboBox
            // 
            this.CleanupStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CleanupStateComboBox.FormattingEnabled = true;
            this.CleanupStateComboBox.Location = new System.Drawing.Point(112, 107);
            this.CleanupStateComboBox.Name = "CleanupStateComboBox";
            this.CleanupStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.CleanupStateComboBox.TabIndex = 157;
            this.CleanupStateComboBox.SelectedIndexChanged += new System.EventHandler(this.CleanupStateComboBoxSelectedIndexChanged);
            this.CleanupStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(30, 110);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 13);
            this.label8.TabIndex = 156;
            this.label8.Text = "Cleanup State:";
            // 
            // DronesStateComboBox
            // 
            this.DronesStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DronesStateComboBox.FormattingEnabled = true;
            this.DronesStateComboBox.Location = new System.Drawing.Point(113, 76);
            this.DronesStateComboBox.Name = "DronesStateComboBox";
            this.DronesStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.DronesStateComboBox.TabIndex = 155;
            this.DronesStateComboBox.SelectedIndexChanged += new System.EventHandler(this.DronesStateComboBoxSelectedIndexChanged);
            this.DronesStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // CombatStateComboBox
            // 
            this.CombatStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CombatStateComboBox.FormattingEnabled = true;
            this.CombatStateComboBox.Location = new System.Drawing.Point(113, 46);
            this.CombatStateComboBox.Name = "CombatStateComboBox";
            this.CombatStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.CombatStateComboBox.TabIndex = 154;
            this.CombatStateComboBox.SelectedIndexChanged += new System.EventHandler(this.CombatStateComboBoxSelectedIndexChanged);
            this.CombatStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // PanicStateComboBox
            // 
            this.PanicStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PanicStateComboBox.FormattingEnabled = true;
            this.PanicStateComboBox.Location = new System.Drawing.Point(113, 17);
            this.PanicStateComboBox.Name = "PanicStateComboBox";
            this.PanicStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.PanicStateComboBox.TabIndex = 153;
            this.PanicStateComboBox.SelectedIndexChanged += new System.EventHandler(this.PanicStateComboBoxSelectedIndexChanged);
            this.PanicStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.AgentInteractionStateComboBox);
            this.panel1.Controls.Add(this.TravelerStateComboBox);
            this.panel1.Controls.Add(this.label17);
            this.panel1.Controls.Add(this.UnloadStateComboBox);
            this.panel1.Controls.Add(this.ArmStateComboBox);
            this.panel1.Controls.Add(this.StorylineStateComboBox);
            this.panel1.Controls.Add(this.CombatMissionCtrlStateComboBox);
            this.panel1.Controls.Add(this.label13);
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Location = new System.Drawing.Point(277, 19);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(268, 236);
            this.panel1.TabIndex = 153;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(-203, 210);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 13);
            this.label5.TabIndex = 168;
            this.label5.Text = "CombatHelper State:";
            // 
            // AgentInteractionStateComboBox
            // 
            this.AgentInteractionStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AgentInteractionStateComboBox.FormattingEnabled = true;
            this.AgentInteractionStateComboBox.Location = new System.Drawing.Point(131, 168);
            this.AgentInteractionStateComboBox.Name = "AgentInteractionStateComboBox";
            this.AgentInteractionStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.AgentInteractionStateComboBox.TabIndex = 167;
            this.AgentInteractionStateComboBox.SelectedIndexChanged += new System.EventHandler(this.AgentInteractionStateComboBoxSelectedIndexChanged);
            this.AgentInteractionStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // TravelerStateComboBox
            // 
            this.TravelerStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TravelerStateComboBox.FormattingEnabled = true;
            this.TravelerStateComboBox.Location = new System.Drawing.Point(131, 138);
            this.TravelerStateComboBox.Name = "TravelerStateComboBox";
            this.TravelerStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.TravelerStateComboBox.TabIndex = 166;
            this.TravelerStateComboBox.SelectedIndexChanged += new System.EventHandler(this.TravelerStateComboBoxSelectedIndexChanged);
            this.TravelerStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(48, 141);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(77, 13);
            this.label17.TabIndex = 165;
            this.label17.Text = "Traveler State:";
            // 
            // UnloadStateComboBox
            // 
            this.UnloadStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.UnloadStateComboBox.FormattingEnabled = true;
            this.UnloadStateComboBox.Location = new System.Drawing.Point(131, 108);
            this.UnloadStateComboBox.Name = "UnloadStateComboBox";
            this.UnloadStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.UnloadStateComboBox.TabIndex = 164;
            this.UnloadStateComboBox.SelectedIndexChanged += new System.EventHandler(this.UnloadStateComboBoxSelectedIndexChanged);
            this.UnloadStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // ArmStateComboBox
            // 
            this.ArmStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ArmStateComboBox.FormattingEnabled = true;
            this.ArmStateComboBox.Location = new System.Drawing.Point(131, 78);
            this.ArmStateComboBox.Name = "ArmStateComboBox";
            this.ArmStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.ArmStateComboBox.TabIndex = 163;
            this.ArmStateComboBox.SelectedIndexChanged += new System.EventHandler(this.ArmStateComboBoxSelectedIndexChanged);
            this.ArmStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // StorylineStateComboBox
            // 
            this.StorylineStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StorylineStateComboBox.FormattingEnabled = true;
            this.StorylineStateComboBox.Location = new System.Drawing.Point(131, 47);
            this.StorylineStateComboBox.Name = "StorylineStateComboBox";
            this.StorylineStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.StorylineStateComboBox.TabIndex = 162;
            this.StorylineStateComboBox.SelectedIndexChanged += new System.EventHandler(this.StorylineStateComboBoxSelectedIndexChanged);
            this.StorylineStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // CombatMissionCtrlStateComboBox
            // 
            this.CombatMissionCtrlStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CombatMissionCtrlStateComboBox.FormattingEnabled = true;
            this.CombatMissionCtrlStateComboBox.Location = new System.Drawing.Point(131, 17);
            this.CombatMissionCtrlStateComboBox.Name = "CombatMissionCtrlStateComboBox";
            this.CombatMissionCtrlStateComboBox.Size = new System.Drawing.Size(130, 21);
            this.CombatMissionCtrlStateComboBox.TabIndex = 160;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(0, 20);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(124, 13);
            this.label13.TabIndex = 159;
            this.label13.Text = "CombatMissionCtrl State:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(46, 49);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(78, 13);
            this.label12.TabIndex = 157;
            this.label12.Text = "Storyline State:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 171);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(116, 13);
            this.label7.TabIndex = 156;
            this.label7.Text = "AgentInteraction State:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(52, 111);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 13);
            this.label6.TabIndex = 155;
            this.label6.Text = "Unload State:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(69, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 154;
            this.label3.Text = "Arm State:";
            // 
            // Schedule
            // 
            this.Schedule.Location = new System.Drawing.Point(4, 22);
            this.Schedule.Name = "Schedule";
            this.Schedule.Padding = new System.Windows.Forms.Padding(3);
            this.Schedule.Size = new System.Drawing.Size(687, 276);
            this.Schedule.TabIndex = 2;
            this.Schedule.Text = "Schedule";
            this.Schedule.UseVisualStyleBackColor = true;
            // 
            // Targets
            // 
            this.Targets.Controls.Add(this.lblDistance3);
            this.Targets.Controls.Add(this.lblDistanceDroneTarget);
            this.Targets.Controls.Add(this.lblDistanceWeaponsTarget);
            this.Targets.Controls.Add(this.label23);
            this.Targets.Controls.Add(this.label22);
            this.Targets.Controls.Add(this.label21);
            this.Targets.Controls.Add(this.CurrentPriorityTargetsData);
            this.Targets.Controls.Add(this.CurrentPocketNumberData);
            this.Targets.Controls.Add(this.MissionPocketNumberlbl);
            this.Targets.Controls.Add(this.CurrentMissionActionData);
            this.Targets.Controls.Add(this.CurrentWeaponsTargetData);
            this.Targets.Controls.Add(this.CurrentDroneTargetData);
            this.Targets.Controls.Add(this.label20);
            this.Targets.Controls.Add(this.label14);
            this.Targets.Controls.Add(this.label4);
            this.Targets.Controls.Add(this.label1);
            this.Targets.Location = new System.Drawing.Point(4, 22);
            this.Targets.Name = "Targets";
            this.Targets.Padding = new System.Windows.Forms.Padding(3);
            this.Targets.Size = new System.Drawing.Size(687, 276);
            this.Targets.TabIndex = 3;
            this.Targets.Text = "Targets";
            this.Targets.UseVisualStyleBackColor = true;
            // 
            // lblDistance3
            // 
            this.lblDistance3.AutoSize = true;
            this.lblDistance3.Location = new System.Drawing.Point(422, 100);
            this.lblDistance3.Name = "lblDistance3";
            this.lblDistance3.Size = new System.Drawing.Size(52, 13);
            this.lblDistance3.TabIndex = 17;
            this.lblDistance3.Text = "Distance:";
            // 
            // lblDistanceDroneTarget
            // 
            this.lblDistanceDroneTarget.AutoSize = true;
            this.lblDistanceDroneTarget.Location = new System.Drawing.Point(222, 100);
            this.lblDistanceDroneTarget.Name = "lblDistanceDroneTarget";
            this.lblDistanceDroneTarget.Size = new System.Drawing.Size(52, 13);
            this.lblDistanceDroneTarget.TabIndex = 16;
            this.lblDistanceDroneTarget.Text = "Distance:";
            // 
            // lblDistanceWeaponsTarget
            // 
            this.lblDistanceWeaponsTarget.AutoSize = true;
            this.lblDistanceWeaponsTarget.Location = new System.Drawing.Point(6, 100);
            this.lblDistanceWeaponsTarget.Name = "lblDistanceWeaponsTarget";
            this.lblDistanceWeaponsTarget.Size = new System.Drawing.Size(52, 13);
            this.lblDistanceWeaponsTarget.TabIndex = 15;
            this.lblDistanceWeaponsTarget.Text = "Distance:";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(436, 77);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(38, 13);
            this.label23.TabIndex = 14;
            this.label23.Text = "Name:";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(236, 77);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(38, 13);
            this.label22.TabIndex = 13;
            this.label22.Text = "Name:";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(19, 77);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(38, 13);
            this.label21.TabIndex = 12;
            this.label21.Text = "Name:";
            // 
            // CurrentPriorityTargetsData
            // 
            this.CurrentPriorityTargetsData.AutoSize = true;
            this.CurrentPriorityTargetsData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentPriorityTargetsData.Location = new System.Drawing.Point(480, 77);
            this.CurrentPriorityTargetsData.Name = "CurrentPriorityTargetsData";
            this.CurrentPriorityTargetsData.Size = new System.Drawing.Size(27, 13);
            this.CurrentPriorityTargetsData.TabIndex = 11;
            this.CurrentPriorityTargetsData.Text = "n/a";
            // 
            // CurrentPocketNumberData
            // 
            this.CurrentPocketNumberData.AutoSize = true;
            this.CurrentPocketNumberData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentPocketNumberData.Location = new System.Drawing.Point(643, 12);
            this.CurrentPocketNumberData.Name = "CurrentPocketNumberData";
            this.CurrentPocketNumberData.Size = new System.Drawing.Size(27, 13);
            this.CurrentPocketNumberData.TabIndex = 10;
            this.CurrentPocketNumberData.Text = "n/a";
            // 
            // MissionPocketNumberlbl
            // 
            this.MissionPocketNumberlbl.AutoSize = true;
            this.MissionPocketNumberlbl.Location = new System.Drawing.Point(516, 12);
            this.MissionPocketNumberlbl.Name = "MissionPocketNumberlbl";
            this.MissionPocketNumberlbl.Size = new System.Drawing.Size(121, 13);
            this.MissionPocketNumberlbl.TabIndex = 9;
            this.MissionPocketNumberlbl.Text = "Current Pocket Number:";
            // 
            // CurrentMissionActionData
            // 
            this.CurrentMissionActionData.AutoSize = true;
            this.CurrentMissionActionData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentMissionActionData.Location = new System.Drawing.Point(140, 12);
            this.CurrentMissionActionData.Name = "CurrentMissionActionData";
            this.CurrentMissionActionData.Size = new System.Drawing.Size(27, 13);
            this.CurrentMissionActionData.TabIndex = 8;
            this.CurrentMissionActionData.Text = "n/a";
            // 
            // CurrentWeaponsTargetData
            // 
            this.CurrentWeaponsTargetData.AutoSize = true;
            this.CurrentWeaponsTargetData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentWeaponsTargetData.Location = new System.Drawing.Point(63, 77);
            this.CurrentWeaponsTargetData.Name = "CurrentWeaponsTargetData";
            this.CurrentWeaponsTargetData.Size = new System.Drawing.Size(27, 13);
            this.CurrentWeaponsTargetData.TabIndex = 7;
            this.CurrentWeaponsTargetData.Text = "n/a";
            // 
            // CurrentDroneTargetData
            // 
            this.CurrentDroneTargetData.AutoSize = true;
            this.CurrentDroneTargetData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentDroneTargetData.Location = new System.Drawing.Point(277, 77);
            this.CurrentDroneTargetData.Name = "CurrentDroneTargetData";
            this.CurrentDroneTargetData.Size = new System.Drawing.Size(27, 13);
            this.CurrentDroneTargetData.TabIndex = 6;
            this.CurrentDroneTargetData.Text = "n/a";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(235, 51);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(110, 13);
            this.label20.TabIndex = 3;
            this.label20.Text = "Current Drone Target:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(436, 51);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(77, 13);
            this.label14.TabIndex = 2;
            this.label14.Text = "PriorityTargets:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(127, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Current Weapons Target:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Current Mission Action:";
            // 
            // EWar
            // 
            this.EWar.Controls.Add(this.dataEntitiesTargetPaintingMe);
            this.EWar.Controls.Add(this.lblTargetPaintingMe);
            this.EWar.Controls.Add(this.label24);
            this.EWar.Controls.Add(this.lblTitleEWar);
            this.EWar.Controls.Add(this.dataEntitiesDampening);
            this.EWar.Controls.Add(this.lblDampeningMe);
            this.EWar.Controls.Add(this.dataEntitiesTrackingDisruptingMe);
            this.EWar.Controls.Add(this.lblTrackingDisruptingMe);
            this.EWar.Controls.Add(this.dataEntitiesNeutralizingMe);
            this.EWar.Controls.Add(this.lblWebbingMe);
            this.EWar.Controls.Add(this.dataEntitiesWebbingMe);
            this.EWar.Controls.Add(this.lblNeutralizingMe);
            this.EWar.Controls.Add(this.dataEntitiesJammingMe);
            this.EWar.Controls.Add(this.lblJammingMe);
            this.EWar.Controls.Add(this.dataEntitiesWarpDisruptingMe);
            this.EWar.Controls.Add(this.lblWarpDisrupingMe);
            this.EWar.Location = new System.Drawing.Point(4, 22);
            this.EWar.Name = "EWar";
            this.EWar.Size = new System.Drawing.Size(687, 276);
            this.EWar.TabIndex = 4;
            this.EWar.Text = "E-War";
            this.EWar.UseVisualStyleBackColor = true;
            // 
            // dataEntitiesTargetPaintingMe
            // 
            this.dataEntitiesTargetPaintingMe.AutoSize = true;
            this.dataEntitiesTargetPaintingMe.Location = new System.Drawing.Point(189, 182);
            this.dataEntitiesTargetPaintingMe.Name = "dataEntitiesTargetPaintingMe";
            this.dataEntitiesTargetPaintingMe.Size = new System.Drawing.Size(24, 13);
            this.dataEntitiesTargetPaintingMe.TabIndex = 15;
            this.dataEntitiesTargetPaintingMe.Text = "n/a";
            // 
            // lblTargetPaintingMe
            // 
            this.lblTargetPaintingMe.AutoSize = true;
            this.lblTargetPaintingMe.Location = new System.Drawing.Point(27, 182);
            this.lblTargetPaintingMe.Name = "lblTargetPaintingMe";
            this.lblTargetPaintingMe.Size = new System.Drawing.Size(94, 13);
            this.lblTargetPaintingMe.TabIndex = 14;
            this.lblTargetPaintingMe.Text = "TargetPaintingMe:";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(35, 254);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(220, 13);
            this.label24.TabIndex = 13;
            this.label24.Text = "Entities that have active EWar Effects on Me";
            // 
            // lblTitleEWar
            // 
            this.lblTitleEWar.AutoSize = true;
            this.lblTitleEWar.Location = new System.Drawing.Point(35, 10);
            this.lblTitleEWar.Name = "lblTitleEWar";
            this.lblTitleEWar.Size = new System.Drawing.Size(220, 13);
            this.lblTitleEWar.TabIndex = 12;
            this.lblTitleEWar.Text = "Entities that have active EWar Effects on Me";
            // 
            // dataEntitiesDampening
            // 
            this.dataEntitiesDampening.AutoSize = true;
            this.dataEntitiesDampening.Location = new System.Drawing.Point(189, 159);
            this.dataEntitiesDampening.Name = "dataEntitiesDampening";
            this.dataEntitiesDampening.Size = new System.Drawing.Size(24, 13);
            this.dataEntitiesDampening.TabIndex = 11;
            this.dataEntitiesDampening.Text = "n/a";
            // 
            // lblDampeningMe
            // 
            this.lblDampeningMe.AutoSize = true;
            this.lblDampeningMe.Location = new System.Drawing.Point(42, 159);
            this.lblDampeningMe.Name = "lblDampeningMe";
            this.lblDampeningMe.Size = new System.Drawing.Size(79, 13);
            this.lblDampeningMe.TabIndex = 10;
            this.lblDampeningMe.Text = "DampeningMe:";
            // 
            // dataEntitiesTrackingDisruptingMe
            // 
            this.dataEntitiesTrackingDisruptingMe.AutoSize = true;
            this.dataEntitiesTrackingDisruptingMe.Location = new System.Drawing.Point(189, 130);
            this.dataEntitiesTrackingDisruptingMe.Name = "dataEntitiesTrackingDisruptingMe";
            this.dataEntitiesTrackingDisruptingMe.Size = new System.Drawing.Size(24, 13);
            this.dataEntitiesTrackingDisruptingMe.TabIndex = 9;
            this.dataEntitiesTrackingDisruptingMe.Text = "n/a";
            // 
            // lblTrackingDisruptingMe
            // 
            this.lblTrackingDisruptingMe.AutoSize = true;
            this.lblTrackingDisruptingMe.Location = new System.Drawing.Point(8, 130);
            this.lblTrackingDisruptingMe.Name = "lblTrackingDisruptingMe";
            this.lblTrackingDisruptingMe.Size = new System.Drawing.Size(114, 13);
            this.lblTrackingDisruptingMe.TabIndex = 8;
            this.lblTrackingDisruptingMe.Text = "TrackingDisruptingMe:";
            // 
            // dataEntitiesNeutralizingMe
            // 
            this.dataEntitiesNeutralizingMe.AutoSize = true;
            this.dataEntitiesNeutralizingMe.Location = new System.Drawing.Point(189, 107);
            this.dataEntitiesNeutralizingMe.Name = "dataEntitiesNeutralizingMe";
            this.dataEntitiesNeutralizingMe.Size = new System.Drawing.Size(24, 13);
            this.dataEntitiesNeutralizingMe.TabIndex = 7;
            this.dataEntitiesNeutralizingMe.Text = "n/a";
            // 
            // lblWebbingMe
            // 
            this.lblWebbingMe.AutoSize = true;
            this.lblWebbingMe.Location = new System.Drawing.Point(54, 89);
            this.lblWebbingMe.Name = "lblWebbingMe";
            this.lblWebbingMe.Size = new System.Drawing.Size(68, 13);
            this.lblWebbingMe.TabIndex = 6;
            this.lblWebbingMe.Text = "WebbingMe:";
            // 
            // dataEntitiesWebbingMe
            // 
            this.dataEntitiesWebbingMe.AutoSize = true;
            this.dataEntitiesWebbingMe.Location = new System.Drawing.Point(189, 89);
            this.dataEntitiesWebbingMe.Name = "dataEntitiesWebbingMe";
            this.dataEntitiesWebbingMe.Size = new System.Drawing.Size(24, 13);
            this.dataEntitiesWebbingMe.TabIndex = 5;
            this.dataEntitiesWebbingMe.Text = "n/a";
            // 
            // lblNeutralizingMe
            // 
            this.lblNeutralizingMe.AutoSize = true;
            this.lblNeutralizingMe.Location = new System.Drawing.Point(42, 107);
            this.lblNeutralizingMe.Name = "lblNeutralizingMe";
            this.lblNeutralizingMe.Size = new System.Drawing.Size(80, 13);
            this.lblNeutralizingMe.TabIndex = 4;
            this.lblNeutralizingMe.Text = "NeutralizingMe:";
            // 
            // dataEntitiesJammingMe
            // 
            this.dataEntitiesJammingMe.AutoSize = true;
            this.dataEntitiesJammingMe.Location = new System.Drawing.Point(189, 66);
            this.dataEntitiesJammingMe.Name = "dataEntitiesJammingMe";
            this.dataEntitiesJammingMe.Size = new System.Drawing.Size(24, 13);
            this.dataEntitiesJammingMe.TabIndex = 3;
            this.dataEntitiesJammingMe.Text = "n/a";
            // 
            // lblJammingMe
            // 
            this.lblJammingMe.AutoSize = true;
            this.lblJammingMe.Location = new System.Drawing.Point(56, 66);
            this.lblJammingMe.Name = "lblJammingMe";
            this.lblJammingMe.Size = new System.Drawing.Size(66, 13);
            this.lblJammingMe.TabIndex = 2;
            this.lblJammingMe.Text = "JammingMe:";
            // 
            // dataEntitiesWarpDisruptingMe
            // 
            this.dataEntitiesWarpDisruptingMe.AutoSize = true;
            this.dataEntitiesWarpDisruptingMe.Location = new System.Drawing.Point(189, 44);
            this.dataEntitiesWarpDisruptingMe.Name = "dataEntitiesWarpDisruptingMe";
            this.dataEntitiesWarpDisruptingMe.Size = new System.Drawing.Size(24, 13);
            this.dataEntitiesWarpDisruptingMe.TabIndex = 1;
            this.dataEntitiesWarpDisruptingMe.Text = "n/a";
            // 
            // lblWarpDisrupingMe
            // 
            this.lblWarpDisrupingMe.AutoSize = true;
            this.lblWarpDisrupingMe.Location = new System.Drawing.Point(23, 44);
            this.lblWarpDisrupingMe.Name = "lblWarpDisrupingMe";
            this.lblWarpDisrupingMe.Size = new System.Drawing.Size(98, 13);
            this.lblWarpDisrupingMe.TabIndex = 0;
            this.lblWarpDisrupingMe.Text = "WarpDisruptingMe:";
            // 
            // BehaviorComboBox
            // 
            this.BehaviorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BehaviorComboBox.FormattingEnabled = true;
            this.BehaviorComboBox.Location = new System.Drawing.Point(54, 28);
            this.BehaviorComboBox.Name = "BehaviorComboBox";
            this.BehaviorComboBox.Size = new System.Drawing.Size(162, 21);
            this.BehaviorComboBox.TabIndex = 121;
            this.BehaviorComboBox.SelectedIndexChanged += new System.EventHandler(this.CombatMissionsBehaviorComboBoxSelectedIndexChanged);
            this.BehaviorComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(-3, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 120;
            this.label2.Text = "Behavior";
            // 
            // QuestorStateComboBox
            // 
            this.QuestorStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.QuestorStateComboBox.FormattingEnabled = true;
            this.QuestorStateComboBox.Location = new System.Drawing.Point(54, 1);
            this.QuestorStateComboBox.Name = "QuestorStateComboBox";
            this.QuestorStateComboBox.Size = new System.Drawing.Size(162, 21);
            this.QuestorStateComboBox.TabIndex = 119;
            this.QuestorStateComboBox.SelectedIndexChanged += new System.EventHandler(this.QuestorStateComboBoxSelectedIndexChanged);
            this.QuestorStateComboBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.DisableMouseWheel);
            // 
            // QuestorStatelbl
            // 
            this.QuestorStatelbl.Location = new System.Drawing.Point(-5, 1);
            this.QuestorStatelbl.Name = "QuestorStatelbl";
            this.QuestorStatelbl.Size = new System.Drawing.Size(50, 18);
            this.QuestorStatelbl.TabIndex = 119;
            this.QuestorStatelbl.Text = "Questor";
            this.QuestorStatelbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.QuestorStatelbl.Click += new System.EventHandler(this.QuestorStatelbl_Click);
            // 
            // buttonOpenSchedulesXML
            // 
            this.buttonOpenSchedulesXML.Location = new System.Drawing.Point(527, 86);
            this.buttonOpenSchedulesXML.Name = "buttonOpenSchedulesXML";
            this.buttonOpenSchedulesXML.Size = new System.Drawing.Size(156, 23);
            this.buttonOpenSchedulesXML.TabIndex = 123;
            this.buttonOpenSchedulesXML.Text = "Open Current Schedules XML";
            this.buttonOpenSchedulesXML.UseVisualStyleBackColor = true;
            this.buttonOpenSchedulesXML.Click += new System.EventHandler(this.ButtonOpenSchedulesXMLClick);
            // 
            // buttonOpenCharacterXML
            // 
            this.buttonOpenCharacterXML.Location = new System.Drawing.Point(527, 57);
            this.buttonOpenCharacterXML.Name = "buttonOpenCharacterXML";
            this.buttonOpenCharacterXML.Size = new System.Drawing.Size(156, 23);
            this.buttonOpenCharacterXML.TabIndex = 122;
            this.buttonOpenCharacterXML.Text = "Open Current Character XML";
            this.buttonOpenCharacterXML.UseVisualStyleBackColor = true;
            this.buttonOpenCharacterXML.Click += new System.EventHandler(this.ButtonOpenCharacterXMLClick);
            // 
            // buttonQuestormanager
            // 
            this.buttonQuestormanager.Location = new System.Drawing.Point(367, 86);
            this.buttonQuestormanager.Name = "buttonQuestormanager";
            this.buttonQuestormanager.Size = new System.Drawing.Size(154, 23);
            this.buttonQuestormanager.TabIndex = 124;
            this.buttonQuestormanager.Text = "QuestorManager";
            this.buttonQuestormanager.UseVisualStyleBackColor = true;
            this.buttonQuestormanager.Click += new System.EventHandler(this.ButtonQuestormanagerClick);
            // 
            // label25
            // 
            this.label25.Location = new System.Drawing.Point(8, 12);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(42, 16);
            this.label25.TabIndex = 125;
            this.label25.Text = "State:";
            this.label25.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label26
            // 
            this.label26.Location = new System.Drawing.Point(-3, 41);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(53, 14);
            this.label26.TabIndex = 126;
            this.label26.Text = "State:";
            this.label26.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // QuestorfrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.ClientSize = new System.Drawing.Size(356, 96);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.buttonQuestormanager);
            this.Controls.Add(this.buttonOpenSchedulesXML);
            this.Controls.Add(this.buttonOpenCharacterXML);
            this.Controls.Add(this.buttonOpenMissionXML);
            this.Controls.Add(this.Tabs);
            this.Controls.Add(this.buttonQuestorSettings);
            this.Controls.Add(this.buttonOpenLogDirectory);
            this.Controls.Add(this.buttonQuestorStatistics);
            this.Controls.Add(this.BehaviorComboBox);
            this.Controls.Add(this.lblCurrentPocketAction);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.QuestorStateComboBox);
            this.Controls.Add(this.QuestorStatelbl);
            this.Controls.Add(this.lblPocketAction);
            this.Controls.Add(this.lblCurrentMissionInfo);
            this.Controls.Add(this.lblMissionName);
            this.Controls.Add(this.chkShowDetails);
            this.Controls.Add(this.Disable3DCheckBox);
            this.Controls.Add(this.PauseCheckBox);
            this.Controls.Add(this.lblDamageType);
            this.Controls.Add(this.DamageTypeComboBox);
            this.Controls.Add(this.AutoStartCheckBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "QuestorfrmMain";
            this.Text = "Questor";
            this.Load += new System.EventHandler(this.FrmMainLoad);
            this.Console.ResumeLayout(false);
            this.Console.PerformLayout();
            this.Tabs.ResumeLayout(false);
            this.States.ResumeLayout(false);
            this.States.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.Targets.ResumeLayout(false);
            this.Targets.PerformLayout();
            this.EWar.ResumeLayout(false);
            this.EWar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox AutoStartCheckBox;
        private System.Windows.Forms.Timer tUpdateUI;
        private System.Windows.Forms.ComboBox DamageTypeComboBox;
        private System.Windows.Forms.Label lblDamageType;
        private System.Windows.Forms.CheckBox PauseCheckBox;
        private System.Windows.Forms.CheckBox Disable3DCheckBox;
        //private System.Windows.Forms.Button chkTraveler;
        //private System.Windows.Forms.CheckBox Anomaly_chk;
        private System.Windows.Forms.CheckBox chkShowDetails;
        private System.Windows.Forms.Label lblMissionName;
        private System.Windows.Forms.Label lblCurrentMissionInfo;
        private System.Windows.Forms.Label lblPocketAction;
        private System.Windows.Forms.Label lblCurrentPocketAction;
        private System.Windows.Forms.Button buttonQuestorStatistics;
        private System.Windows.Forms.Button buttonQuestorSettings;
        private System.Windows.Forms.Button buttonOpenMissionXML;
        private System.Windows.Forms.Button buttonOpenLogDirectory;
        //private System.Windows.Forms.DateTimePicker dateTimePickerStopTime;
        //private System.Windows.Forms.Label lblStopTime;
        //private System.Windows.Forms.Label lblMaxRuntime2;
        //private System.Windows.Forms.TextBox textBoxMaxRunTime;
        //private System.Windows.Forms.DateTimePicker dateTimePickerStartTime;
        //private System.Windows.Forms.Label lblMaxRunTime1;
        //private System.Windows.Forms.Label lblStartTime1;
        private System.Windows.Forms.TabPage Console;
        private System.Windows.Forms.TextBox txtComand;
        private System.Windows.Forms.RichTextBox txtExtConsole;
        private System.Windows.Forms.TabControl Tabs;
        private System.Windows.Forms.TabPage States;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox SalvageStateComboBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox LocalWatchStateComboBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox CleanupStateComboBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox DronesStateComboBox;
        private System.Windows.Forms.ComboBox CombatStateComboBox;
        private System.Windows.Forms.ComboBox PanicStateComboBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox AgentInteractionStateComboBox;
        private System.Windows.Forms.ComboBox TravelerStateComboBox;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox UnloadStateComboBox;
        private System.Windows.Forms.ComboBox ArmStateComboBox;
        private System.Windows.Forms.ComboBox StorylineStateComboBox;
        private System.Windows.Forms.ComboBox CombatMissionCtrlStateComboBox;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox BehaviorComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox QuestorStateComboBox;
        private System.Windows.Forms.Label QuestorStatelbl;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TabPage Schedule;
        private System.Windows.Forms.Button buttonOpenSchedulesXML;
        private System.Windows.Forms.Button buttonOpenCharacterXML;
        private System.Windows.Forms.Button buttonQuestormanager;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabPage Targets;
        private System.Windows.Forms.Label CurrentPriorityTargetsData;
        private System.Windows.Forms.Label CurrentPocketNumberData;
        private System.Windows.Forms.Label MissionPocketNumberlbl;
        private System.Windows.Forms.Label CurrentMissionActionData;
        private System.Windows.Forms.Label CurrentWeaponsTargetData;
        private System.Windows.Forms.Label CurrentDroneTargetData;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblDistance3;
        private System.Windows.Forms.Label lblDistanceDroneTarget;
        private System.Windows.Forms.Label lblDistanceWeaponsTarget;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TabPage EWar;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label lblTitleEWar;
        private System.Windows.Forms.Label dataEntitiesDampening;
        private System.Windows.Forms.Label lblDampeningMe;
        private System.Windows.Forms.Label dataEntitiesTrackingDisruptingMe;
        private System.Windows.Forms.Label lblTrackingDisruptingMe;
        private System.Windows.Forms.Label dataEntitiesNeutralizingMe;
        private System.Windows.Forms.Label lblWebbingMe;
        private System.Windows.Forms.Label dataEntitiesWebbingMe;
        private System.Windows.Forms.Label lblNeutralizingMe;
        private System.Windows.Forms.Label dataEntitiesJammingMe;
        private System.Windows.Forms.Label lblJammingMe;
        private System.Windows.Forms.Label dataEntitiesWarpDisruptingMe;
        private System.Windows.Forms.Label lblWarpDisrupingMe;
        private System.Windows.Forms.Label lblTargetPaintingMe;
        private System.Windows.Forms.Label dataEntitiesTargetPaintingMe;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label26;
    }
}

