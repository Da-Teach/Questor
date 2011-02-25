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

        private static DateTime _lastPulse;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 3 || args.Length == 4)
            {
                _username = args[0];
                _password = args[1];
                _character = args[2];

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
                if (args.Length == 4 && string.Compare(args[3], "false", true) == 0)
                    return;
            }

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
