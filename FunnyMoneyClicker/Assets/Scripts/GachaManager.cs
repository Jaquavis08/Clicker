using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GachaManager;

public class GachaManager : MonoBehaviour
{
    public static GachaManager instance;

    public bool isGacha;
    public GameObject gacha;
    public GameObject effect;
    public GameObject uiEffectContainer;
    public float luckMultiplier = 1f; // 1 = normal luck, 1.1 = +10% luck, etc.
    public bool rolling = false;
    public float cooldownTime;
    public float maxtime = 2f;
    public AudioSource gachaSound;

    // --- New fields for 100-pull movement ---
    [Header("100-Pull Movement")]
    public Transform movingObject;         // assign in inspector: the object to move
    public Transform orbitCenter;          // optional center; if null, will use movingObject's parent or origin
    public float orbitRadius = 1f;         // radius of the circular path
    public float orbitDuration = 2f;       // time (seconds) to complete 360 degrees
    private int pullCount = 0;             // runtime counter of pulls
    private bool isOrbiting = false;
    // --------------------------------------

    private void Update()
    {
        gacha.SetActive(isGacha);
        cooldownTime += Time.deltaTime;
    }

    [System.Serializable]
    public class GachaReward
    {
        public string id;
        public float amount;
        public GameObject prefab;  // optional visual reward
        public int goldAmount = 0; // optional currency reward
        [Range(0f, 100f)] public float chance = 10f; // percent chance (0–100)
        public TextMeshProUGUI rewardTextPrefab;
    }

    public List<GachaReward> rewards = new List<GachaReward>();
    public int pullsPerOpen = 1;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void TryOpenGacha()
    {
        if (cooldownTime >= maxtime)
        {
            gachaSound.Play();

            if (rolling) return;
            rolling = true;

            if (SaveDataController.currentData.moneyCount >= 1000)
            {
                SaveDataController.currentData.moneyCount -= 1000;
                //int pulls = Mathf.Max(1, pullsPerOpen + (gachaLevel / 10));

                for (int i = 0; i < pullsPerOpen; i++)
                {
                    var reward = PullReward();
                    GrantReward(reward);

                }
            }
        cooldownTime = 0f;
        }

    }



    private GachaReward PullReward()
    {

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 1f));

        GameObject effectSystemInstance = Instantiate(effect, worldPos, Quaternion.identity, uiEffectContainer.transform);
        ParticleSystem ps = effectSystemInstance.GetComponent<ParticleSystem>();

        if (rewards == null || rewards.Count == 0)
            return null;

        float totalChance = 0f;
        foreach (var r in rewards)
            totalChance += Mathf.Clamp(r.chance, 0f, 100f);

        float roll = Random.Range(0f, Mathf.Max(totalChance, 100f));
        float acc = 0f;

        effectSystemInstance.SetActive(true);
        ps.Play();

        foreach (var r in rewards)
        {
            acc += Mathf.Clamp(r.chance * luckMultiplier, 0f, 100f);
            if (roll <= acc)
                return r;
        }

        return null;
    }

    private void GrantReward(GachaReward reward)
    {
        if (reward == null)
        {
            Debug.Log("🎲 Gacha: No reward this time.");
            return;
        }

        SaveDataController.currentData.moneyCount += reward.goldAmount;
        ClickerManager.instance?.MoneyEffect(reward.goldAmount);

        if (reward.prefab != null && ClickerManager.instance != null)
        {
            Instantiate(reward.prefab, ClickerManager.instance.transform.position, Quaternion.identity);
        }
        if (SaveDataController.currentData.moneyCount <= 0)
        { SaveDataController.currentData.moneyCount = 0; }
        Debug.Log($"🎉 Gacha: Won {reward.id}! +{reward.goldAmount}");

        rolling = false;
        reward.rewardTextPrefab.text = reward.id + ": " + reward.amount;
        reward.amount += 1;

        // --- Increment pull counter and trigger 360 movement every 100 pulls ---
        pullCount++;
        if (pullCount % 100 == 0)
        {
            if (movingObject != null && !isOrbiting)
                StartCoroutine(Orbit360());
            else
                Debug.Log("GachaManager: movingObject not assigned or already orbiting.");
        }
        // ----------------------------------------------------------------------
    }

    // Coroutine that moves the assigned object around a circle completing 360 degrees
    private IEnumerator Orbit360()
    {
        if (movingObject == null)
            yield break;

        isOrbiting = true;

        Vector3 center;
        if (orbitCenter != null) center = orbitCenter.position;
        else if (movingObject.parent != null) center = movingObject.parent.position;
        else center = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < orbitDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / orbitDuration);
            float angleDeg = Mathf.Lerp(0f, 360f, t);
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(angleRad) * orbitRadius, Mathf.Sin(angleRad) * orbitRadius, 0f);
            //movingObject.position = center + offset;

            // Optional: rotate the object around its own Z to face motion
            movingObject.rotation = Quaternion.Euler(0f, 0f, -angleDeg);

            yield return null;
        }

        // Ensure final position completes the circle
        //movingObject.position = center + new Vector3(Mathf.Cos(360f * Mathf.Deg2Rad) * orbitRadius, Mathf.Sin(360f * Mathf.Deg2Rad) * orbitRadius, 0f);
        isOrbiting = false;
    }



}
