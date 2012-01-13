//------------------------------------------------------------------------------
//  <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//    Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that 
//    applies to this source code. (a copy can also be found at: 
//    http://www.thehackerwithin.com/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using Mono.Options;

namespace Questor
{
    using global::Questor.Modules;
    using LavishScriptAPI;
    using DirectEve;

    static class Program
    {
        private static bool _done;
        private static DirectEve _directEve;

        private static string _username;
        private static string _password;
        private static string _character;
        private static string _scriptFile;
        private static bool   _loginOnly;
        private static bool   _showHelp;
        private static int _maxRuntime;

        private static DateTime _lastPulse;
        private static DateTime _startTime;

        public static DateTime startTime
        {
           get 
           {
              return _startTime; 
           }
        }

        public static int maxRuntime
        {
            get
            {
                return _maxRuntime;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var p = new OptionSet() {
                "Usage: questor [OPTIONS]",
                "Run missions and make uber ISK.",
                "",
                "Options:",
                { "u|user=", "the {USER} we are logging in as.",
                v => _username = v },
                { "p|password=", "the user's {PASSWORD}.",
                v => _password = v },
                { "c|character=", "the {CHARACTER} to use.",
                v => _character = v },
                { "s|script=", "a {SCRIPT} file to execute before login.",
                v => _scriptFile = v },
                { "l|login", "login only and exit.",
                v => _loginOnly = v != null },
                { "r|runtime=", "Quit Questor after {RUNTIME} minutes.",
                v => _maxRuntime = Int32.Parse(v) },
                { "h|help", "show this message and exit",
                v => _showHelp = v != null },
                };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
                //Logging.Log(string.Format("questor: extra = {0}", string.Join(" ", extra.ToArray())));
            }
            catch (OptionException e)
            {
                Logging.Log("questor: ");
                Logging.Log(e.Message);
                Logging.Log("Try `questor --help' for more information.");
                return;
            }

            if (_showHelp)
            {
                System.IO.StringWriter sw = new System.IO.StringWriter();
                p.WriteOptionDescriptions(sw);
                Logging.Log(sw.ToString());
                return;
            }

            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password) && !string.IsNullOrEmpty(_character))
            {
                _directEve = new DirectEve();
                _directEve.OnFrame += OnFrame;

                var started = DateTime.Now;

                // Sleep until we're done
                while (!_done)
                {
                    System.Threading.Thread.Sleep(50);

                    if (DateTime.Now.Subtract(started).TotalSeconds > 180)
                    {
                        Logging.Log("auto login timed out after 3 minutes");
                        break;
                    }
                }

                _directEve.Dispose();

                // If the last parameter is false, then we only auto-login
                if (_loginOnly)
                    return;
            }

            _startTime = DateTime.Now;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        static void OnFrame(object sender, EventArgs e)
        {
            if (DateTime.Now.Subtract(_lastPulse).TotalSeconds < 10)
                return;
            _lastPulse = DateTime.Now;

            // If the session is ready, then we are done :)
            if (_directEve.Session.IsReady)
            {
                Logging.Log("We've successfully logged in");
                _done = true;
                return;
            }

            // We are not ready, lets wait
            if (_directEve.Login.IsConnecting || _directEve.Login.IsLoading)
                return;

            // Are we at the login or character selection screen?
            if (!_directEve.Login.AtLogin && !_directEve.Login.AtCharacterSelection)
                return;

            // We shouldn't get any window
            if (_directEve.Windows.Count != 0)
            {
                foreach(var window in _directEve.Windows)
                {
                    if (string.IsNullOrEmpty(window.Html))
                        continue;

                    if (window.Html.Contains("Please make sure your characters are out of harms way"))
                        continue;

                    if (window.Name == "telecom")
                        continue;

                    Logging.Log("We've got an unexpected window, auto login halted.");
                    _done = true;
                    return;
                }
            }

            if (!string.IsNullOrEmpty(_scriptFile))
            {
                try
                {
                    // Replace this try block with the following once new DirectEve is pushed
                    // _directEve.RunScript(_scriptFile);

                    System.Reflection.MethodInfo info = _directEve.GetType().GetMethod("RunScript");

                    if (info == null)
                    {
                        Logging.Log("DirectEve.RunScript() doesn't exist.  Upgrade DirectEve.dll!");
                    }
                    else
                    {
                        Logging.Log(string.Format("Running {0}...", _scriptFile));
                        info.Invoke(_directEve, new Object[] { _scriptFile });
                    }
                }
                catch (System.Exception ex)
                {
                    Logging.Log(string.Format("Exception {0}...", ex.ToString()));
                    _done = true;
                }
                finally
                {
                    _scriptFile = null;
                }
                return;
            }

            if (_directEve.Login.AtLogin)
            {
                Logging.Log("Login account [" + _username + "]");
                _directEve.Login.Login(_username, _password);
                return;
            }

            if (_directEve.Login.AtCharacterSelection && _directEve.Login.IsCharacterSelectionReady)
            {
                foreach (var slot in _directEve.Login.CharacterSlots)
                {
                    if (slot.CharId.ToString() != _character && string.Compare(slot.CharName, _character, true) != 0)
                        continue;

                    Logging.Log("Activating character [" + slot.CharName + "]");
                    slot.Activate();
                    return;
                }

                Logging.Log("Character id/name [" + _character + "] not found, retrying in 3 seconds");
            }
        }
    }
}
