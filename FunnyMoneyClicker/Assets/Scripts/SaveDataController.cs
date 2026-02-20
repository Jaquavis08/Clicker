//using BreakInfinity;
//using Newtonsoft.Json;
//using System;
//using System.IO;
//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class SaveDataController : MonoBehaviour
//{
//    [SerializeField] private string filePath;
//    [SerializeField] private string fileName;

//    public SaveDataObject defaultData;
//    public static SaveData currentData;

//    public TMP_InputField resetField;
//    private string resetKey = "RESET";
//    private string currentResetKey = "";

//    private void Awake() => Load();

//    private void Start() => Invoke(nameof(HandleOfflineEarnings), 0.5f);

//    private void OnDestroy() => Save();
//    private void OnApplicationQuit() => Save();

//    private void OnApplicationPause(bool pause)
//    {
//        if (pause) Save();
//        else
//        {
//            Load();
//            HandleOfflineEarnings();
//        }
//    }

//    public void Load()
//    {
//        string fullPath = Path.Combine(Application.persistentDataPath, filePath, fileName);

//        if (!File.Exists(fullPath))
//        {
//            currentData = defaultData.defaultData;
//            Save();
//            return;
//        }

//        try
//        {
//            string json = File.ReadAllText(fullPath);

//            // Unwrap old-style JSON if needed
//            if (json.StartsWith("\"") && json.EndsWith("\""))
//            {
//                json = json.Substring(1, json.Length - 2);
//                json = json.Replace("\\\"", "\"");
//                json = json.Replace("\\r\\n", "\n");
//            }

//            JsonSerializer serializer = new JsonSerializer();
//            serializer.Converters.Add(new BigDoubleConverter());
//            serializer.NullValueHandling = NullValueHandling.Ignore;

//            using (StringReader sr = new StringReader(json))
//            using (JsonTextReader reader = new JsonTextReader(sr))
//            {
//                currentData = serializer.Deserialize<SaveData>(reader);
//            }

//            if (currentData == null)
//            {
//                Debug.LogWarning("SaveData deserialized as null. Using default.");
//                currentData = defaultData.defaultData;
//                Save();
//            }
//        }
//        catch (Exception e)
//        {
//            Debug.LogError($"Failed to read main save, restoring default: {e.Message}");
//            currentData = defaultData.defaultData;
//            Save();
//        }
//    }

//    public void Save()
//    {
//        if (currentData == null) return;
//        currentData.lastSaveTime = DateTime.Now.ToBinary();
//        Serializer.Save(currentData, Path.Combine(Application.persistentDataPath, filePath), fileName);


//    }

//    public void OnResetValueChange() => currentResetKey = resetField.text.Trim().ToUpper();

//    public void DeleteData()
//    {
//        OnResetValueChange();
//        if (currentResetKey != resetKey) return;

//        string fullPath = Path.Combine(Application.persistentDataPath, filePath, fileName);

//        try
//        {
//            if (File.Exists(fullPath))
//                File.Delete(fullPath);

//            currentData = defaultData.defaultData;
//            Save();
//            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError($"Failed to delete save data: {ex.Message}");
//        }
//    }

//    private void HandleOfflineEarnings()
//    {
//        if (currentData == null) Load();
//        if (currentData.lastSaveTime == 0) return;

//        DateTime lastSave = DateTime.FromBinary(currentData.lastSaveTime);
//        TimeSpan timeAway = DateTime.Now - lastSave;

//        BigDouble moneyPerSecond = UpgradeManager.instance?.moneyPerSecond ?? 0;
//        float multiplier = currentData.offlineEarningsMultiplier;

//        BigDouble earnings = moneyPerSecond * timeAway.TotalSeconds * multiplier;
//        BigDouble maxEarnings = (moneyPerSecond * 5) * (3600 * 24); // 24-hour cap
//        earnings = BigDouble.Min(earnings, maxEarnings);

//        if (earnings > 0)
//        {
//            currentData.moneyCount += earnings;

//            if (OfflineEarningsUI.instance != null)
//                OfflineEarningsUI.instance.Show(earnings, multiplier, timeAway.TotalMinutes);
//            else
//                Debug.Log($"You earned {earnings} while offline for {timeAway.TotalMinutes:F1} minutes!");
//        }
//    }
//}




using BreakInfinity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveDataController : MonoBehaviour
{
    [Header("File Settings")]
    [SerializeField] private string filePath = "Save";
    [SerializeField] private string fileName = "save.json";

    [Header("Default Data")]
    public SaveDataObject defaultData;

    public static SaveData currentData;

    [Header("Reset")]
    public TMP_InputField resetField;
    private string resetKey = "RESET";
    private string currentResetKey = "";

    private void Awake() => Load();

    private void Start() => Invoke(nameof(HandleOfflineEarnings), 0.5f);

    private void OnDestroy() => SaveAll();
    private void OnApplicationQuit() => SaveAll();

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveAll();
        else
        {
            Load();
            HandleOfflineEarnings();
        }
    }

    // ----------------- SAVE -----------------
    public void SaveAll()
    {
        if (currentData == null) return;

        currentData.lastSaveTime = DateTime.Now.ToBinary();

        // Save runes first
        RuneInventoryManager.Instance?.SaveRunesToData();

        string fullDir = Path.Combine(Application.persistentDataPath, filePath);
        if (!Directory.Exists(fullDir)) Directory.CreateDirectory(fullDir);
        string fullPath = Path.Combine(fullDir, fileName);

        try
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new BigDoubleConverter());
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(fullPath))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, currentData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    // ----------------- LOAD -----------------
    public void Load()
    {
        string fullDir = Path.Combine(Application.persistentDataPath, filePath);
        string fullPath = Path.Combine(fullDir, fileName);

        if (!File.Exists(fullPath))
        {
            currentData = defaultData.defaultData;
            SaveAll();
            return;
        }

        try
        {
            string json = File.ReadAllText(fullPath);

            // Unwrap old-style JSON if needed
            if (json.StartsWith("\"") && json.EndsWith("\""))
            {
                json = json.Substring(1, json.Length - 2);
                json = json.Replace("\\\"", "\"");
                json = json.Replace("\\r\\n", "\n");
            }

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new BigDoubleConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StringReader sr = new StringReader(json))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                currentData = serializer.Deserialize<SaveData>(reader);
            }

            if (currentData == null)
            {
                Debug.LogWarning("SaveData deserialized as null. Using default.");
                currentData = defaultData.defaultData;
                SaveAll();
            }

        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read save, restoring default: {e.Message}");
            currentData = defaultData.defaultData;
            SaveAll();
        }

        // Load runes from saved data
        if (RuneInventoryManager.Instance != null)
        {
            RuneInventoryManager.Instance.LoadRunesFromData(RuneInventoryManager.Instance.runeDatabase.allRunes);
        }
    }


    // ----------------- RESET -----------------
    public void OnResetValueChange() => currentResetKey = resetField.text.Trim().ToUpper();

    public void DeleteData()
    {
        OnResetValueChange();
        if (currentResetKey != resetKey) return;

        string fullPath = Path.Combine(Application.persistentDataPath, filePath, fileName);

        try
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            currentData = defaultData.defaultData;
            SaveAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete save data: {ex.Message}");
        }
    }

    // ----------------- OFFLINE EARNINGS -----------------
    private void HandleOfflineEarnings()
    {
        if (currentData == null) Load();
        if (currentData.lastSaveTime == 0) return;

        DateTime lastSave = DateTime.FromBinary(currentData.lastSaveTime);
        TimeSpan timeAway = DateTime.Now - lastSave;

        BigDouble moneyPerSecond = UpgradeManager.instance?.moneyPerSecond ?? 0;
        float multiplier = currentData.offlineEarningsMultiplier;

        BigDouble earnings = moneyPerSecond * timeAway.TotalSeconds * multiplier;
        BigDouble maxEarnings = (moneyPerSecond * 5) * (3600 * 24); // 24-hour cap
        earnings = BigDouble.Min(earnings, maxEarnings);

        if (earnings > 0)
        {
            currentData.moneyCount += earnings;

            if (OfflineEarningsUI.instance != null)
                OfflineEarningsUI.instance.Show(earnings, multiplier, timeAway.TotalMinutes);
            else
                Debug.Log($"You earned {earnings} while offline for {timeAway.TotalMinutes:F1} minutes!");
        }
    }
}
