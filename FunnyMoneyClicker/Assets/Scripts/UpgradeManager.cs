using UnityEngine;
using TMPro;
using System.Collections.Generic;
using BreakInfinity;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    public List<UpgradeData> upgrades = new List<UpgradeData>();

    [Header("UI")]
    public TMP_Text totalRateText;
    public BigDouble moneyPerSecond;

    private const double costIncreaseRate = 1.2; // 1.45f
    private const double productionIncreaseRate = 1.175; // 1.07f

    public float baseInterval = 1f;

    [Header("Hold To Buy Settings")]
    private bool isHolding = false;
    private int heldUpgradeIndex = -1;
    private float holdTimer = 0f;
    [SerializeField] private float baseRepeatDelay = 0.25f;
    [SerializeField] private float minRepeatDelay = 0.05f;
    [SerializeField] private float accelerationRate = 0.925f;

    private float currentDelay;

    public void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        BigDouble totalRate = 0f;

        for (int i = 0; i < upgrades.Count; i++)
        {
            totalRate += HandleUpgrade(upgrades[i], i);
        }

        if (totalRateText != null)
        {
            moneyPerSecond = totalRate;
            totalRateText.text = $"+${NumberFormatter.Format(moneyPerSecond)} / {NumberFormatter.Format(baseInterval)}s";
        }

        // --- Hold-to-buy logic ---
        if (isHolding && heldUpgradeIndex >= 0)
        {
            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                var upgrade = upgrades[heldUpgradeIndex];
                int level = SaveDataController.currentData.upgradeLevels[heldUpgradeIndex];
                BigDouble cost = GetUpgradeCost(upgrade, level);

                if (SaveDataController.currentData.moneyCount >= cost)
                {
                    BuyUpgrade(heldUpgradeIndex + 1);
                    holdTimer = currentDelay;
                    currentDelay = Mathf.Max(minRepeatDelay, currentDelay * accelerationRate);
                }
                else
                {
                    isHolding = false;
                    heldUpgradeIndex = -1;
                }
            }
        }
    }

    public void OnUpgradeButtonDown(int index)
    {
        heldUpgradeIndex = index - 1;
        isHolding = true;
        currentDelay = baseRepeatDelay;
        holdTimer = baseRepeatDelay;
        BuyUpgrade(index);
    }

    public void OnUpgradeButtonUp()
    {
        isHolding = false;
        heldUpgradeIndex = -1;
    }

    private BigDouble HandleUpgrade(UpgradeData upgrade, int index)
    {
        if (upgrade.costIncreaseRate != costIncreaseRate) upgrade.costIncreaseRate = (float)costIncreaseRate;
        if (upgrade.productionIncreaseRate != productionIncreaseRate) upgrade.productionIncreaseRate = (float)productionIncreaseRate;
        if (upgrade.baseInterval != baseInterval) upgrade.baseInterval = baseInterval;

        int level = SaveDataController.currentData.upgradeLevels[index];
        BigDouble cost = GetUpgradeCost(upgrade, level);
        BigDouble production = GetProduction(upgrade, level);

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
        BigDouble cost = GetUpgradeCost(upgrade, level);

        if (SaveDataController.currentData.moneyCount >= cost)
        {
            SaveDataController.currentData.moneyCount -= cost;
            SaveDataController.currentData.upgradeLevels[index]++;
        }
    }

    private BigDouble GetUpgradeCost(UpgradeData upgrade, int level)
    {
        BigDouble cost = upgrade.baseCost * System.Math.Pow(upgrade.costIncreaseRate, level);


        if (BigDouble.IsInfinity(cost) || BigDouble.IsNaN(cost))
            cost = double.MaxValue;

        return cost;
    }

    private BigDouble GetProduction(UpgradeData upgrade, int level)
    {
        BigDouble production = upgrade.baseProduction * System.Math.Pow(upgrade.productionIncreaseRate, level);
        int milestoneBonus = level / 25;
        production *= System.Math.Pow(2.0, milestoneBonus);


        if (BigDouble.IsInfinity(production) || BigDouble.IsNaN(production))
            production = double.MaxValue;

        return production;
    }
}
