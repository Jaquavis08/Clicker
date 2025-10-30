using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WheelOfFortune : MonoBehaviour
{
    //[Header("References")]
    //public RectTransform wheelContainer; // parent that will rotate
    //public GameObject slicePrefab; // prefab with Image + Text
    //public Transform resultSpawnPoint;
    //public Button spinButton;
    //public TextMeshProUGUI rewardTextUI;

    //[Header("Spin settings")]
    //public float spinDuration = 3.0f;
    //public int extraSpins = 3;
    //public AnimationCurve spinEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    //// Internal
    //private class WheelReward
    //{
    //    public string id;
    //    public int goldAmount;
    //    public bool isSkin;
    //    public float chance;
    //}

    //private List<WheelReward> rewards = new List<WheelReward>();
    //private List<(float startAngle, float sweepAngle)> segments = new List<(float, float)>();
    //private float totalWeight = 0f;
    //private bool isSpinning = false;

    //private void Start()
    //{
    //    if (spinButton != null) spinButton.onClick.AddListener(Spin);
    //    BuildWheel();
    //}

    //[ContextMenu("Build Wheel")]
    //public void BuildWheel()
    //{
    //    rewards.Clear();
    //    segments.Clear();
    //    totalWeight = 0f;

    //    // ✅ Build from your GachaManager
    //    var gacha = GachaManager.instance;
    //    if (gacha == null)
    //    {
    //        Debug.LogWarning("WheelOfFortune: No GachaManager.instance found.");
    //        return;
    //    }

    //    float luck = gacha.luckMultiplier;

    //    // Add money rewards
    //    rewards.Add(new WheelReward { id = "💰 $500", goldAmount = 500, chance = 50f, isSkin = false });
    //    rewards.Add(new WheelReward { id = "💰 $1000", goldAmount = 1000, chance = 30f, isSkin = false });
    //    rewards.Add(new WheelReward { id = "💰 $2500", goldAmount = 2500, chance = 10f, isSkin = false });

    //    // Add skin rewards (optional — weighted lower)
    //    if (gacha.clickerDatabase != null && gacha.clickerDatabase.allClickers != null)
    //    {
    //        foreach (var skin in gacha.clickerDatabase.allClickers)
    //        {
    //            rewards.Add(new WheelReward
    //            {
    //                id = skin.skinName,
    //                goldAmount = 0,
    //                chance = skin.chance > 0 ? skin.chance : 5f,
    //                isSkin = true
    //            });
    //        }
    //    }

    //    foreach (var r in rewards)
    //        totalWeight += Mathf.Max(0.01f, r.chance * luck);

    //    // clear existing slices
    //    for (int i = wheelContainer.childCount - 1; i >= 0; i--)
    //        DestroyImmediate(wheelContainer.GetChild(i).gameObject);

    //    float accAngle = 0f;
    //    foreach (var r in rewards)
    //    {
    //        float sweep = (r.chance * luck / totalWeight) * 360f;

    //        var go = Instantiate(slicePrefab, wheelContainer);
    //        go.name = $"Slice_{r.id}";
    //        var rt = go.GetComponent<RectTransform>();
    //        rt.localRotation = Quaternion.Euler(0f, 0f, -accAngle);

    //        var img = go.GetComponent<Image>();
    //        if (img != null)
    //            img.fillAmount = Mathf.Clamp01(sweep / 360f);

    //        var txt = go.GetComponentInChildren<TextMeshProUGUI>();
    //        if (txt != null)
    //        {
    //            txt.text = r.id;
    //            txt.rectTransform.localRotation = Quaternion.Euler(0f, 0f, accAngle + sweep * 0.5f);
    //        }

    //        segments.Add((accAngle, sweep));
    //        accAngle += sweep;
    //    }
    //}

    //public void Spin()
    //{
    //    if (isSpinning) return;
    //    if (rewards.Count == 0)
    //    {
    //        Debug.LogWarning("WheelOfFortune: No rewards found!");
    //        return;
    //    }

    //    StartCoroutine(SpinRoutine());
    //}

    //private IEnumerator SpinRoutine()
    //{
    //    isSpinning = true;
    //    if (spinButton != null) spinButton.interactable = false;

    //    // Weighted random selection
    //    float luck = GachaManager.instance != null ? GachaManager.instance.luckMultiplier : 1f;
    //    float total = 0f;
    //    foreach (var r in rewards) total += Mathf.Max(0.01f, r.chance * luck);
    //    float roll = Random.Range(0f, total);
    //    float acc = 0f;
    //    int selectedIndex = 0;

    //    for (int i = 0; i < rewards.Count; i++)
    //    {
    //        acc += Mathf.Max(0.01f, rewards[i].chance * luck);
    //        if (roll <= acc)
    //        {
    //            selectedIndex = i;
    //            break;
    //        }
    //    }

    //    var seg = segments[selectedIndex];
    //    float targetAngle = seg.startAngle + seg.sweepAngle * 0.5f;
    //    float startAngle = NormalizeAngle(wheelContainer.localEulerAngles.z);
    //    float endAngle = extraSpins * 360f + targetAngle;

    //    float t = 0f;
    //    while (t < spinDuration)
    //    {
    //        t += Time.deltaTime;
    //        float p = Mathf.Clamp01(t / spinDuration);
    //        float eased = spinEasing.Evaluate(p);
    //        float angle = Mathf.LerpAngle(startAngle, endAngle, eased);
    //        wheelContainer.localEulerAngles = new Vector3(0, 0, angle);
    //        yield return null;
    //    }

    //    wheelContainer.localEulerAngles = new Vector3(0, 0, endAngle);
    //    ApplyReward(rewards[selectedIndex]);

    //    isSpinning = false;
    //    if (spinButton != null) spinButton.interactable = true;
    //}

    //private void ApplyReward(WheelReward reward)
    //{
    //    string msg = "";

    //    if (reward.isSkin)
    //    {
    //        if (!SaveDataController.currentData.unlockedSkins.Contains(reward.id))
    //        {
    //            SaveDataController.currentData.unlockedSkins.Add(reward.id);
    //            msg = $"🎉 NEW SKIN UNLOCKED: {reward.id}";
    //        }
    //        else
    //        {
    //            msg = $"⭐ Duplicate skin: {reward.id}";
    //        }
    //    }
    //    else
    //    {
    //        SaveDataController.currentData.moneyCount += reward.goldAmount;
    //        ClickerManager.instance?.MoneyEffect(reward.goldAmount);
    //        msg = $"💰 You won ${reward.goldAmount}!";
    //    }

    //    if (rewardTextUI != null)
    //        rewardTextUI.text = msg;

    //    Debug.Log($"WheelOfFortune: {msg}");
    //}

    //private float NormalizeAngle(float a)
    //{
    //    a %= 360f;
    //    if (a < 0) a += 360f;
    //    return a;
    //}
}
