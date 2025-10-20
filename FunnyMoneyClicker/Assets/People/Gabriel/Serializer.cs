using System.IO;
using UnityEngine;

public static class Serializer
{
    public static T Load<T>(T defaultValue, string filePath, string fileName) 
    {
        if (!File.Exists(Path.Combine(filePath, fileName)))
        {
            Debug.Log($"Save data successfully loaded default as {JsonUtility.ToJson(defaultValue)}");
            return defaultValue;
        }

        using StreamReader sr = new(Path.Combine(filePath, fileName));
        string json = sr.ReadToEnd();

        Debug.Log($"Save data successfully loaded as {json}");

        return JsonUtility.FromJson<T>(json);
    }

    public static void Save<T>(T data, string filePath, string fileName)
    {
        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        string json = JsonUtility.ToJson(data);

        using StreamWriter sw = new(Path.Combine(filePath, fileName));
        sw.Write(json);
        sw.Flush();

        Debug.Log($"Save data successfully saved as {json}");
    }
}