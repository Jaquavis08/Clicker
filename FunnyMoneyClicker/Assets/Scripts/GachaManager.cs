using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Update()
    {
        gacha.SetActive(isGacha);
        cooldownTime += Time.deltaTime;
    }

    [System.Serializable]
    public class GachaReward
    {
        public string id;
        public GameObject prefab;  // optional visual reward
        public int goldAmount = 0; // optional currency reward
        [Range(0f, 100f)] public float chance = 10f; // percent chance (0–100)
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
    }
}
