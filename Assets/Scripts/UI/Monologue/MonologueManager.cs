using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class MonologueManager : MonoBehaviour
{
    [Header("References")]
    public TMP_Text textLabel;
    public GameObject nextImage;
    public GameObject chatContainer;

    [Header("Typing Settings")]
    public int typingSpeed = 15;

    private UnityEvent onMonologSkip;
    private bool monologing = false;

    private Coroutine currentLineCoro = null;
    private string[] currentMonolog = { };
    private string currentLine;
    private int currentLineIndex = 0;

    public static MonologueManager Instance;

    #region MonoBehaviour Functions

    private void Awake()
    {
        /* -------- Singleton enforcement -------- */
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // might add on left click to here later who knows
        InputManager.OnInteract += ContinueMonolog;
    }

    #endregion

    #region Input Binding

    #endregion

    #region Text Binding Functions

    private Dictionary<string, Func<string>> textBindings = new();

    public void LoadTextBinding(string key, Func<string> func)
    {
        textBindings[key] = func;
    }

    public void UnloadTextBinding(string key)
    {
        if (!textBindings.ContainsKey(key)) { throw new KeyNotFoundException($"{key} was not found as a text binding"); }

        textBindings[key] = null;
    }

    public string CallTextBinding(string key)
    {
        if (!textBindings.ContainsKey(key)) { throw new KeyNotFoundException($"{key} was not found as a text binding"); }

        return textBindings[key].Invoke();
    }

    #endregion

    #region Monologue Functions

    /// <summary>
    /// Starts a Monolog that freezes time
    /// </summary>
    /// <param name="text">the target monolog TextAsset file</param>
    public void StartMonolog(TextAsset text)
    {
        if (monologing)
        {
            Debug.LogWarning($"Tried to Start this monolog when monolog \"{text.name}\"already started", text);
            return;
        }

        monologing = true;

        // apply visual effects here
        Time.timeScale = 0.0f;
        chatContainer.SetActive(true);

        // load monolog and start first line
        currentMonolog = LoadMonologueFile(text);
        currentLineIndex = 0;

        textLabel.text = "";
        currentLineCoro = StartCoroutine(TypeLine());
    }

    private void ContinueMonolog()
    {
        if (!monologing) return;

        if(currentLineCoro != null) // skip typing animation if still running
        {
            StopCoroutine(currentLineCoro);
            textLabel.text = currentLine;
            currentLineCoro = null;
            currentLineIndex++;

            nextImage.SetActive(true);
            return;
        }
        else // move onto the next line if not
        {
            // if it was the last line end the monolog
            if (currentLineIndex >= currentMonolog.Length)
            {
                monologing = false;
                Time.timeScale = 1;
                chatContainer.SetActive(false);
                return;
            }

            // else start a the next line
            textLabel.text = "";
            currentLineCoro = StartCoroutine(TypeLine());
        }
    }

    private IEnumerator TypeLine()
    {
        currentLine = InterpretLine(currentMonolog[currentLineIndex]);
        nextImage.SetActive(false);

        for (int i = 0; i < currentLine.Length; i++)
        {
            textLabel.text += currentLine[i];
            yield return new WaitForSecondsRealtime(1f / typingSpeed);
        }

        nextImage.SetActive(true);
        currentLineCoro = null;
        currentLineIndex++;
    }

    private string[] LoadMonologueFile(TextAsset monolog)
    {
        string[] lines = monolog.text.Split('\n');

        // add \n to the text
        foreach(string line in lines)
            while (line.Contains("\\n")) line.Replace("\\n", "\n"); 

        return lines;
    }

    private string InterpretLine(string line)
    {
        string finalLine = line;

        int textBindingsIndex = finalLine.IndexOf("${");
        List<string> bindingResults = new();

        while(textBindingsIndex > -1)
        {
            // find full text binding
            int bindingEndIndex = finalLine.IndexOf('}', textBindingsIndex + 2);
            if (bindingEndIndex == -1) throw new Exception($"Unclosed textBindings found in line \"{line}\"");

            // get text binding key
            string key = finalLine.Substring(textBindingsIndex + 2, bindingEndIndex - textBindingsIndex - 2);

            string result = CallTextBinding(key);

            // insert text binding key to location
            while (finalLine.Contains("${" + key + "}")) finalLine = finalLine.Replace("${" + key + "}", "{" + bindingResults.Count + "}");

            bindingResults.Add(result);

            textBindingsIndex = finalLine.IndexOf("${");
        }

        return string.Format(finalLine, bindingResults.ToArray());
    }
    
    #endregion
}
