namespace TreeTest1
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using DirectEve;

    public class Cache
    {
        /// <summary>
        ///   Singleton implementation
        /// </summary>
        private static Cache _instance = new Cache();

        public static Cache Instance
        {
            get { return _instance; }
        }

        private DirectEve _directEve;
        public DirectEve DirectEve { get; set; }

        public Cache()
        {
        }

        public bool InSpace
        {
            get { return DirectEve.Session.IsInSpace && !DirectEve.Session.IsInStation && DirectEve.Session.IsReady && DirectEve.ActiveShip.Entity != null; }
        }

        public bool InStation
        {
            get { return DirectEve.Session.IsInStation && !DirectEve.Session.IsInSpace && DirectEve.Session.IsReady; }
        }

        public bool InWarp
        {
            get { return DirectEve.ActiveShip.Entity != null ? DirectEve.ActiveShip.Entity.Mode == 3 : false; }
        }


    }
}