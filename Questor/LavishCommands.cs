using LavishScriptAPI;
using Questor.Modules;
using Questor.Modules.Common;
using Questor.Modules.Data;
using Questor.Modules.States;

namespace Questor
{
    public partial class LavishCommands
    {
        public Questor _questor;
        
        public void AddCommands()
        {
            LavishScript.Commands.AddCommand("SetAutoStart", SetAutoStart);
            LavishScript.Commands.AddCommand("SetDisable3D", SetDisable3D);
            LavishScript.Commands.AddCommand("SetExitWhenIdle", SetExitWhenIdle);
            LavishScript.Commands.AddCommand("SetQuestorStatetoCloseQuestor", SetQuestorStatetoCloseQuestor);
            LavishScript.Commands.AddCommand("SetQuestorStatetoIdle", SetQuestorStatetoIdle);
            LavishScript.Commands.AddCommand("DebugInfo", DebugInfo);
        }

        private int DebugInfo(string[] args)
        {
            Logging.Log("LastSessionIsReady [" + Cache.Instance.LastSessionIsReady + "]");
            //if (Cache.Instance.InSpace != null)
            //    Logging.Log("InSpace   [" + Cache.Instance.InSpace + "]");
            //Logging.Log("InStation [" + Cache.Instance.InStation + "]");
            //Logging.Log("InMission [" + Cache.Instance.InMission + "]");
            Logging.Log("LastFrame [" + Cache.Instance.LastFrame + "]");
            Logging.Log("LastKnownGoodConnectedTime [" + Cache.Instance.LastKnownGoodConnectedTime + "]");
            Logging.Log("LastSessionChange [ " + Cache.Instance.LastSessionChange + "]");
            //if (Cache.Instance.DirectEve.ActiveShip.Entity != null)
            //{
            //    Logging.Log("----------------------------------");
            //    Logging.Log("ActiveShip.Entity.Name [ " + Cache.Instance.DirectEve.ActiveShip.Entity.Name + "]");
            //    Logging.Log("Shields [" + Cache.Instance.DirectEve.ActiveShip.ShieldPercentage + "%]");
            //    Logging.Log("Armor [" + Cache.Instance.DirectEve.ActiveShip.ArmorPercentage + "%]");
            //    Logging.Log("Capacitor [" + Cache.Instance.DirectEve.ActiveShip.ArmorPercentage + "%]");
            //    Logging.Log("MaxVelocity [" + Cache.Instance.DirectEve.ActiveShip.MaxVelocity + "%]");
            //    Logging.Log("MaxtargetRange [" + Cache.Instance.DirectEve.ActiveShip.MaxTargetRange + "%]");
            //    Logging.Log("Radius [" + Cache.Instance.DirectEve.ActiveShip.Radius + "%]");
            //    Logging.Log("----------------------------------");
            //}
            //else
            //{
            //    Logging.Log("ActiveShip.Entity.Name [ " + null + "]");
            //}
            
            return 0;
        }

        private int SetAutoStart(string[] args)
        {
            bool value;
            if (args.Length != 2 || !bool.TryParse(args[1], out value))
            {
                Logging.Log("SetAutoStart true|false");
                return -1;
            }

            Settings.Instance.AutoStart = value;

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

            Settings.Instance.Disable3D = value;

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

            if (value && Settings.Instance.AutoStart)
            {
                Settings.Instance.AutoStart = false;
                Logging.Log("AutoStart is turned [off]");
            }
            return 0;
        }

        private int SetQuestorStatetoCloseQuestor(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("SetQuestorStatetoCloseQuestor - Changes the QuestorState to CloseQuestor which will GotoBase and then Exit");
                return -1;
            }

            //Cache.Instance.EnteredCloseQuestor_DateTime = DateTime.Now;
            _questor.State = QuestorState.CloseQuestor;

            Logging.Log("QuestorState is now: CloseQuestor ");
            return 0;
        }

        private int SetQuestorStatetoIdle(string[] args)
        {
            if (args.Length != 1)
            {
                Logging.Log("SetQuestorStatetoIdle - Changes the QuestorState to Idle which will GotoBase and then Exit");
                return -1;
            }

            _questor.State = QuestorState.Idle;

            Logging.Log("QuestorState is now: Idle ");
            return 0;
        }
    }
}
