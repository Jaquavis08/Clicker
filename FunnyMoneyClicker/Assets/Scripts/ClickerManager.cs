using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClickerManager : MonoBehaviour
{
    public static ClickerManager instance;

    public ClickerItem clickerItem;
    public GameObject clickerParticleSystem;
    public GameObject canvas;

    public TMP_Text moneyText;

    public TMP_Text moneyEffectPrefab;
    public GameObject moneyEffectContainer;
    public List<TMP_Text> moneyEffects = new List<TMP_Text>();

    public Vector2 spawnMin = new Vector2(-155f, -37f);
    public Vector2 spawnMax = new Vector2(166f, 112f);


    public void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (SaveDataController.currentData.moneyCount.ToString() != moneyText.ToString())
        {
            moneyText.text = "$" + SaveDataController.currentData.moneyCount.ToString();
        }
    }

    public void Click()
    {
        int moneyValue = 1;
        SaveDataController.currentData.moneyCount += moneyValue;

        MoneyEffect(moneyValue);
        ClickingParticle();
    }

    public void MoneyEffect(int moneyValue)
    {
        if (moneyEffectPrefab == null)
        {
            return;
        }
        float minX = Mathf.Min(spawnMin.x, spawnMax.x);
        float maxX = Mathf.Max(spawnMin.x, spawnMax.x);
        float minY = Mathf.Min(spawnMin.y, spawnMax.y);
        float maxY = Mathf.Max(spawnMin.y, spawnMax.y);

        float randX1 = Random.Range(minX, maxX);
        float randY1 = Random.Range(minY, maxY);

        Vector3 spawnPos = new Vector3(randX1, randY1, 0f);
        TMP_Text moneyE = Instantiate(moneyEffectPrefab, spawnPos, Quaternion.identity, transform);
        moneyE.transform.SetParent(moneyEffectContainer.transform, false);
        moneyE.text = "$" + moneyValue;

        if (moneyEffects == null) moneyEffects = new List<TMP_Text>();
        moneyEffects.Add(moneyE);
        StartCoroutine(ShakeClicker());
        StartCoroutine(FloatUpAndFade(moneyE, 400f, 1.5f));

    }

    private IEnumerator ShakeClicker()
    {
        Transform target = moneyEffectContainer.transform;
        Quaternion originalRotation = target.rotation;

        // Pick a random rotation angle (never zero)
        float randomZ = Random.Range(-10f, 10f);
        if (Mathf.Abs(randomZ) < 2f) // avoid being too close to 0
            randomZ = Mathf.Sign(randomZ) * 5f;

        // Rotate instantly to the random tilt
        target.rotation = Quaternion.Euler(0f, 0f, randomZ);

        // Hold that tilt briefly
        yield return new WaitForSeconds(0.1f);

        // Smoothly settle to a *different* small final rotation (not 0)
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

        // Ensure it stops at that small angle (never 0)
        target.rotation = endRot;
    }

    private IEnumerator FloatUpAndFade(TMP_Text text, float floatDistance = 50f, float duration = 1.5f)
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
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 1f)
        );

        GameObject clickingParticleSystemInstance = Instantiate(clickerParticleSystem, worldPos, Quaternion.identity, canvas.transform);

        ParticleSystem ps = clickingParticleSystemInstance.GetComponent<ParticleSystem>();
        if (ps != null && ps.GetComponent<Renderer>().material != clickerItem.ClciekrMaterial)
        {
            ps.GetComponent<Renderer>().material = clickerItem.ClciekrMaterial;
        }

        clickingParticleSystemInstance.SetActive(true);
        ps.Play();
        Destroy(clickingParticleSystemInstance, 0.75f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (moneyEffectContainer == null)
            return;

        RectTransform rect = moneyEffectContainer.GetComponent<RectTransform>();
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