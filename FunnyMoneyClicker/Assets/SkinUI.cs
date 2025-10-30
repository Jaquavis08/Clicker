using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkinUI : MonoBehaviour
{
    public ClickerDatabase skinDatabase;
    public Transform contentParent;
    public GameObject skinButtonPrefab;

    private void Start()
    {
        PopulateSkins();
    }

    private void PopulateSkins()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var skin in skinDatabase.allClickers)
        {
            GameObject btnObj = Instantiate(skinButtonPrefab, contentParent);
            var icon = btnObj.transform.Find("Icon").GetComponent<Image>();
            var nameText = btnObj.transform.Find("Name").GetComponent<TMP_Text>();
            var equipButton = btnObj.transform.GetComponent<Button>();

            icon.sprite = skin.clickerSkin;
            nameText.text = skin.skinName;

            bool unlocked = SaveDataController.currentData.unlockedSkins.Contains(skin.skinId);
            equipButton.interactable = unlocked;

            equipButton.onClick.AddListener(() => SkinManager.instance.EquipSkin(skin.skinId));
        }
    }
}
