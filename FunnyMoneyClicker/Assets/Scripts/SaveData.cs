using BreakInfinity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    [JsonConverter(typeof(BigDoubleConverter))]
    public BigDouble moneyCount;

    public int gems;
    public int lastTierIndex;

    public long lastSaveTime;
    public float offlineEarningsMultiplier = 0f; // 10% per hour by default = 0.1

    public List<string> unlockedSkins = new List<string>();
    public string equippedSkinId = "1";

    public int[] upgradeLevels = new int[10];
    public int[] powerLevels = new int[10];


    public List<ItemSaveData> gridItems = new List<ItemSaveData>();
    public InventorySaveData inventoryData = new InventorySaveData();
}

[System.Serializable]
public class ItemSaveData
{
    public int itemId;
    public int slotIndex; // which slot on the grid (0-based)
}

[System.Serializable]
public class InventorySaveData
{
    public List<int> itemIds = new List<int>(); // all items currently in inventory
}
