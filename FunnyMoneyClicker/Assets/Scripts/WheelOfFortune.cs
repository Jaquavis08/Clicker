using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WheelOfFortune : MonoBehaviour
{
    [Header("References")]
    public RectTransform wheelContainer;        // parent that will be rotated
    public GameObject slicePrefab;              // prefab should contain an Image (wedge sprite) and a Text child
    public Transform resultSpawnPoint;          // where to spawn visual reward (e.g., ClickerManager.instance.transform)
    public Button spinButton;

    [Header("Spin settings")]
    public float spinDuration = 3.0f;
    public int extraSpins = 3;                  // full rotations before landing
    public AnimationCurve spinEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // internal
    private List<GachaManager.GachaReward> rewards = new List<GachaManager.GachaReward>();
    private List<(float startAngle, float sweepAngle)> segments = new List<(float, float)>();
    private float totalWeight = 0f;
    private bool isSpinning = false;

    private void Start()
    {
        if (spinButton != null) spinButton.onClick.AddListener(Spin);
        BuildWheel();
    }

    [ContextMenu("Build Wheel")]
    public void BuildWheel()
    {
        if (GachaManager.instance == null)
        {
            Debug.LogWarning("WheelOfFortune: No GachaManager.instance found.");
            return;
        }

        rewards = GachaManager.instance.rewards;
        float luck = GachaManager.instance.luckMultiplier;
        // clear existing slices
        for (int i = wheelContainer.childCount - 1; i >= 0; i--)
            DestroyImmediate(wheelContainer.GetChild(i).gameObject);

        segments.Clear();
        totalWeight = 0f;
        foreach (var r in rewards)
            totalWeight += Mathf.Clamp(r.chance * luck, 0f, 100f);

        // Ensure there's at least a default full circle if totalWeight <= 0
        if (totalWeight <= 0f && rewards.Count > 0)
        {
            // give equal weight
            totalWeight = rewards.Count;
            for (int i = 0; i < rewards.Count; i++)
                rewards[i].chance = 1f;
        }

        float accAngle = 0f;
        for (int i = 0; i < rewards.Count; i++)
        {
            var r = rewards[i];
            float w = Mathf.Clamp(r.chance * luck, 0f, 100f);
            float sweep = (w / totalWeight) * 360f;

            // instantiate slice
            var go = Instantiate(slicePrefab, wheelContainer);
            go.name = $"Slice_{i}_{r.id}";
            var rt = go.GetComponent<RectTransform>();
            // position pivot at center and rotate slice so it occupies the correct wedge
            rt.localRotation = Quaternion.Euler(0f, 0f, -accAngle);

            // if slice prefab has an Image we can set fillAmount (for radial wedge prefabs)
            var img = go.GetComponent<Image>();
            if (img != null)
            {
                // Many wedge prefabs use Image.fillAmount to determine wedge size (0..1)
                img.fillAmount = Mathf.Clamp01(sweep / 360f);
            }

            // try set label text
            var txt = go.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = $"{r.id}\n{r.goldAmount}";
                // position label roughly in middle of wedge by rotating it back so it reads upright
                txt.rectTransform.localRotation = Quaternion.Euler(0f, 0f, accAngle + sweep * 0.5f);
            }

            segments.Add((accAngle, sweep));
            accAngle += sweep;
        }
    }

    public void Spin()
    {
        if (isSpinning) return;
        if (GachaManager.instance == null)
        {
            Debug.LogWarning("WheelOfFortune: No GachaManager.instance found.");
            return;
        }

        if (rewards == null || rewards.Count == 0)
        {
            Debug.LogWarning("WheelOfFortune: No rewards to spin.");
            return;
        }

        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;
        if (spinButton != null) spinButton.interactable = false;

        // Pick reward using the same weighted logic (uses luckMultiplier)
        float luck = GachaManager.instance.luckMultiplier;
        float total = 0f;
        foreach (var r in rewards) total += Mathf.Clamp(r.chance * luck, 0f, 100f);
        float roll = Random.Range(0f, Mathf.Max(total, 100f));
        int selectedIndex = -1;
        float acc = 0f;
        for (int i = 0; i < rewards.Count; i++)
        {
            acc += Mathf.Clamp(rewards[i].chance * luck, 0f, 100f);
            if (roll <= acc)
            {
                selectedIndex = i;
                break;
            }
        }
        if (selectedIndex == -1) selectedIndex = Mathf.Clamp(Mathf.FloorToInt(Random.value * rewards.Count), 0, rewards.Count - 1);

        // compute target angle (choose a random angle inside the selected segment)
        var seg = segments[selectedIndex];
        float localTargetAngle = seg.startAngle + Random.Range(0f, seg.sweepAngle);
        // Wheel rotation: we want the chosen wedge to align with pointer at 0 degrees.
        // Because we rotated slices by -accAngle when building, rotating wheel by +angle will bring that slice to pointer.
        float startAngle = NormalizeAngle(wheelContainer.localEulerAngles.z);
        float endAngle = startAngle + extraSpins * 360f + localTargetAngle - startAngle; // rotate to localTargetAngle plus extra spins
        endAngle = extraSpins * 360f + localTargetAngle; // simpler target

        float t = 0f;
        float dur = spinDuration;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / dur);
            float eased = spinEasing.Evaluate(p);
            float angle = Mathf.LerpAngle(startAngle, endAngle, eased);
            wheelContainer.localEulerAngles = new Vector3(0, 0, angle);
            yield return null;
        }

        // snap to exact angle
        wheelContainer.localEulerAngles = new Vector3(0, 0, endAngle);

        // Give the reward (duplicate of GachaManager.GrantReward logic because that method is private)
        var reward = rewards[selectedIndex];
        ApplyReward(reward);

        isSpinning = false;
        if (spinButton != null) spinButton.interactable = true;
    }

    private void ApplyReward(GachaManager.GachaReward reward)
    {
        if (reward == null)
        {
            Debug.Log("WheelOfFortune: No reward.");
            return;
        }

        SaveDataController.currentData.moneyCount += reward.goldAmount;
        ClickerManager.instance?.MoneyEffect(reward.goldAmount);

        if (reward.prefab != null)
        {
            Transform spawn = resultSpawnPoint != null ? resultSpawnPoint : (ClickerManager.instance != null ? ClickerManager.instance.transform : null);
            if (spawn != null)
                Instantiate(reward.prefab, spawn.position, Quaternion.identity);
            else
                Instantiate(reward.prefab, Vector3.zero, Quaternion.identity);
        }

        if (SaveDataController.currentData.moneyCount <= 0)
            SaveDataController.currentData.moneyCount = 0;

        Debug.Log($"🎉 WheelOfFortune: Won {reward.id}! +{reward.goldAmount}");
    }

    private float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a < 0) a += 360f;
        return a;
    }
}