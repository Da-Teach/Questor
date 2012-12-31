using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;

namespace Injector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            comboBoxProcesses.Items.Clear();
            String s = "Select a process";
            int i = comboBoxProcesses.Items.Add(s);
            comboBoxProcesses.SelectedIndex = i;

            // fill in default values.  mainly to make debugging quicker
            textBoxDLL.Text = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DomainManager.dll");
            textBoxFunction.Text = "Onyx.DomainManager.EntryPoint.Entry";
            textBoxArguments.Text = "DirectEveTester.exe";

        }

        private void comboBoxProcesses_DropDownOpened(object sender, EventArgs e)
        {
            comboBoxProcesses.Items.Clear();
            foreach (Process p in Process.GetProcesses())
            {
                ComboBoxItem cbi = new ComboBoxItem();
                cbi.Content = p.ProcessName + " : " + p.Id;
                cbi.Tag = p.Id;
                comboBoxProcesses.Items.Add(cbi);
            }
            if (comboBoxProcesses.HasItems)
            {
                SortDescription sd = new SortDescription("Content", ListSortDirection.Ascending);
                comboBoxProcesses.Items.SortDescriptions.Add(sd);
            }
        }

        private void UpdateStatus(string message, bool isError = true)
        {
            txtStatus.Text = message;;
            if (isError)
                txtStatus.Background = new LinearGradientBrush(Color.FromRgb(255, 0, 0), Color.FromRgb(200, 0, 0), 90.0);
            else
                txtStatus.Background = new LinearGradientBrush(Color.FromRgb(0, 255, 0), Color.FromRgb(0, 200, 0), 90.0);
        }

        private void btnInject_Click(object sender, RoutedEventArgs e)
        {
            string dll = textBoxDLL.Text.Trim();
            string func = textBoxFunction.Text.Trim();
            string args = textBoxArguments.Text.Trim();

            if (dll == string.Empty || !File.Exists(dll))
            {
                UpdateStatus("You must select a dll to inject");
                return;
            }

            InjectorLib lib = new InjectorLib();
            bool isManaged = lib.IsDllManaged(dll) == 1;
            if (isManaged && func == string.Empty)
            {
                UpdateStatus("You must specify a class method when injecting managed DLLs", false);
                return;
            }

            if (isManaged && !func.Contains('.'))
            {
                UpdateStatus("Class method should be in the form of 'namespace.classname.methodname'");
                return;
            }

            uint pid = 0;
            try
            {
                ComboBoxItem cbi = (ComboBoxItem)comboBoxProcesses.SelectedItem;
                pid = uint.Parse(cbi.Tag.ToString());
            }
            catch
            {
                UpdateStatus("You must select a process to inject to");
                return;
            }

            UInt32 retCode = 0;
            bool b = lib.InjectAndRun(pid, dll, func, args, ref retCode);

            if (!b)
            {
                // See InjectorLib sources to understand what the error codes mean..
                UpdateStatus("Injection failed. Error code " + retCode);
            }
            else
            {
                UpdateStatus("Injection successful. Return value: " + retCode, false);
            }
        }

        private void btnBrowseDLL_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "DLL Files (*.dll)|*.dll|All Files (*.*)|*.*";
            ofd.FilterIndex = 0;
            ofd.Multiselect = false;
            ofd.Title = "Select DLL to inject..";
            try
            {
                if (ofd.ShowDialog() == true)
                {
                    textBoxDLL.Text = ofd.FileName;
                }
            }
            catch { }
        }

        private void btnBrowseProcess_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.Filter = "EXE Files (*.exe)|*.exe|All Files (*.*)|*.*";
            ofd.FilterIndex = 0;
            ofd.Multiselect = false;
            ofd.Title = "Select EXE file to launch..";
            try
            {
                if (ofd.ShowDialog() == true)
                {
                    textBoxProcess.Text = ofd.FileName;
                }
            }
            catch { }
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            string exe = textBoxProcess.Text.Trim();
            string dll = textBoxDLL.Text.Trim();
            string func = textBoxFunction.Text.Trim();
            string args = textBoxArguments.Text.Trim();

            if (dll == string.Empty || !File.Exists(dll))
            {
                UpdateStatus("You must select a dll to inject");
                return;
            }

            InjectorLib lib = new InjectorLib();
            bool isManaged = lib.IsDllManaged(dll) == 1;
            if (isManaged && func == string.Empty)
            {
                UpdateStatus("You must specify a class method when injecting managed DLLs", false);
                return;
            }

            if (isManaged && !func.Contains('.'))
            {
                UpdateStatus("Class method should be in the form of 'namespace.classname.methodname'");
                return;
            }

            if (exe == string.Empty)
            {
                UpdateStatus("You must select an exe file to launch");
                return;
            }

            UInt32 retCode = 0;
            bool b = lib.LaunchAndInject(exe, dll, func, args, ref retCode);

            if (!b)
            {
                // See InjectorLib sources to understand what the error codes mean..
                UpdateStatus("Injection failed. Error code " + retCode);
            }
            else
            {
                UpdateStatus("Injection successful. Return value: " + retCode, false);
            }
        }
    }
}
