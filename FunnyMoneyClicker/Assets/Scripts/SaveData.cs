using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public double moneyCount;

    public long lastSaveTime;
    public float offlineEarningsMultiplier = 0f; // 10% per hour by default = 0.1

    public List<string> unlockedSkins = new List<string>();
    public string equippedSkinId = "default";

    public int[] upgradeLevels = new int[10];
    public int[] powerLevels = new int[10];
}
