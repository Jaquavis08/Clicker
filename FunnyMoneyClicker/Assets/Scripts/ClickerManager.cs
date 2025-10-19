using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClickerManager : MonoBehaviour
{
    public static ClickerManager instance;

    public int money;
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
        if (money.ToString() != moneyText.ToString())
        {
            moneyText.text = "$" + money.ToString();
        }
    }

    public void Click()
    {
        int moneyValue = 1;
        money += moneyValue;

        MoneyEffect(moneyValue);
    }

    public void MoneyEffect(int moneyValue)
    {
        if (moneyEffectPrefab == null) return;

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
        StartCoroutine(FloatUpAndFade(moneyE, 400f, 1.5f));

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

            // Move upward
            rect.anchoredPosition = Vector3.Lerp(startPos, endPos, t);

            // Fade out
            float alpha = Mathf.Lerp(1f, 0f, t);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        moneyEffects.Remove(text);
        Destroy(text.gameObject);
    }



#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (moneyEffectContainer == null)
            return;

        // Get the container's RectTransform (where text spawns)
        RectTransform rect = moneyEffectContainer.GetComponent<RectTransform>();
        if (rect == null)
            return;

        // Calculate center and size in local UI coordinates
        Vector2 spawnCenter = (spawnMin + spawnMax) * 0.5f;
        Vector2 spawnSize = new Vector2(Mathf.Abs(spawnMax.x - spawnMin.x), Mathf.Abs(spawnMax.y - spawnMin.y));

        // Convert local UI position to world position for Gizmos
        Vector3 worldCenter = rect.TransformPoint(spawnCenter);
        Vector3 worldSize = rect.TransformVector(spawnSize);

        // Colors
        Color fill = new Color(1f, 0.8f, 0f, 0.1f);
        Color outline = new Color(1f, 0.8f, 0f, 0.9f);

        // Draw box in the Scene view
        Gizmos.color = fill;
        Gizmos.DrawCube(worldCenter, new Vector3(worldSize.x, worldSize.y, 0.01f));

        Gizmos.color = outline;
        Gizmos.DrawWireCube(worldCenter, new Vector3(worldSize.x, worldSize.y, 0.01f));
    }
#endif
}