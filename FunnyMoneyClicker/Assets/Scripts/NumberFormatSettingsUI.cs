using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NumberFormatSettingsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown formatDropdown; // Assign in Inspector

    private void Start()
    {
        // Populate dropdown options if not already set
        if (formatDropdown.options.Count == 0)
        {
            formatDropdown.options.Add(new TMP_Dropdown.OptionData("Suffix (K, M, B...)"));
            formatDropdown.options.Add(new TMP_Dropdown.OptionData("Scientific (1.23e+12)"));
        }

        // Load saved format
        int savedMode = (int)NumberFormatter.GetFormatMode();
        formatDropdown.value = savedMode;
        formatDropdown.RefreshShownValue();

        // Add listener
        formatDropdown.onValueChanged.AddListener(OnFormatModeChanged);
    }

    private void OnDestroy()
    {
        formatDropdown.onValueChanged.RemoveListener(OnFormatModeChanged);
    }

    private void OnFormatModeChanged(int index)
    {
        NumberFormatter.SetFormatMode((NumberFormatter.FormatMode)index);

        Debug.Log($"Number format changed to: {NumberFormatter.GetFormatMode()}");
    }
}
