using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RuneDatabase", menuName = "Game/Rune Database")]
public class RuneDatabase : ScriptableObject
{
    public List<RuneItem> allRunes;
}
