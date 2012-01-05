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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.QuestorStateComboBox = new System.Windows.Forms.ComboBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.PauseCheckBox = new System.Windows.Forms.CheckBox();
            this.Disable3DCheckBox = new System.Windows.Forms.CheckBox();
            this.txtExtConsole = new System.Windows.Forms.TextBox();
            this.txtComand = new System.Windows.Forms.TextBox();
            this.chkShowConsole = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // AutoStartCheckBox
            // 
            this.AutoStartCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.AutoStartCheckBox.AutoSize = true;
            this.AutoStartCheckBox.Location = new System.Drawing.Point(12, 60);
            this.AutoStartCheckBox.Name = "AutoStartCheckBox";
            this.AutoStartCheckBox.Size = new System.Drawing.Size(62, 23);
            this.AutoStartCheckBox.TabIndex = 11;
            this.AutoStartCheckBox.Text = "Auto start";
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
            this.DamageTypeComboBox.Location = new System.Drawing.Point(91, 6);
            this.DamageTypeComboBox.Name = "DamageTypeComboBox";
            this.DamageTypeComboBox.Size = new System.Drawing.Size(252, 21);
            this.DamageTypeComboBox.TabIndex = 16;
            this.DamageTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.DamageTypeComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Damage Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Questor State";
            // 
            // QuestorStateComboBox
            // 
            this.QuestorStateComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.QuestorStateComboBox.FormattingEnabled = true;
            this.QuestorStateComboBox.Location = new System.Drawing.Point(90, 33);
            this.QuestorStateComboBox.Name = "QuestorStateComboBox";
            this.QuestorStateComboBox.Size = new System.Drawing.Size(253, 21);
            this.QuestorStateComboBox.TabIndex = 19;
            this.QuestorStateComboBox.SelectedIndexChanged += new System.EventHandler(this.QuestorStateComboBox_SelectedIndexChanged);
            // 
            // StartButton
            // 
            this.StartButton.AutoSize = true;
            this.StartButton.Location = new System.Drawing.Point(80, 60);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(47, 23);
            this.StartButton.TabIndex = 20;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // PauseCheckBox
            // 
            this.PauseCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.PauseCheckBox.AutoSize = true;
            this.PauseCheckBox.Location = new System.Drawing.Point(133, 60);
            this.PauseCheckBox.Name = "PauseCheckBox";
            this.PauseCheckBox.Size = new System.Drawing.Size(47, 23);
            this.PauseCheckBox.TabIndex = 21;
            this.PauseCheckBox.Text = "Pause";
            this.PauseCheckBox.UseVisualStyleBackColor = true;
            this.PauseCheckBox.CheckedChanged += new System.EventHandler(this.PauseCheckBox_CheckedChanged);
            // 
            // Disable3DCheckBox
            // 
            this.Disable3DCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.Disable3DCheckBox.AutoSize = true;
            this.Disable3DCheckBox.Location = new System.Drawing.Point(186, 60);
            this.Disable3DCheckBox.Name = "Disable3DCheckBox";
            this.Disable3DCheckBox.Size = new System.Drawing.Size(69, 23);
            this.Disable3DCheckBox.TabIndex = 22;
            this.Disable3DCheckBox.Text = "Disable 3D";
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
            this.txtExtConsole.Size = new System.Drawing.Size(871, 231);
            this.txtExtConsole.TabIndex = 23;
            // 
            // txtComand
            // 
            this.txtComand.Location = new System.Drawing.Point(14, 342);
            this.txtComand.Name = "txtComand";
            this.txtComand.Size = new System.Drawing.Size(869, 20);
            this.txtComand.TabIndex = 24;
            this.txtComand.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtComand_KeyPress);
            // 
            // chkShowConsole
            // 
            this.chkShowConsole.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkShowConsole.AutoSize = true;
            this.chkShowConsole.Location = new System.Drawing.Point(261, 60);
            this.chkShowConsole.Name = "chkShowConsole";
            this.chkShowConsole.Size = new System.Drawing.Size(85, 23);
            this.chkShowConsole.TabIndex = 25;
            this.chkShowConsole.Text = "Show Console";
            this.chkShowConsole.UseVisualStyleBackColor = true;
            this.chkShowConsole.CheckedChanged += new System.EventHandler(this.chkShowConsole_CheckedChanged);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 96);
            this.Controls.Add(this.chkShowConsole);
            this.Controls.Add(this.txtComand);
            this.Controls.Add(this.txtExtConsole);
            this.Controls.Add(this.Disable3DCheckBox);
            this.Controls.Add(this.PauseCheckBox);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.QuestorStateComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DamageTypeComboBox);
            this.Controls.Add(this.AutoStartCheckBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "Questor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox AutoStartCheckBox;
        private System.Windows.Forms.Timer tUpdateUI;
        private System.Windows.Forms.ComboBox DamageTypeComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox QuestorStateComboBox;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.CheckBox PauseCheckBox;
        private System.Windows.Forms.CheckBox Disable3DCheckBox;
        private System.Windows.Forms.TextBox txtExtConsole;
        private System.Windows.Forms.TextBox txtComand;
        private System.Windows.Forms.CheckBox chkShowConsole;
    }
}

