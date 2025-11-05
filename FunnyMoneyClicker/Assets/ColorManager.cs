using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static ColorManager instance;
    private const string SAVE_KEY = "SavedOutputColor";

    public Color CurrentColor { get; private set; } = new Color(0.27f, 0.56f, 0.20f); // default #448E34

    [Header("Optional Camera")]
    [SerializeField] private Camera targetCamera; // assign Main Camera if you want background color to apply

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        // 🔹 When this becomes active, load color immediately
        LoadColor();

        // 🔹 Apply to camera if assigned
        if (targetCamera != null)
            targetCamera.backgroundColor = CurrentColor;

        // 🔹 Apply to ColorPicker if it exists (even if it's disabled)
        if (ColorPickerControl.instance != null)
        {
            ColorPickerControl.instance.SetFromExternalColor(CurrentColor);
        }
    }

    public void SetColor(Color color)
    {
        CurrentColor = color;
        SaveColor(color);

        // update camera too
        if (targetCamera != null)
            targetCamera.backgroundColor = color;
    }

    private void SaveColor(Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGB(color);
        PlayerPrefs.SetString(SAVE_KEY, hex);
        PlayerPrefs.Save();
    }

    public void LoadColor()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string hex = PlayerPrefs.GetString(SAVE_KEY);
            if (ColorUtility.TryParseHtmlString("#" + hex, out Color loadedColor))
            {
                CurrentColor = loadedColor;
                return;
            }
        }

        // fallback to default green
        if (ColorUtility.TryParseHtmlString("#448E34", out Color defaultColor))
        {
            CurrentColor = defaultColor;
        }
    }
}
