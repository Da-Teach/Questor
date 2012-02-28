// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace GridMon
{
    using System;
    using System.Linq;
    using System.Threading;
    using DirectEve;
    using InnerSpaceAPI;

    internal class Program
    {
        private const int WaitMillis = 1500;
        private static DateTime _nextAction;
        private static bool _done;
        private static DirectEve _directEve;

        private static void Main(string[] args)
        {
            Log("Starting GridMon...");
            _directEve = new DirectEve();
            _directEve.OnFrame += OnFrame;

            // Sleep until we're done
            while (!_done)
                Thread.Sleep(50);

            _directEve.Dispose();
            Log("GridMon finished.");
        }

        private static void Log(string line, params object[] parms)
        {
            line = string.Format(line, parms);
            InnerSpace.Echo(string.Format("{0:HH:mm:ss} {1}", DateTime.Now, line));
        }

        private static void OnFrame(object sender, EventArgs eventArgs)
        {
            if (_done)
                return;

            // Wait for the next action
            if (_nextAction >= DateTime.Now)
            {
                return;
            }

            foreach (var entity in _directEve.Entities)
                LogEntity("Entity[" + entity.Id + "].{0}: {1}", entity);

            _nextAction = DateTime.Now.AddMilliseconds(10000);
        }

        private static void LogEntity(string format, DirectEntity entity)
        {
            if (entity == null)
                return;

            Log(format, "Id", entity.Id);
            Log(format, "OwnerId", entity.OwnerId);
            Log(format, "CorpId", entity.CorpId);
            Log(format, "AllianceId", entity.AllianceId);

            Log(format, "FollowId", entity.FollowId);

            Log(format, "IsNpc", entity.IsNpc);
            Log(format, "IsPc", entity.IsPc);

            Log(format, "TypeId", entity.TypeId);
            Log(format, "GroupId", entity.GroupId);
            Log(format, "TypeName", entity.TypeName);
            Log(format, "Name", entity.Name);
            Log(format, "GivenName", entity.GivenName);

            Log(format, "Distance", entity.Distance);
            Log(format, "Velocity", entity.Velocity);

            Log(format, "IsAttacking", entity.IsAttacking);
            Log(format, "IsCloaked", entity.IsCloaked);
            Log(format, "IsNeutralizingMe", entity.IsNeutralizingMe);
            Log(format, "IsJammingMe", entity.IsJammingMe);
            Log(format, "IsWebbingMe", entity.IsWebbingMe);
            Log(format, "IsSensorDampeningMe", entity.IsSensorDampeningMe);
        }

    }
}