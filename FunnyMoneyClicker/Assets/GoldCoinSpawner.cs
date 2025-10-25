using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GoldCoinSpawner : MonoBehaviour
{
    public static GoldCoinSpawner instance;

    [Header("References")]
    public GameObject goldenCoinPrefab;
    public RectTransform spawnArea;

    [Header("Settings")]
    [Range(0f, 1f)] public float spawnChance = 0.05f; // 5% chance each check
    public float checkInterval = 1f;                  // how often to roll chance
    public float moveSpeed = 250f;
    public float coinLifetime = 6f;
    public float bonusAmount = 1000f;

    private bool canSpawn = false;
    private bool coinActive = false;
    private GameObject currentCoin;
    private Coroutine spawnRoutine;

    private void Awake()
    {
        instance = this;
    }

    public void EnableSpawner(bool state)
    {
        canSpawn = state;

        if (!state)
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
            return;
        }

        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (canSpawn)
        {
            yield return new WaitForSeconds(checkInterval);

            // Only try to spawn if no coin exists
            if (!coinActive && Random.value <= spawnChance)
            {
                SpawnCoin();
            }
        }

        spawnRoutine = null;
    }

    private void SpawnCoin()
    {
        if (coinActive || !canSpawn)
            return;

        coinActive = true;

        if (currentCoin != null)
        {
            Destroy(currentCoin);
            currentCoin = null;
        }

        currentCoin = Instantiate(goldenCoinPrefab, spawnArea);
        currentCoin.SetActive(true);

        RectTransform rt = currentCoin.GetComponent<RectTransform>();
        rt.anchoredPosition = GetSafeRandomPosition(rt);

        Button btn = currentCoin.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnCoinClicked(currentCoin));

        StartCoroutine(MoveCoinSmoothly(rt));
        StartCoroutine(CoinLifetime(currentCoin));
    }

    private IEnumerator MoveCoinSmoothly(RectTransform rt)
    {
        Vector2 startPos = rt.anchoredPosition;

        while (rt != null && coinActive)
        {
            Vector2 targetPos = GetSafeRandomPosition(rt);
            float journey = 0f;
            float duration = Random.Range(1.5f, 3f);

            while (journey < duration)
            {
                if (rt == null || !coinActive)
                    yield break;

                journey += Time.deltaTime;
                rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, journey / duration));
                yield return null;
            }

            startPos = targetPos;
        }
    }

    private Vector2 GetSafeRandomPosition(RectTransform coin)
    {
        Vector2 coinSize = coin.sizeDelta * 0.5f;
        Vector2 areaSize = spawnArea.rect.size;

        float x = Random.Range(-areaSize.x / 2 + coinSize.x, areaSize.x / 2 - coinSize.x);
        float y = Random.Range(-areaSize.y / 2 + coinSize.y, areaSize.y / 2 - coinSize.y);

        return new Vector2(x, y);
    }

    private IEnumerator CoinLifetime(GameObject coin)
    {
        yield return new WaitForSeconds(coinLifetime);

        if (coin != null)
            Destroy(coin);

        coinActive = false;
        currentCoin = null;
    }

    private void OnCoinClicked(GameObject coin)
    {
        bonusAmount = (float)SaveDataController.currentData.moneyCount * 0.3f; // 30%
        SaveDataController.currentData.moneyCount += bonusAmount;
        Debug.Log($"🪙 Golden Coin clicked! +${bonusAmount}");

        if (coin != null)
            Destroy(coin);

        coinActive = false;
        currentCoin = null;
    }
}
