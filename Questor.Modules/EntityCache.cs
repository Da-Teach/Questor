// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
// 
//     Please look in the accompanying license.htm file for the license that 
//     applies to this source code. (a copy can also be found at: 
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------
namespace Questor.Modules
{
    using System;
    using System.Linq;
    using DirectEve;

    public class EntityCache
    {
        private DirectEntity _directEntity;

        public EntityCache(DirectEntity entity)
        {
            _directEntity = entity;
        }

        public int GroupId
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.GroupId;

                return 0;
            }
        }

        public int CategoryId
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.CategoryId;

                return 0;
            }
        }

        public long Id
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.Id;

                return 0;
            }
        }

        public long FollowId
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.FollowId;

                return 0;
            }
        }

        public string Name
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.Name ?? string.Empty;

                return string.Empty;
            }
        }

        public double Distance
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.Distance;

                return 0;
            }
        }

        public double ShieldPct
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.ShieldPct;

                return 0;
            }
        }

        public double ArmorPct
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.ArmorPct;

                return 0;
            }
        }

        public double StructurePct
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.StructurePct;

                return 0;
            }
        }

        public bool IsNpc
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.IsNpc;

                return false;
            }
        }

        public double Velocity
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.Velocity;

                return 0;
            }
        }

        public bool IsTarget
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.IsTarget;

                return false;
            }
        }

        public bool IsActiveTarget
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.IsActiveTarget;

                return false;
            }
        }

        public bool IsTargeting
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.IsTargeting;

                return false;
            }
        }

        public bool IsTargetedBy
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.IsTargetedBy;

                return false;
            }
        }

        public bool IsAttacking
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.IsAttacking;

                return false;
            }
        }

        public bool IsWreckEmpty
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.IsEmpty;

                return false;
            }
        }

        public bool HasReleased
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.HasReleased;

                return false;
            }
        }

        public bool HasExploded
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.HasExploded;

                return false;
            }
        }

        public bool IsWarpScramblingMe
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.Attacks.Contains("effects.WarpScramble");

                return false;
            }
        }

        public bool IsWebbingMe
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.Attacks.Contains("effects.ModifyTargetSpeed");

                return false;
            }
        }

        public bool IsNeutralizingMe
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.ElectronicWarfare.Contains("ewEnergyNeut");

                return false;
            }
        }

        public bool IsJammingMe
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.ElectronicWarfare.Contains("electronic");

                return false;
            }
        }

        public bool IsSensorDampeningMe
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.ElectronicWarfare.Contains("ewRemoteSensorDamp");

                return false;
            }
        }

        public bool IsTargetPaintingMe
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.ElectronicWarfare.Contains("ewTargetPaint");

                return false;
            }
        }

        public bool IsTrackingDisruptingMe
        {
            get
            {
                if (_directEntity != null)
                    return _directEntity.ElectronicWarfare.Contains("ewTrackingDisrupt");

                return false;
            }
        }

        public bool IsSentry
        {
            get
            {
                if (GroupId == (int) Group.SentryGun) return true;
                if (GroupId == (int) Group.ProtectiveSentryGun) return true;
                if (GroupId == (int) Group.MobileSentryGun) return true;
                if (GroupId == (int) Group.DestructibleSentryGun) return true;
                if (GroupId == (int) Group.MobileMissileSentry) return true;
                if (GroupId == (int) Group.MobileProjectileSentry) return true;
                if (GroupId == (int) Group.MobileLaserSentry) return true;
                if (GroupId == (int) Group.MobileHybridSentry) return true;
                if (GroupId == (int) Group.DeadspaceOverseersSentry) return true;
                if (GroupId == (int) Group.StasisWebificationBattery) return true;
                if (GroupId == (int) Group.EnergyNeutralizingBattery) return true;
                return false;
            }
        }

        public bool HaveLootRights
        {
            get
            {
                if (GroupId == (int) Group.SpawnContainer)
                    return true;

                if (_directEntity != null)
                {
                    var haveLootRights = false;
                    if (Cache.Instance.DirectEve.ActiveShip.Entity != null)
                    {
                        haveLootRights |= _directEntity.CorpId == Cache.Instance.DirectEve.ActiveShip.Entity.CorpId;
                        haveLootRights |= _directEntity.OwnerId == Cache.Instance.DirectEve.ActiveShip.Entity.CharId;
                    }

                    return haveLootRights;
                }

                return false;
            }
        }

        public int? TargetValue
        {
            get
            {
                var value = Cache.Instance.ShipTargetValues.FirstOrDefault(v => v.GroupId == GroupId);
                if (value == null)
                    return null;

                return value.TargetValue;
            }
        }

        public DirectContainerWindow CargoWindow
        {
            get { return Cache.Instance.Windows.OfType<DirectContainerWindow>().FirstOrDefault(w => w.ItemId == Id); }
        }

        public bool IsValid
        {
            get
            {
                if (_directEntity == null)
                    return false;

                return _directEntity.IsValid;
            }
        }

        public bool IsContainer
        {
            get
            {
                if (GroupId == (int) Group.Wreck) return true;
                if (GroupId == (int) Group.CargoContainer) return true;
                if (GroupId == (int) Group.SpawnContainer) return true;
                if (GroupId == (int) Group.MissionContainer) return true;
                return false;
            }
        }

        public bool IsPlayer
        {
            get { return _directEntity.IsPc; }
        }

        /// <summary>
        /// Frigate includes all elite-variants
        /// </summary>
        public bool IsFrigate
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.Frigate;
                result |= GroupId == (int)Group.AssaultShip;
                result |= GroupId == (int)Group.StealthBomber;
                result |= GroupId == (int)Group.ElectronicAttackShip;
                result |= GroupId == (int)Group.PrototypeExplorationShip;

                // Technically not frigs, but for our purposes they are
                result |= GroupId == (int)Group.Destroyer; 
                result |= GroupId == (int)Group.Interdictor;
                result |= GroupId == (int)Group.Interceptor;
                return result;
            }
        }

        /// <summary>
        /// Frigate includes all elite-variants
        /// </summary>
        public bool IsCruiser
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.Cruiser;
                result |= GroupId == (int)Group.HeavyAssaultShip;
                result |= GroupId == (int)Group.Logistics;
                result |= GroupId == (int)Group.ForceReconShip;
                result |= GroupId == (int)Group.CombatReconShip;
                result |= GroupId == (int)Group.HeavyInterdictor;
                return result;
            }
        }

        /// <summary>
        /// Frigate includes all elite-variants
        /// </summary>
        public bool IsBattlecruiser
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.Battlecruiser;
                result |= GroupId == (int)Group.CommandShip;
                result |= GroupId == (int)Group.StrategicCruiser; // Technically a cruiser, but hits hard enough to be a BC :)
                return result;
            }
        }
        /// <summary>
        /// Frigate includes all elite-variants
        /// </summary>
        public bool IsBattleship
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.Battleship;
                result |= GroupId == (int)Group.EliteBattleship;
                result |= GroupId == (int)Group.BlackOps;
                result |= GroupId == (int)Group.Marauder;
                return result;
            }
        }

        /// <summary>
        /// A bad idea to attack these targets
        /// </summary>
        public bool IsBadIdea
        {
            get
            {
                bool result = false;
                result |= GroupId == (int)Group.ConcordDrone;
                result |= GroupId == (int)Group.CustomsOfficial;
                result |= GroupId == (int)Group.Billboard;
                result |= IsFrigate;
                result |= IsCruiser;
                result |= IsBattlecruiser;
                result |= IsBattleship;
                result |= _directEntity.IsPc;
                return result;
            }
        }

        public void LockTarget()
        {
            // If the bad idea is attacking, attack back
            if (IsBadIdea && !IsAttacking)
            {
                Logging.Log("EntityCache: Attempting to target a player or concord entity! [" + Name + "]");
                return;
            }

            if (Cache.Instance.TargetingIDs.ContainsKey(Id))
            {
                var lastTargeted = Cache.Instance.TargetingIDs[Id];

                // Ignore targeting request
                var seconds = DateTime.Now.Subtract(lastTargeted).TotalSeconds;
                if (seconds < 45)
                {
                    Logging.Log("EntityCache: LockTarget is ignored for [" + Name + "][" + Id + "], can retarget in [" + (45 - seconds) + "]");
                    return;
                }
            }

            // Only add targeting id's when its actually being targeted
            if (_directEntity != null && _directEntity.LockTarget())
                Cache.Instance.TargetingIDs[Id] = DateTime.Now;
        }

        public void UnlockTarget()
        {
            if (_directEntity != null)
                _directEntity.UnlockTarget();
        }

        public void Jump()
        {
            if (_directEntity != null)
                _directEntity.Jump();
        }

        public void Activate()
        {
            if (_directEntity != null)
                _directEntity.Activate();
        }

        public void Approach()
        {
            Cache.Instance.Approaching = this;

            if (_directEntity != null)
                _directEntity.Approach();
        }

        public void Approach(int range)
        {
            Cache.Instance.Approaching = this;

            if (_directEntity != null)
                _directEntity.Approach(range);
        }

        public void Orbit(int range)
        {
            Cache.Instance.Approaching = this;

            if (_directEntity != null)
                _directEntity.Orbit(range);
        }

        public void WarpTo()
        {
            if (_directEntity != null)
                _directEntity.WarpTo();
        }

        public void AlignTo()
        {
            if (_directEntity != null)
                _directEntity.AlignTo();
        }

        public void WarpToAndDock()
        {
            if (_directEntity != null)
                _directEntity.WarpToAndDock();
        }

        internal void Dock()
        {
            if (_directEntity != null)
                _directEntity.Dock();
        }

        public void OpenCargo()
        {
            if (_directEntity != null)
                _directEntity.OpenCargo();
        }

        public void MakeActiveTarget()
        {
            if (_directEntity != null)
                _directEntity.MakeActiveTarget();
        }
    }
}