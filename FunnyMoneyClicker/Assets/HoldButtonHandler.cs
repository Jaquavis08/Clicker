using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public int index; // 1-based (same as your buttons)
    public string type;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (type == "Upgrade")
        {
            UpgradeManager.instance.OnUpgradeButtonDown(index);
        }
        else if (type == "Power")
        {
            Power1.instance.OnPowerButtonDown(index);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (type == "Upgrade")
        {
            UpgradeManager.instance.OnUpgradeButtonUp();
        }
        else if (type == "Power")
        {
            Power1.instance.OnPowerButtonUp();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (type == "Upgrade")
        {
            UpgradeManager.instance.OnUpgradeButtonUp();
        }
        else if (type == "Power")
        {
            Power1.instance.OnPowerButtonUp();
        }
    }
}


