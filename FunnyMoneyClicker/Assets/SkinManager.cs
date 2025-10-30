using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkinManager : MonoBehaviour
{
    public static SkinManager instance;
    public ClickerDatabase clickerDatabase;
    public Transform skinParent; // where the skin prefab will spawn

    private ClickerItem currentSkinObject;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ApplyEquippedSkin();
    }

    public void EquipSkin(string skinId)
    {
        if (!SaveDataController.currentData.unlockedSkins.Contains(skinId))
        {
            Debug.LogWarning($"Cannot equip skin {skinId} (not unlocked)");
            return;
        }

        SaveDataController.currentData.equippedSkinId = skinId;
        ApplyEquippedSkin();
    }

    private void ApplyEquippedSkin()
    {
        //if (currentSkinObject != null)
        //    Destroy(currentSkinObject);

        string id = SaveDataController.currentData.equippedSkinId;
        var clickerData = clickerDatabase.allClickers.FirstOrDefault(s => s.skinId == id);
        if (clickerData != null && clickerData.clickerSkin != null)
        {
            currentSkinObject = clickerData;
            ClickerManager.instance.clickerItem = currentSkinObject;
            skinParent.GetComponent<Image>().sprite = clickerData.clickerSkin;
        }
        else
        {
            Debug.LogWarning($"Skin {id} not found in database!");
        }
    }
}
