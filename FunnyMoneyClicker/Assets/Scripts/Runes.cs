using System;
using System.Collections.Generic;
using UnityEngine;
using BreakInfinity; // <-- Add this line if BigDouble is defined in BreakInfinity

// Simple rune/buff manager.
// - Provides click multipliers
// - Per-upgrade multipliers and extra effective levels
// - Power-up multipliers (string id keyed)
// Designed to be non-invasive: managers call Runes.instance.* when present.
public class Runes : MonoBehaviour
{
    public static Runes instance;

    [Header("Global Multipliers")]
    [Tooltip("Multiplier applied to each click amount.")]
    public double clickMultiplier = 1.0;

    [Tooltip("Global multiplier applied to upgrade production.")]
    public double upgradeMultiplier = 1.0;

    //[Header("Per-upgrade buffs")]
    // Small editor-friendly list; runtime converted to lookup for fast access.
    [Serializable]
    public struct UpgradeBuff
    {
        public int upgradeIndex;      // zero-based index matching UpgradeManager.upgrades
        public double multiplier;     // production multiplier for this upgrade (multiplicative)
        public int extraLevels;       // virtual extra levels to add when computing production
    }

    public List<UpgradeBuff> upgradeBuffs = new List<UpgradeBuff>();

    [Header("PowerUp buffs")]
    [Tooltip("Keyed multipliers for powerup effects (use powerup id or name).")]
    public List<string> powerUpKeys = new List<string>();
    public List<double> powerUpMultipliers = new List<double>();

    // internal fast lookups
    private Dictionary<int, UpgradeBuff> upgradeLookup = new Dictionary<int, UpgradeBuff>();
    private Dictionary<string, double> powerUpLookup = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

    void Awake()
    {
        // singleton: prefer this instance and keep it simple (don't auto-Destroy children)
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        RebuildLookups();
    }

    // Called in editor when inspector values change
    private void OnValidate()
    {
        // Keep lookups in sync while editing
        RebuildLookups();
    }

    // Call when you change upgradeBuffs/powerUp lists at runtime
    public void RebuildLookups()
    {
        upgradeLookup.Clear();
        for (int i = 0; i < upgradeBuffs.Count; i++)
        {
            var ub = upgradeBuffs[i];
            // validate upgradeIndex (ignore invalid negative indexes)
            if (ub.upgradeIndex < 0) continue;
            upgradeLookup[ub.upgradeIndex] = ub;
        }

        powerUpLookup.Clear();
        int count = Math.Min(powerUpKeys.Count, powerUpMultipliers.Count);
        for (int i = 0; i < count; i++)
        {
            var key = powerUpKeys[i];
            if (string.IsNullOrWhiteSpace(key)) continue;
            powerUpLookup[key.Trim()] = powerUpMultipliers[i];
        }
    }

    // --- Click API ---
    // Apply runes to a click amount (BigDouble-friendly usage in managers)
    public BigDouble ApplyClick(BigDouble amount)
    {
        // Use BigDouble constructor / Multiply to avoid relying on cast operators
        return BigDouble.Multiply(amount, new BigDouble(clickMultiplier));
    }

    public double ApplyClick(double amount)
    {
        return amount * clickMultiplier;
    }

    // --- Upgrade API ---
    // Returns the effective level for an upgrade (baseLevel + rune extraLevels)
    public int GetEffectiveUpgradeLevel(int upgradeIndex, int baseLevel)
    {
        if (upgradeIndex < 0) return baseLevel;
        if (upgradeLookup.TryGetValue(upgradeIndex, out var ub))
            return baseLevel + ub.extraLevels;
        return baseLevel;
    }

    // Apply production multiplier for an upgrade
    public BigDouble ApplyUpgradeProduction(BigDouble production, int upgradeIndex)
    {
        double mul = upgradeMultiplier;
        if (upgradeIndex >= 0 && upgradeLookup.TryGetValue(upgradeIndex, out var ub))
            mul *= ub.multiplier; // allow multiplier < 1.0 (but negative multipliers are clamped)
        if (mul < 0.0) mul = 0.0;
        return BigDouble.Multiply(production, new BigDouble(mul));
    }

    public double ApplyUpgradeProduction(double production, int upgradeIndex)
    {
        double mul = upgradeMultiplier;
        if (upgradeIndex >= 0 && upgradeLookup.TryGetValue(upgradeIndex, out var ub))
            mul *= ub.multiplier;
        if (mul < 0.0) mul = 0.0;
        return production * mul;
    }

    // --- PowerUp API ---
    // Apply a named powerup multiplier if present
    public BigDouble ApplyPowerUpValue(BigDouble value, string powerUpId)
    {
        if (string.IsNullOrWhiteSpace(powerUpId)) return value;
        if (powerUpLookup.TryGetValue(powerUpId.Trim(), out var mul))
            return BigDouble.Multiply(value, new BigDouble(mul));
        return value;
    }

    public double ApplyPowerUpValue(double value, string powerUpId)
    {
        if (string.IsNullOrWhiteSpace(powerUpId)) return value;
        if (powerUpLookup.TryGetValue(powerUpId.Trim(), out var mul))
            return value * mul;
        return value;
    }

    // Helper methods to change runes at runtime
    public void SetClickMultiplier(double m) { clickMultiplier = m; }
    public void SetUpgradeMultiplier(double m) { upgradeMultiplier = m; }

    public void SetUpgradeBuff(int upgradeIndex, double multiplier, int extraLevels = 0)
    {
        if (upgradeIndex < 0) return;
        var ub = new UpgradeBuff { upgradeIndex = upgradeIndex, multiplier = multiplier, extraLevels = extraLevels };
        upgradeLookup[upgradeIndex] = ub;

        // keep list in sync for editor visibility (best-effort)
        int idx = upgradeBuffs.FindIndex(x => x.upgradeIndex == upgradeIndex);
        if (idx >= 0) upgradeBuffs[idx] = ub;
        else upgradeBuffs.Add(ub);
    }

    public void SetPowerUpMultiplier(string key, double multiplier)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        key = key.Trim();
        if (powerUpLookup.ContainsKey(key)) powerUpLookup[key] = multiplier;
        else powerUpLookup.Add(key, multiplier);

        int idx = powerUpKeys.IndexOf(key);
        if (idx >= 0 && idx < powerUpMultipliers.Count) powerUpMultipliers[idx] = multiplier;
        else
        {
            powerUpKeys.Add(key);
            powerUpMultipliers.Add(multiplier);
        }
    }
}
