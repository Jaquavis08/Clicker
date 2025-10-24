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

    [Tooltip("How many top ranks to show on the leaderboard.")]
    public int leaderboardLimit = 100;

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
            await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in: " + AuthenticationService.Instance.IsSignedIn);

            isInitialized = true;

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
            _ = RefreshLeaderboard();
        }
    }

    private void SubmitIfChanged()
    {
        long currentMoney = (long)SaveDataController.currentData.moneyCount;

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

            if (scoresResponse.Results.Count == 0)
            {
                Debug.Log("Leaderboard is empty.");
            }

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
            long scoreToSubmit = (long)System.Math.Min(totalMoney, long.MaxValue);

            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, scoreToSubmit);

            Debug.Log($"Score submitted: {scoreToSubmit}");

            await RefreshLeaderboard();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to submit score: {e.Message}");
        }
    }
}
