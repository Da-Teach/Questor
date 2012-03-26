using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Questor.Modules;

namespace Questor
{
    using LavishScriptAPI;

    public partial class frmMain : Form
    {
        private Questor _questor;
        private Panic _panic;
        private DateTime _lastlogmessage;
            
        public frmMain()
        {
            InitializeComponent();

            foreach (var text in Enum.GetNames(typeof(DamageType)))
                DamageTypeComboBox.Items.Add(text);

            foreach (var text in Enum.GetNames(typeof(QuestorState)))
                QuestorStateComboBox.Items.Add(text);

            _questor = new Questor(this);

            LavishScript.Commands.AddCommand("SetAutoStart", SetAutoStart);
            LavishScript.Commands.AddCommand("SetDisable3D", SetDisable3D);
            LavishScript.Commands.AddCommand("SetExitWhenIdle", SetExitWhenIdle);
            LavishScript.Commands.AddCommand("SetQuestorStatetoCloseQuestor", SetQuestorStatetoCloseQuestor);
            LavishScript.Commands.AddCommand("SetQuestorStatetoIdle", SetQuestorStatetoIdle);
        }

        private int SetAutoStart(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("SetAutoStart true|false");
                return -1;
            }

            Settings.Instance.AutoStart = value;

            Logging.Log("AutoStart is turned " + (value ? "[on]" : "[off]"));
            return 0;
        }

        private int SetDisable3D(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("SetDisable3D true|false");
                return -1;
            }

            Settings.Instance.Disable3D = value;

            Logging.Log("Disable3D is turned " + (value ? "[on]" : "[off]"));
            return 0;
        }

        private int SetExitWhenIdle(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("SetExitWhenIdle true|false");
                Logging.Log("Note: AutoStart is automatically turned off when ExitWhenIdle is turned on");
                return -1;
            }

            _questor.ExitWhenIdle = value;

            Logging.Log("ExitWhenIdle is turned " + (value ? "[on]" : "[off]"));

            if (value && Settings.Instance.AutoStart)
            {
                Settings.Instance.AutoStart = false;
                Logging.Log("AutoStart is turned [off]");
            }
            return 0;
        }

        private int SetQuestorStatetoCloseQuestor(string[] args)
        {
            if (args.Length != 1 )
            {
                Logging.Log("SetQuestorStatetoCloseQuestor - Changes the QuestorState to CloseQuestor which will GotoBase and then Exit");
                return -1;
            }

            _questor.State = QuestorState.CloseQuestor;

            Logging.Log("QuestorState is now: CloseQuestor ");
            return 0;
        }

        private int SetQuestorStatetoIdle(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("SetQuestorStatetoIdle - Changes the QuestorState to Idle which will GotoBase and then Exit");
                return -1;
            }

            _questor.State = QuestorState.Idle;

            Logging.Log("QuestorState is now: Idle ");
            return 0;
        }

        private void tUpdateUI_Tick(object sender, EventArgs e)
        {
            // The if's in here stop the UI from flickering
            var text = "Questor";
            if (_questor.CharacterName != string.Empty)
            {
                text = "Questor [" + _questor.CharacterName + "]";
            }
            if (_questor.CharacterName != string.Empty && Cache.Instance.Wealth > 10000000)
            {
                text = "Questor [" + _questor.CharacterName + "][" + String.Format("{0:0,0}", Cache.Instance.Wealth / 1000000) + "mil isk]";
            }

            if (Text != text)
                Text = text;

            text = _questor.State.ToString();
            if ((string)QuestorStateComboBox.SelectedItem != text && !QuestorStateComboBox.DroppedDown)
                QuestorStateComboBox.SelectedItem = text;

            text = Cache.Instance.DamageType.ToString();
            if ((string)DamageTypeComboBox.SelectedItem != text && !DamageTypeComboBox.DroppedDown)
                DamageTypeComboBox.SelectedItem = text;

            if (AutoStartCheckBox.Checked != Settings.Instance.AutoStart)
            {
                AutoStartCheckBox.Checked = Settings.Instance.AutoStart;
                StartButton.Enabled = !Settings.Instance.AutoStart;
            }

            if (PauseCheckBox.Checked != _questor.Paused)
                PauseCheckBox.Checked = _questor.Paused;

            if (Disable3DCheckBox.Checked != Settings.Instance.Disable3D)
                Disable3DCheckBox.Checked = Settings.Instance.Disable3D;

            if (Settings.Instance.WindowXPosition.HasValue)
            {
                Left = Settings.Instance.WindowXPosition.Value;
                Settings.Instance.WindowXPosition = null;
            }

            if (Settings.Instance.WindowYPosition.HasValue)
            {
                Top = Settings.Instance.WindowYPosition.Value;
                Settings.Instance.WindowYPosition = null;
            }
            if (_questor.State == QuestorState.ExecuteMission)
            {
                var newlblCurrentPocketActiontext = "[ " + Cache.Instance.CurrentPocketAction + " ] Action";
                if (lblCurrentPocketAction.Text != newlblCurrentPocketActiontext)
                    lblCurrentPocketAction.Text = newlblCurrentPocketActiontext;
            }
            else if (_questor.State == QuestorState.Salvage)
            {
                var newlblCurrentPocketActiontext = "[ " + "Salvaging" + " ] ";
                if (lblCurrentPocketAction.Text != newlblCurrentPocketActiontext)
                    lblCurrentPocketAction.Text = newlblCurrentPocketActiontext;
            }
            else
            {
                var newlblCurrentPocketActiontext = "[ " + "" + " ] ";
                if (lblCurrentPocketAction.Text != newlblCurrentPocketActiontext)
                    lblCurrentPocketAction.Text = newlblCurrentPocketActiontext;
            }
            if (Cache.Instance.MissionName != string.Empty)
            {
                var newlblCurrentMissionInfotext = "[ " + Cache.Instance.MissionName + " ][ " + Math.Round(DateTime.Now.Subtract(Statistics.Instance.StartedMission).TotalMinutes,0) + " min][ #" + Statistics.Instance.MissionsThisSession + " ]";
                if (lblCurrentMissionInfo.Text != newlblCurrentMissionInfotext)
                    lblCurrentMissionInfo.Text = newlblCurrentMissionInfotext;
                buttonOpenMissionXML.Enabled = true;
            }
            else
            {
                lblCurrentMissionInfo.Text = "No Mission Selected Yet";
                buttonOpenMissionXML.Enabled = false;
            }
            if (Cache.Instance.ExtConsole != null)
            {  
                if (txtExtConsole.Lines.Count() >= Settings.Instance.maxLineConsole)
                    txtExtConsole.Text = "";

                txtExtConsole.AppendText(Cache.Instance.ExtConsole);
                Cache.Instance.ExtConsole = null;
            }
            if (DateTime.Now.Subtract(_questor._lastFrame).TotalSeconds > 90 && DateTime.Now.Subtract(Program.AppStarted).TotalSeconds > 300)
            {
                if (DateTime.Now.Subtract(_lastlogmessage).TotalSeconds > 60)
                {
                    Logging.Log("The Last UI Frame Drawn by EVE was more than 90 seconds ago! This is bad.");
                    //
                    // closing eve would be a very good idea here
                    //
                    _lastlogmessage = DateTime.Now;
                }
            }
            if (Cache.Instance.MaxRuntime > 0 && Cache.Instance.MaxRuntime != Int32.MaxValue) //if runtime is specified, overrides stop time
            {
                if (DateTime.Now.Subtract(Program.startTime).TotalSeconds > 120)
                {
                    if (Cache.Instance.MaxRuntime.ToString() != textBoxMaxRunTime.Text)
                    {
                        textBoxMaxRunTime.Text = Cache.Instance.MaxRuntime.ToString();
                    }
                }
            }
            else
            {
                textBoxMaxRunTime.Text = string.Empty;
            }

            if (Cache.Instance.StartTime != null)
            {
                if (dateTimePickerStartTime.Value != Cache.Instance.StartTime)
                {
                    dateTimePickerStartTime.Value = Cache.Instance.StartTime;
                }
            }
            
            if (Cache.Instance.StopTimeSpecified)
            {
                if (dateTimePickerStopTime.Value == Cache.Instance.StartTime)
                {
                    dateTimePickerStopTime.Value = Cache.Instance.StopTime;
                }
            }

            if (dateTimePickerStopTime.Value > Cache.Instance.StartTime.AddMinutes(5))
            {
                Cache.Instance.StopTimeSpecified = true;
                Cache.Instance.StopTime = dateTimePickerStopTime.Value;
            }
            else
            {
                Cache.Instance.StopTimeSpecified = false;
                dateTimePickerStopTime.Value = Cache.Instance.StartTime;
            }
        }


        private void DamageTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cache.Instance.DamageType = (DamageType) Enum.Parse(typeof (DamageType), DamageTypeComboBox.Text);
        }

        private void QuestorStateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _questor.State = (QuestorState)Enum.Parse(typeof(QuestorState), QuestorStateComboBox.Text);
            // If you are at the controls enough to change states... assume that panic needs to do nothing
            _panic.State = PanicState.Resume;
        }

        private void AutoStartCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.AutoStart = AutoStartCheckBox.Checked;
            StartButton.Enabled = !Settings.Instance.AutoStart;
        }

        private void PauseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _questor.Paused = PauseCheckBox.Checked;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            _questor.State = QuestorState.Start;
        }

        private void Disable3DCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Instance.Disable3D = Disable3DCheckBox.Checked;
        }

        private void txtComand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                LavishScript.ExecuteCommand(txtComand.Text);
            }
        }

        private void chkShowConsole_CheckedChanged(object sender, EventArgs e)
        {
            Form frmMain = new Form();
            if (chkShowDetails.Checked)
            {
                this.Size = new System.Drawing.Size(901, 406);
            }
            else
            {
                this.Size = new System.Drawing.Size(362, 124);
            }
        }
        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        void DamageTypeComboBox_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }
        void QuestorStateComboBox_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void textBoxMaxRunTime_TextChanged(object sender, EventArgs e)
        {
            int number2;
            if (int.TryParse(textBoxMaxRunTime.Text, out number2))
            {
                Cache.Instance.MaxRuntime = number2;
            }
            else
            {
                textBoxMaxRunTime.Text = Cache.Instance.MaxRuntime.ToString();
            }
        }
        
        private void textBoxMaxRunTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void buttonQuestormanager_Click(object sender, EventArgs e)
        {
            LavishScript.ExecuteCommand("dotnet QuestorManager QuestorManager");
        }

        private void buttonQuestorStatistics_Click(object sender, EventArgs e)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var processes = System.Diagnostics.Process.GetProcessesByName("QuestorStatistics");

            if (processes.Length == 0)
            {
                // QuestorStatistics
                try
                {
                    System.Diagnostics.Process.Start(path + "\\QuestorStatistics.exe");
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Logging.Log("QuestorStatistics could not be launched the error was: " +  ex.Message); 
                }

            }
        }

        private void buttonOpenLogDirectory_Click(object sender, EventArgs e)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            System.Diagnostics.Process.Start(Settings.Instance.logpath);
        }

        private void buttonOpenMissionXML_Click(object sender, EventArgs e)
        {
            var missionXmlPath = Path.Combine(Settings.Instance.MissionsPath, Cache.Instance.MissionName + ".xml");
            System.Diagnostics.Process.Start(missionXmlPath);
        }
    }
}
