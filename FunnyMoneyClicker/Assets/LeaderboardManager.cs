using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using System.Threading.Tasks;
using Unity.Services.Leaderboards.Models;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI")]
    public LeaderboardUI ui;

    [Header("Settings")]
    public float refreshInterval = 10f;

    private float curTime = 0f;
    private const string leaderboardId = "Top_Funny_Money";

    private long lastSubmittedScore = 0;
    private bool isInitialized = false;

    async void Start()
    {
        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        try
        {
            // Initialize Unity Services
            await UnityServices.InitializeAsync();

            // Sign in anonymously
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in: " + AuthenticationService.Instance.IsSignedIn);

            isInitialized = true;

            // Load initial leaderboard
            await RefreshLeaderboard();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Leaderboard initialization failed: {e.Message}");
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        curTime += Time.deltaTime;
        if (curTime >= refreshInterval)
        {
            curTime = 0f;

            SubmitIfChanged();
            _ = RefreshLeaderboard(); // Refresh UI
        }
    }

    private void SubmitIfChanged()
    {
        // Get current money from save data
        long currentMoney = (long)SaveDataController.currentData.moneyCount;

        // Only submit if it has changed since last submission
        if (currentMoney > lastSubmittedScore)
        {
            lastSubmittedScore = currentMoney;
            _ = SubmitScore(currentMoney);
        }
    }

    public async Task RefreshLeaderboard()
    {
        if (!isInitialized) return;

        try
        {
            // Get top 10 scores
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                leaderboardId,
                new GetScoresOptions { Limit = 10 });

            if (scoresResponse.Results.Count == 0)
            {
                Debug.Log("Leaderboard is empty.");
            }

            // Update UI
            ui.PopulateLeaderboard(scoresResponse.Results);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to refresh leaderboard: {e.Message}");
        }
    }

    public async Task SubmitScore(double totalMoney)
    {
        if (!isInitialized) return;

        try
        {
            // Cast double to long safely
            long scoreToSubmit = (long)System.Math.Min(totalMoney, long.MaxValue);

            // Submit total money to leaderboard
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, scoreToSubmit);

            Debug.Log($"Score submitted: {scoreToSubmit}");

            // Optional: Refresh after submitting
            await RefreshLeaderboard();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to submit score: {e.Message}");
        }
    }
}
