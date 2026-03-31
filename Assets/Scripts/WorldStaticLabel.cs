using TMPro;
using UnityEngine;

public class WorldStaticLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private string customText = "Label";

    void Reset()
    {
        labelText = GetComponent<TMP_Text>();
    }

    void Start()
    {
        if (labelText == null)
            labelText = GetComponent<TMP_Text>();

        if (labelText != null)
            labelText.text = customText;
    }

    public void SetText(string newText)
    {
        customText = newText;

        if (labelText == null)
            labelText = GetComponent<TMP_Text>();

        if (labelText != null)
            labelText.text = customText;
    }
}