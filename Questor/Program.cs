//------------------------------------------------------------------------------
//  <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//    Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that
//    applies to this source code. (a copy can also be found at:
//    http://www.thehackerwithin.com/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------

using System.Globalization;

namespace Questor
{
    using System;
    using System.Windows.Forms;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;
    using System.IO;
    using System.Timers;
    using Mono.Options;
    using global::Questor.Modules.Logging;
    using global::Questor.Modules.Lookup;
    using DirectEve;

    internal static class Program
    {
        private static bool _done;
        private static DirectEve _directEve;

        public static List<CharSchedule> CharSchedules { get; private set; }

        private static int _pulsedelay = (int)Time.QuestorBeforeLoginPulseDelay_seconds;

        public static DateTime AppStarted = DateTime.Now;
        private static string _username;
        private static string _password;
        private static string _character;
        private static string _scriptFile;
        private static bool _loginOnly;
        private static bool _showHelp;
        private static int _maxRuntime;
        private static bool _chantlingScheduler;

        public static DateTime StopTime;
        public static DateTime ScheduledstopTime;
        private static double _minutesToStart;
        private static bool _readyToStarta;
        private static bool _readyToStart;
        private static bool _humaninterventionrequired;

        static readonly System.Timers.Timer Timer = new System.Timers.Timer();
        private const int RandStartDelay = 30; //Random startup delay in minutes
        private static readonly Random R = new Random();
        public static bool StopTimeSpecified; //false;

        private static DateTime _lastPulse;
        private static DateTime _startTime;

        public static DateTime startTime
        {
            get
            {
                return _startTime;
            }
        }

        public static int MaxRuntime
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
        private static void Main(string[] args)
        {
            _maxRuntime = Int32.MaxValue;
            var p = new OptionSet {
                "Usage: questor [OPTIONS]",
                "Run missions and make uber ISK.",
                "",
                "Options:",
                {"u|user=", "the {USER} we are logging in as.", v => _username = v},
                {"p|password=", "the user's {PASSWORD}.", v => _password = v},
                {"c|character=", "the {CHARACTER} to use.", v => _character = v},
                {"s|script=", "a {SCRIPT} file to execute before login.", v => _scriptFile = v},
                {"l|login", "login only and exit.", v => _loginOnly = v != null},
                {"r|runtime=", "Quit Questor after {RUNTIME} minutes.", v => _maxRuntime = Int32.Parse(v)},
                {"x|chantling", "use chantling's scheduler", v => _chantlingScheduler = v != null},
                {"h|help", "show this message and exit", v => _showHelp = v != null}
                };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
                //Logging.Log(string.Format("questor: extra = {0}", string.Join(" ", extra.ToArray())));
            }
            catch (OptionException e)
            {
                Logging.Log("Startup", "questor: ", Logging.white);
                Logging.Log("Startup", e.Message, Logging.white);
                Logging.Log("Startup", "Try `questor --help' for more information.", Logging.white);
                return;
            }
            _readyToStart = true;

            if (_showHelp)
            {
                System.IO.StringWriter sw = new System.IO.StringWriter();
                p.WriteOptionDescriptions(sw);
                Logging.Log("Startup", sw.ToString(), Logging.white);
                return;
            }

            if (_chantlingScheduler && string.IsNullOrEmpty(_character))
            {
                Logging.Log("Startup", "Error: to use chantling's scheduler, you also need to provide a character name!", Logging.red);
                return;
            }

            if (_chantlingScheduler && !string.IsNullOrEmpty(_character))
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                _character = _character.Replace("\"", "");  // strip quotation marks if any are present

                CharSchedules = new List<CharSchedule>();
                if (path != null)
                {
                    XDocument values = XDocument.Load(Path.Combine(path, "Schedules.xml"));
                    if (values.Root != null)
                        foreach (XElement value in values.Root.Elements("char"))
                            CharSchedules.Add(new CharSchedule(value));
                }
                //
                // chantling scheduler
                //
                CharSchedule schedule = CharSchedules.FirstOrDefault(v => v.Name == _character);
                if (schedule == null)
                {
                    Logging.Log("Startup", "Error - character not found!", Logging.red);
                    return;
                }
                else
                {
                    Logging.Log("Startup", "User: " + schedule.User + " PW: " + schedule.PW + " Name: " + schedule.Name + " Start: " + schedule.Start + " Stop: " +
                             schedule.Stop + " RunTime: " + schedule.RunTime, Logging.white);
                    if (schedule.User == null || schedule.PW == null)
                    {
                        Logging.Log("Startup", "Error - Login details not specified in Schedules.xml!", Logging.red);
                        return;
                    }
                    else
                    {
                        _username = schedule.User;
                        _password = schedule.PW;
                    }
                    _startTime = schedule.Start;

                    if (schedule.StartTimeSpecified)
                        _startTime = _startTime.AddSeconds(R.Next(0, (RandStartDelay * 60)));

                    //_scheduledstartTime = schedule.Start;
                    ScheduledstopTime = schedule.Stop;
                    StopTime = schedule.Stop;

                    //if ((DateTime.Now > _scheduledstopTime))
                    //{
                    //	_startTime = _startTime.AddDays(1); //otherwise, start tomorrow at start time
                    //	_readyToStarta = false;
                    //}
                    if ((DateTime.Now > _startTime))
                    {
                        if ((DateTime.Now.Subtract(_startTime).TotalMinutes < 1200)) //if we're less than x hours past start time, start now
                        {
                            _startTime = DateTime.Now;
                            _readyToStarta = true;
                        }
                        else
                            _startTime = _startTime.AddDays(1); //otherwise, start tomorrow at start time
                    }
                    else
                        if ((_startTime.Subtract(DateTime.Now).TotalMinutes > 1200)) //if we're more than x hours shy of start time, start now
                        {
                            _startTime = DateTime.Now;
                            _readyToStarta = true;
                        }

                    if (StopTime < _startTime)
                        StopTime = StopTime.AddDays(1);

                    if (schedule.RunTime > 0) //if runtime is specified, overrides stop time
                        StopTime = _startTime.AddMinutes(schedule.RunTime); //minutes of runtime

                    if (schedule.RunTime < 18 && schedule.RunTime > 0)     //if runtime is 10 or less, assume they meant hours
                        StopTime = _startTime.AddHours(schedule.RunTime);   //hours of runtime

                    string stopTimeText = "No stop time specified";
                    StopTimeSpecified = schedule.StopTimeSpecified;
                    if (StopTimeSpecified)
                        stopTimeText = StopTime.ToString(CultureInfo.InvariantCulture);

                    Logging.Log("Startup", " Start Time: " + _startTime + " - Stop Time: " + stopTimeText, Logging.white);

                    if (!_readyToStarta)
                    {
                        _minutesToStart = _startTime.Subtract(DateTime.Now).TotalMinutes;
                        Logging.Log("Startup", "Starting at " + _startTime + ". " + String.Format("{0:0.##}", _minutesToStart) + " minutes to go.", Logging.yellow);
                        Timer.Elapsed += new ElapsedEventHandler(TimerEventProcessor);
                        if (_minutesToStart > 0)
                            Timer.Interval = (int)(_minutesToStart * 60000);
                        else
                            Timer.Interval = 1000;
                        Timer.Enabled = true;
                        Timer.Start();
                    }
                    else
                    {
                        _readyToStart = true;
                        Logging.Log("Startup", "Already passed start time.  Starting in 15 seconds.", Logging.white);
                        System.Threading.Thread.Sleep(15000);
                    }
                }
                //
                // chantling scheduler (above)
                //
                _directEve = new DirectEve();
                _directEve.OnFrame += OnFrame;

                while (!_done)
                {
                    System.Threading.Thread.Sleep(50);
                }

                _directEve.Dispose();
            }

            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password) && !string.IsNullOrEmpty(_character))
            {
                _readyToStart = true;

                _directEve = new DirectEve();
                _directEve.OnFrame += OnFrame;

                // Sleep until we're done
                while (!_done)
                {
                    System.Threading.Thread.Sleep(50);
                }

                _directEve.Dispose();

                // If the last parameter is false, then we only auto-login
                if (_loginOnly)
                    return;
            }

            _startTime = DateTime.Now;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new QuestorfrmMain());
        }

        private static void OnFrame(object sender, EventArgs e)
        {
            if (!_readyToStart || _humaninterventionrequired)
            {
                //Logging.Log("if (!_readyToStart) then return");
                return;
            }

            if (_chantlingScheduler && !string.IsNullOrEmpty(_character) && !_readyToStarta)
            {
                //Logging.Log("if (_chantlingScheduler && !string.IsNullOrEmpty(_character) && !_readyToStarta) then return");
                return;
            }

            if (DateTime.Now.Subtract(_lastPulse).TotalSeconds < _pulsedelay)
            {
                //Logging.Log("if (DateTime.Now.Subtract(_lastPulse).TotalSeconds < _pulsedelay) then return");
                return;
            }

            _lastPulse = DateTime.Now;

            // If the session is ready, then we are done :)
            if (_directEve.Session.IsReady)
            {
                Logging.Log("Startup", "We've successfully logged in", Logging.white);
                _done = true;
                return;
            }

            // We shouldn't get any window
            if (_directEve.Windows.Count != 0)
            {
                foreach (var window in _directEve.Windows)
                {
                    if (string.IsNullOrEmpty(window.Html))
                        continue;
                    Logging.Log("Startup", "windowtitles:" + window.Name + "::" + window.Html, Logging.white);
                    //
                    // Close these windows and continue
                    //
                    if (window.Name == "telecom")
                    {
                        Logging.Log("Startup", "Closing telecom message...", Logging.yellow);
                        Logging.Log("Startup", "Content of telecom window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]", Logging.yellow);
                        window.Close();
                        continue;
                    }

                    // Modal windows must be closed
                    // But lets only close known modal windows
                    if (window.Name == "modal")
                    {
                        bool close = false;
                        bool restart = false;
                        bool needhumanintervention = false;

                        if (!string.IsNullOrEmpty(window.Html))
                        {
                            //errors that are repeatable and unavoidable even after a restart of eve/questor
                            needhumanintervention = window.Html.Contains("reason: Account subscription expired");

                            // Server going down
                            //Logging.Log("[Startup] (1) close is: " + close);
                            close |= window.Html.Contains("Please make sure your characters are out of harms way");
                            close |= window.Html.Contains("The socket was closed");
                            close |= window.Html.Contains("accepting connections");
                            close |= window.Html.Contains("Could not connect");
                            close |= window.Html.Contains("The connection to the server was closed");
                            close |= window.Html.Contains("server was closed");
                            close |= window.Html.Contains("Unable to connect to the selected server. Please check the address and try again.");
                            close |= window.Html.Contains("make sure your characters are out of harm");
                            close |= window.Html.Contains("Connection to server lost");
                            close |= window.Html.Contains("The socket was closed");
                            close |= window.Html.Contains("The specified proxy or server node");
                            close |= window.Html.Contains("Starting up");
                            close |= window.Html.Contains("Unable to connect to the selected server");
                            close |= window.Html.Contains("Could not connect to the specified address");
                            close |= window.Html.Contains("Connection Timeout");
                            close |= window.Html.Contains("The cluster is not currently accepting connections");
                            close |= window.Html.Contains("Your character is located within");
                            close |= window.Html.Contains("The transport has not yet been connected");
                            close |= window.Html.Contains("The user's connection has been usurped");
                            close |= window.Html.Contains("The EVE cluster has reached its maximum user limit");
                            close |= window.Html.Contains("The connection to the server was closed");
                            close |= window.Html.Contains("client is already connecting to the server");
                            //close |= window.Html.Contains("A client update is available and will now be installed");
                            //
                            // eventually it would be nice to hit ok on this one and let it update
                            //
                            close |= window.Html.Contains("client update is available and will now be installed");
                            close |= window.Html.Contains("You are on a <b>14 day trial</b>.");
                            //
                            // these windows require a quit of eve all together
                            //
                            restart |= window.Html.Contains("The connection was closed");
                            restart |= window.Html.Contains("Connection to server lost."); //INFORMATION
                            restart |= window.Html.Contains("Connection to server lost"); //INFORMATION
                            restart |= window.Html.Contains("Local cache is corrupt");
                            restart |= window.Html.Contains("Local session information is corrupt");
                            restart |= window.Html.Contains("The client's local session"); // information is corrupt");
                            restart |= window.Html.Contains("restart the client prior to logging in");

                            //Logging.Log("[Startup] (2) close is: " + close);
                            //Logging.Log("[Startup] (1) window.Html is: " + window.Html);
                            _pulsedelay = 60;
                        }

                        if (restart)
                        {
                            Logging.Log("Startup", "Restarting eve...", Logging.red);
                            Logging.Log("Startup", "Content of modal window (HTML): [" +
                                        (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]", Logging.red);
                            _directEve.ExecuteCommand(DirectCmd.CmdQuitGame);
                            continue;
                        }

                        if (close)
                        {
                            Logging.Log("Startup", "Closing modal window...", Logging.yellow);
                            Logging.Log("Startup", "Content of modal window (HTML): [" +
                                        (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]", Logging.yellow);
                            window.Close();
                            continue;
                        }
                        if (needhumanintervention)
                        {
                            Logging.Log("Startup", "ERROR! - Human Intervention is required in this case: halting all login attempts - ERROR!", Logging.red);
                            Logging.Log("Startup", "window.Name is: " + window.Name, Logging.red);
                            Logging.Log("Startup", "window.Caption is: " + window.Caption, Logging.red);
                            Logging.Log("Startup", "window.ID is: " + window.Id, Logging.red);
                            Logging.Log("Startup", "window.Html is: " + window.Html, Logging.red);
                            Logging.Log("Startup", "ERROR! - Human Intervention is required in this case: halting all login attempts - ERROR!", Logging.red);
                            _humaninterventionrequired = true;
                            return;
                        }
                    }

                    if (string.IsNullOrEmpty(window.Html))
                        continue;
                    if (window.Name == "telecom")
                        continue;
                    Logging.Log("Startup", "We've got an unexpected window, auto login halted.", Logging.red);
                    Logging.Log("Startup", "window.Name is: " + window.Name, Logging.red);
                    Logging.Log("Startup", "window.Caption is: " + window.Caption, Logging.red);
                    Logging.Log("Startup", "window.ID is: " + window.Id, Logging.red);
                    Logging.Log("Startup", "window.Html is: " + window.Html, Logging.red);
                    _done = true;
                    return;
                }
                return;
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
                        Logging.Log("Startup", "DirectEve.RunScript() doesn't exist.  Upgrade DirectEve.dll!", Logging.red);
                    }
                    else
                    {
                        Logging.Log("Startup", string.Format("Running {0}...", _scriptFile), Logging.white);
                        info.Invoke(_directEve, new Object[] { _scriptFile });
                    }
                }
                catch (System.Exception ex)
                {
                    Logging.Log("Startup", string.Format("Exception {0}...", ex), Logging.white);
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
                if (DateTime.Now.Subtract(AppStarted).TotalSeconds > 15)
                {
                    Logging.Log("Startup", "Login account [" + _username + "]", Logging.white);
                    _directEve.Login.Login(_username, _password);
                    Logging.Log("Startup", "Waiting for Character Selection Screen", Logging.white);
                    _pulsedelay = (int)Time.QuestorBeforeLoginPulseDelay_seconds;
                    return;
                }
            }

            if (_directEve.Login.AtCharacterSelection && _directEve.Login.IsCharacterSelectionReady)
            {
                if (DateTime.Now.Subtract(AppStarted).TotalSeconds > 30)
                {
                    foreach (DirectLoginSlot slot in _directEve.Login.CharacterSlots)
                    {
                        if (slot.CharId.ToString(CultureInfo.InvariantCulture) != _character && System.String.Compare(slot.CharName, _character, System.StringComparison.OrdinalIgnoreCase) != 0)
                            continue;

                        Logging.Log("Startup", "Activating character [" + slot.CharName + "]", Logging.white);
                        slot.Activate();
                        return;
                    }
                    Logging.Log("Startup", "Character id/name [" + _character + "] not found, retrying in 10 seconds", Logging.white);
                }
            }
        }

        private static void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            Timer.Stop();
            Logging.Log("Startup", "Timer elapsed.  Starting now.", Logging.white);
            _readyToStart = true;
            _readyToStarta = true;
        }
    }
}