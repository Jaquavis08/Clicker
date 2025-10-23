using TMPro;
using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    [Header("Upgrade Settings")]
    public string upgradeName; // The display name of the upgrade (e.g. "Auto Clicker", "Miner", "Factory")

    public float baseCost = 10f; // Starting price for the first level of this upgrade
    public float baseProduction = 1f; // How much money (or resources) this upgrade produces per cycle at level 1
    [HideInInspector] public float costIncreaseRate = 1.145f; // Multiplier for cost growth each level (e.g. cost *= 1.15)
    [HideInInspector] public float productionIncreaseRate = 1.07f; // Multiplier for production increase each level (e.g. production *= 1.05)
    [HideInInspector] public float baseInterval = 1f; // Time in seconds between each automatic production tick

    [Header("UI")]
    public TMP_Text levelText; // Reference to the UI text showing the upgrade's current level
    public TMP_Text costText;  // Reference to the UI text showing the current cost to upgrade
    public TMP_Text rateText;  // Reference to the UI text showing production rate (e.g. "$10 / 1s")

    [HideInInspector] public float currentTime; // Internal timer used to track production intervals (hidden in inspector)
}
