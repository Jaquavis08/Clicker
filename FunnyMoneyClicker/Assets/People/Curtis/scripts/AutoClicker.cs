using UnityEngine;

public class AutoClicker : MonoBehaviour
{
    public bool CreatingDollar = false;
    public static int DollarIncrease = 1;
    public int InternalIncrease;

    void Update()
    {
        InternalIncrease = DollarIncrease;
        if (CreatingDollar == false)
        {
            CreatingDollar = true;
            //StartCoroutine(CreatingDollar)
            ClickerManager.instance.Click();
        }
    }
}
