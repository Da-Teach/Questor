﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Questor.Modules;

namespace Questor
{
    using LavishScriptAPI;

    public partial class frmMain : Form
    {
        private Questor _questor;

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
        }

        private int SetAutoStart(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("SetAutoStart true|false");
                return -1;
            }

            _questor.AutoStart = value;

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

            _questor.Disable3D = value;

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

            if (value && _questor.AutoStart)
            {
                _questor.AutoStart = false;
                Logging.Log("AutoStart is turned [off]");
            }

            return 0;

        }

        private void tUpdateUI_Tick(object sender, EventArgs e)
        {
            // The if's in here stop the UI from flickering
            var text = "Questor [" + _questor.CharacterName + "]";
            if (Text != text)
                Text = text;

            text = _questor.State.ToString();
            if ((string)QuestorStateComboBox.SelectedItem != text && !QuestorStateComboBox.DroppedDown)
                QuestorStateComboBox.SelectedItem = text;

            text = Cache.Instance.DamageType.ToString();
            if ((string)DamageTypeComboBox.SelectedItem != text && !DamageTypeComboBox.DroppedDown)
                DamageTypeComboBox.SelectedItem = text;

            if (AutoStartCheckBox.Checked != _questor.AutoStart)
            {
                AutoStartCheckBox.Checked = _questor.AutoStart;
                StartButton.Enabled = !_questor.AutoStart;
            }

            if (PauseCheckBox.Checked != _questor.Paused)
                PauseCheckBox.Checked = _questor.Paused;

            if (Disable3DCheckBox.Checked != _questor.Disable3D)
                Disable3DCheckBox.Checked = _questor.Disable3D;

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
        }

        private void DamageTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cache.Instance.DamageType = (DamageType) Enum.Parse(typeof (DamageType), DamageTypeComboBox.Text);
        }

        private void QuestorStateComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _questor.State = (QuestorState)Enum.Parse(typeof(QuestorState), QuestorStateComboBox.Text);
        }

        private void AutoStartCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _questor.AutoStart = AutoStartCheckBox.Checked;
            StartButton.Enabled = !_questor.AutoStart;
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
            _questor.Disable3D = Disable3DCheckBox.Checked;
        }
    }
}
