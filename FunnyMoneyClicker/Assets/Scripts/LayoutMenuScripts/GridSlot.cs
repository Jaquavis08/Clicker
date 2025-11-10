using UnityEngine;

public class GridSlot : MonoBehaviour
{
    public DraggableItem currentItem;

    public void ClearSlot()
    {
        currentItem = null;
    }
}
