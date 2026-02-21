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

    private const double costIncreaseRate = 1.2; // default fallback
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
            totalRate += HandleUpgrade(upgrades[i], i);
        }

        moneyPerSecond = totalRate;

        if (totalRateText != null)
        {
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
    private BigDouble HandleUpgrade(UpgradeData upgrade, int index)
    {
        if (upgrade.costIncreaseRate != costIncreaseRate) upgrade.costIncreaseRate = (float)costIncreaseRate;
        if (upgrade.productionIncreaseRate != productionIncreaseRate) upgrade.productionIncreaseRate = (float)productionIncreaseRate;
        if (upgrade.baseInterval != baseInterval) upgrade.baseInterval = baseInterval;

        int level = SaveDataController.currentData.upgradeLevels[index];
        BigDouble cost = GetUpgradeCost(upgrade, level);
        BigDouble production = GetProduction(upgrade, level, index);

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

    //// Cost to buy the next level given the current level value passed in (level 0 => first buy = baseCost)
    //private BigDouble GetUpgradeCost(UpgradeData upgrade, int level)
    //{
    //    BigDouble baseCost = (BigDouble)upgrade.baseCost;
    //    BigDouble rate = (BigDouble)upgrade.costIncreaseRate;
    //
    //    // Use level as exponent for consistent, predictable progression.
    //    BigDouble cost = baseCost * BigDouble.Pow(rate, level);
    //
    //    if (BigDouble.IsInfinity(cost) || BigDouble.IsNaN(cost))
    //        cost = double.MaxValue;
    //
    //    return cost;
    //}


    private BigDouble GetUpgradeCost(UpgradeData upgrade, int level)
    {
        // Base cost and exponential rate
        BigDouble baseCost = (BigDouble)upgrade.baseCost;
        BigDouble rate = (BigDouble)upgrade.costIncreaseRate;

        // Exponential cost scaling
        // level 0 => baseCost
        BigDouble cost = baseCost * BigDouble.Pow(rate, level);

        // Round up to whole currency units
        cost = BigDouble.Ceiling(cost);

        // Safety clamp
        if (BigDouble.IsInfinity(cost) || BigDouble.IsNaN(cost))
            cost = new BigDouble(double.MaxValue);

        return cost;
    }





    // Production at current level — balanced smoothing applied:
    // - exponential base growth using productionIncreaseRate
    // - mild polynomial smoothing on level (reduces sharp jumps at high levels)
    // - small negative bias vs baseCost to reduce the advantage of extremely expensive upgrades
    //private BigDouble GetProduction(UpgradeData upgrade, int level)
    //{
    //    if (level <= 0) return BigDouble.Zero;
    //
    //    BigDouble baseProd = (BigDouble)upgrade.baseProduction;
    //    BigDouble rate = (BigDouble)upgrade.productionIncreaseRate;
    //
    //    // core exponential growth
    //    BigDouble production = baseProd * BigDouble.Pow(rate, level);
    //
    //    // mild polynomial smoothing based on level (makes increases feel more fluid)
    //    production *= BigDouble.Pow((BigDouble)(1.0 + (double)level), productionLevelExponent);
    //
    //    // slight negative bias relative to baseCost so expensive upgrades don't automatically dominate.
    //    if (upgrade.baseCost > 0.0)
    //    {
    //        production *= BigDouble.Pow((BigDouble)upgrade.baseCost, productionCostBias);
    //    }
    //
    //    // existing milestone bonus (double every 25 levels)
    //    int milestoneBonus = level / 25;
    //    if (milestoneBonus > 0)
    //        production *= BigDouble.Pow(2.0, milestoneBonus);
    //
    //    // apply achievement multiplier (>= 1.0)
    //    double achMultiplier = GetAchievementMultiplier();
    //    production *= (BigDouble)achMultiplier;
    //
    //    if (BigDouble.IsInfinity(production) || BigDouble.IsNaN(production))
    //        production = double.MaxValue;
    //
    //    return production;
    //}


    private BigDouble GetProduction(UpgradeData upgrade, int level, int index)
    {
        if (level <= 0) return BigDouble.Zero;

        // Allow runes to provide extra effective levels
        int effectiveLevel = level;

        // Linear production based on effective level
        BigDouble production = (BigDouble)upgrade.baseProduction * effectiveLevel;

        // Milestone bonus every 25 levels
        int milestones = effectiveLevel / 25;
        if (milestones > 0)
        {
            production *= BigDouble.Pow(1.25, milestones);
        }

        // Apply rune per-upgrade / global upgrade multipliers if present
        production *= (1 + RuneInventoryManager.Instance.RuneMoneyBoost);

        return production;
    }

    // Formual : amount per sec / upgrade cost ~=  .../ per sec







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
//csharp Assets\Scripts\ClickerManager.cs
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using BreakInfinity;
//using JetBrains.Annotations;

//public class clicky : MonoBehaviour
//{
//    public static ClickerManager instance;
//    public BigDouble moneyValue = 1;
//    public ClickerItem clickerItem;
//    public GameObject clickerParticleSystem;
//    public GameObject canvas;
//    public AudioSource clicksound;

//    // Shrink effect variables
//    private bool isShrinking = false;
//    private float shrinkTimer = 0f;
//    private float shrinkDuration = 0.1f;
//    private Vector3 originalScale;
//    private Vector3 targetScale;

//    //public AnimationCurve test;

//    public TMP_Text moneyText;

//    public TMP_Text moneyEffectPrefab;
//    public GameObject funnyMoney;
//    public GameObject moneyEffectContainer;

//    public List<TMP_Text> moneyEffects = new List<TMP_Text>();
//    private Queue<TMP_Text> moneyEffectPool = new Queue<TMP_Text>();
//    private Queue<GameObject> particlePool = new Queue<GameObject>();
//    private int moneyEffectPoolSize = 50;
//    private int particlePoolSize = 40;
//    public bool isCrit;

//    public Vector2 spawnMin = new Vector2(-155f, -37f);
//    public Vector2 spawnMax = new Vector2(166f, 112f);

//    private Camera mainCam;

//    [Header("Critical Click Settings")]
//    [Range(0f, 1f)] public float critChance = 0.05f; // 5% chance
//    public float critMultiplier = 2f;
//    public Color critTextColor = Color.red;

//    [Header("Rune Drop Settings")]
//    [Range(0f,1f)] public float runeDropChance = 0.02f; // 2% default chance per click
//    public Color runeTextColor = Color.cyan;

//    //private double displayedMoney = 0;
//    private float moneyUpdateTimer;

//    // Debug variables for pool tracking
//    private int peakMoneyEffects = 0;
//    private int peakParticles = 0;
//    private float debugTimer = 0f;
//    public bool TESTING;

//    public ParticleSystem ParticleSystem1;
//    public ParticleSystem ParticleSystem2;
//    public ParticleSystem ParticleSystem3;
//    public ParticleSystem ParticleSystem4;
//    public ParticleSystem ParticleSystem5;
//    public ParticleSystem ParticleSystem6;
//    public ParticleSystem ParticleSystem7;
//    public ParticleSystem ParticleSystem8;
//    public ParticleSystem ParticleSystem9;
//    public ParticleSystem ParticleSystem10;
//    public ParticleSystem ParticleSystem11;
//    public ParticleSystem ParticleSystem12;
//    public ParticleSystem ParticleSystem13;



//    public void Awake()
//    {
//        originalScale = funnyMoney.transform.localScale;

//        instance = this;
//        mainCam = Camera.main;

//        // Prepopulate money effect pool
//        for (int i = 0; i < moneyEffectPoolSize; i++)
//        {
//            TMP_Text tmp = Instantiate(moneyEffectPrefab, moneyEffectContainer.transform);
//            tmp.gameObject.SetActive(false);
//            moneyEffectPool.Enqueue(tmp);
//        }

//        // Prepopulate particle pool
//        for (int i = 0; i < particlePoolSize; i++)
//        {
//            GameObject ps = Instantiate(clickerParticleSystem, moneyEffectContainer.transform);
//            ps.SetActive(false);
//            particlePool.Enqueue(ps);
//        }
//    }

//    private void Update()
//    {
//        if (isShrinking)
//        {
//            shrinkTimer += Time.deltaTime;
//            float halfDuration = shrinkDuration * 0.5f;

//            if (shrinkTimer <= halfDuration)
//            {
//                // Shrink down
//                float t = shrinkTimer / halfDuration;
//                funnyMoney.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
//            }
//            else if (shrinkTimer <= shrinkDuration)
//            {
//                // Grow back up
//                float t = (shrinkTimer - halfDuration) / halfDuration;
//                funnyMoney.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
//            }
//            else
//            {
//                // Done
//                funnyMoney.transform.localScale = originalScale;
//                isShrinking = false;
//            }
//        }




//        moneyUpdateTimer += Time.deltaTime;
//        if (moneyUpdateTimer >= 0.25f)
//        {
//            moneyUpdateTimer = 0f;
//            BigDouble targetMoney = SaveDataController.currentData.moneyCount;

//            if (BigDouble.IsNaN(targetMoney) || BigDouble.IsInfinity(targetMoney) || targetMoney < 0)
//            {
//                targetMoney = 0;
//                SaveDataController.currentData.moneyCount = 0;
//            }

//            // Smooth but stable interpolation
//            //displayedMoney = Mathf.Lerp((float)displayedMoney, (float)targetMoney, 0.25f);
//            //moneyText.text = "$" + NumberFormatter.Format(targetMoney);
//            moneyText.text = "$" + NumberFormatter.Format(SaveDataController.currentData.moneyCount);

//            GemMilestoneManager.instance?.CheckGemReward(SaveDataController.currentData.moneyCount);
//        }

//        if (TESTING)
//        {
//            // Debug: every 5 seconds, log peak usage
//            debugTimer += Time.deltaTime;
//            if (debugTimer >= 5f)
//            {
//                debugTimer = 0f;
//                Debug.Log($"Peak Money Effects Needed: {peakMoneyEffects}");
//                Debug.Log($"Peak Particles Needed: {peakParticles}");
//            }
//        }
//    }

//    public void Click()
//    {
//        BigDouble finalValue = moneyValue;

//        // Apply runes click multiplier if present
//        if (Runes.instance != null)
//        {
//            finalValue = Runes.instance.ApplyClick(finalValue);
//        }

//        isCrit = Random.value < critChance;
//        StartShrinkEffect();

//        clicksound.Play();

//        if (isCrit)
//        {
//            finalValue *= critMultiplier;
//            MoneyEffect(finalValue, true);

//                ParticleSystem1.Play();
//                ParticleSystem2.Play();
//                ParticleSystem3.Play();
//                ParticleSystem4.Play();
//                ParticleSystem5.Play();
//                ParticleSystem6.Play();
//                ParticleSystem7.Play();
//                ParticleSystem8.Play();
//                ParticleSystem9.Play();
//                ParticleSystem10.Play();
//                ParticleSystem11.Play();
//                ParticleSystem12.Play();
//                ParticleSystem13.Play();

//        }
//        else
//        {
//            MoneyEffect(finalValue, false);
//        }

//        SaveDataController.currentData.moneyCount += finalValue;

//        // Chance to drop a random rune that goes into the player's rune inventory (runtime)
//        if (Random.value < runeDropChance)
//        {
//            var rune = RuneInventory.instance.GenerateRandomRune();
//            RuneInventory.instance.AddRune(rune);
//            ShowRunePickup(rune.displayName);
//            Debug.Log($"Dropped rune: {rune.displayName}");
//        }

//        ClickingParticle();
//    }

//    public void MoneyEffect(BigDouble moneyValue, bool isCrit = false)
//    {
//        if (moneyEffectPrefab == null)
//            return;

//        TMP_Text moneyE;

//        if (moneyEffectPool.Count > 0)
//        {
//            moneyE = moneyEffectPool.Dequeue();
//            moneyE.gameObject.SetActive(true);
//        }
//        else
//        {
//            moneyE = Instantiate(moneyEffectPrefab, moneyEffectContainer.transform);
//        }

//        // Random position inside bounds
//        float randX = Random.Range(spawnMin.x, spawnMax.x);
//        float randY = Random.Range(spawnMin.y, spawnMax.y);
//        moneyE.rectTransform.anchoredPosition = new Vector3(randX, randY, 0f);

//        // Set text & style
//        moneyE.color = isCrit ? critTextColor : Color.white;
//        moneyE.text = "$" + NumberFormatter.Format(moneyValue);
//        moneyE.fontSize = isCrit ? moneyEffectPrefab.fontSize * 1.4f : moneyEffectPrefab.fontSize;

//        moneyEffects.Add(moneyE);

//        // Track peak money effects
//        if (moneyEffects.Count > peakMoneyEffects)
//            peakMoneyEffects = moneyEffects.Count;

//        StartCoroutine(ShakeClicker());
//        StartCoroutine(FloatUpAndFadePooled(moneyE, isCrit ? 500f : 400f, isCrit ? 2f : 2f));
//    }

//    // Reuse the pooled text to show a rune pickup message
//    private void ShowRunePickup(string runeName)
//    {
//        if (moneyEffectPrefab == null) return;

//        TMP_Text txt;
//        if (moneyEffectPool.Count > 0)
//        {
//            txt = moneyEffectPool.Dequeue();
//            txt.gameObject.SetActive(true);
//        }
//        else
//        {
//            txt = Instantiate(moneyEffectPrefab, moneyEffectContainer.transform);
//        }

//        // position near center of spawn area
//        Vector3 pos = (spawnMin + spawnMax) * 0.5f;
//        txt.rectTransform.anchoredPosition = new Vector3(pos.x, pos.y, 0f);

//        txt.color = runeTextColor;
//        txt.text = "Rune: " + runeName;
//        txt.fontSize = moneyEffectPrefab.fontSize;

//        moneyEffects.Add(txt);

//        if (moneyEffects.Count > peakMoneyEffects)
//            peakMoneyEffects = moneyEffects.Count;

//        StartCoroutine(FloatUpAndFadePooled(txt, 450f, 2.0f));
//    }

//    private void StartShrinkEffect()
//    {
//        isShrinking = true;
//        shrinkTimer = 0f;
//        targetScale = originalScale * 0.8f; // shrink to 80%
//    }




//    private IEnumerator ShakeClicker()
//    {
//        Transform target = funnyMoney.transform;
//        Quaternion originalRotation = target.rotation;

//        float randomZ = Random.Range(-10f, 10f);
//        if (Mathf.Abs(randomZ) < 2f)
//            randomZ = Mathf.Sign(randomZ) * 5f;

//        target.rotation = Quaternion.Euler(0f, 0f, randomZ);
//        yield return new WaitForSeconds(0.1f);

//        float finalZ = Random.Range(-5f, 5f);
//        if (Mathf.Abs(finalZ) < 1f)
//            finalZ = Mathf.Sign(finalZ) * 2f;

//        float resetDuration = 0.15f;
//        float elapsed = 0f;

//        Quaternion startRot = target.rotation;
//        Quaternion endRot = Quaternion.Euler(0f, 0f, finalZ);

//        while (elapsed < resetDuration)
//        {
//            elapsed += Time.deltaTime;
//            target.rotation = Quaternion.Slerp(startRot, endRot, elapsed / resetDuration);
//            yield return null;
//        }

//        target.rotation = endRot;
//    }

//    private IEnumerator FloatUpAndFadePooled(TMP_Text text, float floatDistance, float duration)
//    {
//        RectTransform rect = text.rectTransform;
//        Vector3 startPos = rect.anchoredPosition;
//        Vector3 endPos = startPos + new Vector3(0, floatDistance, 0);
//        Color originalColor = text.color;
//        float elapsed = 0f;

//        while (elapsed < duration)
//        {
//            elapsed += Time.deltaTime;
//            float t = elapsed / duration;
//            rect.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
//            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1f, 0f, t));
//            yield return null;
//        }

//        moneyEffects.Remove(text);
//        text.gameObject.SetActive(false);
//        moneyEffectPool.Enqueue(text);
//    }

//    private void ClickingParticle()
//    {
//        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 1f));
//        GameObject psInstance;

//        if (particlePool.Count > 0)
//        {
//            psInstance = particlePool.Dequeue();
//            psInstance.transform.position = worldPos;
//            psInstance.SetActive(true);
//        }
//        else
//        {
//            psInstance = Instantiate(clickerParticleSystem, worldPos, Quaternion.identity, moneyEffectContainer.transform);
//        }

//        ParticleSystem ps = psInstance.GetComponent<ParticleSystem>();
//        if (ps != null && ps.GetComponent<Renderer>().material != clickerItem.clickerSkin)
//            ps.GetComponent<Renderer>().material = clickerItem.clickerMaterial;

//        ps.Play();

//        // Track peak particles
//        int activeParticles = particlePoolSize - particlePool.Count;
//        if (activeParticles > peakParticles)
//            peakParticles = activeParticles;

//        StartCoroutine(ReturnParticleToPool(psInstance, ps.main.duration));
//    }

//    private IEnumerator ReturnParticleToPool(GameObject psInstance, float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        psInstance.SetActive(false);
//        particlePool.Enqueue(psInstance);
//    }

//#if UNITY_EDITOR
//    private void OnDrawGizmos()
//    {
//        if (funnyMoney == null) return;

//        RectTransform rect = funnyMoney.GetComponent<RectTransform>();
//        if (rect == null) return;

//        Vector2 spawnCenter = (spawnMin + spawnMax) * 0.5f;
//        Vector2 spawnSize = new Vector2(Mathf.Abs(spawnMax.x - spawnMin.x), Mathf.Abs(spawnMax.y - spawnMin.y));

//        Vector3 worldCenter = rect.TransformPoint(spawnCenter);
//        Vector3 worldSize = rect.TransformVector(spawnSize);

//        Color fill = new Color(1f, 0.8f, 0f, 0.1f);
//        Color outline = new Color(1f, 0.8f, 0f, 0.9f);

//        Gizmos.color = fill;
//        Gizmos.DrawCube(worldCenter, new Vector3(worldSize.x, worldSize.y, 0.01f));

//        Gizmos.color = outline;
//        Gizmos.DrawWireCube(worldCenter, new Vector3(worldSize.x, worldSize.y, 0.01f));
//    }
//#endif
//}
