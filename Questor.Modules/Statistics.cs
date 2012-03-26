namespace Questor.Modules
{
    using System;
    using System.Linq;
    using DirectEve;
    using System.IO; 

    public class Statistics
    {
        public StatisticsState State { get; set; }
        private DateTime _lastStatisticsAction;
        public DateTime MissionLoggingStartedTimestamp { get; set; }

        public DateTime StartedMission = DateTime.Now;
        public DateTime FinishedMission = DateTime.Now;
        public DateTime StartedSalvaging = DateTime.Now;
        public DateTime FinishedSalvaging = DateTime.Now;

        public int LootValue { get; set; }
        public int LoyaltyPoints { get; set; }
        public int LostDrones { get; set; }
        public int AmmoConsumption { get; set; }
        public int AmmoValue { get; set; }
        public int MissionsThisSession { get; set; }

        public bool MissionLoggingCompleted = false;
        //private bool PocketLoggingCompleted = false;
        //private bool SessionLoggingCompleted = false;
        public bool DebugMissionStatistics = false;
        public bool MissionLoggingStarted = true;

        /// <summary>
        ///   Singleton implementation
        /// </summary>
        private static Statistics _instance = new Statistics();

        public static Statistics Instance
        {
            get { return _instance; }
        }

        public void ProcessState()
        {

            switch (State)
            {
                case StatisticsState.Idle:
                    //This State should only start every 20 seconds
                    //if (DateTime.Now.Subtract(_lastCleanupAction).TotalSeconds < 20)
                    //    break;

                    //State = StatisticsState.CheckModalWindows;
                    break;

                case StatisticsState.MissionLog:
                    //Logging.Log("StatisticsState: StatisticsState.MissionLog: Entered this state");
                    if (!Statistics.Instance.MissionLoggingStarted) //this makes sure we only set the timestamp once (keep in mind this will potentially run many times before completing)
                    {
                        if (Statistics.Instance.DebugMissionStatistics) Logging.Log("MissionLoggingStarted is" + Statistics.Instance.MissionLoggingStartedTimestamp);
                        Statistics.Instance.MissionLoggingStartedTimestamp = DateTime.Now;
                        Statistics.Instance.MissionLoggingStarted = true;
                    }

                    //Logging.Log("StatisticsState: MissionLogCompleted is false: we still need to create the mission logs for this last mission");
                    //if (Cache.Instance.Agent.LoyaltyPoints == -1) //if this agent has no loyalty points associated then you didn't complete a mission yet.
                    //{
                    //    Logging.Log("We do not have loyalty points with the current agent yet, still -1");
                    //    _lastAction = DateTime.Now;
                    //    return;
                    //}
                    if (DateTime.Now.Subtract(Statistics.Instance.FinishedSalvaging).TotalMinutes > 10 || DateTime.Now.Subtract(Cache.Instance.StartTime).TotalMinutes < 5) //FinishedSalvaging is the later of the 2 timestamps (FinishedMission and FinishedSalvaging), if you aren't after mission salvaging this timestamp is the same as FinishedMission
                    {
                        Logging.Log("Statistics: It is unlikely a mission has been run... No Mission log will be written.");
                        Statistics.Instance.MissionLoggingCompleted = true; //if the mission was completed more than 10 min ago assume the logging has been done already.
                        return;
                    }
                    else
                    {
                        //Logging.Log("Statistics: it has not been more than 10 minutes since the last mission was finished. The Mission log should be written.");                            
                    }

                    if (Statistics.Instance.DebugMissionStatistics) // we only need to see the following wall of comments if debugging mission statistics
                    {
                        Logging.Log("...Checking to see if we should create a mission log now...");
                        Logging.Log(" ");
                        Logging.Log(" ");
                        Logging.Log("The Rules for After Mission Logging are as Follows...");
                        Logging.Log("1)  we must have loyalty points with the current agent (disabled at the moment)"); //which we already verified if we got this far
                        Logging.Log("2) Cache.Instance.MissionName must not be empty - we must have had a mission already this session");
                        Logging.Log("AND");
                        Logging.Log("3a Cache.Instance.mission == null - their must not be a current mission OR");
                        Logging.Log("3b Cache.Instance.mission.State != (int)MissionState.Accepted) - the missionstate isn't 'Accepted'");
                        Logging.Log(" ");
                        Logging.Log(" ");
                        Logging.Log("If those are all met then we get to create a log for the previous mission.");

                        if (!string.IsNullOrEmpty(Cache.Instance.MissionName)) //condition 1
                        {
                            Logging.Log("1 We must have a mission because Missionmame is filled in");
                            Logging.Log("1 Mission is: " + Cache.Instance.MissionName);


                            if (Cache.Instance.mission != null) //condition 2
                            {
                                Logging.Log("2 Cache.Instance.mission is: " + Cache.Instance.mission);
                                Logging.Log("2 Cache.Instance.mission.Name is: " + Cache.Instance.mission.Name);
                                Logging.Log("2 Cache.Instance.mission.State is: " + Cache.Instance.mission.State);

                                if (Cache.Instance.mission.State != (int)MissionState.Accepted) //condition 3
                                {
                                    Logging.Log("MissionState is NOT Accepted: which is correct if we want to do logging");
                                }
                                else
                                {
                                    Logging.Log("MissionState is Accepted: which means the mission is not yet complete");
                                    Statistics.Instance.MissionLoggingCompleted = true; //if if it isn't true - this means we shouldn't be trying to log mission stats atm
                                }
                            }
                            else
                            {
                                Logging.Log("mission is NUL - which means we have no current mission");
                                Statistics.Instance.MissionLoggingCompleted = true; //if if it isn't true - this means we shouldn't be trying to log mission stats atm
                            }
                        }
                        else
                        {
                            Logging.Log("1 We must NOT have had a mission yet because MissionName is not filled in");
                            Statistics.Instance.MissionLoggingCompleted = true; //if if it isn't true - this means we shouldn't be trying to log mission stats atm
                        }
                    }
                    if (!string.IsNullOrEmpty(Cache.Instance.MissionName) && (Cache.Instance.mission == null || (Cache.Instance.mission.State != (int)MissionState.Accepted)))
                    {
                        Statistics.Instance.MissionsThisSession = Statistics.Instance.MissionsThisSession + 1;
                        if (Statistics.Instance.DebugMissionStatistics) Logging.Log("We jumped through all the hoops: now do the mission logging");
                        Cache.Instance.SessionIskGenerated = (Cache.Instance.SessionIskGenerated + (Cache.Instance.DirectEve.Me.Wealth - Cache.Instance.Wealth));
                        Cache.Instance.SessionLootGenerated = (Cache.Instance.SessionLootGenerated + (int)LootValue);
                        Cache.Instance.SessionLPGenerated = (Cache.Instance.SessionLPGenerated + (Cache.Instance.Agent.LoyaltyPoints - LoyaltyPoints));
                        if (Settings.Instance.MissionStats1Log)
                        {
                            if (!Directory.Exists(Settings.Instance.MissionStats1LogPath))
                                Directory.CreateDirectory(Settings.Instance.MissionStats1LogPath);

                            // Write the header
                            if (!File.Exists(Settings.Instance.MissionStats1LogFile))
                                File.AppendAllText(Settings.Instance.MissionStats1LogFile, "Date;Mission;TimeMission;TimeSalvage;TotalTime;Isk;Loot;LP;\r\n");

                            // Build the line
                            var line = DateTime.Now + ";";                                                      // Date
                            line += Cache.Instance.MissionName + ";";                                                               // Mission
                            line += ((int)Statistics.Instance.FinishedMission.Subtract(StartedMission).TotalMinutes) + ";";         // TimeMission
                            line += ((int)DateTime.Now.Subtract(Statistics.Instance.FinishedMission).TotalMinutes) + ";";           // Time Doing After Mission Salvaging
                            line += ((int)DateTime.Now.Subtract(Statistics.Instance.StartedMission).TotalMinutes) + ";";            // Total Time doing Mission
                            line += ((int)(Cache.Instance.DirectEve.Me.Wealth - Cache.Instance.Wealth)) + ";";                      // Isk (balance difference from start and finish of mission: is not accurate as the wallet ticks from bounty kills are every x minuts)
                            line += ((int)Statistics.Instance.LootValue) + ";";                                                     // Loot
                            line += (Cache.Instance.Agent.LoyaltyPoints - LoyaltyPoints) + ";\r\n";                                 // LP

                            // The mission is finished
                            File.AppendAllText(Settings.Instance.MissionStats1LogFile, line);
                            Logging.Log("Questor: writing mission log1 to  [ " + Settings.Instance.MissionStats1LogFile);
                        }
                        if (Settings.Instance.MissionStats2Log)
                        {
                            if (!Directory.Exists(Settings.Instance.MissionStats2LogPath))
                                Directory.CreateDirectory(Settings.Instance.MissionStats2LogPath);

                            // Write the header
                            if (!File.Exists(Settings.Instance.MissionStats2LogFile))
                                File.AppendAllText(Settings.Instance.MissionStats2LogFile, "Date;Mission;Time;Isk;Loot;LP;LostDrones;AmmoConsumption;AmmoValue\r\n");

                            // Build the line
                            var line2 = string.Format("{0:MM/dd/yyyy HH:mm:ss}", DateTime.Now) + ";";                               // Date
                            line2 += Cache.Instance.MissionName + ";";                                                              // Mission
                            line2 += ((int)Statistics.Instance.FinishedMission.Subtract(StartedMission).TotalMinutes) + ";";        // TimeMission
                            line2 += ((int)(Cache.Instance.DirectEve.Me.Wealth - Cache.Instance.Wealth)) + ";";                     // Isk
                            line2 += ((int)Statistics.Instance.LootValue) + ";";                                                    // Loot
                            line2 += (Cache.Instance.Agent.LoyaltyPoints - LoyaltyPoints) + ";";                                    // LP
                            line2 += ((int)Statistics.Instance.LostDrones) + ";";                                                   // Lost Drones
                            line2 += ((int)Statistics.Instance.AmmoConsumption) + ";";                                              // Ammo Consumption
                            line2 += ((int)Statistics.Instance.AmmoValue) + ";\r\n";                                                // Ammo Value

                            // The mission is finished
                            Logging.Log("Questor: writing mission log2 to [ " + Settings.Instance.MissionStats2LogFile);
                            File.AppendAllText(Settings.Instance.MissionStats2LogFile, line2);
                        }
                        if (Settings.Instance.MissionStats3Log)
                        {
                            if (!Directory.Exists(Settings.Instance.MissionStats3LogPath))
                                Directory.CreateDirectory(Settings.Instance.MissionStats3LogPath);

                            // Write the header
                            if (!File.Exists(Settings.Instance.MissionStats3LogFile))
                                File.AppendAllText(Settings.Instance.MissionStats3LogFile, "Date;Mission;Time;Isk;Loot;LP;LostDrones;AmmoConsumption;AmmoValue;Panics;LowestShield;LowestArmor;LowestCap;RepairCycles\r\n");

                            // Build the line
                            var line3 = DateTime.Now + ";";                                                                          // Date
                            line3 += Cache.Instance.MissionName + ";";                                                               // Mission
                            line3 += ((int)Statistics.Instance.FinishedMission.Subtract(StartedMission).TotalMinutes) + ";";         // TimeMission
                            line3 += ((long)(Cache.Instance.DirectEve.Me.Wealth - Cache.Instance.Wealth)) + ";";                     // Isk
                            line3 += ((long)Statistics.Instance.LootValue) + ";";                                                    // Loot
                            line3 += ((long)Cache.Instance.Agent.LoyaltyPoints - LoyaltyPoints) + ";";                               // LP
                            line3 += ((int)Statistics.Instance.LostDrones) + ";";                                                    // Lost Drones
                            line3 += ((int)Statistics.Instance.AmmoConsumption) + ";";                                               // Ammo Consumption
                            line3 += ((int)Statistics.Instance.AmmoValue) + ";";                                                     // Ammo Value
                            line3 += ((int)Cache.Instance.panic_attempts_this_mission) + ";";                                        // Panics
                            line3 += ((int)Cache.Instance.lowest_shield_percentage_this_mission) + ";";                              // Lowest Shield %
                            line3 += ((int)Cache.Instance.lowest_armor_percentage_this_mission) + ";";                               // Lowest Armor %
                            line3 += ((int)Cache.Instance.lowest_capacitor_percentage_this_mission) + ";";                           // Lowest Capacitor %
                            line3 += ((int)Cache.Instance.repair_cycle_time_this_mission) + ";";                                     // repair Cycle Time
                            line3 += ((int)Statistics.Instance.FinishedSalvaging.Subtract(Statistics.Instance.StartedSalvaging).TotalMinutes) + ";"; // After Mission Salvaging Time
                            line3 += ((int)Statistics.Instance.FinishedSalvaging.Subtract(Statistics.Instance.StartedSalvaging).TotalMinutes) + ((int)FinishedMission.Subtract(StartedMission).TotalMinutes) + ";\r\n"; // Total Time, Mission + After Mission Salvaging (if any)

                            // The mission is finished
                            Logging.Log("Questor: writing mission log3 to  [ " + Settings.Instance.MissionStats3LogFile);
                            File.AppendAllText(Settings.Instance.MissionStats3LogFile, line3);
                        }
                        // Disable next log line
                        Cache.Instance.MissionName = null;
                        Statistics.Instance.MissionLoggingCompleted = true;
                    }
                    State = StatisticsState.Idle;
                    break;

                case StatisticsState.PocketLog:
                    State = StatisticsState.Idle;
                    break;

                case StatisticsState.SessionLog:
                    State = StatisticsState.Idle;
                    break;

                case StatisticsState.Done:
                    _lastStatisticsAction = DateTime.Now;
                    State = StatisticsState.Idle;
                    break;

                default:
                    // Next state
                    State = StatisticsState.Idle;
                    break;
            }
        }
    }
}
