using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class Serializer
{
    public static T Load<T>(T defaultValue, string filePath, string fileName)
    {
        string fullPath = Path.Combine(filePath, fileName);

        // If file doesn't exist, return a copy of default data
        if (!File.Exists(fullPath))
        {
            Debug.Log($"No save file found. Using default data: {JsonConvert.SerializeObject(defaultValue)}");
            return DeepCopy(defaultValue);
        }

        string json = File.ReadAllText(fullPath);

        // If file is empty, return default
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log($"Save file empty. Using default data: {JsonConvert.SerializeObject(defaultValue)}");
            return DeepCopy(defaultValue);
        }

        T loadedData = JsonConvert.DeserializeObject<T>(json);

        // If deserialization fails or returns null, return default
        if (loadedData == null)
        {
            Debug.LogWarning("Loaded data was null or incompatible. Using default data.");
            return DeepCopy(defaultValue);
        }

        Debug.Log($"Save data successfully loaded: {json}");
        return loadedData;
    }

    public static void Save<T>(T data, string filePath, string fileName)
    {
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(Path.Combine(filePath, fileName), json);

        Debug.Log($"Save data successfully saved: {json}");
    }

    // Deep copy helper to avoid modifying the ScriptableObject default
    private static T DeepCopy<T>(T obj)
    {
        if (obj == null) return default;
        string json = JsonConvert.SerializeObject(obj);
        return JsonConvert.DeserializeObject<T>(json);
    }
}
