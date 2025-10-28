//using Newtonsoft.Json;
//using System.IO;
//using UnityEngine;

//public class SaveDataController : MonoBehaviour
//{
//    [SerializeField] private string filePath;
//    [SerializeField] private string fileName;

//    public SaveDataObject defaultData;
//    public static SaveData currentData;

//    public void Load()
//    {
//        SaveData loadedData = Serializer.Load(defaultData.defaultData, Path.Combine(Application.persistentDataPath, filePath), fileName);

//        currentData = JsonConvert.DeserializeObject<SaveData>(JsonConvert.SerializeObject(loadedData));
//    }

//    public void Save()
//    {
//        Serializer.Save(currentData, Path.Combine(Application.persistentDataPath, filePath), fileName);
//    }

//    private void Awake()
//    {
//        Load();
//    }

//    private void OnDestroy()
//    {
//        Save();
//    }
//}



using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public class SaveDataController : MonoBehaviour
{
    [SerializeField] private string filePath;
    [SerializeField] private string fileName;

    public SaveDataObject defaultData;
    public static SaveData currentData;

    private void Awake()
    {
        Load();
    }

    public void Start()
    {
        Invoke(nameof(HandleOfflineEarnings), 0.5f);
    }

    private void OnDestroy()
    {
        Save();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Save(); // save when leaving
        }
        else
        {
            // load and calculate offline earnings when returning
            Load();
            HandleOfflineEarnings();
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void Load()
    {
        SaveData loadedData = Serializer.Load(defaultData.defaultData, Path.Combine(Application.persistentDataPath, filePath), fileName);
        currentData = JsonConvert.DeserializeObject<SaveData>(JsonConvert.SerializeObject(loadedData));
    }

    public void Save()
    {
        currentData.lastSaveTime = DateTime.Now.ToBinary();
        Serializer.Save(currentData, Path.Combine(Application.persistentDataPath, filePath), fileName);
    }

    private void HandleOfflineEarnings()
    {
        if (currentData.lastSaveTime == 0)
        {
            Debug.Log("No previous save time found — skipping offline earnings.");
            return;
        }

        DateTime lastSave = DateTime.FromBinary(currentData.lastSaveTime);
        TimeSpan timeAway = DateTime.Now - lastSave;
        double secondsAway = timeAway.TotalSeconds;

        double moneyPerSecond = UpgradeManager.instance != null ? UpgradeManager.instance.moneyPerSecond : 0;
        float multiplier = currentData.offlineEarningsMultiplier;

        double earnings = moneyPerSecond * secondsAway * multiplier;

        double maxEarnings = moneyPerSecond * 3600 * 12; // 12 = max hours
        earnings = Math.Min(earnings, maxEarnings);

        Debug.Log($"Last Save: {lastSave}");
        Debug.Log($"Time Away: {timeAway.TotalMinutes:F2} minutes ({secondsAway:F0} seconds)");
        Debug.Log($"Money Per Second: {moneyPerSecond}");
        Debug.Log($"Offline Multiplier: {multiplier}");
        Debug.Log($"Calculated Earnings: {earnings}");

        if (earnings > 0)
        {
            currentData.moneyCount += earnings;

            if (OfflineEarningsUI.instance != null)
                OfflineEarningsUI.instance.Show(earnings, multiplier, timeAway.TotalMinutes);
            else
                Debug.Log($"💰 You earned ${NumberFormatter.Format(earnings)} while offline for {timeAway.TotalMinutes:F1} minutes!");
        }
        else
        {
            Debug.Log("No earnings — maybe moneyPerSecond is 0 or timeAway too short.");
        }
    }

}
