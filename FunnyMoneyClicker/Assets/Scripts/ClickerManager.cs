using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using BreakInfinity;

public class ClickerManager : MonoBehaviour
{
    public static ClickerManager instance;
    public BigDouble moneyValue = 1;
    public ClickerItem clickerItem;
    public GameObject clickerParticleSystem;
    public GameObject canvas;
    public AudioSource clicksound;

    // Shrink effect variables
    private bool isShrinking = false;
    private float shrinkTimer = 0f;
    private float shrinkDuration = 0.1f;
    private Vector3 originalScale;
    private Vector3 targetScale;


    public TMP_Text moneyText;

    public TMP_Text moneyEffectPrefab;
    public GameObject funnyMoney;
    public GameObject moneyEffectContainer;

    public List<TMP_Text> moneyEffects = new List<TMP_Text>();
    private Queue<TMP_Text> moneyEffectPool = new Queue<TMP_Text>();
    private Queue<GameObject> particlePool = new Queue<GameObject>();
    private int moneyEffectPoolSize = 50;
    private int particlePoolSize = 40;

    public Vector2 spawnMin = new Vector2(-155f, -37f);
    public Vector2 spawnMax = new Vector2(166f, 112f);

    private Camera mainCam;

    [Header("Critical Click Settings")]
    [Range(0f, 1f)] public float critChance = 0.05f; // 5% chance
    public float critMultiplier = 2f;
    public Color critTextColor = Color.red;

    //private double displayedMoney = 0;
    private float moneyUpdateTimer;

    // Debug variables for pool tracking
    private int peakMoneyEffects = 0;
    private int peakParticles = 0;
    private float debugTimer = 0f;
    public bool TESTING;

    public void Awake()
    {
        originalScale = funnyMoney.transform.localScale;

        instance = this;
        mainCam = Camera.main;

        // Prepopulate money effect pool
        for (int i = 0; i < moneyEffectPoolSize; i++)
        {
            TMP_Text tmp = Instantiate(moneyEffectPrefab, moneyEffectContainer.transform);
            tmp.gameObject.SetActive(false);
            moneyEffectPool.Enqueue(tmp);
        }

        // Prepopulate particle pool
        for (int i = 0; i < particlePoolSize; i++)
        {
            GameObject ps = Instantiate(clickerParticleSystem, moneyEffectContainer.transform);
            ps.SetActive(false);
            particlePool.Enqueue(ps);
        }
    }

    private void Update()
    {
        if (isShrinking)
        {
            shrinkTimer += Time.deltaTime;
            float halfDuration = shrinkDuration * 0.5f;

            if (shrinkTimer <= halfDuration)
            {
                // Shrink down
                float t = shrinkTimer / halfDuration;
                funnyMoney.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            }
            else if (shrinkTimer <= shrinkDuration)
            {
                // Grow back up
                float t = (shrinkTimer - halfDuration) / halfDuration;
                funnyMoney.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            }
            else
            {
                // Done
                funnyMoney.transform.localScale = originalScale;
                isShrinking = false;
            }
        }




        moneyUpdateTimer += Time.deltaTime;
        if (moneyUpdateTimer >= 0.25f)
        {
            moneyUpdateTimer = 0f;
            BigDouble targetMoney = SaveDataController.currentData.moneyCount;

            if (BigDouble.IsNaN(targetMoney) || BigDouble.IsInfinity(targetMoney) || targetMoney < 0)
            {
                targetMoney = 0;
                SaveDataController.currentData.moneyCount = 0;
            }

            // Smooth but stable interpolation
            //displayedMoney = Mathf.Lerp((float)displayedMoney, (float)targetMoney, 0.25f);
            //moneyText.text = "$" + NumberFormatter.Format(targetMoney);
            moneyText.text = "$" + NumberFormatter.Format(SaveDataController.currentData.moneyCount);

            GemMilestoneManager.instance?.CheckGemReward(SaveDataController.currentData.moneyCount);
        }

        if (TESTING)
        {
            // Debug: every 5 seconds, log peak usage
            debugTimer += Time.deltaTime;
            if (debugTimer >= 5f)
            {
                debugTimer = 0f;
                Debug.Log($"Peak Money Effects Needed: {peakMoneyEffects}");
                Debug.Log($"Peak Particles Needed: {peakParticles}");
            }
        }
    }

    public void Click()
    {
        BigDouble finalValue = moneyValue;
        bool isCrit = Random.value < critChance;
        StartShrinkEffect();

        clicksound.Play();

        if (isCrit)
        {
            finalValue *= critMultiplier;
            MoneyEffect(finalValue, true);
        }
        else
        {
            MoneyEffect(finalValue, false);
        }

        SaveDataController.currentData.moneyCount += finalValue;

        ClickingParticle();
    }

    public void MoneyEffect(BigDouble moneyValue, bool isCrit = false)
    {
        if (moneyEffectPrefab == null)
            return;

        TMP_Text moneyE;

        if (moneyEffectPool.Count > 0)
        {
            moneyE = moneyEffectPool.Dequeue();
            moneyE.gameObject.SetActive(true);
        }
        else
        {
            moneyE = Instantiate(moneyEffectPrefab, moneyEffectContainer.transform);
        }

        // Random position inside bounds
        float randX = Random.Range(spawnMin.x, spawnMax.x);
        float randY = Random.Range(spawnMin.y, spawnMax.y);
        moneyE.rectTransform.anchoredPosition = new Vector3(randX, randY, 0f);

        // Set text & style
        moneyE.color = isCrit ? critTextColor : Color.white;
        moneyE.text = "$" + NumberFormatter.Format(moneyValue);
        moneyE.fontSize = isCrit ? moneyEffectPrefab.fontSize * 1.4f : moneyEffectPrefab.fontSize;

        moneyEffects.Add(moneyE);

        // Track peak money effects
        if (moneyEffects.Count > peakMoneyEffects)
            peakMoneyEffects = moneyEffects.Count;

        StartCoroutine(ShakeClicker());
        StartCoroutine(FloatUpAndFadePooled(moneyE, isCrit ? 500f : 400f, isCrit ? 2f : 2f));
    }

    private void StartShrinkEffect()
    {
        isShrinking = true;
        shrinkTimer = 0f;
        targetScale = originalScale * 0.8f; // shrink to 80%
    }




    private IEnumerator ShakeClicker()
    {
        Transform target = funnyMoney.transform;
        Quaternion originalRotation = target.rotation;

        float randomZ = Random.Range(-10f, 10f);
        if (Mathf.Abs(randomZ) < 2f)
            randomZ = Mathf.Sign(randomZ) * 5f;

        target.rotation = Quaternion.Euler(0f, 0f, randomZ);
        yield return new WaitForSeconds(0.1f);

        float finalZ = Random.Range(-5f, 5f);
        if (Mathf.Abs(finalZ) < 1f)
            finalZ = Mathf.Sign(finalZ) * 2f;

        float resetDuration = 0.15f;
        float elapsed = 0f;

        Quaternion startRot = target.rotation;
        Quaternion endRot = Quaternion.Euler(0f, 0f, finalZ);

        while (elapsed < resetDuration)
        {
            elapsed += Time.deltaTime;
            target.rotation = Quaternion.Slerp(startRot, endRot, elapsed / resetDuration);
            yield return null;
        }

        target.rotation = endRot;
    }

    private IEnumerator FloatUpAndFadePooled(TMP_Text text, float floatDistance, float duration)
    {
        RectTransform rect = text.rectTransform;
        Vector3 startPos = rect.anchoredPosition;
        Vector3 endPos = startPos + new Vector3(0, floatDistance, 0);
        Color originalColor = text.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        moneyEffects.Remove(text);
        text.gameObject.SetActive(false);
        moneyEffectPool.Enqueue(text);
    }

    private void ClickingParticle()
    {
        Vector3 worldPos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 1f));
        GameObject psInstance;

        if (particlePool.Count > 0)
        {
            psInstance = particlePool.Dequeue();
            psInstance.transform.position = worldPos;
            psInstance.SetActive(true);
        }
        else
        {
            psInstance = Instantiate(clickerParticleSystem, worldPos, Quaternion.identity, moneyEffectContainer.transform);
        }

        ParticleSystem ps = psInstance.GetComponent<ParticleSystem>();
        if (ps != null && ps.GetComponent<Renderer>().material != clickerItem.clickerSkin)
            ps.GetComponent<Renderer>().material = clickerItem.clickerMaterial;

        ps.Play();

        // Track peak particles
        int activeParticles = particlePoolSize - particlePool.Count;
        if (activeParticles > peakParticles)
            peakParticles = activeParticles;

        StartCoroutine(ReturnParticleToPool(psInstance, ps.main.duration));
    }

    private IEnumerator ReturnParticleToPool(GameObject psInstance, float delay)
    {
        yield return new WaitForSeconds(delay);
        psInstance.SetActive(false);
        particlePool.Enqueue(psInstance);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (funnyMoney == null) return;

        RectTransform rect = funnyMoney.GetComponent<RectTransform>();
        if (rect == null) return;

        Vector2 spawnCenter = (spawnMin + spawnMax) * 0.5f;
        Vector2 spawnSize = new Vector2(Mathf.Abs(spawnMax.x - spawnMin.x), Mathf.Abs(spawnMax.y - spawnMin.y));

        Vector3 worldCenter = rect.TransformPoint(spawnCenter);
        Vector3 worldSize = rect.TransformVector(spawnSize);

        Color fill = new Color(1f, 0.8f, 0f, 0.1f);
        Color outline = new Color(1f, 0.8f, 0f, 0.9f);

        Gizmos.color = fill;
        Gizmos.DrawCube(worldCenter, new Vector3(worldSize.x, worldSize.y, 0.01f));

        Gizmos.color = outline;
        Gizmos.DrawWireCube(worldCenter, new Vector3(worldSize.x, worldSize.y, 0.01f));
    }
#endif
}
