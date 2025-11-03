using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ManageUI : MonoBehaviour
{
    public List<GameObject> UI = new List<GameObject>();

    public void UIManage(int uiIndex)
    {
        if (uiIndex < 1 || uiIndex > UI.Count)
        {
            foreach (var uiElement in UI)
                uiElement.SetActive(false);
            return;
        }

        if (UI[uiIndex - 1].activeSelf)
        {
            foreach (var uiElement in UI)
                uiElement.SetActive(false);
        }
        else
        {
            // Disable all first
            foreach (var uiElement in UI)
                uiElement.SetActive(false);

            UI[uiIndex - 1].SetActive(true);
        }
    }

}
