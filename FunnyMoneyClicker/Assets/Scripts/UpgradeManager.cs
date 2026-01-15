using UnityEngine;
using TMPro;
using System.Collections.Generic;
using BreakInfinity;
using System.Reflection;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;

    public List<UpgradeData> upgrades = new List<UpgradeData>();

    [Header("UI")]
    public TMP_Text totalRateText;
    public BigDouble moneyPerSecond;

    private const double costIncreaseRate = 1.7; // default fallback
    private const double productionIncreaseRate = 1.55; // default fallback

    // Tunables for the production balancing curve (adjust to taste)
    private const double productionLevelExponent = 0.72;   // mild polynomial smoothing based on level
    private const double productionCostBias = -0.035;     // slight negative bias vs baseCost to reduce advantage of expensive upgrades

    // Enforce monotonic efficiency using index-based targets so leveling one upgrade
    // doesn't change the target for other upgrades. This prevents leveling upgrade 1
    // from causing production of later upgrades to drop.
    private const double minEfficiencyFactor = 1.10; // per-index multiplicative efficiency target

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
        BigDouble totalRate = BigDouble.Zero;

        // Compute a stable baseline efficiency using inspector base values on upgrade0.
        // This baseline is stable and will not change when the player's levels change.
        BigDouble baselineEfficiency = BigDouble.Zero;
        if (upgrades.Count > 0)
        {
            var u0 = upgrades[0];
            if (u0.baseCost > 0.0)
                baselineEfficiency = (BigDouble)u0.baseProduction / (BigDouble)u0.baseCost;
        }

        // If baseline is zero (defensive), set to small positive value so targets are reasonable.
        if (baselineEfficiency <= BigDouble.Zero)
            baselineEfficiency = (BigDouble)1e-9;

        for (int i = 0; i < upgrades.Count; i++)
        {
            var upgrade = upgrades[i];
            int level = SaveDataController.currentData.upgradeLevels[i];

            // Respect inspector values (use defaults only when not set)
            if (upgrade.costIncreaseRate <= 0f) upgrade.costIncreaseRate = (float)costIncreaseRate;
            if (upgrade.productionIncreaseRate <= 0f) upgrade.productionIncreaseRate = (float)productionIncreaseRate;
            if (upgrade.baseInterval <= 0f) upgrade.baseInterval = baseInterval;

            BigDouble cost = GetUpgradeCost(upgrade, level);
            BigDouble production = GetProduction(upgrade, level);

            // Calculate raw efficiency (production per unit cost). Guard divide-by-zero.
            BigDouble efficiency = (cost > BigDouble.Zero) ? (production / cost) : production;

            // Compute an index-based target efficiency so targets are deterministic and
            // won't change when player levels other upgrades.
            BigDouble indexFactor = BigDouble.Pow((BigDouble)minEfficiencyFactor, i);
            BigDouble targetEfficiency = baselineEfficiency * indexFactor;

            // If current efficiency is below deterministic target, boost production to match it.
            // This keeps higher-tier upgrades at or above a predictable efficiency relative to upgrade 0.
            if (efficiency < targetEfficiency && cost > BigDouble.Zero)
            {
                production = targetEfficiency * cost;
                efficiency = targetEfficiency;
            }

            // Use the computed cost/production when updating UI / awarding income.
            totalRate += HandleUpgrade(upgrade, i, cost, production);
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

    // Now accepts precomputed cost & production so we can enforce monotonicity centrally.
    private BigDouble HandleUpgrade(UpgradeData upgrade, int index, BigDouble cost, BigDouble production)
    {
        int level = SaveDataController.currentData.upgradeLevels[index];

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

        return (level > 0) ? (production / upgrade.baseInterval) : BigDouble.Zero;
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

    //// Cost to buy the next level given the current level value passed in (level 0 => first buy = baseCost)
    //private BigDouble GetUpgradeCost(UpgradeData upgrade, int level)
    //{
    //    BigDouble baseCost = (BigDouble)upgrade.baseCost;
    //    BigDouble rate = (BigDouble)upgrade.costIncreaseRate;

    //    // Use level as exponent for consistent, predictable progression.
    //    BigDouble cost = baseCost * BigDouble.Pow(rate, level);

    //    if (BigDouble.IsInfinity(cost) || BigDouble.IsNaN(cost))
    //        cost = double.MaxValue;

    //    return cost;
    //}


    private BigDouble GetUpgradeCost(UpgradeData upgrade, int level)
    {
        double baseCost = upgrade.baseCost; // 15
        double step = 2.5;

        double cost = baseCost + level * step;
        return new BigDouble(System.Math.Ceiling(cost));
    }






    // Production at current level — balanced smoothing applied:
    // - exponential base growth using productionIncreaseRate
    // - mild polynomial smoothing on level (reduces sharp jumps at high levels)
    // - small negative bias vs baseCost to reduce the advantage of extremely expensive upgrades
    //private BigDouble GetProduction(UpgradeData upgrade, int level)
    //{
    //    if (level <= 0) return BigDouble.Zero;

    //    BigDouble baseProd = (BigDouble)upgrade.baseProduction;
    //    BigDouble rate = (BigDouble)upgrade.productionIncreaseRate;

    //    // core exponential growth
    //    BigDouble production = baseProd * BigDouble.Pow(rate, level);

    //    // mild polynomial smoothing based on level (makes increases feel more fluid)
    //    production *= BigDouble.Pow((BigDouble)(1.0 + (double)level), productionLevelExponent);

    //    // slight negative bias relative to baseCost so expensive upgrades don't automatically dominate.
    //    if (upgrade.baseCost > 0.0)
    //    {
    //        production *= BigDouble.Pow((BigDouble)upgrade.baseCost, productionCostBias);
    //    }

    //    // existing milestone bonus (double every 25 levels)
    //    int milestoneBonus = level / 25;
    //    if (milestoneBonus > 0)
    //        production *= BigDouble.Pow(2.0, milestoneBonus);

    //    // apply achievement multiplier (>= 1.0)
    //    double achMultiplier = GetAchievementMultiplier();
    //    production *= (BigDouble)achMultiplier;

    //    if (BigDouble.IsInfinity(production) || BigDouble.IsNaN(production))
    //        production = double.MaxValue;

    //    return production;
    //}


    private BigDouble GetProduction(UpgradeData upgrade, int level)
    {
        if (level <= 0) return BigDouble.Zero;

        BigDouble production = (BigDouble)upgrade.baseProduction * level;

        int milestones = level / 25;
        if (milestones > 0)
        {
            production *= (1.0 + 0.25 * milestones);
        }

        return production;
    }






    // Safe lookup for player achievement data. Uses reflection so it won't break if the
    // specific achievement field isn't present in currentData. Returns >= 1.0.
    private double GetAchievementMultiplier()
    {
        var data = SaveDataController.currentData;
        if (data == null) return 1.0;

        System.Type t = data.GetType();

        // candidates for property/field names
        string[] names = new[] { "achievementMultiplier", "achievementPoints", "achievementCount", "achievements", "achievementsUnlocked", "achievementLevel" };

        object raw = null;
        foreach (var n in names)
        {
            var prop = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
            {
                raw = prop.GetValue(data);
                break;
            }

            var field = t.GetField(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                raw = field.GetValue(data);
                break;
            }
        }

        if (raw == null) return 1.0;

        try
        {
            if (raw is double d)
            {
                if (d >= 1.0) return d;
                return 1.0 + d;
            }
            if (raw is float f)
            {
                if (f >= 1.0f) return f;
                return 1.0 + f;
            }
            if (raw is int i)
            {
                return 1.0 + (i * 0.02);
            }
            if (raw is long l)
            {
                return 1.0 + (l * 0.02);
            }
            var col = raw as System.Collections.ICollection;
            if (col != null)
            {
                return 1.0 + (col.Count * 0.03);
            }
        }
        catch
        {
        }

        return 1.0;
    }
}
