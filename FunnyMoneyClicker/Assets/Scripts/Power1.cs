using System.Collections.Generic;
using Unity.Services.Economy.Model;
using UnityEditor.PackageManager;
using UnityEngine;

public class Power1 : MonoBehaviour
{


    public List<PowerData> Powers = new List<PowerData>();

    //private const float costIncreaseRate = 1.225f;
    //private const float productionIncreaseRate = 1.175f;
    private const float baseInterval = 0.1f;
    public GameObject goldCoin;
    public float timeForGoldCoin;
    public float goldCoinTimer = 0f;


    private void Update()
    {
        for (int i = 0; i < Powers.Count; i++)
        {
            HandlePowers(Powers[i], i);
        }
        goldCoinTimer += Time.deltaTime;
        
    }

    private void HandlePowers(PowerData power, int index)
    {
        //if (power.costIncreaseRate != costIncreaseRate) power.costIncreaseRate = costIncreaseRate;
        //if (power.productionIncreaseRate != productionIncreaseRate) power.productionIncreaseRate = productionIncreaseRate;
        if (power.baseInterval != baseInterval) power.baseInterval = baseInterval;

        int level = SaveDataController.currentData.powerLevels[index];
        
        float cost = GetPowerCost(power, level);
        float production = GetProduction(power, level);

        if (level > 0)
        {
            power.currentTime += Time.deltaTime;
            if (power.currentTime >= power.baseInterval)
            {
                print(index);
                switch (index)
                {
                    case 0: // Upgrade 1
                        ClickerManager.instance.moneyValue = production;
                        break;
                    case 1: // Upgrade 2
                        timeForGoldCoin = 5f - (level * 0.1f);
                        if (timeForGoldCoin < goldCoinTimer )
                        { Instantiate(goldCoin, ClickerManager.instance.transform.position, Quaternion.identity);
                            goldCoinTimer = 0f;
                        }
                        break;
                    case 2: // Upgrade 3

                        break;
                    case 3: // Upgrade 4
                        ClickerManager.instance.critChance = production;
                        break;
                    case 4: // Upgrade 5

                        break;
                    case 5: // Upgrade 6

                        break;
                    case 6: // Upgrade 7

                        break;
                }

                //if (index == 0) // Upgrade 1
                //{
                //    ClickerManager.instance.moneyValue = production;
                //}
                //else if (index == 1) // Upgrade 2
                //{
                    
                //}
                //else if (index == 2)// Upgrade 3
                //{

                //}
                //else if (index == 3)// Upgrade 4
                //{
                //    ClickerManager.instance.critChance = production;
                //}
                //else if (index == 4)// Upgrade 5
                //{

                //}
                //else if (index == 5)// Upgrade 6
                //{

                //}
                //else if (index == 6)// Upgrade 7
                //{

                //}

                power.currentTime = 0f;
            }
        }

        if (power.levelText != null)
            power.levelText.text = $"Level {level}";

        if (power.costText != null)
            power.costText.text = $"${NumberFormatter.Format(cost)}";

        if (power.rateText != null)
        {
            if (level > 0)
                power.rateText.text = GetPowerDescription(index, production);
            else
                power.rateText.text = "";
        }
    }

    private string GetPowerDescription(int index, float production)
    {
        switch (index)
        {
            case 0: return $"+${NumberFormatter.Format(production)} per click";
            case 3: return $"🎯 Crit chance: {production * 10:F1}%";
            default: return "";
        }
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
