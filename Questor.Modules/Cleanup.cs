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
                            Logging.Log("Questor: Closing telecom message...");
                            Logging.Log("Questor: Content of telecom window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                            window.Close();
                        }

                        // Modal windows must be closed
                        // But lets only close known modal windows
                        if (window.Name == "modal")
                        {
                            bool close = false;
                            bool restart = false;
                            if (!string.IsNullOrEmpty(window.Html))
                            {
                                // Server going down
                                close |= window.Html.Contains("Please make sure your characters are out of harm");
                                close |= window.Html.Contains("the servers are down for 30 minutes each day for maintenance and updates");
                                if (window.Html.Contains("The socket was closed"))
                                {
                                    Logging.Log("Questor: This window indicates we are disconnected: Content of modal window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
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
                                //_pulsedelay = 20;
                            }

                            if (close)
                            {
                                Logging.Log("Questor: Closing modal window...");
                                Logging.Log("Questor: Content of modal window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                                window.Close();
                            }

                            if (restart)
                            {
                                Logging.Log("Questor: Restarting eve...");
                                Logging.Log("Questor: Content of modal window (HTML): [" + (window.Html ?? string.Empty).Replace("\n", "").Replace("\r", "") + "]");
                                Cache.Instance.CloseQuestorCMDLogoff = false;
                                Cache.Instance.CloseQuestorCMDExitGame = true;
                                Cache.Instance.ReasonToStopQuestor = "A message from ccp indicated we were disonnected";
                                Cache.Instance.SessionState = "Quitting";
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
