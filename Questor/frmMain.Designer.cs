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
            this.txtExtConsole = new System.Windows.Forms.TextBox();
            this.txtComand = new System.Windows.Forms.TextBox();
            this.chkShowConsole = new System.Windows.Forms.CheckBox();
            this.lblMissionName = new System.Windows.Forms.Label();
            this.lblCurrentMissionInfo = new System.Windows.Forms.Label();
            this.lblPocketAction = new System.Windows.Forms.Label();
            this.lblCurrentPocketAction = new System.Windows.Forms.Label();
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
            // txtExtConsole
            // 
            this.txtExtConsole.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExtConsole.Location = new System.Drawing.Point(12, 105);
            this.txtExtConsole.Multiline = true;
            this.txtExtConsole.Name = "txtExtConsole";
            this.txtExtConsole.ReadOnly = true;
            this.txtExtConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExtConsole.Size = new System.Drawing.Size(875, 235);
            this.txtExtConsole.TabIndex = 23;
            // 
            // txtComand
            // 
            this.txtComand.Location = new System.Drawing.Point(12, 345);
            this.txtComand.Name = "txtComand";
            this.txtComand.Size = new System.Drawing.Size(875, 20);
            this.txtComand.TabIndex = 24;
            this.txtComand.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtComand_KeyPress);
            // 
            // chkShowConsole
            // 
            this.chkShowConsole.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkShowConsole.Location = new System.Drawing.Point(270, 69);
            this.chkShowConsole.Name = "chkShowConsole";
            this.chkShowConsole.Size = new System.Drawing.Size(83, 26);
            this.chkShowConsole.TabIndex = 7;
            this.chkShowConsole.Text = "Show Console";
            this.chkShowConsole.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkShowConsole.UseVisualStyleBackColor = true;
            this.chkShowConsole.CheckedChanged += new System.EventHandler(this.chkShowConsole_CheckedChanged);
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
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.ClientSize = new System.Drawing.Size(356, 96);
            this.Controls.Add(this.lblCurrentPocketAction);
            this.Controls.Add(this.lblPocketAction);
            this.Controls.Add(this.lblCurrentMissionInfo);
            this.Controls.Add(this.lblMissionName);
            this.Controls.Add(this.chkShowConsole);
            this.Controls.Add(this.txtComand);
            this.Controls.Add(this.txtExtConsole);
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
        private System.Windows.Forms.TextBox txtExtConsole;
        private System.Windows.Forms.TextBox txtComand;
        private System.Windows.Forms.CheckBox chkShowConsole;
        private System.Windows.Forms.Label lblMissionName;
        private System.Windows.Forms.Label lblCurrentMissionInfo;
        private System.Windows.Forms.Label lblPocketAction;
        private System.Windows.Forms.Label lblCurrentPocketAction;
    }
}

