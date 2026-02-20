using UnityEngine;

public class RuneSlot : MonoBehaviour
{
    public RuneButton currentRune;

    public bool IsEmpty()
    {
        return currentRune == null;
    }

    public void SetRune(RuneButton rune)
    {
        currentRune = rune;
    }

    public void ClearSlot()
    {
        currentRune = null;
    }
}