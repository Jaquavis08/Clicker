using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public double moneyCount;
    public float playTime;
    public int[] upgradeLevels = new int[10]; // Supports up to 10 upgrades
    public int[] powerLevels = new int[10]; // Supports up to 10 upgrades
}
