using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Serializer
{
    public static bool isSaving = false;

    public static T Load<T>(T defaultValue, string filePath, string fileName, JsonConverter[] converters = null)
    {
        string fullPath = Path.Combine(filePath, fileName);
        string backupPath = fullPath + ".bak";
        T loadedData = defaultValue;

        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Converters = converters ?? new JsonConverter[0],
            NullValueHandling = NullValueHandling.Ignore
        };

        try
        {
            string json;

            if (File.Exists(fullPath))
                json = File.ReadAllText(fullPath);
            else if (File.Exists(backupPath))
                json = File.ReadAllText(backupPath);
            else
                return DeepCopy(defaultValue);

            // Fix wrapped JSON from older saves
            if (json.StartsWith("\"") && json.EndsWith("\""))
            {
                json = json.Substring(1, json.Length - 2);
                json = json.Replace("\\\"", "\"");
                json = json.Replace("\\r\\n", "\n");
            }

            loadedData = JsonConvert.DeserializeObject<T>(json, settings) ?? DeepCopy(defaultValue);
        }
        catch
        {
            loadedData = DeepCopy(defaultValue);
        }

        return loadedData;
    }

    public static void Save<T>(T data, string filePath, string fileName)
    {
        if (isSaving) return;
        isSaving = true;

        try
        {
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            string json = JsonConvert.SerializeObject(
                data,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new BigDoubleConverter() },
                    NullValueHandling = NullValueHandling.Ignore
                });

            string fullPath = Path.Combine(filePath, fileName);
            string tempPath = fullPath + ".tmp";

            if (File.Exists(fullPath))
                File.Copy(fullPath, fullPath + ".bak", true);

            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, fullPath, true);
            File.Delete(tempPath);

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
