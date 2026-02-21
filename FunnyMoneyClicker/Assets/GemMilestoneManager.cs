using BreakInfinity;
using System;
using UnityEngine;

public class GemMilestoneManager : MonoBehaviour
{
    public static GemMilestoneManager instance;

    [Header("Gem Reward Settings")]
    public double gemsPerTier = 1;

    private int lastTierIndex = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (SaveDataController.currentData != null)
            lastTierIndex = SaveDataController.currentData.lastTierIndex;
    }

    public void CheckGemReward(BigDouble moneyCount)
    {
        if (SaveDataController.currentData == null)
            return;

        int currentTier = GetTierIndex(moneyCount);

        while (currentTier > lastTierIndex)
        {
            lastTierIndex++;

            // Base gem reward
            double gemsToGive = gemsPerTier;

            // Apply rune boost (example: 0.10 = +10%)
            double runeBoost = RuneInventoryManager.Instance?.RuneGemsBoost ?? 0f;
            gemsToGive *= (1 + runeBoost);

            SaveDataController.currentData.gems += gemsToGive;
            SaveDataController.currentData.lastTierIndex = lastTierIndex;

            Debug.Log($"Reached {GetSuffix(lastTierIndex)} tier! Gained {gemsToGive} gems (Boost: {runeBoost * 100:F0}%)");
        }
    }

    private int GetTierIndex(BigDouble number)
    {
        if (number < 100d) return 0;

        int tier = 0;
        BigDouble threshold = 100d;

        while (number >= threshold)
        {
            tier++;
            threshold *= 100d;
        }

        return Math.Min(tier, GetSuffixCount() - 1);
    }

    private string GetSuffix(int index)
    {
        var field = typeof(NumberFormatter)
            .GetField("suffixes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        string[] suffixes = (string[])field.GetValue(null);
        return index < suffixes.Length ? suffixes[index] : "??";
    }

    private int GetSuffixCount()
    {
        var field = typeof(NumberFormatter)
            .GetField("suffixes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        string[] suffixes = (string[])field.GetValue(null);
        return suffixes.Length;
    }
}