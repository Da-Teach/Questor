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
    public enum Group
    {
        Star = 6,
        Station = 15,
        Stargate = 10,

        Capsule = 29,

        // Note: This includes microwarpdrives as well!
        Afterburner = 46,

        ShieldBoosters = 40,
        ShieldHardeners = 77,
        ArmorRepairer = 62,
        ArmorHardeners = 328,
        DamageControl = 60,
        ECCM = 202,
        Concord1 = 25, //frigate - concord and player
        Concord2 = 26, //cruiser - concord and player
        Concord3 = 27, //battleship - concord and player
        Concord4 = 301,
        Concord5 = 446,
        ConcBillboard = 323,

        Indy = 28,
        Ceptor = 831,
        HAC = 358,
        AssaultFrigate = 324,
        Covops = 830,
        StrategicCruiser = 963,
        Marauder = 900,

        SensorBooster = 212,
        TrackingComputer = 213,

        CapacitorGroupCharge = 87,
        CriminalTags = 370,
        EmpireInsigniaDrops = 409,

        StasisWeb = 65,
        TargetPainter = 379,

        EnergyWeapon = 53,
        ProjectileWeapon = 55,
        HybridWeapon = 74,

        SentryGun = 99,
        ProtectiveSentryGun = 180,
        MobileSentryGun = 336,
        DestructibleSentryGun = 383,
        MobileMissileSentry = 417,
        MobileProjectileSentry = 426,
        MobileLaserSentry = 430,
        StasisWebificationBattery = 441,
        MobileHybridSentry = 449,
        DeadspaceOverseersSentry = 495,
        EnergyNeutralizingBattery = 837,

        LargeCollidableStructure = 319,

        CargoContainer = 12,
        SpawnContainer = 306,
        SecureContainer = 340,
        AuditLogSecureContainer = 448,
        FreightContainer = 649,
        MissionContainer = 952,

        SiegeMissileLaunchers = 508,
        CruiseMissileLaunchers = 506,

        Salvager = 538,
        TractorBeam = 650,

        Wreck = 186,
    }
}