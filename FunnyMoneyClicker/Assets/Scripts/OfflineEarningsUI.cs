using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OfflineEarningsUI : MonoBehaviour
{
    public static OfflineEarningsUI instance;

    [Header("UI References")]
    public GameObject panel;
    public TMP_Text offlineIncomeText;
    public TMP_Text offlineMultiplierText;
    public TMP_Text offlineTimeText;

    private void Awake()
    {
        instance = this;
    }

    public void Show(double earnings, double multiplier, double minutes)
    {
        panel.SetActive(true);

        if (earnings <= 0) return;

        offlineIncomeText.text = $"Offline Income: <b>${NumberFormatter.Format(earnings)}";
        offlineMultiplierText.text = $"Offline Multiplier: <b>{NumberFormatter.Format(multiplier * 100)}%";
        offlineTimeText.text = $"Offline Time: <b>{minutes:F1} minutes";
    }
}
