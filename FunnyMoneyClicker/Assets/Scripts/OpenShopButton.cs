using UnityEngine;

public class OpenShopButton : MonoBehaviour
{

    public GameObject shop;
    public void OpenShop()
    {
        // Assuming you have a ShopManager that handles opening the shop UI
        shop.SetActive(!shop.activeSelf);

    }
}
