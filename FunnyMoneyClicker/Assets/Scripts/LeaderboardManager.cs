using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BreakInfinity;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI")]
    public LeaderboardUI ui;

    [Header("Settings")]
    public float refreshInterval = 10f;
    public int leaderboardLimit = 100;

    private float curTime = 0f;
    private const string leaderboardId = "Top_Funny_Money";
    private BigDouble lastSubmittedScore = 0;
    private bool isInitialized = false;

    // Nickname support
    private const string NicknamePrefKeyPrefix = "LB_NICK_"; // usage: LB_NICK_<playerId> or LB_NICK_local
    public string LocalNickname
    {
        get
        {
            if (isInitialized && AuthenticationService.Instance != null)
            {
                string pidKey = NicknamePrefKeyPrefix + AuthenticationService.Instance.PlayerId;
                return PlayerPrefs.GetString(pidKey, PlayerPrefs.GetString(NicknamePrefKeyPrefix + "local", "Player"));
            }
            return PlayerPrefs.GetString(NicknamePrefKeyPrefix + "local", "Player");
        }
        set
        {
            if (isInitialized && AuthenticationService.Instance != null)
                PlayerPrefs.SetString(NicknamePrefKeyPrefix + AuthenticationService.Instance.PlayerId, value ?? "");
            else
                PlayerPrefs.SetString(NicknamePrefKeyPrefix + "local", value ?? "");
            PlayerPrefs.Save();
        }
    }

    public void SetNickname(string nickname) => LocalNickname = nickname;

    public string GetNicknameForPlayer(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return "Player";
        return PlayerPrefs.GetString(NicknamePrefKeyPrefix + playerId, playerId);
    }

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

            // migrate any locally stored nickname into a player-scoped key after we know PlayerId
            var pid = AuthenticationService.Instance.PlayerId;
            var localKey = NicknamePrefKeyPrefix + "local";
            var pidKey = NicknamePrefKeyPrefix + pid;
            if (PlayerPrefs.HasKey(localKey) && !PlayerPrefs.HasKey(pidKey))
            {
                PlayerPrefs.SetString(pidKey, PlayerPrefs.GetString(localKey));
                PlayerPrefs.Save();
            }

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

        BigDouble currentMoney = SaveDataController.currentData.moneyCount;

        if (currentMoney > lastSubmittedScore)
        {
            lastSubmittedScore = currentMoney;
            _ = SubmitScore(currentMoney);
        }
    }

    public async Task SubmitScore(BigDouble totalMoney)
    {
        if (!isInitialized) return;
        if (BigDouble.IsNaN(totalMoney) || totalMoney <= 0) totalMoney = 1;

        try
        {
            // Note: Unity Leaderboards server-side does not automatically store arbitrary client nicknames.
            // We still submit the score normally. Local nickname is stored in PlayerPrefs so the UI can
            // display it for the local player.
            // Unity Leaderboards only supports double, so convert safely
            double submitValue = (double)Math.Min(totalMoney.ToDouble(), double.MaxValue);

            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, submitValue);
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
