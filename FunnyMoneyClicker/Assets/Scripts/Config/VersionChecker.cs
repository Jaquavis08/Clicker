using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class VersionChecker : MonoBehaviour
{
    [Header("Version Settings")]
    public string versionUrl = "https://raw.githubusercontent.com/Anthony966-web/FunnyMoneyClicker-Version-Check/main/version.json";

    [Header("UI References")]
    [SerializeField] private GameObject updatePanel;
    [SerializeField] private TMPro.TextMeshProUGUI updateMessageText;

    private async void Awake()
    {
        await CheckVersion();
    }

    private async Task CheckVersion()
    {
        Debug.Log($"🔍 Checking version... Current Version: {GameVersion.CURRENT}");

        using (UnityWebRequest request = UnityWebRequest.Get(versionUrl))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Failed to check version: " + request.error);
                return;
            }

            string json = request.downloadHandler.text;
            Debug.Log($"📦 Version JSON received:\n{json}");

            VersionData data;
            try
            {
                data = JsonUtility.FromJson<VersionData>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"⚠️ JSON Parse Error: {e.Message}");
                return;
            }

            // Default safeguard
            if (data == null)
            {
                Debug.LogError("❌ Version data is null. Aborting check.");
                return;
            }

            Debug.Log($"✅ Parsed Data → Latest: {data.latestVersion}, Minimum: {data.minimumVersion}, Testing: {data.testing}");

            // 🔒 Prevent false quitting: only quit if Testing is explicitly false
            if (data.testing == false)
            {
                Debug.LogError("🚫 Testing mode disabled in version.json — quitting application.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return;
            }

            // Version checks
            if (IsOutdated(GameVersion.CURRENT, data.minimumVersion))
            {
                ShowForceUpdateMessage(data.forceUpdateMessage, true);
            }
            else if (IsOutdated(GameVersion.CURRENT, data.latestVersion))
            {
                Debug.Log($"🟡 Update available — but not required. Current: {GameVersion.CURRENT}, Latest: {data.latestVersion}");
                ShowForceUpdateMessage($"A new update ({data.latestVersion}) is available! You are on {GameVersion.CURRENT}.", false);
            }
            else
            {
                Debug.Log($"🟢 Version OK — Current: {GameVersion.CURRENT}, Latest: {data.latestVersion}");
            }
        }
    }

    private bool IsOutdated(string current, string minimum)
    {
        try
        {
            System.Version curVer = new System.Version(current);
            System.Version minVer = new System.Version(minimum);
            return curVer < minVer;
        }
        catch
        {
            Debug.LogWarning($"⚠️ Version parse error: current='{current}' minimum='{minimum}'");
            return false;
        }
    }

    private void ShowForceUpdateMessage(string message, bool force)
    {
        if (updatePanel != null && updateMessageText != null)
        {
            updatePanel.SetActive(true);
            updateMessageText.text = message;
        }

        if (force)
            Time.timeScale = 0f;

        Debug.LogWarning($"🚨 {message}");
    }

    [System.Serializable]
    private class VersionData
    {
        public string latestVersion;
        public string minimumVersion;
        public string forceUpdateMessage;
        public string updateUrl;
        public string news;
        public bool testing = true;
    }
}
