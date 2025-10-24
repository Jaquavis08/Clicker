using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class VersionChecker : MonoBehaviour
{
    [Header("Version Settings")]
    public string versionUrl = "https://github.com/Anthony966-web/FunnyMoneyClicker-Version-Check/blob/main/version.json";

    private async void Awake()
    {
        await CheckVersion();
    }

    private async Task CheckVersion()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(versionUrl))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to check version: " + request.error);
                return;
            }

            var data = JsonUtility.FromJson<VersionData>(request.downloadHandler.text);

            if (IsOutdated(GameVersion.CURRENT, data.minimumVersion))
            {
                Debug.LogError("Game version too old! Blocking play.");
                ShowForceUpdateMessage(data.forceUpdateMessage);
            }
            else
            {
                Debug.Log($"Version OK ({GameVersion.CURRENT}) — continuing.");
            }
        }
    }

    private bool IsOutdated(string current, string minimum)
    {
        System.Version curVer = new System.Version(current);
        System.Version minVer = new System.Version(minimum);
        return curVer < minVer;
    }

    private void ShowForceUpdateMessage(string message)
    {
        Time.timeScale = 0f;
        Debug.LogWarning(message);
        // Example: open your game’s store page
        Application.OpenURL("https://storepage.link/yourgame");
    }

    [System.Serializable]
    private class VersionData
    {
        public string latestVersion;
        public string minimumVersion;
        public string forceUpdateMessage;
    }
}
