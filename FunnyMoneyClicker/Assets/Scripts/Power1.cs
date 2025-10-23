using System.Collections.Generic;
using Unity.Services.Economy.Model;
using UnityEngine;

public class Power1 : MonoBehaviour
{


    public List<PowerData> Powers = new List<PowerData>();

    private void Update()
    {
        for (int i = 0; i < Powers.Count; i++)
        {
            HandlePowers(Powers[i], i);
        }
    }

    private void HandlePowers(PowerData power, int index)
    {
        int level = SaveDataController.currentData.powerLevels[index];

        float cost = GetPowerCost(power, level);
        float production = GetProduction(power, level);

        // Auto income
        if (level > 0)
        {
            ClickerManager.instance.moneyValue = level + 1;
            power.currentTime += Time.deltaTime;
            if (power.currentTime >= power.baseInterval)
            {
                power.currentTime = 0f;
                //SaveDataController.currentData.moneyCount += production;
            }
        }

        // Update UI
        if (power.levelText != null)
            power.levelText.text = $"Level {level}";

        if (power.costText != null)
            power.costText.text = $"${NumberFormatter.Format(cost)}";

        //if (power.rateText != null)
        //{
        //    if (level > 0)
        //        power.rateText.text = $"Rate: +{NumberFormatter.Format(production)} per {power.baseInterval}s";
        //    else
        //        power.rateText.text = "";
        //}
    }

    public void BuyUpgrade(int index)
    {
        index -= 1;

        if (index < 0 || index >= Powers.Count) return;

        var power = Powers[index];
        int level = SaveDataController.currentData.powerLevels[index];
        float cost = GetPowerCost(power, level);

        if (SaveDataController.currentData.moneyCount >= cost)
        {
            SaveDataController.currentData.moneyCount -= cost;
            SaveDataController.currentData.powerLevels[index]++;
        }
    }

    private float GetPowerCost(PowerData power, int level)
    {
        return Mathf.Round(power.baseCost * Mathf.Pow(power.costIncreaseRate, level));
    }

    private float GetProduction(PowerData power, int level)
    {

        float production = power.baseProduction * Mathf.Pow(power.productionIncreaseRate, level);
        int milestoneBonus = level / 25;
        production *= Mathf.Pow(2f, milestoneBonus);
        return production;
    }
}
