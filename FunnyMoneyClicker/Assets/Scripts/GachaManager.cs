using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager instance;

    [Header("General Gacha Settings")]
    public bool isGacha;
    public GameObject gacha;
    public GameObject effect;
    public GameObject uiEffectContainer;
    public float luckMultiplier = 1f;
    public bool rolling = false;
    public float cooldownTime;
    public float maxtime = 2f;
    public AudioSource gachaSound;
    public int pullsPerOpen = 1;
    public GameObject tokyodrift;
    public float value;

    [Header("Databases & UI")]
    public ClickerDatabase clickerDatabase; // your skin/clicker database
    public TextMeshProUGUI rewardTextUI;    // optional reward text display

    [Header("💰 Money Rewards")]
    public List<MoneyReward> moneyRewards = new List<MoneyReward>();

    [Header("🎨 Skin / Clicker Rewards")]
    public List<SkinReward> skinRewards = new List<SkinReward>();

    // ------------------ DATA CLASSES ------------------ //
    [System.Serializable]
    public class MoneyReward
    {
        public string id = "Cash";
        public int goldAmount = 100;
        [Range(0f, 100f)] public float chance = 10f;
    }

    [System.Serializable]
    public class SkinReward
    {
        public string id = "Random Skin";
        [Range(0f, 100f)] public float chance = 10f;
    }

    // ------------------ UNITY METHODS ------------------ //
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (gacha != null)
            gacha.SetActive(isGacha);

        cooldownTime += Time.deltaTime;
    }

    // ------------------ MAIN GACHA FLOW ------------------ //
    public void TryOpenGacha()
    {
        
            if (cooldownTime < maxtime || rolling) return;

        if (SaveDataController.currentData.moneyCount < 1000)
        {
            Debug.Log("❌ Not enough money for gacha pull.");
            return;
        }
        value = Mathf.Repeat(value + Time.deltaTime * 500f, 360f);
        tokyodrift.transform.rotation = Quaternion.Euler(0, 0, value);

        //if (value >= 359f)
        //{
        //    value = 0f;
        //}

        SaveDataController.currentData.moneyCount -= 1000;
        gachaSound?.Play();
        rolling = true;

        List<string> allMessages = new List<string>();

        for (int i = 0; i < pullsPerOpen; i++)
        {
            // Decide randomly: money or skin reward (50/50 chance, adjust if needed)
            bool getSkin = Random.value < 0.5f;

            if (getSkin)
            {
                var skin = PullSkinReward();
                allMessages.Add(GrantSkinReward(skin, false));
            }
            else
            {
                var money = PullMoneyReward();
                allMessages.Add(GrantMoneyReward(money, false));
            }
        }

        // Update UI once
        if (rewardTextUI != null)
            rewardTextUI.text = string.Join("\n", allMessages);

        cooldownTime = 0f;
        rolling = false;
    }

    // ------------------ MONEY REWARDS ------------------ //
    private MoneyReward PullMoneyReward()
    {
        if (moneyRewards == null || moneyRewards.Count == 0) return null;

        float totalChance = 0f;
        foreach (var r in moneyRewards)
            totalChance += r.chance * luckMultiplier;

        if (totalChance <= 0f)
            return null; // nothing to pull

        float roll = Random.Range(0f, totalChance);
        float acc = 0f;

        foreach (var r in moneyRewards)
        {
            acc += r.chance * luckMultiplier;
            if (roll <= acc)
                return r;
        }


        // fallback in case of rounding errors
        return moneyRewards[Random.Range(0, moneyRewards.Count)];
    }

    private string GrantMoneyReward(MoneyReward reward, bool playEffect = true)
    {
        if (reward == null) return "";

        SaveDataController.currentData.moneyCount += reward.goldAmount;
        ClickerManager.instance?.MoneyEffect(reward.goldAmount);

        string message = $"💰 You won ${reward.goldAmount}!";

        if (playEffect) DisplayEffect();

        Debug.Log(message);
        return message;
    }

    // ------------------ SKIN REWARDS ------------------ //
    private SkinReward PullSkinReward()
    {
        if (skinRewards == null || skinRewards.Count == 0) return null;

        // sum only items with chance > 0
        float totalChance = 0f;
        foreach (var r in skinRewards)
            if (r.chance > 0f)
                totalChance += r.chance * luckMultiplier;

        if (totalChance <= 0f) return null;

        float roll = Random.Range(0f, totalChance);
        float acc = 0f;

        foreach (var r in skinRewards)
        {
            if (r.chance <= 0f) continue; // skip 0% chance
            acc += r.chance * luckMultiplier;
            if (roll <= acc)
                return r;
        }

        return skinRewards[Random.Range(0, skinRewards.Count)];
    }

    private string GrantSkinReward(SkinReward reward, bool playEffect = true)
    {
        if (reward == null) return "";

        string message = "🎲 No skin selected.";

        var allSkins = clickerDatabase?.allClickers;
        if (allSkins != null && allSkins.Count > 0)
        {
            // Pick a random skin from the database (no chance on ClickerItem)
            ClickerItem selectedSkin = allSkins[Random.Range(0, allSkins.Count)];

            if (!SaveDataController.currentData.unlockedSkins.Contains(selectedSkin.skinId))
            {
                SaveDataController.currentData.unlockedSkins.Add(selectedSkin.skinId);
                message = $"🎉 NEW SKIN UNLOCKED: {selectedSkin.skinName}!";
            }
            else
            {
                message = $"⭐ Duplicate skin: {selectedSkin.skinName}";
            }
        }

        if (playEffect) DisplayEffect();
        Debug.Log(message);
        return message;
    }

    // ------------------ VISUAL EFFECT ------------------ //
    private void DisplayEffect()
    {
        if (effect != null && uiEffectContainer != null)
        {
            var effectSystem = Instantiate(effect, uiEffectContainer.transform);
            var ps = effectSystem.GetComponent<ParticleSystem>();
            ps?.Play();
        }
    }
}
