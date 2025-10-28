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

    private const float costIncreaseRate = 1.145f;
    private const float productionIncreaseRate = 1.07f;

    public float baseInterval = 1;

    public void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        float totalRate = 0f;

        for (int i = 0; i < upgrades.Count; i++)
        {
            totalRate += HandleUpgrade(upgrades[i], i);
        }

        if (totalRateText != null)
        {
            moneyPerSecond = totalRate;
            totalRateText.text = $"+${NumberFormatter.Format(moneyPerSecond)} / " + baseInterval + " Seconds";
        }
    }

    private float HandleUpgrade(UpgradeData upgrade, int index)
    {
        if (upgrade.costIncreaseRate != costIncreaseRate) upgrade.costIncreaseRate = costIncreaseRate;
        if (upgrade.productionIncreaseRate != productionIncreaseRate) upgrade.productionIncreaseRate = productionIncreaseRate;
        if (upgrade.baseInterval != baseInterval) upgrade.baseInterval = baseInterval;

        int level = SaveDataController.currentData.upgradeLevels[index];

        float cost = GetUpgradeCost(upgrade, level);
        float production = GetProduction(upgrade, level);

        if (level > 0)
        {
            upgrade.currentTime += Time.deltaTime;
            if (upgrade.currentTime >= upgrade.baseInterval)
            {
                upgrade.currentTime = 0f;
                SaveDataController.currentData.moneyCount += production;
            }
        }

        if (upgrade.levelText != null)
            upgrade.levelText.text = $"Level {level}";

        if (upgrade.costText != null)
            upgrade.costText.text = $"${NumberFormatter.Format(cost)}";

        if (upgrade.rateText != null)
        {
            if (level > 0)
            {
                upgrade.rateText.text = $"Rate: +{NumberFormatter.Format(production)} per {upgrade.baseInterval}s";
            }
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
        float cost = GetUpgradeCost(upgrade, level);

        if (SaveDataController.currentData.moneyCount >= cost)
        {
            SaveDataController.currentData.moneyCount -= cost;
            SaveDataController.currentData.upgradeLevels[index]++;
        }
    }

    private float GetUpgradeCost(UpgradeData upgrade, int level)
    {
        return Mathf.Round(upgrade.baseCost * Mathf.Pow(upgrade.costIncreaseRate, level));
    }

    private float GetProduction(UpgradeData upgrade, int level)
    {
        float production = upgrade.baseProduction * Mathf.Pow(upgrade.productionIncreaseRate, level);
        int milestoneBonus = level / 25;
        production *= Mathf.Pow(2f, milestoneBonus);
        return production;
    }
}
