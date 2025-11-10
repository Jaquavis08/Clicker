using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    public List<GridSlot> gridSlots = new List<GridSlot>();

    private void Awake()
    {
        instance = this;
    }

    public double CalculateTotalModifier()
    {
        double totalMultiplier = 1.0;
        double totalAdditive = 0.0;

        foreach (var slot in gridSlots)
        {
            if (slot.currentItem == null) continue;

            Item item = slot.currentItem.item;
            if (item == null) continue;

            switch (item.itemType)
            {
                case ItemType.Upgrader:
                    totalMultiplier *= 1 + item.value;
                    break;
                case ItemType.Dropper:
                    totalAdditive += item.value;
                    break;
                case ItemType.Processor:
                    totalMultiplier *= 1 + item.value * 0.5;
                    break;
                case ItemType.Special:
                    totalMultiplier *= 2;
                    break;
            }
        }

        double finalValue = (1.0 + totalAdditive) * totalMultiplier;
        return finalValue;
    }

    // ----------------- Save & Load -----------------

    public List<ItemSaveData> GetGridSaveData()
    {
        List<ItemSaveData> saveList = new List<ItemSaveData>();

        for (int i = 0; i < gridSlots.Count; i++)
        {
            GridSlot slot = gridSlots[i];
            if (slot.currentItem == null || slot.currentItem.item == null) continue;

            saveList.Add(new ItemSaveData
            {
                slotIndex = i,
                itemId = slot.currentItem.item.id
            });
        }

        return saveList;
    }

    public void LoadGridFromSave(List<ItemSaveData> savedItems)
    {
        // Clear existing items
        foreach (var slot in gridSlots)
        {
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem.gameObject);
                slot.currentItem = null;
            }
        }

        if (savedItems == null) return;

        // Recreate items from saved data
        foreach (var data in savedItems)
        {
            if (data.slotIndex < 0 || data.slotIndex >= gridSlots.Count) continue;

            GridSlot slot = gridSlots[data.slotIndex];
            Item item = ItemDatabase.instance.GetItemById(data.itemId);
            if (item == null) continue;

            GameObject newItem = Instantiate(ItemDatabase.instance.itemPrefab, slot.transform);
            DraggableItem draggable = newItem.GetComponent<DraggableItem>();
            if (draggable == null)
            {
                Debug.LogWarning("Item prefab missing DraggableItem component!");
                continue;
            }

            draggable.item = item;

            // Assign currentSlot so dragging works properly
            draggable.currentSlot = slot;
            slot.currentItem = draggable;

            // Reset transform
            newItem.transform.localPosition = Vector3.zero;
            newItem.transform.localRotation = Quaternion.identity;
            newItem.transform.localScale = Vector3.one;
        }
    }
}
