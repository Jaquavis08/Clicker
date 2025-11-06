using BreakInfinity;
using System;
using UnityEngine;

public class GemMilestoneManager : MonoBehaviour
{
    public static GemMilestoneManager instance;

    [Header("Gem Reward Settings")]
    public int gemsPerTier = 1;

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
        int currentTier = GetTierIndex(moneyCount);

        // Force at least one gem per new suffix reached
        while (currentTier > lastTierIndex)
        {
            lastTierIndex++;
            SaveDataController.currentData.gems += gemsPerTier;
            SaveDataController.currentData.lastTierIndex = lastTierIndex;

            Debug.Log(NumberFormatterSuffix(lastTierIndex) + gemsPerTier);
        }
    }

    private int GetTierIndex(BigDouble number)
    {
        if (number < 100d) return 0;

        int tier = 0;
        double threshold = 100d;

        while (number >= threshold)
        {
            tier++;
            threshold *= 100d;
        }

        return Math.Min(tier, NumberFormatterSuffixCount() - 1);
    }

    private string NumberFormatterSuffix(int index)
    {
        var field = typeof(NumberFormatter).GetField("suffixes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        string[] suffixes = (string[])field.GetValue(null);
        return index < suffixes.Length ? suffixes[index] : "??";
    }

    private int NumberFormatterSuffixCount()
    {
        var field = typeof(NumberFormatter).GetField("suffixes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        string[] suffixes = (string[])field.GetValue(null);
        return suffixes.Length;
    }
}
