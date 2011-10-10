﻿// ------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;

    public class UnloadLoot
    {
        public const int StationContainer = 17366;

        private DateTime _lastAction;

        public UnloadLootState State { get; set; }
        public double LootValue { get; set; }

        public void ProcessState()
        {
            var cargo = Cache.Instance.DirectEve.GetShipsCargo();
            var hangar = Cache.Instance.DirectEve.GetItemHangar();

            DirectContainer corpAmmoHangar = null;
            if (!string.IsNullOrEmpty(Settings.Instance.AmmoHangar))
                corpAmmoHangar = Cache.Instance.DirectEve.GetCorporationHangar(Settings.Instance.AmmoHangar);

            DirectContainer corpLootHangar = null;
            if (!string.IsNullOrEmpty(Settings.Instance.LootHangar))
                corpLootHangar = Cache.Instance.DirectEve.GetCorporationHangar(Settings.Instance.LootHangar);

            DirectContainer corpBookmarkHangar = null;
            if (!string.IsNullOrEmpty(Settings.Instance.BookmarkHangar))
                corpBookmarkHangar = Cache.Instance.DirectEve.GetCorporationHangar(Settings.Instance.BookmarkHangar);

            switch (State)
            {
                case UnloadLootState.Idle:
                case UnloadLootState.Done:
                    break;

                case UnloadLootState.Begin:
                    Logging.Log("UnloadLoot: Opening station hangar");
                    State = UnloadLootState.OpenItemHangar;
                    break;

                case UnloadLootState.OpenItemHangar:
                    // Is the hangar open?
                    if (hangar.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenHangarFloor);
                        break;
                    }

                    // Not ready yet
                    if (!hangar.IsReady)
                        break;

                    Logging.Log("UnloadLoot: Opening ship's cargo");
                    State = UnloadLootState.OpenShipsCargo;
                    break;

                case UnloadLootState.OpenShipsCargo:
                    // Is cargo open?
                    if (cargo.Window == null)
                    {
                        // No, command it to open
                        Cache.Instance.DirectEve.ExecuteCommand(DirectCmd.OpenCargoHoldOfActiveShip);
                        break;
                    }

                    if (!cargo.IsReady)
                        break;

                    if (corpAmmoHangar != null || corpLootHangar != null)
                    {
                        Logging.Log("UnloadLoot: Opening corporation hangar");
                        State = UnloadLootState.OpenCorpHangar;
                    }
                    else
                    {
                        Logging.Log("UnloadLoot: Moving items");
                        State = UnloadLootState.MoveLoot;
                    }
                    break;

                case UnloadLootState.OpenCorpHangar:
                    // Is cargo open?
                    var corpHangar = corpAmmoHangar ?? corpLootHangar;
                    if (corpHangar != null)
                    {
                        if (corpHangar.Window == null)
                        {
                            // No, command it to open
                            Cache.Instance.DirectEve.OpenCorporationHangar();
                            break;
                        }

                        if (!corpHangar.IsReady)
                            break;
                    }

                    Logging.Log("UnloadLoot: Moving loot");
                    State = UnloadLootState.MoveLoot;
                    break;

                case UnloadLootState.MoveLoot:
                    var lootHangar = corpLootHangar ?? hangar;

                    var lootToMove = cargo.Items.Where(i => (i.TypeName ?? string.Empty).ToLower() != Cache.Instance.BringMissionItem && !Settings.Instance.Ammo.Any(a => a.TypeId == i.TypeId));
                    LootValue = 0;
                    foreach (var item in lootToMove)
                    {
                        if (!Cache.Instance.InvTypesById.ContainsKey(item.TypeId))
                            continue;

                        var invType = Cache.Instance.InvTypesById[item.TypeId];
                        LootValue += (invType.MedianBuy ?? 0)*Math.Max(item.Quantity, 1);
                    }

                    // Move loot to the loot hangar
                    lootHangar.Add(lootToMove);
                    _lastAction = DateTime.Now;

                    Logging.Log("UnloadLoot: Loot was worth an estimated [" + LootValue.ToString("#,##0") + "] isk in buy-orders");

                    //Move bookmarks to the bookmarks hangar
                    if (!string.IsNullOrEmpty(Settings.Instance.BookmarkHangar) && Settings.Instance.CreateSalvageBookmarks == true)
                    {
                        Logging.Log("UnloadLoot: Creating salvage bookmarks in hangar");
                        var bookmarks = Cache.Instance.BookmarksByLabel(Settings.Instance.BookmarkPrefix + " ");
                        List<long> salvageBMs = new List<long>();
                        int i = 0;
                        foreach (DirectBookmark bookmark in bookmarks)
                        {
                            salvageBMs.Add((long)bookmark.BookmarkId);
                            i++;
                            if (i == 4)
                            {
                                hangar.AddBookmarks(salvageBMs);
                                salvageBMs.Clear();
                                i = 0;
                            }
                        }
                        if (salvageBMs.Count > 0)
                        {
                            hangar.AddBookmarks(salvageBMs);
                            salvageBMs.Clear();
                        }
                    }

                    Logging.Log("UnloadLoot: Moving ammo");
                    State = UnloadLootState.MoveAmmo;
                    break;

                case UnloadLootState.MoveAmmo:
                    // Wait 5 seconds after moving
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    var ammoHangar = corpAmmoHangar ?? hangar;

                    // Move the mission item & ammo to the ammo hangar
                    ammoHangar.Add(cargo.Items.Where(i => ((i.TypeName ?? string.Empty).ToLower() == Cache.Instance.BringMissionItem || Settings.Instance.Ammo.Any(a => a.TypeId == i.TypeId))));
                    _lastAction = DateTime.Now;

                    Logging.Log("UnloadLoot: Waiting for items to move");
                    State = UnloadLootState.WaitForMove;
                    break;

                case UnloadLootState.WaitForMove:
                    if (cargo.Items.Count != 0)
                    {
                        _lastAction = DateTime.Now;
                        break;
                    }

                    // Wait 5 seconds after moving
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        if (corpBookmarkHangar != null && Settings.Instance.CreateSalvageBookmarks)
                        {
                            Logging.Log("UnloadLoot: Moving salvage bookmarks to corp hangar");
                            corpBookmarkHangar.Add(hangar.Items.Where(i => i.TypeId == 51));
                        }

                        Logging.Log("UnloadLoot: Stacking items");
                        State = UnloadLootState.StackItems;
                        break;
                    }

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds > 120)
                    {
                        Logging.Log("UnloadLoot: Moving items timed out, clearing item locks");
                        Cache.Instance.DirectEve.UnlockItems();

                        Logging.Log("UnloadLoot: Stacking items");
                        State = UnloadLootState.StackItems;
                        break;
                    }
                    break;

                case UnloadLootState.StackItems:
                    // Dont stack until 5 seconds after the cargo has cleared
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    // Stack everything
                    _lastAction = DateTime.Now;
                    hangar.StackAll();
                    if (corpAmmoHangar != null)
                        corpAmmoHangar.StackAll();
                    if (corpLootHangar != null)
                        corpLootHangar.StackAll();

                    State = UnloadLootState.WaitForStacking;
                    break;

                case UnloadLootState.WaitForStacking:
                    // Wait 5 seconds after stacking
                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds < 5)
                        break;

                    if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        Logging.Log("UnloadLoot: Done");
                        State = UnloadLootState.Done;
                        break;
                    }

                    if (DateTime.Now.Subtract(_lastAction).TotalSeconds > 120)
                    {
                        Logging.Log("UnloadLoot: Stacking items timed out, clearing item locks");
                        Cache.Instance.DirectEve.UnlockItems();

                        Logging.Log("UnloadLoot: Done");
                        State = UnloadLootState.Done;
                        break;
                    }
                    break;
            }
        }
    }
}