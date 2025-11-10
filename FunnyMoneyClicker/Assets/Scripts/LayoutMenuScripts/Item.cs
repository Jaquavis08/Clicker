using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Rarity & Visuals")]
    public Rarity rarity;

    [Header("Effect Settings")]
    public ItemType itemType;
    public double value;

    public int id;
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum ItemType
{
    Upgrader,
    Dropper,
    Processor,
    Special
}