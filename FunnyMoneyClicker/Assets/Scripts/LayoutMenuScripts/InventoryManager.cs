using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public Transform inventoryContents;
    public GameObject draggablePrefab;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (SaveDataController.currentData == null) return;

        // If no items, give one starter item
        if (SaveDataController.currentData.inventoryData.itemIds.Count == 0 && SaveDataController.currentData.gridItems.Count == 0)
        {
            var starterItem = ItemDatabase.instance.GetItemById(1);
            if (starterItem != null)
            {
                GameObject newItem = Instantiate(ItemDatabase.instance.itemPrefab, inventoryContents);
                DraggableItem draggable = newItem.GetComponent<DraggableItem>();
                draggable.item = starterItem;
            }
        }
    }

    public void AddItemToInventory()
    {
        GameObject newItem = Instantiate(draggablePrefab, inventoryContents);
        DraggableItem draggable = newItem.GetComponent<DraggableItem>();
    }
}
