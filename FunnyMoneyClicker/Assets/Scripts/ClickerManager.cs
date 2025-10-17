using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ClickerManager : MonoBehaviour
{
    public static ClickerManager instance;

    public int money;
    public TMP_Text moneyText;

    public TMP_Text moneyEffectPrefab;
    public GameObject moneyEffectContainer;
    public List<TMP_Text> moneyEffects = new List<TMP_Text>();

    public Vector2 spawnMin = new Vector2(-0.96f, 0.5f);
    public Vector2 spawnMax = new Vector2(-0.15f, 0.92f);


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
    }

    private void OnDrawGizmos()
    {

        Vector3 min = new Vector3(Mathf.Min(spawnMin.x, spawnMax.x), Mathf.Min(spawnMin.y, spawnMax.y), 0f);
        Vector3 max = new Vector3(Mathf.Max(spawnMin.x, spawnMax.x), Mathf.Max(spawnMin.y, spawnMax.y), 0f);

        Vector3 centerLocal = (min + max) * 0.5f;
        Vector3 sizeLocal = max - min;

        Vector3 centerWorld = transform.TransformPoint(centerLocal);
        Vector3 sizeWorld = transform.TransformVector(sizeLocal);

        Color fill = new Color(1f, 0.8f, 0f, 0.12f);
        Color outline = new Color(1f, 0.8f, 0f, 0.9f);

        Gizmos.color = fill;
        Gizmos.DrawCube(centerWorld, new Vector3(sizeWorld.x, sizeWorld.y, 0.01f));

        Gizmos.color = outline;
        Gizmos.DrawWireCube(centerWorld, new Vector3(sizeWorld.x, sizeWorld.y, 0.01f));
    }
}