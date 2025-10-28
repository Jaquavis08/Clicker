using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using System.Threading.Tasks;
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

    private Dictionary<string, double> playerScales = new Dictionary<string, double>();

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
            //await RefreshLeaderboard();
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

            ui.PopulateLeaderboard(scoresResponse.Results, playerScales);
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

            double scale = 1.0;

            if (totalMoney > long.MaxValue)
            {
                scale = totalMoney / long.MaxValue;
            }

            long scoreToSubmit = (long)(totalMoney / scale);

            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, scoreToSubmit);

            Debug.Log($"Score submitted: {scoreToSubmit} / realMoney: {totalMoney} / scale: {scale}");

            playerScales[AuthenticationService.Instance.PlayerId] = scale;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to submit score: {e.Message}");
        }
    }
}
