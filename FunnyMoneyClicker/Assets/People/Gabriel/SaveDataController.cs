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
        currentData = Serializer.Load(defaultData.defaultData, Path.Combine(Application.persistentDataPath, filePath), fileName);
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
