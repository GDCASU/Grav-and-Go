using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    TMP_Text textBox;
    string resultText;

    public float letterFrequency;
    float currentTime;

    Coroutine coroutine;
    bool textUpdating = false;

    private void Start()
    {
        textBox = GetComponent<TMP_Text>();
    }

    public bool UpdateText(string text)
    {
        if (textUpdating) { 
            StopCoroutine(coroutine); 
            textUpdating = false;  
            textBox.text = resultText;
            return false;
        }
        else
        {
            resultText = text;
            coroutine = StartCoroutine(TypeWriterText(text));
            return true;
        }
    }

    IEnumerator TypeWriterText(string text)
    {
        textUpdating = true;
        string currentText = "";
        foreach (char letter in text)
        {
            currentText += letter;
            textBox.text = currentText;

            yield return new WaitForSeconds(letterFrequency);
        }
        textUpdating = false;
    }
}
