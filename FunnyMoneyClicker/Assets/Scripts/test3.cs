using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Demonstrates Exception Handling, Data Structures, File I/O, Generics, LINQ and Attributes.
/// Press Space at runtime to run the demo again.
/// </summary>
public class test3 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RunDemo();
    }

    // Update is called once per frame
    void Update()
    {
        // press Space to recompute and log values at runtime
        if (Input.GetKeyDown(KeyCode.Space))
            RunDemo();
    }

    private void RunDemo()
    {
        try
        {
            Debug.Log("=== Demo Start ===");

            // GENERICS: repository for items
            var repo = new Repository<ItemData>("items.json");

            // DATA STRUCTURES: List, Dictionary, Queue, Stack
            var list = new List<ItemData>
            {
                new ItemData(1, "Bronze", 10),
                new ItemData(2, "Silver", 25),
                new ItemData(3, "Gold", 50),
                new ItemData(4, "Platinum", 75)
            };

            var dict = list.ToDictionary(i => i.Id); // Dictionary<int, ItemData>
            var queue = new Queue<ItemData>(list);
            var stack = new Stack<ItemData>(list);

            // Add to generic repository
            foreach (var item in list) repo.Add(item);

            // LINQ: queries over repository
            var expensive = repo.Items.Where(i => i.Value >= 50).OrderByDescending(i => i.Value).ToList();
            var avg = repo.Items.Any() ? repo.Items.Average(i => i.Value) : 0;

            Debug.Log($"Found {expensive.Count} expensive items. Average value = {avg}");

            // FILE I/O with Exception Handling: Save repository to disk
            repo.Save();

            // Clear and reload to demonstrate Load
            repo.Clear();
            repo.Load();

            Debug.Log($"After load: repo has {repo.Items.Count} items");

            // Demonstrate Queue/Stack operations
            if (queue.Count > 0) Debug.Log($"Queue peek: {queue.Peek().Name}");
            if (stack.Count > 0) Debug.Log($"Stack peek: {stack.Peek().Name}");

            Debug.Log("=== Demo End ===");
        }
        catch (Exception ex)
        {
            // Exception handling: log unhandled errors
            Debug.LogError($"Demo failed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}

/// <summary>
/// Custom attribute for demonstration purposes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class DeveloperNoteAttribute : Attribute
{
    public string Note { get; }
    public DeveloperNoteAttribute(string note) => Note = note;
}

/// <summary>
/// Simple interface used as generic constraint (identifiable items).
/// </summary>
public interface IIdentifiable
{
    int Id { get; }
}

/// <summary>
/// Example data class. Marked serializable for Unity's JsonUtility and annotated with custom attribute.
/// </summary>
[Serializable]
[DeveloperNote("Example item used by Repository<T>")]
public class ItemData : IIdentifiable
{
    // Fields must be public for Unity's JsonUtility to serialize them
    public int Id;
    public string Name;
    public float Value;

    // Parameterless constructor needed for deserialization
    public ItemData() { }

    public ItemData(int id, string name, float value)
    {
        Id = id;
        Name = name;
        Value = value;
    }

    // Explicit interface implementation
    int IIdentifiable.Id => Id;
}

/// <summary>
/// Simple serialization wrapper so JsonUtility can (de)serialize lists.
/// </summary>
[Serializable]
public class SerializationWrapper<T>
{
    public List<T> items;
}

/// <summary>
/// GENERICS: basic repository that persists a list of T to a JSON file using Unity's JsonUtility.
/// Constrained to reference types so JsonUtility can handle them; requires that T is serializable for Unity.
/// </summary>
public class Repository<T> where T : class
{
    private readonly string _filePath;
    private List<T> _items = new List<T>();

    public IReadOnlyList<T> Items => _items;
    public string FilePath => _filePath;

    public Repository(string fileName)
    {
        // FILE I/O: store file in persistent data path
        _filePath = Path.Combine(Application.persistentDataPath, fileName);
    }

    public void Add(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    public void Clear() => _items.Clear();

    public void Save()
    {
        try
        {
            var wrapper = new SerializationWrapper<T> { items = _items };
            string json = JsonUtility.ToJson(wrapper, true);

            // Ensure directory exists
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(_filePath, json);
            Debug.Log($"Saved {(_items?.Count ?? 0)} items to {_filePath}");
        }
        catch (UnauthorizedAccessException uaEx)
        {
            Debug.LogError($"Permission error while saving file: {uaEx.Message}");
        }
        catch (IOException ioEx)
        {
            Debug.LogError($"I/O error while saving file: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error while saving: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                Debug.LogWarning($"File not found: {_filePath}");
                return;
            }

            string json = File.ReadAllText(_filePath);
            var wrapper = JsonUtility.FromJson<SerializationWrapper<T>>(json);
            _items = wrapper?.items ?? new List<T>();
            Debug.Log($"Loaded {_items.Count} items from {_filePath}");
        }
        catch (IOException ioEx)
        {
            Debug.LogError($"I/O error while loading file: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error while loading: {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }
}