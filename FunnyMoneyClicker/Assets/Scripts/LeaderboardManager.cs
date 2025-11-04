using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    async void Start()
    {
        await InitializeServices();
    }

    private async Task InitializeServices()
    {

        try
        {
            await UnityServices.InitializeAsync();


            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            isInitialized = true;
            Debug.Log($"[LB] Initialized. PlayerId={AuthenticationService.Instance.PlayerId}");
            _ = RefreshLeaderboard();
        }
        catch (Exception e)
        {
            Debug.LogError($"[LB] Initialization failed: {e}");
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
        if (SaveDataController.currentData == null) return;

        double currentMoney = SaveDataController.currentData.moneyCount;

        if (currentMoney > lastSubmittedScore)
        {
            lastSubmittedScore = currentMoney;
            _ = SubmitScore(currentMoney);
        }
    }

    public async Task SubmitScore(double totalMoney)
    {
        if (!isInitialized) return;
        if (double.IsNaN(totalMoney) || totalMoney <= 0) totalMoney = 1;

        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, totalMoney);
            Debug.Log($"[LB] Submitted: {totalMoney} (${NumberFormatter.Format(totalMoney)})");
            _ = RefreshLeaderboard();
        }
        catch (Exception e)
        {
            Debug.LogError($"[LB] Submit failed: {e}");
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

            ui.PopulateLeaderboard(scoresResponse.Results);
        }
        catch (Exception e)
        {
            Debug.LogError($"[LB] Failed to refresh leaderboard: {e}");
        }
    }

    public void ForceRefresh() => _ = RefreshLeaderboard();
}
