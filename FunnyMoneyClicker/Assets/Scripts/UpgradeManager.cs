using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    public List<UpgradeData> upgrades = new List<UpgradeData>();

    [Header("UI")]
    public TMP_Text totalRateText;
    public double moneyPerSecond;

    private const double costIncreaseRate = 1.145;
    private const double productionIncreaseRate = 1.07;

    public float baseInterval = 1f;

    public void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        double totalRate = 0f;

        for (int i = 0; i < upgrades.Count; i++)
        {
            totalRate += HandleUpgrade(upgrades[i], i);
        }

        if (totalRateText != null)
        {
            moneyPerSecond = totalRate;
            totalRateText.text = $"+${NumberFormatter.Format(moneyPerSecond)} / {NumberFormatter.Format(baseInterval)}s";
        }
    }

    private double HandleUpgrade(UpgradeData upgrade, int index)
    {
        // Sync constants
        if (upgrade.costIncreaseRate != costIncreaseRate) upgrade.costIncreaseRate = (float)costIncreaseRate;
        if (upgrade.productionIncreaseRate != productionIncreaseRate) upgrade.productionIncreaseRate = (float)productionIncreaseRate;
        if (upgrade.baseInterval != baseInterval) upgrade.baseInterval = baseInterval;

        int level = SaveDataController.currentData.upgradeLevels[index];

        double cost = GetUpgradeCost(upgrade, level);
        double production = GetProduction(upgrade, level);

        // Passive income tick
        if (level > 0)
        {
            upgrade.currentTime += Time.deltaTime;
            if (upgrade.currentTime >= upgrade.baseInterval)
            {
                upgrade.currentTime = 0f;
                SaveDataController.currentData.moneyCount += production;
            }
        }

        // UI Updates
        if (upgrade.levelText != null)
            upgrade.levelText.text = $"Level {level}";

        if (upgrade.costText != null)
            upgrade.costText.text = $"${NumberFormatter.Format(cost)}";

        if (upgrade.rateText != null)
        {
            if (level > 0)
                upgrade.rateText.text = $"Rate: ${NumberFormatter.Format(production)} per {NumberFormatter.Format(upgrade.baseInterval)}s";
            else
                upgrade.rateText.text = "";
        }

        return (level > 0) ? (production / upgrade.baseInterval) : 0f;
    }

    public void BuyUpgrade(int index)
    {
        index -= 1;
        if (index < 0 || index >= upgrades.Count) return;

        var upgrade = upgrades[index];
        int level = SaveDataController.currentData.upgradeLevels[index];
        double cost = GetUpgradeCost(upgrade, level);

        if (SaveDataController.currentData.moneyCount >= cost)
        {
            SaveDataController.currentData.moneyCount -= cost;
            SaveDataController.currentData.upgradeLevels[index]++;
        }
    }

    private double GetUpgradeCost(UpgradeData upgrade, int level)
    {
        // Use double math for large numbers
        double cost = upgrade.baseCost * System.Math.Pow(upgrade.costIncreaseRate, level);

        // Protect from overflow
        if (double.IsInfinity(cost) || double.IsNaN(cost))
            cost = double.MaxValue;

        return System.Math.Round(cost, 2);
    }

    private double GetProduction(UpgradeData upgrade, int level)
    {
        double production = upgrade.baseProduction * System.Math.Pow(upgrade.productionIncreaseRate, level);
        int milestoneBonus = level / 25;
        production *= System.Math.Pow(2.0, milestoneBonus);

        // Prevent overflow
        if (double.IsInfinity(production) || double.IsNaN(production))
            production = double.MaxValue;

        return System.Math.Round(production, 2);
    }
}
