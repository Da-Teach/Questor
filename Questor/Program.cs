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
using System.Timers;

namespace Questor
{
    using global::Questor.Modules;
    using LavishScriptAPI;
    using DirectEve;

    static class Program
    {
        private static bool _done;
        private static bool _readyToStart;
        private static bool _readyToStarta;
        private static DirectEve _directEve;
        private static bool _autoStart;

        private static string _username;
        private static string _password;
        private static string _character;

        private static DateTime _lastPulse;
        private static DateTime _startTime;
        private static double secondsToStart;

        private static string _startTimeString;
        private static string _stopTimeString;
        private static string _runTimeString;

        static System.Timers.Timer _timer = new System.Timers.Timer();
        private static int _randStartDelay = 2; //Random startup delay in minutes
        private static Random _r = new Random();
        public static DateTime stopTime;
        public static bool stopTimeSpecified;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            _autoStart = false;

            _readyToStart = false;
            _readyToStarta = false;

            secondsToStart = 10;

            if (args.Length >= 1)
            {

                string myArg;
                int strNumber;
                int strIndex;
                
                myArg = "-user";
                strIndex = 0;
                for (strNumber = 0; strNumber < args.Length; strNumber++)
                {
                    strIndex = args[strNumber].IndexOf(myArg);
                    if (strIndex >= 0)
                    {
                        _username = args[strNumber + 1];
                        break;
                    }
                }

                myArg = "-pw";
                strIndex = 0;
                for (strNumber = 0; strNumber < args.Length; strNumber++)
                {
                    strIndex = args[strNumber].IndexOf(myArg);
                    if (strIndex >= 0)
                    {
                        _password = args[strNumber + 1];
                        break;
                    }
                }

                myArg = "-char";
                strIndex = 0;
                for (strNumber = 0; strNumber < args.Length; strNumber++)
                {
                    strIndex = args[strNumber].IndexOf(myArg);
                    if (strIndex >= 0)
                    {
                        _character = args[strNumber + 1];
                        break;
                    }
                }

                myArg = "-start";
                strIndex = 0;
                for (strNumber = 0; strNumber < args.Length; strNumber++)
                {
                    strIndex = args[strNumber].IndexOf(myArg);
                    if (strIndex >= 0)
                    {
                        _startTimeString = args[strNumber + 1];
                        break;
                    }
                }

                myArg = "-stop";
                strIndex = 0;
                for (strNumber = 0; strNumber < args.Length; strNumber++)
                {
                    strIndex = args[strNumber].IndexOf(myArg);
                    if (strIndex >= 0)
                    {
                        _stopTimeString = args[strNumber + 1];
                        break;
                    }
                }

                myArg = "-run";
                strIndex = 0;
                for (strNumber = 0; strNumber < args.Length; strNumber++)
                {
                    strIndex = args[strNumber].IndexOf(myArg);
                    if (strIndex >= 0)
                    {
                        _runTimeString = args[strNumber + 1];
                        break;
                    }
                }

                myArg = "-autostart";
                strIndex = 0;
                for (strNumber = 0; strNumber < args.Length; strNumber++)
                {
                    strIndex = args[strNumber].IndexOf(myArg);
                    if (strIndex >= 0)
                    {
                        _autoStart = true;
                        break;
                    }
                }

                Logging.Log("User: " + _username + " PW: " + _password + " Char: " + _character + " Start: " + _startTimeString + " Stop: " + _stopTimeString + " Run: " + _runTimeString + " AutoStart: " + _autoStart);

                if ((_username == null) || (_password == null) || (_character == null))
                    return;
                Logging.Log("test1");

                if (_startTimeString != null)
                {
                    if (DateTime.TryParse(_startTimeString, out _startTime))
                    {
                        Logging.Log("Start time: " + _startTime);
                        _startTime = _startTime.AddSeconds((double)(_r.Next(0, (_randStartDelay * 60))));
                        Logging.Log("Randomized start time: " + _startTime);
                        if (!(DateTime.Now > _startTime))
                        {
                            secondsToStart = _startTime.Subtract(DateTime.Now).TotalSeconds;
                            if (secondsToStart <= 43200) //If more than 12 hours out, assume we already missed the start time
                            {
                                Logging.Log(String.Format("{0:0.##}", (secondsToStart / 60)) + " minutes to go.");
                                _timer.Elapsed += new ElapsedEventHandler(TimerEventProcessor);
                                _timer.Interval = (int)(secondsToStart * 1000);
                                _timer.Enabled = true;
                                _timer.Start();
                            }
                        }
                        else
                        {
                            Logging.Log("Already passed start time.  Starting now.");
                            _readyToStarta = true;
                        }
                    }
                    else
                        Logging.Log("Couldn't parse starttime");
                }
                else
                {
                    Logging.Log("No start time specified.");
                    _readyToStarta = true;
                }

                if (_runTimeString != null)
                {
                    double runtime;
                    if (Double.TryParse(_runTimeString, out runtime))
                    {
                        if (DateTime.Now < _startTime)
                            stopTime = _startTime.AddHours(runtime);
                        else
                            stopTime = DateTime.Now.AddHours(runtime);
                        Logging.Log("Stop time: " + stopTime);
                        stopTimeSpecified = true;
                    }
                }
                else if (_stopTimeString != null)
                {
                    if (!DateTime.TryParse(_stopTimeString, out stopTime))
                    {
                        Logging.Log("Unable to parse stop time.");
                        stopTimeSpecified = false;
                    }
                    else
                    {
                        if (DateTime.Now > stopTime) //if we've already passed the stop time, set it for tomorrow
                            stopTime = stopTime.AddDays(1);
                        stopTimeSpecified = true;
                    }

                }
                else
                {
                    Logging.Log("No stop time specified.");
                    stopTimeSpecified = false;
                }

                _readyToStart = _readyToStarta;

                _directEve = new DirectEve();
                _directEve.OnFrame += OnFrame;                

                var started = DateTime.Now;

                // Sleep until we're done
                while (!_done)
                {
                    System.Threading.Thread.Sleep(50);
                }

                _directEve.Dispose();
                if (_autoStart == false)
                    return;


            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        static void OnFrame(object sender, EventArgs e)
        {
            if (!_readyToStart)
                return;

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

        static void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            _timer.Stop();
            Logging.Log("Timer elapsed.  Starting now.");
            _readyToStart = true;
        }
    
    }
}
