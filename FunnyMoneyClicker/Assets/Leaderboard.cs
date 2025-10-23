using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    private const string leaderboardId = "Top_Funny_Money";

    public float maxTime = 10;
    public float curTime;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async Task SubmitScoreAsync()
    {
        try
        {
            var leaderboardEntry = await LeaderboardsService.Instance.AddPlayerScoreAsync(
                leaderboardId,
                SaveDataController.currentData.moneyCount
            );
            Debug.Log($"Score submitted: {leaderboardEntry.Score}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to submit score: {e.Message}");
        }
    }

    public async Task GetTopScoresAsync()
    {
        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId);
            foreach (var entry in scoresResponse.Results)
            {
                Debug.Log($"Player: {entry.PlayerId}, Score: {entry.Score}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to get scores: {e.Message}");
        }
    }

    private float lastSubmittedScore = 0;
    private void Update()
    {
        curTime += Time.deltaTime;
        if (curTime >= maxTime)
        {
            curTime = 0;
            if (SaveDataController.currentData.moneyCount != lastSubmittedScore)
            {
                lastSubmittedScore = SaveDataController.currentData.moneyCount;
                _ = SubmitScoreAsync();
                _ = GetTopScoresAsync();
            }
        }
    }

    public void OnSubmitScoreButton()
    {
        _ = SubmitScoreAsync(); // Fire and forget
    }

    public void OnGetTopScoresButton()
    {
        _ = GetTopScoresAsync(); // Fire and forget
    }
}
