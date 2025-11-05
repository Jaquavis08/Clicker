using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPickerControl : MonoBehaviour
{
    public static ColorPickerControl instance;

    [Header("HSV Values")]
    [Range(0, 1)] public float currentHue;
    [Range(0, 1)] public float currentSat;
    [Range(0, 1)] public float currentVal;

    [Header("UI References")]
    [SerializeField] private RawImage hueImage;
    [SerializeField] private RawImage satValImage;
    [SerializeField] private RawImage outputImage;
    [SerializeField] private Slider hueSlider;
    [SerializeField] private TMP_InputField hexInputField;

    [Header("Apply Color To")]
    [SerializeField] private Camera targetCamera;

    private Texture2D hueTexture;
    private Texture2D svTexture;
    private Texture2D outputTexture;
    private bool suppressHexUpdate = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();

        // Listen to UI
        hueSlider.onValueChanged.AddListener(delegate { UpdateSVImage(); });
        if (hexInputField != null)
            hexInputField.onEndEdit.AddListener(delegate { OnColorCodeChanged(); });

        // Initialize color from ColorManager
        ApplyColor(ColorManager.instance != null ? ColorManager.instance.CurrentColor : new Color(0.27f, 0.56f, 0.20f));
    }

    private void CreateHueImage()
    {
        hueTexture = new Texture2D(1, 100);
        hueTexture.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < hueTexture.height; i++)
            hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1, 1));

        hueTexture.Apply();
        hueImage.texture = hueTexture;
    }

    private void CreateSVImage()
    {
        svTexture = new Texture2D(100, 100);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        UpdateSVTexture();
        satValImage.texture = svTexture;
    }

    private void CreateOutputImage()
    {
        outputTexture = new Texture2D(1, 1);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        outputImage.texture = outputTexture;
    }

    private void UpdateSVTexture()
    {
        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                float s = (float)x / (svTexture.width - 1);
                float v = (float)y / (svTexture.height - 1);
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, s, v));
            }
        }
        svTexture.Apply();
    }

    public void UpdateSVImage()
    {
        currentHue = hueSlider.value;
        UpdateSVTexture();
        UpdateOutputImage();
    }

    public void SetSV(float s, float v)
    {
        currentSat = Mathf.Clamp01(s);
        currentVal = Mathf.Clamp01(v);
        UpdateOutputImage();
    }

    private void UpdateOutputImage()
    {
        Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

        // Update preview
        outputTexture.SetPixel(0, 0, currentColor);
        outputTexture.Apply();
        outputImage.color = currentColor;

        // Apply to camera
        if (targetCamera != null)
            targetCamera.backgroundColor = currentColor;

        // Update hex input without recursion
        if (hexInputField != null && !suppressHexUpdate)
        {
            suppressHexUpdate = true;
            hexInputField.text = $"#{ColorUtility.ToHtmlStringRGB(currentColor)}";
            suppressHexUpdate = false;
        }

        // Update ColorManager
        if (ColorManager.instance != null)
            ColorManager.instance.SetColor(currentColor);
    }

    public void OnColorCodeChanged()
    {
        if (hexInputField == null) return;

        string hex = hexInputField.text.Trim();
        if (!hex.StartsWith("#")) hex = "#" + hex;

        if (ColorUtility.TryParseHtmlString(hex, out Color parsedColor))
            ApplyColor(parsedColor);
        else
            Debug.LogWarning("Invalid hex color: " + hex);
    }

    private void ApplyColor(Color color)
    {
        Color.RGBToHSV(color, out currentHue, out currentSat, out currentVal);
        hueSlider.value = currentHue;
        UpdateSVTexture();
        UpdateOutputImage();
    }

    public void SetFromExternalColor(Color color)
    {
        Color.RGBToHSV(color, out currentHue, out currentSat, out currentVal);
        hueSlider.value = currentHue;
        UpdateSVTexture();
        UpdateOutputImage();
    }

}
