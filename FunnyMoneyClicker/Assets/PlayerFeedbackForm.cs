using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class PlayerFeedbackForm : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField entry1QuestionField;
    public TMP_InputField entry2QuestionField;
    public TMP_InputField entry3QuestionField;
    public TMP_InputField entry4QuestionField;
    public TMP_InputField entry5QuestionField;
    public TMP_InputField entry6QuestionField;
    public TMP_InputField entry7QuestionField;
    public TMP_InputField entry8QuestionField;
    public TMP_InputField entry9QuestionField;
    public TMP_InputField entry10QuestionField;
    public TMP_InputField entry11QuestionField;
    public TMP_InputField entry12QuestionField;
    public TMP_InputField entry13QuestionField;
    public TMP_InputField entry14QuestionField;
    public TMP_InputField entry15QuestionField;
    public TMP_InputField entry16QuestionField;

    public Button submitButton;

    [Header("Google Form Info")]
    private string googleFormURL = "https://docs.google.com/forms/d/e/1FAIpQLSeCoqBl2bD5cm-A0KleIyELt6CFOvTa9dazonURPC9Ikaxu-A/formResponse";

    // Using all the entries from your original URL
    private string entry1 = "entry.397724331";
    private string entry2 = "entry.912916300";
    private string entry3 = "entry.1145368364";
    private string entry4 = "entry.335606283";
    private string entry5 = "entry.1169989219";
    private string entry6 = "entry.790855912";
    private string entry7 = "entry.1837706941";
    private string entry8 = "entry.858628972";
    private string entry9 = "entry.1452851885";
    private string entry10 = "entry.457240469";
    private string entry11 = "entry.1577053512";
    private string entry12 = "entry.157374896";
    private string entry13 = "entry.2053543427";
    private string entry14 = "entry.298848723";
    private string entry15 = "entry.1242770074";
    private string entry16 = "entry.1389126750";

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
    }

    public void OnSubmit()
    {
        StartCoroutine(SendFeedback());
    }

    IEnumerator SendFeedback()
    {
        WWWForm form = new WWWForm();

        // Assign input values to entry fields
        //form.AddField(entry1, nameInput.text);        // Example: Name
        //form.AddField(entry3, feedbackInput.text);    // Example: Feedback
        //form.AddField(entry4, ratingDropdown.options[ratingDropdown.value].text); // Example: Rating

        // The rest of the entries can be left blank or assigned default values
        form.AddField(entry1, entry1QuestionField.text);
        form.AddField(entry2, entry2QuestionField.text);
        form.AddField(entry3, entry3QuestionField.text);
        form.AddField(entry4, entry4QuestionField.text);
        form.AddField(entry5, entry5QuestionField.text);
        form.AddField(entry6, entry6QuestionField.text);
        form.AddField(entry7, entry7QuestionField.text);
        form.AddField(entry8, entry8QuestionField.text);
        form.AddField(entry9, entry9QuestionField.text);
        form.AddField(entry10, entry10QuestionField.text);
        form.AddField(entry11, entry11QuestionField.text);
        form.AddField(entry12, entry12QuestionField.text);
        form.AddField(entry13, entry13QuestionField.text);
        form.AddField(entry14, entry14QuestionField.text);
        form.AddField(entry15, entry15QuestionField.text);
        form.AddField(entry16, entry16QuestionField.text);

        using (UnityWebRequest www = UnityWebRequest.Post(googleFormURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Form submission failed: " + www.error);
            }
            else
            {
                Debug.Log("Form submitted successfully!");
            }
        }
    }
}
