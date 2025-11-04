using UnityEngine;

public class GemMilestoneManager : MonoBehaviour
{
    public static GemMilestoneManager instance;

    [Header("Gem Reward Settings")]
    public int gemsPerTier = 1; // how many gems per new suffix tier

    private int lastTierIndex = 0; // tracks the previous suffix tier index

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Load saved tier progress if it exists
        lastTierIndex = SaveDataController.currentData.lastTierIndex;
    }

    public void CheckGemReward(double moneyCount)
    {
        lastTierIndex = SaveDataController.currentData.lastTierIndex;

        int currentTier = GetTierIndex(moneyCount);

        // only reward if the player reached a new, higher tier
        if (currentTier > lastTierIndex)
        {
            int gainedTiers = currentTier - lastTierIndex;
            int totalGems = gainedTiers * gemsPerTier;

            SaveDataController.currentData.gems += totalGems;
            lastTierIndex = currentTier;
            SaveDataController.currentData.lastTierIndex = currentTier;

            // save changes immediately
            //SaveDataController.Save(); // <-- make sure your SaveDataController has a Save() method

            Debug.Log($"💜 Reached new money tier ({NumberFormatterSuffix(currentTier)})! +{totalGems} 💎 Gems");
        }
    }

    private int GetTierIndex(double number)
    {
        if (number < 1000) return 0;
        double log10 = Mathf.Log10((float)number);
        int magnitude = Mathf.FloorToInt((float)(log10 / 3.0));
        return Mathf.Clamp(magnitude, 0, NumberFormatterSuffixCount() - 1);
    }

    private string NumberFormatterSuffix(int index)
    {
        // Access the suffix array from your NumberFormatter
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
