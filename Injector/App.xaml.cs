using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Injector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            int state = 0;
            string dll = string.Empty;
            string function = string.Empty;
            string args = string.Empty;
            bool silent = false;
            uint pid = 0;

            foreach (string a in e.Args)
            {
                string arg = a.ToLower();
                switch (state)
                {
                    case 0:
                        if (arg[0] != '-')
                            continue;
                        if (arg == "-inject")
                            state = 1;
                        else if (arg == "-function")
                            state = 2;
                        else if (arg == "-args")
                            state = 3;
                        else if (arg == "-pid")
                            state = 4;
                        else if (arg == "-silent")
                            silent = true;
                        break;
                        
                    // DLL name
                    case 1:
                        dll = a; state = 0; break;

                    // function name
                    case 2:
                        function = a; state = 0; break;

                    // arguments
                    case 3:
                        args = a; state = 0; break;

                    case 4:
                        try { pid = uint.Parse(a); }
                        catch { }
                        state = 0;
                        break;

                    default: break;
                }
            }

            if (pid != 0 && dll != string.Empty)
            {
                InjectorLib injector = new InjectorLib();

                // For text output, in case we were launched from a command prompt.
                AttachConsole(0xffffffff);

                Console.WriteLine("Injecting '" + dll + "' into process " + pid);

                UInt32 retCode = 0;
                bool b = injector.InjectAndRun(pid, dll, function, args, ref retCode);

                if (b)
                {
                    Console.WriteLine("Successful!\nReturn value: " + retCode);
                    if (!silent) MessageBox.Show("DLL Injection Successful!\nReturn value: " + retCode);
                }
                else
                {
                    Console.WriteLine("Failed!");
                    if (!silent) MessageBox.Show("DLL Injection failed!");
                }

                this.Shutdown(12);
            }
        }
    }
}
