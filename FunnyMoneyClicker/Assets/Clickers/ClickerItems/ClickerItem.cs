using UnityEngine;

[CreateAssetMenu(fileName = "ClcikerItem", menuName = "Scriptable Objects/ClcikerItem")]
public class ClickerItem : ScriptableObject
{
    public Material clickerMaterial;
    public Sprite clickerSkin;
    public string skinId;
    public string skinName;
    //public float chance = 10f;
    public clickerRarity rarity;
}

public enum clickerRarity { Common, Uncommon, Rare, Epic, Legendary }