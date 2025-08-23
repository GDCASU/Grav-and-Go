using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    /* --------------------------------------------------------
     * Author:
     * Cami Lee
     * 
     * Modified By:
     * 
     * Purpose: Create a typewriter effect when text is updated
     * --------------------------------------------------------
    */

    TMP_Text textBox;
    string resultText;

    public float letterFrequency;

    Coroutine coroutine;
    bool textUpdating = false;

    private void Start()
    {
        textBox = GetComponent<TMP_Text>();
    }

    /// <summary> 
    /// Updates text in a typewriter format. 
    /// Returning true means that it is updating the text and 
    /// returning false means that it skipped the typewriter effect
    /// to go to the end of the line
    /// </summary>
    /// <param name="text"> The new text </param>
    /// <returns></returns>
    public bool UpdateText(string text)
    {
        if (textUpdating) { 
            StopCoroutine(coroutine); 
            textUpdating = false;  
            textBox.text = resultText;
            return false;
        }
        else if (text != resultText)
        {
            resultText = text;
            coroutine = StartCoroutine(TypeWriterText(text));
            return true;
        }
        else { return true; }
    }

    /// <summary>
    /// Makes the current text empty
    /// </summary>
    public void ClearText()
    {
        if (textUpdating) { StopCoroutine(coroutine); textUpdating = false; }
        textBox.text = "";
    }

    IEnumerator TypeWriterText(string text)
    {
        textUpdating = true;
        string currentText = "";
        foreach (char letter in text)
        {
            currentText += letter;
            textBox.text = currentText;

            yield return new WaitForSeconds(1/letterFrequency);
        }
        textUpdating = false;
    }
}
