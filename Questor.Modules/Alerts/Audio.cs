using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace Questor.Modules.Alerts
{
   

    public class Audio : System.Windows.Forms.Form
    {
         
        private System.Windows.Forms.Label _label1;
        private System.Windows.Forms.TextBox _filepathTextbox;        
        private System.Windows.Forms.Button _playOnceSyncButton;
        private System.Windows.Forms.Button _playOnceAsyncButton;
        private System.Windows.Forms.Button _playLoopAsyncButton;
        private System.Windows.Forms.Button _selectFileButton;

        private System.Windows.Forms.Button _stopButton;
        private System.Windows.Forms.StatusBar _statusBar;
        private System.Windows.Forms.Button _loadSyncButton;
        private System.Windows.Forms.Button _loadAsyncButton;        
        private SoundPlayer _player;
        //private Audio _localalarm;
        //private string _localalarmmp3 = "test.mp3";
        //private Audio _convoalarm;
        //private string _convoalarmmp3 = "test.mp3";
        //private Audio _miscalarm;
        //private string _miscalarmmp3 = "test.mp3";
        //private  Audio _music;
        public Audio()
        {
            // Initialize Forms Designer generated code.
            InitializeComponent();
			
            // Disable playback controls until a valid .wav file 
            // is selected.
            EnablePlaybackControls(false);

            // Set up the status bar and other controls.
            InitializeControls();

            // Set up the SoundPlayer object.
            InitializeSound();
        }

        // Sets up the status bar and other controls.
        private void InitializeControls()
        {
            // Set up the status bar.
            StatusBarPanel panel = new StatusBarPanel();
            panel.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            panel.Text = "Ready.";
            panel.AutoSize = StatusBarPanelAutoSize.Spring;
            this._statusBar.ShowPanels = true;
            this._statusBar.Panels.Add(panel);
        }

        // Sets up the SoundPlayer object.
        private void InitializeSound()
        {
            // Create an instance of the SoundPlayer class.
            _player = new SoundPlayer();

            // Listen for the LoadCompleted event.
            _player.LoadCompleted += new AsyncCompletedEventHandler(player_LoadCompleted);

            // Listen for the SoundLocationChanged event.
            _player.SoundLocationChanged += new EventHandler(player_LocationChanged);
        }

        private void selectFileButton_Click(object sender, 
            System.EventArgs e)
        {
            // Create a new OpenFileDialog.
            OpenFileDialog dlg = new OpenFileDialog();

            // Make sure the dialog checks for existence of the 
            // selected file.
            dlg.CheckFileExists = true;

            // Allow selection of .wav files only.
            dlg.Filter = "WAV files (*.wav)|*.wav";
            dlg.DefaultExt = ".wav";

            // Activate the file selection dialog.
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file's path from the dialog.
                this._filepathTextbox.Text = dlg.FileName;

                // Assign the selected file's path to 
                // the SoundPlayer object.  
                _player.SoundLocation = _filepathTextbox.Text;
            }
        }

        // Convenience method for setting message text in 
        // the status bar.
        private void ReportStatus(string statusMessage)
        {
            // If the caller passed in a message...
            if ((statusMessage != null) && (statusMessage != String.Empty))
            {
                // ...post the caller's message to the status bar.
                this._statusBar.Panels[0].Text = statusMessage;
            }
        }

        // Enables and disables play controls.
        private void EnablePlaybackControls(bool enabled)
        {   
            this._playOnceSyncButton.Enabled = enabled;
            this._playOnceAsyncButton.Enabled = enabled;
            this._playLoopAsyncButton.Enabled = enabled;
            this._stopButton.Enabled = enabled;
        }

        private void filepathTextbox_TextChanged(object sender, 
            EventArgs e)
        {
            // Disable playback controls until the new .wav is loaded.
            EnablePlaybackControls(false);
        }

        private void loadSyncButton_Click(object sender, 
            System.EventArgs e)
        {   
            // Disable playback controls until the .wav is 
            // successfully loaded. The LoadCompleted event 
            // handler will enable them.
            EnablePlaybackControls(false);

            try
            {
                // Assign the selected file's path to 
                // the SoundPlayer object.  
                _player.SoundLocation = _filepathTextbox.Text;

                // Load the .wav file.
                _player.Load();
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message);
            }
        }

        private void loadAsyncButton_Click(System.Object sender, 
            System.EventArgs e)
        {
            // Disable playback controls until the .wav is 
            // successfully loaded. The LoadCompleted event 
            // handler will enable them.
            EnablePlaybackControls(false);

            try
            {
                // Assign the selected file's path to 
                // the SoundPlayer object.  
                _player.SoundLocation = this._filepathTextbox.Text;

                // Load the .wav file.
                _player.LoadAsync();
            }
            catch (Exception ex)
            {
                ReportStatus(ex.Message);
            }
        }

        // Synchronously plays the selected .wav file once.
        // If the file is large, UI response will be visibly 
        // affected.
        private void playOnceSyncButton_Click(object sender, 
            System.EventArgs e)
        {	
            ReportStatus("Playing .wav file synchronously.");
            _player.PlaySync();
            ReportStatus("Finished playing .wav file synchronously.");
        }

        // Asynchronously plays the selected .wav file once.
        private void playOnceAsyncButton_Click(object sender, 
            System.EventArgs e)
        {
            ReportStatus("Playing .wav file asynchronously.");
            _player.Play();
        }

        // Asynchronously plays the selected .wav file until the user
        // clicks the Stop button.
        private void playLoopAsyncButton_Click(object sender, 
            System.EventArgs e)
        {
            ReportStatus("Looping .wav file asynchronously.");
            _player.PlayLooping();
        }

        // Stops the currently playing .wav file, if any.
        private void stopButton_Click(System.Object sender,
            System.EventArgs e)
        {	
            _player.Stop();
            ReportStatus("Stopped by user.");
        }

        // Handler for the LoadCompleted event.
        private void player_LoadCompleted(object sender, 
            AsyncCompletedEventArgs e)
        {   
            string message = String.Format("LoadCompleted: {0}", 
                this._filepathTextbox.Text);
            ReportStatus(message);
            EnablePlaybackControls(true);
        }

        // Handler for the SoundLocationChanged event.
        private void player_LocationChanged(object sender, EventArgs e)
        {   
            string message = String.Format("SoundLocationChanged: {0}", 
                _player.SoundLocation);
            ReportStatus(message);
        }

        private void playSoundFromResource(object sender, EventArgs e)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.Stream s = a.GetManifestResourceStream("<AssemblyName>.chimes.wav");
            SoundPlayer player = new SoundPlayer(s);
            player.Play();
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this._filepathTextbox = new System.Windows.Forms.TextBox();
            this._selectFileButton = new System.Windows.Forms.Button();
            this._label1 = new System.Windows.Forms.Label();
            this._loadSyncButton = new System.Windows.Forms.Button();
            this._playOnceSyncButton = new System.Windows.Forms.Button();
            this._playOnceAsyncButton = new System.Windows.Forms.Button();
            this._stopButton = new System.Windows.Forms.Button();
            this._playLoopAsyncButton = new System.Windows.Forms.Button();
            this._statusBar = new System.Windows.Forms.StatusBar();
            this._loadAsyncButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _filepathTextbox
            // 
            this._filepathTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._filepathTextbox.Location = new System.Drawing.Point(7, 25);
            this._filepathTextbox.Name = "_filepathTextbox";
            this._filepathTextbox.Size = new System.Drawing.Size(263, 20);
            this._filepathTextbox.TabIndex = 1;
            this._filepathTextbox.TextChanged += new System.EventHandler(this.filepathTextbox_TextChanged);
            // 
            // _selectFileButton
            // 
            this._selectFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._selectFileButton.Location = new System.Drawing.Point(276, 25);
            this._selectFileButton.Name = "_selectFileButton";
            this._selectFileButton.Size = new System.Drawing.Size(23, 21);
            this._selectFileButton.TabIndex = 2;
            this._selectFileButton.Text = "...";
            this._selectFileButton.Click += new System.EventHandler(this.selectFileButton_Click);
            // 
            // _label1
            // 
            this._label1.Location = new System.Drawing.Point(7, 7);
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(145, 17);
            this._label1.TabIndex = 3;
            this._label1.Text = ".wav path or URL:";
            // 
            // _loadSyncButton
            // 
            this._loadSyncButton.Location = new System.Drawing.Point(7, 53);
            this._loadSyncButton.Name = "_loadSyncButton";
            this._loadSyncButton.Size = new System.Drawing.Size(142, 23);
            this._loadSyncButton.TabIndex = 4;
            this._loadSyncButton.Text = "Load Synchronously";
            this._loadSyncButton.Click += new System.EventHandler(this.loadSyncButton_Click);
            // 
            // _playOnceSyncButton
            // 
            this._playOnceSyncButton.Location = new System.Drawing.Point(7, 86);
            this._playOnceSyncButton.Name = "_playOnceSyncButton";
            this._playOnceSyncButton.Size = new System.Drawing.Size(142, 23);
            this._playOnceSyncButton.TabIndex = 5;
            this._playOnceSyncButton.Text = "Play Once Synchronously";
            this._playOnceSyncButton.Click += new System.EventHandler(this.playOnceSyncButton_Click);
            // 
            // _playOnceAsyncButton
            // 
            this._playOnceAsyncButton.Location = new System.Drawing.Point(149, 86);
            this._playOnceAsyncButton.Name = "_playOnceAsyncButton";
            this._playOnceAsyncButton.Size = new System.Drawing.Size(147, 23);
            this._playOnceAsyncButton.TabIndex = 6;
            this._playOnceAsyncButton.Text = "Play Once Asynchronously";
            this._playOnceAsyncButton.Click += new System.EventHandler(this.playOnceAsyncButton_Click);
            // 
            // _stopButton
            // 
            this._stopButton.Location = new System.Drawing.Point(149, 109);
            this._stopButton.Name = "_stopButton";
            this._stopButton.Size = new System.Drawing.Size(147, 23);
            this._stopButton.TabIndex = 7;
            this._stopButton.Text = "Stop";
            this._stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // _playLoopAsyncButton
            // 
            this._playLoopAsyncButton.Location = new System.Drawing.Point(7, 109);
            this._playLoopAsyncButton.Name = "_playLoopAsyncButton";
            this._playLoopAsyncButton.Size = new System.Drawing.Size(142, 23);
            this._playLoopAsyncButton.TabIndex = 8;
            this._playLoopAsyncButton.Text = "Loop Asynchronously";
            this._playLoopAsyncButton.Click += new System.EventHandler(this.playLoopAsyncButton_Click);
            // 
            // _statusBar
            // 
            this._statusBar.Location = new System.Drawing.Point(0, 146);
            this._statusBar.Name = "_statusBar";
            this._statusBar.Size = new System.Drawing.Size(306, 22);
            this._statusBar.SizingGrip = false;
            this._statusBar.TabIndex = 9;
            this._statusBar.Text = "(no status)";
            this._statusBar.PanelClick += new System.Windows.Forms.StatusBarPanelClickEventHandler(this._statusBar_PanelClick);
            // 
            // _loadAsyncButton
            // 
            this._loadAsyncButton.Location = new System.Drawing.Point(149, 53);
            this._loadAsyncButton.Name = "_loadAsyncButton";
            this._loadAsyncButton.Size = new System.Drawing.Size(147, 23);
            this._loadAsyncButton.TabIndex = 10;
            this._loadAsyncButton.Text = "Load Asynchronously";
            this._loadAsyncButton.Click += new System.EventHandler(this.loadAsyncButton_Click);
            // 
            // Audio
            // 
            this.ClientSize = new System.Drawing.Size(306, 168);
            this.Controls.Add(this._loadAsyncButton);
            this.Controls.Add(this._statusBar);
            this.Controls.Add(this._playLoopAsyncButton);
            this.Controls.Add(this._stopButton);
            this.Controls.Add(this._playOnceAsyncButton);
            this.Controls.Add(this._playOnceSyncButton);
            this.Controls.Add(this._loadSyncButton);
            this.Controls.Add(this._label1);
            this.Controls.Add(this._selectFileButton);
            this.Controls.Add(this._filepathTextbox);
            this.MinimumSize = new System.Drawing.Size(310, 165);
            this.Name = "Audio";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Sound API Test Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        [STAThread]
        static void Main()
        {
            Application.Run(new Audio());
        }

        private void _statusBar_PanelClick(object sender, StatusBarPanelClickEventArgs e)
        {

        }
    }
}