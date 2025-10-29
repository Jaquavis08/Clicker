using System;
using Unity.Collections;
using UnityEngine;

[Serializable]
public class SaveData
{
    public double moneyCount;

    public long lastSaveTime;
    public float offlineEarningsMultiplier = 0f; // 10% per hour by default = 0.1
    public int[] upgradeLevels = new int[10]; // Supports up to 10 upgrades
    public int[] powerLevels = new int[10]; // Supports up to 10 upgrades
}
