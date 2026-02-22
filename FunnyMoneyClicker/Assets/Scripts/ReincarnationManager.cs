using BreakInfinity;
using UnityEngine;

public class ReincarnationManager : MonoBehaviour
{
    public GameObject ReincarnationButton;
    public GameObject RunesButton;

    private float ReincarnationIncreassRequirementRate = 2f;
    private BigDouble BaseMoneyRequirement = 1000000;

    void Update()
    {
        RunesButton.SetActive(SaveDataController.currentData.Reincarnations >= 1);
        ReincarnationButton.SetActive(CanReincarnate());

        Debug.LogWarning(CanReincarnate());
        Debug.LogWarning(SaveDataController.currentData.Reincarnations);
    }

    private bool CanReincarnate()
    {
        return SaveDataController.currentData.moneyCount >= GetReincarnationRequirement();
    }

    private BigDouble GetReincarnationRequirement()
    {
        return BaseMoneyRequirement *
               BigDouble.Pow(ReincarnationIncreassRequirementRate,
               SaveDataController.currentData.Reincarnations);
    }
}