using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuneButton : MonoBehaviour
{
    public RuneItem runeItem;
    public TMP_Text runeName;
    public Image Icon;
    private bool isInventoryButton;
    private int equippedSlotIndex;

    /// <summary>
    /// Setup the button
    /// </summary>
    public void Setup(RuneItem rune, bool inventory = true, int slotIndex = -1)
    {
        runeItem = rune;
        Icon.sprite = rune.runeIcon;
        runeName.text = rune.runeName;
        isInventoryButton = inventory;
        equippedSlotIndex = slotIndex;
    }

    public void OnClick()
    {
        if (isInventoryButton)
        {
            RuneInventoryManager.Instance.EquipRune(runeItem);
        }
        else
        {
            RuneInventoryManager.Instance.UnequipRuneAt(equippedSlotIndex);
        }
    }
}