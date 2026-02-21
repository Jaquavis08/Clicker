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
    public GameObject GachaObject;

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
        { "Common", Color.gray },
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

        if (isGacha == false)
        {
            GachaObject.SetActive(false);
        }

        if (cooldownTime >= 1)
        {
            gemText.text = "Gems: " + NumberFormatter.Format(SaveDataController.currentData.gems);
        }

        cooldownTime += Time.deltaTime;
    }

    public void TryOpenGacha()
    {
        if (cooldownTime < maxtime || rolling) return;

        // perform the gacha opening as a coroutine so we can animate and stop on specific rarities
        StartCoroutine(HandleGachaOpen());
    }

    // New coroutine to handle the visual rotation + reward granting per pull.
    private IEnumerator HandleGachaOpen()
    {
        rolling = true;

        List<string> allMessages = new List<string>();

        if (SaveDataController.currentData.gems < 1)
        {
            allMessages.Add("Cost: 1 Gem!");
            rewardTextUI.color = Color.red;
            rewardTextUI.text = string.Join("\n", allMessages);
            rolling = false;
            yield break;
        }

        SaveDataController.currentData.gems -= 1;
        gachaSound?.Play();

        for (int i = 0; i < pullsPerOpen; i++)
        {
            // Determine rarity first so we can stop the wheel on that sector
            string rarity = RollRarity();

            // Compute target sector angle for this rarity
            float sectorAngle = GetAngleForRarity(rarity);

            // If a wheel exists, rotate and wait until rotation completes
            if (tokyodrift != null)
            {
                // stop any existing rotation coroutine
                if (tokyoRotateCoroutine != null)
                    StopCoroutine(tokyoRotateCoroutine);

                // Start rotation that ends on the sectorAngle and wait for it to finish
                tokyoRotateCoroutine = StartCoroutine(RotateTokyoDrift360(tokyodrift.transform, 2f, sectorAngle));
                yield return tokyoRotateCoroutine;
            }

            // Grant reward after the wheel stops (or immediately if no wheel)
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
        float totalLuck = luckBonus + ((float)RuneInventoryManager.Instance?.RuneLuckBoost);

        // Convert luck to multiplier
        float luckFactor = 1f + (totalLuck / 100f);

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

    // Helper: map rarity name to a wheel sector angle (0-360).
    // Adjust these angles to match your wheel art. This example assumes five equal sectors:
    // Common=0, Uncommon=72, Rare=144, Epic=216, Legendary=288.
    private float GetAngleForRarity(string rarity)
    {
        switch (rarity)
        {
            case "Common": return Random.Range(287.262f, 418.683f);
            case "Uncommon": return Random.Range(60.673f, 162.559f);
            case "Rare": return Random.Range(165.253f, 219.135f);
            case "Epic": return Random.Range(221.662f, 269.402f);
            case "Legendary": return Random.Range(271.293f, 285.111f);
            default: return Random.Range(0f, 360f);
        }
    }

    // ------------------ ROTATION ------------------ //
    // Modified to accept a target sector angle (0-360). The coroutine will spin multiple full turns
    // then land exactly on the sectorAngle.
    private IEnumerator RotateTokyoDrift360(Transform target, float duration, float targetSectorAngle, Vector3 axis = default, AnimationCurve ease = null)
    {
        if (target == null)
            yield break;

        if (axis == default)
            axis = Vector3.forward; // rotate around Z by default

        if (duration <= 0f)
        {
            // instant single rotation fallback; rotate and then align to target sector if provided
            Vector3 e = target.rotation.eulerAngles;
            float finalZz = (e.z + targetSectorAngle) % 360f;
            target.rotation = Quaternion.Euler(e.x, e.y, finalZz);
            tokyoRotateCoroutine = null;
            yield break;
        }

        Quaternion startRotation = target.rotation;
        float startZ = startRotation.eulerAngles.z;
        float startX = startRotation.eulerAngles.x;
        float startY = startRotation.eulerAngles.y;

        // compute positive delta from current Z to desired sector angle (0..360)
        float currentModulo = startZ % 360f;
        if (currentModulo < 0) currentModulo += 360f;
        float deltaToTarget = (targetSectorAngle - currentModulo + 360f) % 360f;

        // add two full spins (720 deg) so the wheel visibly spins before landing
        float finalZ = startZ + 720f + deltaToTarget;

        float elapsed = 0f;

        try
        {
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = (ease != null) ? ease.Evaluate(t) : Mathf.SmoothStep(0f, 1f, t);
                float z = Mathf.Lerp(startZ, finalZ, easedT);
                target.rotation = Quaternion.Euler(startX, startY, z);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // ensure exact final rotation
            target.rotation = Quaternion.Euler(startX, startY, finalZ);
        }
        finally
        {
            tokyoRotateCoroutine = null;
        }
    }
}