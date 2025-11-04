using System.Collections.Generic;
using UnityEngine;

public class Power1 : MonoBehaviour
{
    public static Power1 instance;

    public List<PowerData> Powers = new List<PowerData>();

    private const float baseInterval = 0.1f;

    // --- Hold to Buy Settings ---
    private bool isHolding = false;
    private int heldPowerIndex = -1;
    private float holdTimer = 0f;
    [SerializeField] private float baseRepeatDelay = 0.25f;
    [SerializeField] private float minRepeatDelay = 0.05f;
    [SerializeField] private float accelerationRate = 0.925f;

    private float currentDelay;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        for (int i = 0; i < Powers.Count; i++)
        {
            HandlePowers(Powers[i], i);
        }

        // --- Hold-to-buy logic ---
        if (isHolding && heldPowerIndex >= 0)
        {
            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                var upgrade = Powers[heldPowerIndex];
                int level = SaveDataController.currentData.powerLevels[heldPowerIndex];
                double cost = GetPowerCost(upgrade, level);

                if (SaveDataController.currentData.moneyCount >= cost)
                {
                    BuyUpgrade(heldPowerIndex + 1); // +1 because BuyUpgrade subtracts 1 internally
                    holdTimer = currentDelay; // Wait before next buy
                    currentDelay = Mathf.Max(minRepeatDelay, currentDelay * accelerationRate);
                }
                else
                {
                    // Not enough money — stop auto-buy
                    isHolding = false;
                    heldPowerIndex = -1;
                }
            }
        }
    }

    // --- Called from button events ---
    public void OnPowerButtonDown(int index)
    {
        print(index);
        // Safety checks before using index
        if (index <= 0 || index > Powers.Count)
        {
            Debug.LogWarning($"Invalid power index: {index}");
            return;
        }
        print(index);

        heldPowerIndex = index - 1;
        isHolding = true;
        currentDelay = baseRepeatDelay;
        holdTimer = baseRepeatDelay; // start countdown, prevents instant 2nd buy
        BuyUpgrade(index); // Single instant click
    }

    public void OnPowerButtonUp()
    {
        isHolding = false;
        heldPowerIndex = -1;
    }

    private void HandlePowers(PowerData power, int index)
    {
        if (power.baseInterval != baseInterval)
            power.baseInterval = baseInterval;

        int level = SaveDataController.currentData.powerLevels[index];

        double cost = GetPowerCost(power, level);
        double production = GetProduction(power, level);

        if (level > 0)
        {
            power.currentTime += Time.deltaTime;
            if (power.currentTime >= power.baseInterval)
            {
                switch (index)
                {
                    case 0: // Power 1
                        ClickerManager.instance.moneyValue = (float)production;
                        break;
                    case 1: // Power 2
                        GoldCoinSpawner.instance.EnableSpawner(true);
                        double goldCoinChance = power.baseProduction + (level * power.productionIncreaseRate);
                        GoldCoinSpawner.instance.spawnChance = (float)goldCoinChance;
                        break;
                    case 2: // Power 3
                        if (GachaManager.instance != null)
                            GachaManager.instance.isGacha = true;
                        double luckBoost = power.baseProduction + (level * power.productionIncreaseRate);
                        GachaManager.instance.luckBonus = (float)luckBoost;
                        break;
                    case 3: // Power 4
                        double critChance = power.baseProduction + (level * power.productionIncreaseRate);
                        ClickerManager.instance.critChance = (float)critChance;
                        break;
                    case 4: // Power 5
                        double newTime = power.baseProduction - (level * power.productionIncreaseRate);
                        UpgradeManager.instance.baseInterval = (float)newTime;
                        break;
                    case 5: // Power 6
                        double offlineIncome = power.baseProduction + (level * power.productionIncreaseRate);
                        SaveDataController.currentData.offlineEarningsMultiplier = (float)offlineIncome;
                        break;
                    case 6: // Power 7
                        // Add custom power logic here
                        break;
                }

                power.currentTime = 0f;
            }
        }

        // Update UI
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

        // Handle max level
        if (power.levelText != null && power.maxLevel > 0 && level >= power.maxLevel)
        {
            power.levelText.text = "MAX LEVEL";
            if (power.costText != null)
                power.costText.text = "";
        }
    }

    private string GetPowerDescription(int index, double production)
    {
        switch (index)
        {
            case 0: return $"${NumberFormatter.Format(production)} Per click";
            case 1: return $"Spawn chance: {GoldCoinSpawner.instance.spawnChance * 100:F1}%";
            case 2: return $"Luck Boost: {(GachaManager.instance.luckBonus - 1f) * 100f:F1}%";
            case 3: return $"Crit chance: {ClickerManager.instance.critChance * 100:F1}%";
            case 4: return $"Upgrade Rate: {UpgradeManager.instance.baseInterval:F2}/s";
            case 5: return $"Offline Income: {SaveDataController.currentData.offlineEarningsMultiplier * 100:F1}%";
            default: return "";
        }
    }

    public void BuyUpgrade(int index)
    {
        index -= 1;
        if (index < 0 || index >= Powers.Count) return;

        var power = Powers[index];
        int level = SaveDataController.currentData.powerLevels[index];

        if (power.maxLevel > 0 && level >= power.maxLevel)
        {
            Debug.Log($"{power.upgradeName} is already at max level!");
            return;
        }

        double cost = GetPowerCost(power, level);

        if (SaveDataController.currentData.moneyCount >= cost)
        {
            SaveDataController.currentData.moneyCount -= cost;
            SaveDataController.currentData.powerLevels[index]++;

            if (power.maxLevel > 0 && SaveDataController.currentData.powerLevels[index] > power.maxLevel)
            {
                SaveDataController.currentData.powerLevels[index] = power.maxLevel;
            }
        }
    }

    private double GetPowerCost(PowerData power, int level)
    {
        double cost = power.baseCost * System.Math.Pow(power.costIncreaseRate, level);

        // Prevent overflow / NaN
        if (double.IsInfinity(cost) || double.IsNaN(cost))
            cost = double.MaxValue;

        return System.Math.Round(cost, 2);
    }

    private double GetProduction(PowerData power, int level)
    {
        double production = power.baseProduction * System.Math.Pow(power.productionIncreaseRate, level);
        int milestoneBonus = level / 25;
        production *= System.Math.Pow(2.0, milestoneBonus);

        if (double.IsInfinity(production) || double.IsNaN(production))
            production = double.MaxValue;

        return System.Math.Round(production, 2);
    }
}


