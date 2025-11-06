using UnityEngine;
using TMPro;
using UnityEngine.UI;
using BreakInfinity;

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

    public void Show(BigDouble earnings, double multiplier, double minutes)
    {
        panel.SetActive(true);

        if (earnings <= 0) return;

        string timeString;
        if (minutes < 1)
        {
            double seconds = minutes * 60;
            timeString = $"{seconds:F0} seconds";
        }
        else if (minutes < 60)
        {
            timeString = $"{minutes:F0} minutes";
        }
        else
        {
            int hours = Mathf.FloorToInt((float)minutes / 60f);
            int mins = Mathf.FloorToInt((float)minutes % 60f);
            timeString = $"{hours}h {mins}m";
        }

        offlineIncomeText.text = $"Offline Income: <b>${NumberFormatter.Format(earnings)}";
        offlineMultiplierText.text = $"Offline Multiplier: <b>{NumberFormatter.Format(multiplier * 100)}%";
        offlineTimeText.text = $"Offline Time: <b>{timeString}";
    }
}
