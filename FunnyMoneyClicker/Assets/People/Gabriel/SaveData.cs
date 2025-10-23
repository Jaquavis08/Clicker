using System;
using UnityEngine;

[Serializable]
public class SaveData
{
    public float moneyCount;
    public float playTime;
    public int[] upgradeLevels = new int[10]; // Supports up to 10 upgrades
}
