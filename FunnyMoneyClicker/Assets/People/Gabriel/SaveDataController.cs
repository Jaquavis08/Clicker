using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class SaveDataController : MonoBehaviour
{
    [SerializeField] private string filePath;
    [SerializeField] private string fileName;

    public SaveDataObject defaultData;
    public static SaveData currentData;

    public void Load()
    {
        SaveData loadedData = Serializer.Load(defaultData.defaultData, Path.Combine(Application.persistentDataPath, filePath), fileName);

        currentData = JsonConvert.DeserializeObject<SaveData>(JsonConvert.SerializeObject(loadedData));
    }

    public void Save()
    {
        Serializer.Save(currentData, Path.Combine(Application.persistentDataPath, filePath), fileName);
    }

    private void Awake()
    {
        Load();
    }

    private void OnDestroy()
    {
        Save();
    }
}
