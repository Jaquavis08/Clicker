using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

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

    //public Color localPlayerColor = Color.green;      // Color for your player
    [HideInInspector] public bool boldLocalPlayer = true;               // Bold text for your player

    private void Awake()
    {
        if (leaderboardVisual != null)
            leaderboardVisual.SetActive(false);
    }

    public void PopulateLeaderboard(List<LeaderboardEntry> entries)
    {
        if (contentParent == null || entryPrefab == null) return;

        // Clear previous entries
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (entries == null || entries.Count == 0)
        {
            if (leaderboardVisual != null) leaderboardVisual.SetActive(false);
            return;
        }

        if (leaderboardVisual != null) leaderboardVisual.SetActive(true);

        string localPlayerId = AuthenticationService.Instance.PlayerId;
        int rank = 1;

        foreach (var entry in entries)
        {
            try
            {
                double score = entry.Score; // Already a double
                if (score <= 0) continue;

                GameObject lEntry = Instantiate(entryPrefab, contentParent);
                TMP_Text text = lEntry.GetComponentInChildren<TMP_Text>();
                if (text == null) { Destroy(lEntry); continue; }

                string formatted = NumberFormatter.Format(score);
                bool isLocalPlayer = entry.PlayerId == localPlayerId;
                string displayName = isLocalPlayer
                    ? $"{(string.IsNullOrEmpty(entry.PlayerName) ? entry.PlayerId : entry.PlayerName)} (YOU)"
                    : (string.IsNullOrEmpty(entry.PlayerName) ? entry.PlayerId : entry.PlayerName);

                text.text = $"{rank}. {displayName} - ${formatted}";

                text.color = rank switch
                {
                    1 => firstPlaceColor,
                    2 => secondPlaceColor,
                    3 => thirdPlaceColor,
                    _ => defaultColor
                };

                rank++;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LB UI] Skipped entry {entry.PlayerId}: {ex.Message}");
            }
        }
    }
}
