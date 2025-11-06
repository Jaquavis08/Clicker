using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager instance;

    public TMP_Text gemText;

    [Header("General Gacha Settings")]
    public bool isGacha;
    public GameObject gacha;
    public GameObject effect;
    public GameObject uiEffectContainer;
    public AudioSource gachaSound;
    public int pullsPerOpen = 1;
    public GameObject tokyodrift;

    public float cooldownTime;
    public float maxtime = 2f;
    private bool rolling = false;
    private Coroutine tokyoRotateCoroutine;

    [Header("Databases & UI")]
    public ClickerDatabase clickerDatabase;
    public TextMeshProUGUI rewardTextUI;

    public float luckBonus;

    [Header("Rarity Chances (weights)")]
    [Range(0f, 100f)] public float commonChance = 50f;
    [Range(0f, 100f)] public float uncommonChance = 25f;
    [Range(0f, 100f)] public float rareChance = 15f;
    [Range(0f, 100f)] public float epicChance = 8f;
    [Range(0f, 100f)] public float legendaryChance = 2f;

    private Dictionary<string, Color> rarityColors = new Dictionary<string, Color>
{
    { "Common", Color.white },
    { "Uncommon", Color.green },
    { "Rare", Color.blue },
    { "Epic", new Color(0.64f, 0.21f, 0.93f) }, // purple
    { "Legendary", new Color(1f, 0.65f, 0f) }   // orange/gold
};

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (gacha != null)
            gacha.SetActive(isGacha);

        if (cooldownTime >= 1)
        {
            gemText.text = "Gems: " + NumberFormatter.Format(SaveDataController.currentData.gems);
        }

        cooldownTime += Time.deltaTime;
    }

    public void TryOpenGacha()
    {
        if (cooldownTime < maxtime || rolling) return;

        List<string> allMessages = new List<string>();

        if (SaveDataController.currentData.gems < 1)
        {
            allMessages.Add("Cost: 1 Gem!");
            rewardTextUI.color = Color.red;
            rewardTextUI.text = string.Join("\n", allMessages);
            return;
        }

        SaveDataController.currentData.gems -= 1;
        gachaSound?.Play();
        rolling = true;

        if (tokyodrift != null)
        {
            if (tokyoRotateCoroutine != null)
                StopCoroutine(tokyoRotateCoroutine);

            tokyoRotateCoroutine = StartCoroutine(RotateTokyoDrift360(tokyodrift.transform, 2f));
        }

        for (int i = 0; i < pullsPerOpen; i++)
        {
            string rarity = RollRarity();
            string message = GrantRandomSkin(rarity);

            Color color;
            if (!rarityColors.TryGetValue(rarity, out color)) color = Color.black;

            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            allMessages.Add($"<color=#{hexColor}>{rarity}: {message}</color>");
        }

        rewardTextUI.text = string.Join("\n", allMessages);

        cooldownTime = 0f;
        rolling = false;
    }

    // ------------------ RARITY LOGIC ------------------ //
    private string RollRarity()
    {
        // Convert luckBonus (0–100) into a multiplier (1.0–2.0)
        float luckFactor = 1f + (luckBonus / 100f);

        // Boost rarer chances proportionally and reduce common
        float adjustedCommon = commonChance / luckFactor;
        float adjustedUncommon = uncommonChance;
        float adjustedRare = rareChance * luckFactor;
        float adjustedEpic = epicChance * luckFactor * 1.2f;
        float adjustedLegendary = legendaryChance * luckFactor * 1.5f;

        // Normalize total
        float total = adjustedCommon + adjustedUncommon + adjustedRare + adjustedEpic + adjustedLegendary;
        float roll = Random.Range(0f, total);
        float acc = 0f;

        print(total);
        print(roll);
        print(luckBonus);
        print(luckFactor);

        if ((acc += adjustedCommon) >= roll) return "Common";
        if ((acc += adjustedUncommon) >= roll) return "Uncommon";
        if ((acc += adjustedRare) >= roll) return "Rare";
        if ((acc += adjustedEpic) >= roll) return "Epic";
        return "Legendary";
    }

    // ------------------ SKIN REWARD LOGIC ------------------ //
    private string GrantRandomSkin(string rarity)
    {
        if (clickerDatabase == null || clickerDatabase.allClickers.Count == 0)
            return "No skins in database!";

        var raritySkins = clickerDatabase.allClickers
            .Where(s => s.rarity.ToString() == rarity)
            .ToList();

        if (raritySkins.Count == 0)
            return $"No {rarity} skins available!";

        var selected = raritySkins[Random.Range(0, raritySkins.Count)];
        string skinId = selected.skinId;

        bool isNew = !SaveDataController.currentData.unlockedSkins.Contains(skinId);
        if (isNew)
        {
            SaveDataController.currentData.unlockedSkins.Add(skinId);
            DisplayEffect();
            Debug.Log($"Unlocked {rarity} skin: {selected.skinName}");
            return $"Unlocked {selected.skinName}!";
        }
        else
        {
            Debug.Log($"Duplicate {rarity} skin: {selected.skinName}");
            return $"Duplicate: {selected.skinName}";
        }
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

    // ------------------ ROTATION ------------------ //
    private IEnumerator RotateTokyoDrift360(Transform target, float duration)
    {
        Quaternion startRotation = target.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = Mathf.Lerp(0f, 360f, t);
            target.rotation = startRotation * Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.rotation = startRotation * Quaternion.Euler(0f, 0f, 360f);
        tokyoRotateCoroutine = null;
    }
}
