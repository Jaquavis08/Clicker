using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkinUI : MonoBehaviour
{
    public ClickerDatabase skinDatabase;
    public Transform contentParent;
    public GameObject skinButtonPrefab;
    public TMP_Text tipText;

    public float updateInterval = 1f;
    private float currentInterval = 0f;

    private void Start()
    {
        PopulateSkins();
    }

    public void Update()
    {
        currentInterval += Time.deltaTime;
        if (currentInterval >= updateInterval)
        {
            currentInterval = 0f;
            PopulateSkins();
        }
    }

    private void PopulateSkins()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        int totalSkins = skinDatabase.allClickers.Count;
        int ownedSkins = 0;

        string equippedId = SaveDataController.currentData.equippedSkinId;

        foreach (var skin in skinDatabase.allClickers)
        {
            bool unlocked = SaveDataController.currentData.unlockedSkins.Contains(skin.skinId);
            bool isEquipped = skin.skinId == equippedId;

            if (unlocked)
            {
                ownedSkins++;

                GameObject btnObj = Instantiate(skinButtonPrefab, contentParent);
                var icon = btnObj.transform.Find("Icon").GetComponent<Image>();
                var nameText = btnObj.transform.Find("Name").GetComponent<TMP_Text>();
                var equipButton = btnObj.transform.GetComponent<Button>();
                var bg = btnObj.GetComponent<Image>();

                icon.sprite = skin.clickerSkin;
                nameText.text = skin.skinName;
                equipButton.interactable = unlocked;

                if (isEquipped)
                    bg.color = new Color(0.4f, 0.9f, 0.4f); // light green
                else
                    bg.color = Color.gray;

                equipButton.onClick.AddListener(() =>
                {
                    SkinManager.instance.EquipSkin(skin.skinId);
                    PopulateSkins();
                });
            }
        }

        tipText.text = $"[Skins]: {ownedSkins} / {totalSkins}";
    }
}
