using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.Services.Economy.Model;
using UnityEngine;


public class Power1 : MonoBehaviour
{
    public List<PowerData> Powers = new List<PowerData>();

    //private const float costIncreaseRate = 1.225f;
    //private const float productionIncreaseRate = 1.175f;
    private const float baseInterval = 0.1f;
    public GameObject jumpscareHeHe;
    public float jumpscareTime;
    public float jumpscareMax = 10f;
    public AudioSource Jumpsound;


    private void Update()
    {
        for (int i = 0; i < Powers.Count; i++)
        {
            HandlePowers(Powers[i], i);
        }
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
                switch (index)
                {
                    case 0: // Upgrade 1
                        ClickerManager.instance.moneyValue = production;
                        break;
                    case 1: // Upgrade 2
                        GoldCoinSpawner.instance.EnableSpawner(true);
                        float goldCoinChance = power.baseProduction + (level * power.productionIncreaseRate);
                        GoldCoinSpawner.instance.spawnChance = goldCoinChance;
                        break;
                    case 2: // Upgrade 3
                        if (GachaManager.instance != null)
                            GachaManager.instance.isGacha = true;
                        float luckBoost = power.baseProduction + (level * power.productionIncreaseRate);
                        GachaManager.instance.luckMultiplier = luckBoost;
                        break;
                    case 3: // Upgrade 4
                        float critChance = power.baseProduction + (level * power.productionIncreaseRate);
                        ClickerManager.instance.critChance = critChance;
                        break;
                    case 4: // Upgrade 5
                        float newTime = power.baseProduction - (level * power.productionIncreaseRate);
                        UpgradeManager.instance.baseInterval = newTime;
                        print(newTime);
                        break;
                    case 5: // Upgrade 6
                        float offlineIncome = power.baseProduction + (level * power.productionIncreaseRate);
                        SaveDataController.currentData.offlineEarningsMultiplier = offlineIncome;
                        break;
                    case 6: // Upgrade 7
                        if (jumpscareTime >= jumpscareMax)
                        {
                            jumpscareHeHe.SetActive(true);
                            Jumpsound.Play();
                            jumpscareTime = 0f;
                            jumpscareMax = Random.Range(10f, 30f);
                        }
                        else
                        {
                            jumpscareHeHe.SetActive(false);
                            jumpscareTime += Time.deltaTime;
                        }

                        break;
                }

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
            case 0: return $"${NumberFormatter.Format(production)} Per click";
            case 1: return $"Spawn chance: {GoldCoinSpawner.instance.spawnChance * 100:F1}%";
            case 2: return $"Luck Boost: {(GachaManager.instance.luckMultiplier - 1f) * 100f:F1}%";
            case 3: return $"Crit chance: {ClickerManager.instance.critChance * 100:F1}%";
            case 4: return $"Upgrade Rate: {UpgradeManager.instance.baseInterval:F2}/s";
            case 5: return $"Offline Income: {SaveDataController.currentData.offlineEarningsMultiplier * 100:F1}%"; ;
            case 6: return null;
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
