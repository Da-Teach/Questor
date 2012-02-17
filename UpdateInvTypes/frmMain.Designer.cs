namespace UpdateInvTypes
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
            this.UpdateButton = new System.Windows.Forms.Button();
            this.Progress = new System.Windows.Forms.ProgressBar();
            this.tUpdate = new System.Windows.Forms.Timer(this.components);
            this.chkfast = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // UpdateButton
            // 
            this.UpdateButton.Location = new System.Drawing.Point(12, 12);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(260, 23);
            this.UpdateButton.TabIndex = 0;
            this.UpdateButton.Text = "Update";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Click += new System.EventHandler(this.Update_Click);
            // 
            // Progress
            // 
            this.Progress.Location = new System.Drawing.Point(12, 41);
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(260, 23);
            this.Progress.TabIndex = 1;
            // 
            // tUpdate
            // 
            this.tUpdate.Enabled = true;
            this.tUpdate.Tick += new System.EventHandler(this.tUpdate_Tick);
            // 
            // chkfast
            // 
            this.chkfast.AutoSize = true;
            this.chkfast.Location = new System.Drawing.Point(23, 69);
            this.chkfast.Name = "chkfast";
            this.chkfast.Size = new System.Drawing.Size(97, 17);
            this.chkfast.TabIndex = 2;
            this.chkfast.Text = "Update all now";
            this.chkfast.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 89);
            this.Controls.Add(this.chkfast);
            this.Controls.Add(this.Progress);
            this.Controls.Add(this.UpdateButton);
            this.Name = "frmMain";
            this.Text = "Update InvTypes.xml";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button UpdateButton;
        private System.Windows.Forms.ProgressBar Progress;
        private System.Windows.Forms.Timer tUpdate;
        private System.Windows.Forms.CheckBox chkfast;
    }
}

