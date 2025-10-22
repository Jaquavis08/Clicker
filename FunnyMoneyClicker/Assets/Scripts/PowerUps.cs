using UnityEngine;
using TMPro;

public class PowerUps : MonoBehaviour
{
    public float maxTime = 1f;
    public float currentTime = 0f;
    public float upgrade1Cost = 10f;
    public TMP_Text upgrade1LevelText;
    public TMP_Text cost1;
    public float moneyPerInterval = 1f;
    public TMP_Text range;


    void Update()
    {
        upgrade1Cost = Mathf.Round(5f * Mathf.Pow(1.5f, SaveDataController.currentData.upgrade1Level + 1));
        if (SaveDataController.currentData.upgrade1Level >= 1 && currentTime >= maxTime)
        {
            moneyPerInterval = Mathf.Round(2f * Mathf.Pow(1.4f, SaveDataController.currentData.upgrade1Level));
            SaveDataController.currentData.moneyCount += moneyPerInterval;

            currentTime = 0f;
        }
        currentTime += Time.deltaTime;
        upgrade1LevelText.text = "Level " + SaveDataController.currentData.upgrade1Level.ToString();
        
        cost1.text = "$" + upgrade1Cost.ToString();
        range.text = "Rate:  " + moneyPerInterval + "/" + maxTime + "s";
    }

    public void ActivatePowerUp1()
    {
        if (SaveDataController.currentData.moneyCount >= upgrade1Cost)
        {
         
            SaveDataController.currentData.moneyCount -= upgrade1Cost;

            SaveDataController.currentData.upgrade1Level += 1;
        }
    }

}
