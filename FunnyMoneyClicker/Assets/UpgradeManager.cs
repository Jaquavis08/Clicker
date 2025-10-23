using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public List<UpgradeData> upgrades = new List<UpgradeData>();

    private void Update()
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            HandleUpgrade(upgrades[i], i);
        }
    }

    private void HandleUpgrade(UpgradeData upgrade, int index)
    {
        int level = SaveDataController.currentData.upgradeLevels[index];

        float cost = GetUpgradeCost(upgrade, level);
        float production = GetProduction(upgrade, level);

        // Auto income
        if (level > 0)
        {
            upgrade.currentTime += Time.deltaTime;
            if (upgrade.currentTime >= upgrade.baseInterval)
            {
                upgrade.currentTime = 0f;
                SaveDataController.currentData.moneyCount += production;
            }
        }

        // Update UI
        if (upgrade.levelText != null)
            upgrade.levelText.text = $"Level {level}";

        if (upgrade.costText != null)
            upgrade.costText.text = $"${NumberFormatter.Format(cost)}";

        if (upgrade.rateText != null)
        {
            if (level > 0)
                upgrade.rateText.text = $"Rate: +{NumberFormatter.Format(production)} per {upgrade.baseInterval}s";
            else
                upgrade.rateText.text = "";
        }
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
