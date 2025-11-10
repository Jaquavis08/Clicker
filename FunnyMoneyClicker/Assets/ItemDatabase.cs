using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;

    [Header("All Items")]
    public List<Item> allItems = new List<Item>();

    [Header("Prefab for UI/Drag")]
    public GameObject itemPrefab;

    private Dictionary<int, Item> itemDict = new Dictionary<int, Item>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Build dictionary for quick lookup
        foreach (var item in allItems)
        {
            if (!itemDict.ContainsKey(item.id))
                itemDict.Add(item.id, item);
        }
    }

    public Item GetItemById(int id)
    {
        if (itemDict.TryGetValue(id, out Item found))
            return found;
        Debug.LogWarning($"Item with ID {id} not found!");
        return null;
    }
}
