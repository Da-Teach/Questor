namespace ValueDump
{
    partial class ValueDumpUI
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
            this.btnHangar = new System.Windows.Forms.Button();
            this.lvItems = new System.Windows.Forms.ListView();
            this.chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chQuantity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chQuantitySold = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chMedianBuy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chStationBuy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chTotalBuy = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.tbTotalMedian = new System.Windows.Forms.TextBox();
            this.tbTotalSold = new System.Windows.Forms.TextBox();
            this.cbxSell = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxUndersell = new System.Windows.Forms.CheckBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.RefineCheckBox = new System.Windows.Forms.CheckBox();
            this.RefineEfficiencyInput = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.UpdateMineralPricesButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.RefineEfficiencyInput)).BeginInit();
            this.SuspendLayout();
            // 
            // btnHangar
            // 
            this.btnHangar.Location = new System.Drawing.Point(12, 12);
            this.btnHangar.Name = "btnHangar";
            this.btnHangar.Size = new System.Drawing.Size(75, 23);
            this.btnHangar.TabIndex = 1;
            this.btnHangar.Text = "Start";
            this.btnHangar.UseVisualStyleBackColor = true;
            this.btnHangar.Click += new System.EventHandler(this.btnHangar_Click);
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
            this.lvItems.Location = new System.Drawing.Point(12, 64);
            this.lvItems.Name = "lvItems";
            this.lvItems.Size = new System.Drawing.Size(607, 271);
            this.lvItems.TabIndex = 2;
            this.lvItems.UseCompatibleStateImageBehavior = false;
            this.lvItems.View = System.Windows.Forms.View.Details;
            this.lvItems.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvItems_ColumnClick);
            this.lvItems.SelectedIndexChanged += new System.EventHandler(this.lvItems_SelectedIndexChanged);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(174, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Amount (Median Buy):";
            // 
            // tbTotalMedian
            // 
            this.tbTotalMedian.Location = new System.Drawing.Point(291, 14);
            this.tbTotalMedian.Name = "tbTotalMedian";
            this.tbTotalMedian.Size = new System.Drawing.Size(96, 20);
            this.tbTotalMedian.TabIndex = 4;
            // 
            // tbTotalSold
            // 
            this.tbTotalSold.Location = new System.Drawing.Point(507, 12);
            this.tbTotalSold.Name = "tbTotalSold";
            this.tbTotalSold.Size = new System.Drawing.Size(112, 20);
            this.tbTotalSold.TabIndex = 6;
            // 
            // cbxSell
            // 
            this.cbxSell.AutoSize = true;
            this.cbxSell.Location = new System.Drawing.Point(12, 41);
            this.cbxSell.Name = "cbxSell";
            this.cbxSell.Size = new System.Drawing.Size(70, 17);
            this.cbxSell.TabIndex = 7;
            this.cbxSell.Text = "Sell items";
            this.cbxSell.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(393, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Amount (Station Sell):";
            // 
            // cbxUndersell
            // 
            this.cbxUndersell.AutoSize = true;
            this.cbxUndersell.Location = new System.Drawing.Point(88, 41);
            this.cbxUndersell.Name = "cbxUndersell";
            this.cbxUndersell.Size = new System.Drawing.Size(70, 17);
            this.cbxUndersell.TabIndex = 8;
            this.cbxUndersell.Text = "Undersell";
            this.cbxUndersell.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(93, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 9;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // RefineCheckBox
            // 
            this.RefineCheckBox.AutoSize = true;
            this.RefineCheckBox.Location = new System.Drawing.Point(164, 41);
            this.RefineCheckBox.Name = "RefineCheckBox";
            this.RefineCheckBox.Size = new System.Drawing.Size(117, 17);
            this.RefineCheckBox.TabIndex = 10;
            this.RefineCheckBox.Text = "Check refine prices";
            this.RefineCheckBox.UseVisualStyleBackColor = true;
            // 
            // RefineEfficiencyInput
            // 
            this.RefineEfficiencyInput.Location = new System.Drawing.Point(570, 38);
            this.RefineEfficiencyInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.RefineEfficiencyInput.Name = "RefineEfficiencyInput";
            this.RefineEfficiencyInput.Size = new System.Drawing.Size(49, 20);
            this.RefineEfficiencyInput.TabIndex = 11;
            this.RefineEfficiencyInput.Value = new decimal(new int[] {
            95,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(475, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Refine efficiency:";
            // 
            // UpdateMineralPricesButton
            // 
            this.UpdateMineralPricesButton.Location = new System.Drawing.Point(291, 37);
            this.UpdateMineralPricesButton.Name = "UpdateMineralPricesButton";
            this.UpdateMineralPricesButton.Size = new System.Drawing.Size(178, 23);
            this.UpdateMineralPricesButton.TabIndex = 13;
            this.UpdateMineralPricesButton.Text = "Update mineral prices";
            this.UpdateMineralPricesButton.UseVisualStyleBackColor = true;
            this.UpdateMineralPricesButton.Click += new System.EventHandler(this.UpdateMineralPricesButton_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(631, 347);
            this.Controls.Add(this.UpdateMineralPricesButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.RefineEfficiencyInput);
            this.Controls.Add(this.RefineCheckBox);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.cbxUndersell);
            this.Controls.Add(this.cbxSell);
            this.Controls.Add(this.tbTotalSold);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbTotalMedian);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lvItems);
            this.Controls.Add(this.btnHangar);
            this.Name = "frmMain";
            this.Text = "Value Dump";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.RefineEfficiencyInput)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnHangar;
        private System.Windows.Forms.ListView lvItems;
        private System.Windows.Forms.ColumnHeader chName;
        private System.Windows.Forms.ColumnHeader chQuantity;
        private System.Windows.Forms.ColumnHeader chQuantitySold;
        private System.Windows.Forms.ColumnHeader chMedianBuy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbTotalMedian;
        private System.Windows.Forms.ColumnHeader chStationBuy;
        private System.Windows.Forms.TextBox tbTotalSold;
        private System.Windows.Forms.CheckBox cbxSell;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox cbxUndersell;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ColumnHeader chTotalBuy;
        private System.Windows.Forms.CheckBox RefineCheckBox;
        private System.Windows.Forms.NumericUpDown RefineEfficiencyInput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button UpdateMineralPricesButton;
    }
}

