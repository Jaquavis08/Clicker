using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClickerManager : MonoBehaviour
{
    public static ClickerManager instance;
    public float moneyValue = 1;
    public ClickerItem clickerItem;
    public GameObject clickerParticleSystem;
    public GameObject canvas;
    public AudioSource clicksound;

    public TMP_Text moneyText;

    public TMP_Text moneyEffectPrefab;
    public GameObject funnyMoney;
    public GameObject moneyEffectContainer;
    public List<TMP_Text> moneyEffects = new List<TMP_Text>();

    public Vector2 spawnMin = new Vector2(-155f, -37f);
    public Vector2 spawnMax = new Vector2(166f, 112f);

    private Camera mainCam;

    [Header("Critical Click Settings")]
    [Range(0f, 1f)] public float critChance = 0.05f; // 5% chance
    public float critMultiplier = 2f;
    public Color critTextColor = Color.red;

    private double displayedMoney = 0;
    private float moneyUpdateTimer;

    public void Awake()
    {
        instance = this;
        mainCam = Camera.main;
    }

    private void Update()
    {
        moneyUpdateTimer += Time.deltaTime;
        if (moneyUpdateTimer >= 0.1f) // update 10 times per second
        {
            moneyUpdateTimer = 0f;
            displayedMoney += (SaveDataController.currentData.moneyCount - displayedMoney) * Time.deltaTime * 4.0;
            displayedMoney = SaveDataController.currentData.moneyCount;
            moneyText.text = "$" + NumberFormatter.Format(displayedMoney);
        }
    }

    public void Click()
    {
        float finalValue = moneyValue;
        bool isCrit = Random.value < critChance;
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

    public void MoneyEffect(float moneyValue, bool isCrit = false)
    {
        if (moneyEffectPrefab == null)
            return;

        float minX = Mathf.Min(spawnMin.x, spawnMax.x);
        float maxX = Mathf.Max(spawnMin.x, spawnMax.x);
        float minY = Mathf.Min(spawnMin.y, spawnMax.y);
        float maxY = Mathf.Max(spawnMin.y, spawnMax.y);

        float randX1 = Random.Range(minX, maxX);
        float randY1 = Random.Range(minY, maxY);

        Vector3 spawnPos = new Vector3(randX1, randY1, 0f);
        TMP_Text moneyE = Instantiate(moneyEffectPrefab, spawnPos, Quaternion.identity, transform);
        moneyE.transform.SetParent(moneyEffectContainer.transform, false);

        if (isCrit)
        {
            //moneyE.text = $"<b>CRIT! ${NumberFormatter.Format(moneyValue)}</b>";
            moneyE.text = "$" + NumberFormatter.Format(moneyValue);
            moneyE.color = critTextColor;
            moneyE.fontSize *= 1.4f;
        }
        else
        {
            moneyE.text = "$" + NumberFormatter.Format(moneyValue);
        }

        if (moneyEffects == null)
            moneyEffects = new List<TMP_Text>();

        moneyEffects.Add(moneyE);
        StartCoroutine(ShakeClicker());
        StartCoroutine(FloatUpAndFade(moneyE, isCrit ? 500f : 400f, isCrit ? 2f : 2f));
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

    private IEnumerator FloatUpAndFade(TMP_Text text, float floatDistance = 50f, float duration = 2f)
    {
        RectTransform rect = text.GetComponent<RectTransform>();
        Vector3 startPos = rect.anchoredPosition;
        Vector3 endPos = startPos + new Vector3(0, floatDistance, 0);
        Color originalColor = text.color;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rect.anchoredPosition = Vector3.Lerp(startPos, endPos, t);

            float alpha = Mathf.Lerp(1f, 0f, t);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        moneyEffects.Remove(text);
        Destroy(text.gameObject);
    }

    private void ClickingParticle()
    {
        Vector3 worldPos = mainCam.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 1f)
        );

        GameObject clickingParticleSystemInstance = Instantiate(clickerParticleSystem, worldPos, Quaternion.identity, moneyEffectContainer.transform);

        ParticleSystem ps = clickingParticleSystemInstance.GetComponent<ParticleSystem>();
        if (ps != null && ps.GetComponent<Renderer>().material != clickerItem.clickerSkin)
        {
            ps.GetComponent<Renderer>().material = clickerItem.clickerMaterial;
        }

        clickingParticleSystemInstance.SetActive(true);
        ps.Play();
        //ps.Emit(1000);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (funnyMoney == null)
            return;

        RectTransform rect = funnyMoney.GetComponent<RectTransform>();
        if (rect == null)
            return;

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