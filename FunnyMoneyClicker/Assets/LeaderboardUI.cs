using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Leaderboards.Models;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;
    public GameObject entryPrefab;

    [Header("Rank Colors")]
    public Color firstPlaceColor = new Color(1f, 0.84f, 0f);
    public Color secondPlaceColor = new Color(0.75f, 0.75f, 0.75f);
    public Color thirdPlaceColor = new Color(0.8f, 0.5f, 0.2f);
    public Color defaultColor = Color.white;

    public void PopulateLeaderboard(List<LeaderboardEntry> entries)
    {
        // Clear existing entries
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (entries == null || entries.Count == 0)
        {
            Debug.Log("No leaderboard entries to display.");
            return;
        }

        int rank = 1;
        foreach (var entry in entries)
        {
            GameObject lEntry = Instantiate(entryPrefab, contentParent);

            TMP_Text textComponent = lEntry.GetComponentInChildren<TMP_Text>();

            string formattedScore = NumberFormatter.Format(entry.Score);
            string displayName = !string.IsNullOrEmpty(entry.PlayerName) ? entry.PlayerName : entry.PlayerId;

            textComponent.text = $"{rank}. {displayName} - ${formattedScore}";

            switch (rank)
            {
                case 1:
                    textComponent.color = firstPlaceColor;
                    break;
                case 2:
                    textComponent.color = secondPlaceColor;
                    break;
                case 3:
                    textComponent.color = thirdPlaceColor;
                    break;
                default:
                    textComponent.color = defaultColor;
                    break;
            }

            rank++;
        }
    }
}
