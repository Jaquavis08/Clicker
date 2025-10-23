//using UnityEngine;
//using TMPro;

//public class PowerUps : MonoBehaviour
//{
//    [Header("Base Settings")]
//    public float baseCost = 10f;
//    public float baseProduction = 1f;
//    public float costIncreaseRate = 1.15f; // each level increases cost by +15%
//    public float productionIncreaseRate = 1.05f; // small boost per level
//    public float baseInterval = 1f;

//    [Header("UI References")]
//    public TMP_Text levelText;
//    public TMP_Text costText;
//    public TMP_Text rateText;

//    private float currentTime = 0f;

//    private void Update()
//    {
//        int level = SaveDataController.currentData.upgrade1Level;

//        float cost = GetUpgradeCost(level);
//        float production = GetProduction(level);

//        // Auto money generation
//        if (level > 0)
//        {
//            currentTime += Time.deltaTime;
//            if (currentTime >= baseInterval)
//            {
//                currentTime = 0f;
//                SaveDataController.currentData.moneyCount += production;
//            }
//        }

//        // UI
//        levelText.text = $"Level {level}";
//        costText.text = $"${NumberFormatter.Format(cost)}";
//        rateText.text = $"Rate: {NumberFormatter.Format(production)}/{baseInterval}s";
//    }

//    public void ActivatePowerUp1()
//    {
//        int level = SaveDataController.currentData.upgrade1Level;
//        float cost = GetUpgradeCost(level);

//        if (SaveDataController.currentData.moneyCount >= cost)
//        {
//            SaveDataController.currentData.moneyCount -= cost;
//            SaveDataController.currentData.upgrade1Level++;
//        }
//    }

//    // --- Math Functions ---

//    private float GetUpgradeCost(int level)
//    {
//        return Mathf.Round(baseCost * Mathf.Pow(costIncreaseRate, level));
//    }

//    private float GetProduction(int level)
//    {
//        // Base production grows slightly faster per level
//        // with milestone multipliers every 25 levels
//        float production = baseProduction * Mathf.Pow(productionIncreaseRate, level);

//        int milestoneBonus = level / 25;
//        production *= Mathf.Pow(2f, milestoneBonus); // doubles every 25 levels

//        return production;
//    }
//}
