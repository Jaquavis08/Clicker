using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager instance;

    public bool isGacha;
    public GameObject gacha;

    private void Update()
    {
        if (isGacha)
        {
            gacha.SetActive(true);
        }
        else
        {
            gacha.SetActive(false);
        }
    }

    [System.Serializable]
    public class GachaReward
    {
        public string id;
        public GameObject prefab;      // optional visual reward
        public int goldAmount = 0;     // optional currency reward
        public float weight = 1f; // relative probability
    }

    public List<GachaReward> rewards = new List<GachaReward>();
    public int pullsPerOpen = 1;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Called from Power1 with the power level that triggered the gacha.
    public void TryOpenGacha(int level)
    {
        // Example: increase pulls with level or use cost checks here
        int pulls = Mathf.Max(1, pullsPerOpen + (level / 10));
        for (int i = 0; i < pulls; i++)
        {
            var reward = PullReward();
            GrantReward(reward);
        }
    }

    private GachaReward PullReward()
    {
        if (rewards == null || rewards.Count == 0) return null;
        float total = 0f;
        foreach (var r in rewards) total += Mathf.Max(0f, r.weight);
        float roll = Random.Range(0f, total);
        float acc = 0f;
        foreach (var r in rewards)
        {
            acc += Mathf.Max(0f, r.weight);
            if (roll <= acc) return r;
        }
        return rewards[0];
    }

    private void GrantReward(GachaReward reward)
    {
        if (reward == null) return;

        if (reward.goldAmount < SaveDataController.currentData.moneyCount)
        {
            SaveDataController.currentData.moneyCount += reward.goldAmount;
            // show effect if manager exists
            ClickerManager.instance?.MoneyEffect(reward.goldAmount);
        }
        if (SaveDataController.currentData.moneyCount <= 0)
        {
            SaveDataController.currentData.moneyCount = 0;
        }

        if (reward.prefab != null && ClickerManager.instance != null)
        {
            Instantiate(reward.prefab, ClickerManager.instance.transform.position, Quaternion.identity);
        }

        // You can add other reward-handling logic here (upgrades, items, etc.)
    }
}