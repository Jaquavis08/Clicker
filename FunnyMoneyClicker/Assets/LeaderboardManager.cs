using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using System.Threading.Tasks;
using Unity.Services.Leaderboards.Models;
using System;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI")]
    public LeaderboardUI ui;

    [Header("Settings")]
    public float refreshInterval = 10f;
    public int leaderboardLimit = 100;

    private float curTime = 0f;
    private const string leaderboardId = "Top_Funny_Money";

    private double lastSubmittedScore = 0;
    private bool isInitialized = false;

    // Always store a scaling factor for leaderboard
    private double leaderboardScale = 1.0;

    async void Start()
    {
        await InitializeServices();
    }

    private async Task InitializeServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in: " + AuthenticationService.Instance.IsSignedIn);

            isInitialized = true;
            await RefreshLeaderboard();
        }
        catch (Exception e)
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
            _ = RefreshLeaderboard();
        }
    }

    private void SubmitIfChanged()
    {
        double currentMoney = SaveDataController.currentData.moneyCount;

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
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                leaderboardId,
                new GetScoresOptions { Limit = leaderboardLimit });

            ui.PopulateLeaderboard(scoresResponse.Results, SaveDataController.currentData.moneyCount);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to refresh leaderboard: {e.Message}");
        }
    }

    public async Task SubmitScore(double totalMoney)
    {
        if (!isInitialized) return;

        try
        {
            if (totalMoney < 0) totalMoney = 0;

            long scaledScore = 0;

            if (totalMoney <= long.MaxValue)
            {
                // Safe to submit directly
                scaledScore = (long)totalMoney;
                leaderboardScale = 1.0;
            }
            else
            {
                // Scale down so it fits in long.MaxValue
                leaderboardScale = totalMoney / long.MaxValue;
                scaledScore = long.MaxValue;
            }

            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, scaledScore);

            Debug.Log($"Score submitted: {scaledScore} / realMoney: {totalMoney} / scale: {leaderboardScale}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to submit score: {e.Message}");
        }
    }
}
