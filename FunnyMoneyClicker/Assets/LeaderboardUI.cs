using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Leaderboards.Models;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;
    public TMP_Text entryPrefab;

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
            TMP_Text entryText = Instantiate(entryPrefab, contentParent);

            string formattedScore = NumberFormatter.Format(entry.Score);

            entryText.text = $"{rank}. {entry.PlayerId} - ${formattedScore}";
            rank++;
        }
    }
}
