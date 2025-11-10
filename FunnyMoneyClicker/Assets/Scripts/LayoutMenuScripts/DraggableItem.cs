using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DraggableItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public Item item;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Image icon;
    private Transform infoMenu;

    [HideInInspector] public GridSlot currentSlot; // null if in inventory

    private Transform inventoryParent;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        icon = transform.Find("Icon").GetComponent<Image>();

        // Find info menu safely
        Transform t = transform;
        for (int i = 0; i < 6 && t != null; i++) t = t.parent;
        infoMenu = t != null ? t.Find("Info") : null;
    }

    private void Start()
    {
        inventoryParent = InventoryManager.instance?.inventoryContents;
        UpdateVisuals(true); // <-- Force color refresh on start
    }


    private void UpdateVisuals(bool forceInstant = false)
    {
        if (item == null) return;

        if (icon != null && item.icon != null)
            icon.sprite = item.icon;

        var img = GetComponent<Image>();
        if (img != null)
        {
            Color rarityColor = GetRarityColor(item.rarity);
            img.color = rarityColor;

            // Ensure it applies immediately in grid slots
            img.canvasRenderer.SetAlpha(1f);
            img.CrossFadeColor(rarityColor, 0f, true, true);
        }

        if (forceInstant)
        {
            // Force Unity UI to re-render this frame so colors apply correctly
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        transform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        GridSlot targetSlot = eventData.pointerCurrentRaycast.gameObject != null ?
            eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<GridSlot>() : null;

        if (targetSlot != null)
        {
            // Drop into slot
            
            if (targetSlot.currentItem == null)
            {
                print("2");
                MoveToSlot(targetSlot);
            }
            else if (targetSlot.currentItem != this)
            {
                print("3");
                SwapWithSlot(targetSlot);
            }
            else
            {
                print("1");
                MoveToSlot(targetSlot);
            }
        }
        else
        {
            // Drop into inventory
            print("4");
            MoveToInventory();
        }

        UpdateVisuals();
    }

    private void MoveToSlot(GridSlot slot)
    {
        // clear old slot
        if (currentSlot != null)
            currentSlot.currentItem = null;

        // parent to slot
        transform.SetParent(slot.transform, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        slot.currentItem = this;
        currentSlot = slot;

        ForceLayoutRefresh(slot.transform);
    }

    private void SwapWithSlot(GridSlot slot)
    {
        DraggableItem other = slot.currentItem;
        if (other == null)
        {
            MoveToSlot(slot);
            return;
        }

        if (currentSlot != null)
        {
            // Move other to this item's old slot
            other.transform.SetParent(currentSlot.transform, false);
            other.transform.localPosition = Vector3.zero;
            currentSlot.currentItem = other;
            other.currentSlot = currentSlot;
        }
        else
        {
            // Move other to inventory
            other.transform.SetParent(inventoryParent, false);
            other.transform.localPosition = Vector3.zero;
            other.currentSlot = null;
        }

        MoveToSlot(slot);
    }

    private void MoveToInventory()
    {
        if (currentSlot != null)
        {
            currentSlot.currentItem = null;
            currentSlot = null;
        }

        transform.SetParent(inventoryParent, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        ForceLayoutRefresh(inventoryParent);
    }

    private void ForceLayoutRefresh(Transform target)
    {
        if (target == null) return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(target as RectTransform);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (infoMenu == null) return;

        infoMenu.Find("Name").GetComponent<TMP_Text>().text = item.itemName;
        infoMenu.Find("ItemType").GetComponent<TMP_Text>().text = item.itemType.ToString();
        infoMenu.Find("Rarity").GetComponent<TMP_Text>().text = item.rarity.ToString();
        infoMenu.Find("Value").GetComponent<TMP_Text>().text = "Value = " + item.value;
        infoMenu.Find("Description").GetComponent<TMP_Text>().text = "Description: " + item.description;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoMenu == null) return;

        infoMenu.Find("Name").GetComponent<TMP_Text>().text = "";
        infoMenu.Find("ItemType").GetComponent<TMP_Text>().text = "";
        infoMenu.Find("Rarity").GetComponent<TMP_Text>().text = "";
        infoMenu.Find("Value").GetComponent<TMP_Text>().text = "";
        infoMenu.Find("Description").GetComponent<TMP_Text>().text = "";
    }

    private Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => Color.gray,
            Rarity.Uncommon => Color.green,
            Rarity.Rare => Color.blue,
            Rarity.Epic => new Color(0.64f, 0.21f, 0.93f),
            Rarity.Legendary => new Color(1f, 0.65f, 0f),
            _ => Color.red
        };
    }
}
