using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

public static class Serializer
{
    public static bool isSaving = false;

    public static T Load<T>(T defaultValue, string filePath, string fileName)
    {
        string fullPath = Path.Combine(filePath, fileName);
        string backupPath = fullPath + ".bak";
        T loadedData = defaultValue;

        if (!File.Exists(fullPath))
        {
            if (File.Exists(backupPath))
            {
                try
                {
                    string backupJson = File.ReadAllText(backupPath);
                    loadedData = JsonConvert.DeserializeObject<T>(backupJson);
                    if (loadedData != null)
                        Debug.Log("Loaded save from backup.");
                    else
                        loadedData = DeepCopy(defaultValue);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to read backup: {ex.Message}");
                    loadedData = DeepCopy(defaultValue);
                }
            }
            else
            {
                Debug.Log("No save or backup found. Using default data.");
                loadedData = DeepCopy(defaultValue);
            }
        }
        else
        {
            try
            {
                string json = File.ReadAllText(fullPath);
                loadedData = JsonConvert.DeserializeObject<T>(json);

                if (loadedData == null)
                {
                    Debug.LogWarning("Main save invalid. Trying backup...");
                    if (File.Exists(backupPath))
                    {
                        string backupJson = File.ReadAllText(backupPath);
                        loadedData = JsonConvert.DeserializeObject<T>(backupJson);
                        if (loadedData != null)
                            Debug.Log("Backup restored successfully.");
                        else
                            loadedData = DeepCopy(defaultValue);
                    }
                    else
                    {
                        loadedData = DeepCopy(defaultValue);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to read main save, restoring backup: {ex.Message}");
                if (File.Exists(backupPath))
                {
                    string backupJson = File.ReadAllText(backupPath);
                    loadedData = JsonConvert.DeserializeObject<T>(backupJson);
                    if (loadedData != null)
                        Debug.Log("Backup restored successfully.");
                    else
                        loadedData = DeepCopy(defaultValue);
                }
                else
                {
                    loadedData = DeepCopy(defaultValue);
                }
            }
        }

        return loadedData;
    }


    public static void Save<T>(T data, string filePath, string fileName)
    {
        if (isSaving) return;
        isSaving = true;

        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        string fullPath = Path.Combine(filePath, fileName);
        string tempPath = fullPath + ".tmp";

        if (File.Exists(fullPath))
            File.Copy(fullPath, fullPath + ".bak", true);

        try
        {
            File.WriteAllText(tempPath, json);       // write to temp
            File.Copy(tempPath, fullPath, true);     // overwrite real save
            File.Delete(tempPath);                   // clean temp
            Debug.Log($"✅ Save data safely written: {fullPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to save data: {ex.Message}");
        }
        finally
        {
            isSaving = false;
        }
    }


    private static T DeepCopy<T>(T obj)
    {
        if (obj == null) return default;
        string json = JsonConvert.SerializeObject(obj);
        return JsonConvert.DeserializeObject<T>(json);
    }
}
