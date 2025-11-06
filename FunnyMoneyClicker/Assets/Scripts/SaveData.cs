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
}
