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
            this.ActionButton = new System.Windows.Forms.Button();
            this.SearchLabel = new System.Windows.Forms.Label();
            this.RefreshBookmarksButton = new System.Windows.Forms.Button();
            this.SearchTextBox = new System.Windows.Forms.TextBox();
            this.SearchResults = new System.Windows.Forms.ListView();
            this.NameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TypeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.UpdateSearchResults = new System.Windows.Forms.Timer(this.components);
            this.DestinationLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ActionButton
            // 
            this.ActionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ActionButton.Location = new System.Drawing.Point(567, 235);
            this.ActionButton.Name = "ActionButton";
            this.ActionButton.Size = new System.Drawing.Size(75, 23);
            this.ActionButton.TabIndex = 1;
            this.ActionButton.Text = "Travel";
            this.ActionButton.UseVisualStyleBackColor = true;
            this.ActionButton.Click += new System.EventHandler(this.ActionButtonClick);
            // 
            // SearchLabel
            // 
            this.SearchLabel.AutoSize = true;
            this.SearchLabel.Location = new System.Drawing.Point(12, 9);
            this.SearchLabel.Name = "SearchLabel";
            this.SearchLabel.Size = new System.Drawing.Size(81, 13);
            this.SearchLabel.TabIndex = 2;
            this.SearchLabel.Text = "Enter a location";
            // 
            // RefreshBookmarksButton
            // 
            this.RefreshBookmarksButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RefreshBookmarksButton.Location = new System.Drawing.Point(12, 235);
            this.RefreshBookmarksButton.Name = "RefreshBookmarksButton";
            this.RefreshBookmarksButton.Size = new System.Drawing.Size(128, 23);
            this.RefreshBookmarksButton.TabIndex = 3;
            this.RefreshBookmarksButton.Text = "Refresh Bookmarks";
            this.RefreshBookmarksButton.UseVisualStyleBackColor = true;
            this.RefreshBookmarksButton.Click += new System.EventHandler(this.RefreshBookmarksClick);
            // 
            // SearchTextBox
            // 
            this.SearchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchTextBox.Location = new System.Drawing.Point(99, 6);
            this.SearchTextBox.Name = "SearchTextBox";
            this.SearchTextBox.Size = new System.Drawing.Size(543, 20);
            this.SearchTextBox.TabIndex = 4;
            this.SearchTextBox.TextChanged += new System.EventHandler(this.SearchTextBoxChanged);
            // 
            // SearchResults
            // 
            this.SearchResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameHeader,
            this.TypeHeader});
            this.SearchResults.FullRowSelect = true;
            this.SearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.SearchResults.HideSelection = false;
            this.SearchResults.Location = new System.Drawing.Point(15, 32);
            this.SearchResults.MultiSelect = false;
            this.SearchResults.Name = "SearchResults";
            this.SearchResults.Size = new System.Drawing.Size(627, 197);
            this.SearchResults.TabIndex = 5;
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
            // UpdateSearchResults
            // 
            this.UpdateSearchResults.Enabled = true;
            this.UpdateSearchResults.Interval = 250;
            this.UpdateSearchResults.Tick += new System.EventHandler(this.UpdateSearchResultsTick);
            // 
            // DestinationLabel
            // 
            this.DestinationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DestinationLabel.AutoEllipsis = true;
            this.DestinationLabel.Location = new System.Drawing.Point(146, 240);
            this.DestinationLabel.Name = "DestinationLabel";
            this.DestinationLabel.Size = new System.Drawing.Size(415, 13);
            this.DestinationLabel.TabIndex = 6;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 270);
            this.Controls.Add(this.DestinationLabel);
            this.Controls.Add(this.SearchResults);
            this.Controls.Add(this.SearchTextBox);
            this.Controls.Add(this.RefreshBookmarksButton);
            this.Controls.Add(this.SearchLabel);
            this.Controls.Add(this.ActionButton);
            this.Name = "MainForm";
            this.Text = "Traveler";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ActionButton;
        private System.Windows.Forms.Label SearchLabel;
        private System.Windows.Forms.Button RefreshBookmarksButton;
        private System.Windows.Forms.TextBox SearchTextBox;
        private System.Windows.Forms.ListView SearchResults;
        private System.Windows.Forms.ColumnHeader NameHeader;
        private System.Windows.Forms.ColumnHeader TypeHeader;
        private System.Windows.Forms.Timer UpdateSearchResults;
        private System.Windows.Forms.Label DestinationLabel;


    }
}

