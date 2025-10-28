using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Leaderboards.Models;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;
    public GameObject entryPrefab;
    public GameObject leaderboardVisual;

    [Header("Rank Colors")]
    public Color firstPlaceColor = new Color(1f, 0.84f, 0f);
    public Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f);
    public Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f);
    public Color defaultColor = Color.white;

    private Dictionary<string, double> playerScales = new Dictionary<string, double>();

    private void Awake()
    {
        leaderboardVisual.gameObject.SetActive(false);
    }

    public void PopulateLeaderboard(List<LeaderboardEntry> entries, Dictionary<string, double> scales)
    {

        playerScales = scales;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (entries == null || entries.Count == 0) return;

        int rank = 1;
        foreach (var entry in entries)
        {
            GameObject lEntry = Instantiate(entryPrefab, contentParent);
            TMP_Text textComponent = lEntry.GetComponentInChildren<TMP_Text>();

            double scale = 1.0;
            if (playerScales != null && playerScales.ContainsKey(entry.PlayerId))
                scale = playerScales[entry.PlayerId];

            double realScore = entry.Score * scale;
            string formattedScore = NumberFormatter.Format(realScore);

            string displayName = !string.IsNullOrEmpty(entry.PlayerName) ? entry.PlayerName : entry.PlayerId;
            textComponent.text = $"{rank}. {displayName} - ${formattedScore}";

            switch (rank)
            {
                case 1: textComponent.color = firstPlaceColor; break;
                case 2: textComponent.color = secondPlaceColor; break;
                case 3: textComponent.color = thirdPlaceColor; break;
                default: textComponent.color = defaultColor; break;
            }

            rank++;

        }

        leaderboardVisual.gameObject.SetActive(true);
    }
}
