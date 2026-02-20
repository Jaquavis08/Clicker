using UnityEngine;

[CreateAssetMenu(fileName = "RuneItem", menuName = "Scriptable Objects/RuneItem")]
public class RuneItem : ScriptableObject
{
    public Sprite runeIcon;
    public string runeId;
    public string runeName;
    public double boostmultiplier;
    public runeRarity rarity;
    public runeType type;
}

public enum runeRarity { Common, Uncommon, Rare, Epic, Legendary }
public enum runeType { MoreClick, MoreMoney, MoreLuck }