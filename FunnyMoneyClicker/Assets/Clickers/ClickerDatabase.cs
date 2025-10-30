using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClickerDatabase", menuName = "Game/Clicker Database")]
public class ClickerDatabase : ScriptableObject
{
    public List<ClickerItem> allClickers;
}
