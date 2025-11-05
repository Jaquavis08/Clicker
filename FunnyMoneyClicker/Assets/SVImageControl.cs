using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SVImageControl : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField] private Image pickerImage;

    private RawImage SVimage;
    private ColorPickerControl CC;
    private RectTransform rectTransform, pickerTransform;

    private void Awake()
    {
        SVimage = GetComponent<RawImage>();
        CC = FindAnyObjectByType<ColorPickerControl>();
        rectTransform = GetComponent<RectTransform>();
        pickerTransform = pickerImage.GetComponent<RectTransform>();
    }

    private void UpdateColor(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        // Normalize position between 0–1
        float xNorm = Mathf.Clamp01((localPoint.x + width * 0.5f) / width);
        float yNorm = Mathf.Clamp01((localPoint.y + height * 0.5f) / height);

        // Set picker position
        pickerTransform.localPosition = new Vector2(
            Mathf.Lerp(-width * 0.5f, width * 0.5f, xNorm),
            Mathf.Lerp(-height * 0.5f, height * 0.5f, yNorm)
        );

        // Value should increase from bottom → top
        float correctedValue = yNorm;

        // Update displayed color of the picker
        pickerImage.color = Color.HSVToRGB(CC.currentHue, xNorm, correctedValue);

        // Update actual color selection
        CC.SetSV(xNorm, correctedValue);
    }

    public void OnDrag(PointerEventData eventData) => UpdateColor(eventData);

    public void OnPointerClick(PointerEventData eventData) => UpdateColor(eventData);
}
