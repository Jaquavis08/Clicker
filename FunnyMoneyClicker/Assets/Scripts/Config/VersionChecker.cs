using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class VersionChecker : MonoBehaviour
{
    [Header("Version Check URL")]
    public string versionFileURL = "https://raw.githubusercontent.com/Anthony966-web/FunnyMoneyClicker-Version-Check/main/version.json";

    [Header("Current Game Version")]
    public string currentVersion = "1.1.0";

    private void Start()
    {
        StartCoroutine(CheckForUpdate());
        // Optional: repeat check every few minutes
        // StartCoroutine(CheckPeriodically(300f));
    }

    IEnumerator CheckForUpdate()
    {
        UnityWebRequest www = UnityWebRequest.Get(versionFileURL);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Failed to check version: " + www.error);
            yield break;
        }

        VersionData data = JsonUtility.FromJson<VersionData>(www.downloadHandler.text);
        if (IsNewerVersion(data.latestVersion, currentVersion))
        {
            Debug.Log("Update available! Latest: " + data.latestVersion);
            if (data.mandatory)
            {
                // For PC, automatically open download link
                Application.OpenURL(data.updateURL);
                // Optionally quit game automatically
                Application.Quit();
            }
            else
            {
                // Show update UI (button to download)
                Debug.Log("Optional update available at: " + data.updateURL);
            }
        }
        else
        {
            Debug.Log("Game is up to date!");
        }
    }

    bool IsNewerVersion(string latest, string current)
    {
        System.Version latestV = new System.Version(latest);
        System.Version currentV = new System.Version(current);
        return latestV > currentV;
    }

    [System.Serializable]
    public class VersionData
    {
        public string latestVersion;
        public string updateURL;
        public bool mandatory;
    }

    // Optional: check periodically
    IEnumerator CheckPeriodically(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            yield return CheckForUpdate();
        }
    }
}
