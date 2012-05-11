namespace Questor.Modules
{
    using System;
    using System.Linq;

    public class Cleanup
    {
        private CleanupState State { get; set; }
        private DateTime _lastCleanupAction;

        public void ProcessState()
        {

            switch (State)
            {
                case CleanupState.Start:
                    //Cleanup State should only start every 20 seconds
                    if (DateTime.Now.Subtract(_lastCleanupAction).TotalSeconds < 20)
                        break;

                    State = CleanupState.CheckModalWindows;
                    break;

                case CleanupState.CheckModalWindows:
                    //
                    // go through *every* window
                    //
                    foreach (var window in Cache.Instance.Windows)
                    {
                        // Telecom messages are generally mission info messages: close them
                        if (window.Name == "telecom")
                        {
                            Logging.Log("Cleanup: Closing telecom message...");
                            Logging.Log("Cleanup: Content of telecom window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                            window.Close();
                        }

                        // Modal windows must be closed
                        // But lets only close known modal windows
                        if (window.Name == "modal")
                        {
                            bool close = false;
                            bool restart = false;
                            bool gotobasenow = false;
                            bool sayyes = false;
                            //bool sayno = false;
                            if (!string.IsNullOrEmpty(window.Html))
                            {
                                // Server going down /unscheduled/ potentially very soon! 
                                // CCP does not reboot in the middle of the day because the server is behaving
                                // dock now to avoid problems
                                gotobasenow |= window.Html.Contains("for a short unscheduled reboot");
                                
                                // Server going down
                                close |= window.Html.Contains("Please make sure your characters are out of harm");
                                close |= window.Html.Contains("the servers are down for 30 minutes each day for maintenance and updates");
                                if (window.Html.Contains("The socket was closed"))
                                {
                                    Logging.Log("Cleanup: This window indicates we are disconnected: Content of modal window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                                    //Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.CmdLogOff); //this causes the questor window to not re-appear
                                    Cache.Instance.CloseQuestorCMDLogoff = false;
                                    Cache.Instance.CloseQuestorCMDExitGame = true;
                                    Cache.Instance.ReasonToStopQuestor = "The socket was closed";
                                    Cache.Instance.SessionState = "Quitting";
                                    break;
                                }

                                // In space "shit"
                                close |= window.Html.Contains("Item cannot be moved back to a loot container.");
                                close |= window.Html.Contains("you do not have the cargo space");
                                close |= window.Html.Contains("cargo units would be required to complete this operation.");
                                close |= window.Html.Contains("You are too far away from the acceleration gate to activate it!");
                                close |= window.Html.Contains("maximum distance is 2500 meters");
                                // Stupid warning, lets see if we can find it
                                close |= window.Html.Contains("Do you wish to proceed with this dangerous action?");
                                // Yes we know the mission isnt complete, Questor will just redo the mission
                                close |= window.Html.Contains("Please check your mission journal for further information.");
                                close |= window.Html.Contains("weapons in that group are already full");
                                close |= window.Html.Contains("You have to be at the drop off location to deliver the items in person");
                                // Lag :/
                                close |= window.Html.Contains("This gate is locked!");
                                close |= window.Html.Contains("The Zbikoki's Hacker Card");
                                close |= window.Html.Contains(" units free.");
                                close |= window.Html.Contains("already full");
                                //
                                // restart the client if these are encountered
                                //
                                restart |= window.Html.Contains("Local cache is corrupt");
                                restart |= window.Html.Contains("Local session information is corrupt");
                                restart |= window.Html.Contains("The connection to the server was closed"); 										//CONNECTION LOST
                                restart |= window.Html.Contains("server was closed");  																//CONNECTION LOST
                                restart |= window.Html.Contains("The socket was closed"); 															//CONNECTION LOST
                                restart |= window.Html.Contains("The connection was closed"); 														//CONNECTION LOST
                                restart |= window.Html.Contains("Connection to server lost"); 														//INFORMATION
                                restart |= window.Html.Contains("The user connection has been usurped on the proxy"); 								//CONNECTION LOST
                                restart |= window.Html.Contains("The transport has not yet been connected, or authentication was not successful"); 	//CONNECTION LOST
                                //
                                // Modal Dialogs the need "yes" pressed
                                //
                                sayyes |= window.Html.Contains("objectives requiring a total capacity");
                                sayyes |= window.Html.Contains("your ship only has space for");
                                //
                                // Modal Dialogs the need "no" pressed
                                //
                                //sayno |= window.Html.Contains("Do you wish to proceed with this dangerous action
                            }
                            if (sayyes)
                            {
                                Logging.Log("Cleanup: Found a window that needs 'yes' chosen...");
                                Logging.Log("Cleanup: Content of modal window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                                window.AnswerModal("Yes");
                                continue;
                            }
                            if (close)
                            {
                                Logging.Log("Cleanup: Closing modal window...");
                                Logging.Log("Cleanup: Content of modal window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                                window.Close();
                                continue;
                            }

                            if (restart)
                            {
                                Logging.Log("Cleanup: Restarting eve...");
                                Logging.Log("Cleanup: Content of modal window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                                Cache.Instance.CloseQuestorCMDLogoff = false;
                                Cache.Instance.CloseQuestorCMDExitGame = true;
                                Cache.Instance.ReasonToStopQuestor = "A message from ccp indicated we were disconnected";
                                Cache.Instance.SessionState = "Quitting";
                                window.Close();
                                continue;
                            }
                            if (gotobasenow)
                            {
                                Logging.Log("Cleanup: Evidentially the cluster is dieing... and CCP is restarting the server");
                                Logging.Log("Cleanup: Content of modal window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                                Cache.Instance.GotoBaseNow = true;
                                Settings.Instance.AutoStart = false;
                                //
                                // do not close eve, let the shutdown of the server do that
                                //
                                //Cache.Instance.CloseQuestorCMDLogoff = false;
                                //Cache.Instance.CloseQuestorCMDExitGame = true;
                                //Cache.Instance.ReasonToStopQuestor = "A message from ccp indicated we were disonnected";
                                //Cache.Instance.SessionState = "Quitting";
                                window.Close();
                                continue;
                            }
                        }
                    }
                    State = CleanupState.Done;
                    break;



                case CleanupState.Done:
                    _lastCleanupAction = DateTime.Now;
                    State = CleanupState.Start;
                    break;

                default:
                    // Next state
                    State = CleanupState.Start;
                    break;
            }
        }
    }
}
