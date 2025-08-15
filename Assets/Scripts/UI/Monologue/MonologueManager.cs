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

    #region Private Variables

    private UnityEvent onLineFinish;
    private bool monologing = false;

    private Coroutine currentLineCoro = null;
    private string[] currentMonolog = { };
    private string currentLine;
    private int currentLineIndex = 0;

    #endregion

    public static MonologueManager Instance;

    #region MonoBehaviour Functions

    /// <summary>
    /// Ensures a singleton instance of the MonologueManager exists and binds input actions.
    /// </summary>
    private void Awake()
    {
        /* -------- Singleton enforcement -------- */
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Bind monologue continuation to the interact event
        InputManager.OnInteract += ContinueMonolog;
    }

    #endregion

    #region Text Binding Functions

    private Dictionary<string, Func<string[], string>> textBindings = new();

    /// <summary>
    /// Registers a text binding key and its associated function for dynamic text replacement.
    /// </summary>
    /// <param name="key">Unique identifier for the text binding.</param>
    /// <param name="func">Function to call when the binding is encountered.</param>
    public void LoadTextBinding(string key, Func<string[], string> func)
    {
        textBindings[key] = func;
    }

    /// <summary>
    /// Removes a registered text binding by key.
    /// </summary>
    /// <param name="key">Text binding key to remove.</param>
    /// <returns>
    /// if the binding existed and was removed successfully
    /// </returns>
    public bool UnloadTextBinding(string key)
    {
        return textBindings.Remove(key);
    }

    /// <summary>
    /// Invokes a registered text binding function.
    /// Throws an exception if the key is not found.
    /// </summary>
    /// <param name="key">Text binding key.</param>
    /// <param name="arg">Optional arguments to pass to the function.</param>
    /// <returns>Processed string result from the binding function.</returns>
    private string CallTextBinding(string key, string[] arg)
    {
        if (!textBindings.ContainsKey(key))
            throw new KeyNotFoundException($"{key} was not found as a text binding");

        return textBindings[key].Invoke(arg);
    }

    #endregion

    #region Monologue Functions

    /// <summary>
    /// Starts a new monologue sequence and displays the first line.
    /// </summary>
    /// <param name="text">TextAsset containing the monologue script.</param>
    public void StartMonolog(TextAsset text)
    {
        if (monologing)
        {
            Debug.LogWarning($"Tried to start this monolog when monolog \"{text.name}\" already started", text);
            return;
        }

        monologing = true;

        // Apply visual effects
        chatContainer.SetActive(true);

        // Load monologue and start first line
        currentMonolog = LoadMonologueFile(text);
        currentLineIndex = 0;

        textLabel.text = "";
        currentLineCoro = StartCoroutine(TypeLine());
    }

    /// <summary>
    /// Continues the current monologue.
    /// Skips typing animation if the line is still being typed, otherwise moves to the next line.
    /// </summary>
    private void ContinueMonolog()
    {
        if (!monologing) return;

        if (currentLineCoro != null) // Skip typing animation
        {
            StopCoroutine(currentLineCoro);
            onLineFinish?.Invoke();
            textLabel.text = currentLine;
            currentLineCoro = null;
            currentLineIndex++;

            nextImage.SetActive(true);
            return;
        }
        else // Move to the next line
        {
            if (currentLineIndex >= currentMonolog.Length) // End monologue
            {
                monologing = false;
                chatContainer.SetActive(false);
                return;
            }

            textLabel.text = "";
            currentLineCoro = StartCoroutine(TypeLine());
        }
    }

    /// <summary>
    /// Coroutine that types out the current line character by character at the set typing speed.
    /// </summary>
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
        onLineFinish?.Invoke();
        currentLineCoro = null;
        currentLineIndex++;
    }

    /// <summary>
    /// Reads and splits the monologue text file into individual lines.
    /// </summary>
    /// <param name="monolog">The TextAsset containing the monologue script.</param>
    /// <returns>Array of monologue lines.</returns>
    private string[] LoadMonologueFile(TextAsset monolog)
    {
        string[] lines = monolog.text.Split('\n');

        // Replace escaped newlines with actual newlines
        foreach (string line in lines)
            while (line.Contains("\\n")) line.Replace("\\n", "\n");

        return lines;
    }

    /// <summary>
    /// Parses a line and replaces any embedded text binding commands with their evaluated results.
    /// </summary>
    /// <param name="line">The line to interpret.</param>
    /// <returns>Processed line with bindings replaced.</returns>
    private string InterpretLine(string line)
    {
        string finalLine = line;

        int textBindingsIndex = finalLine.IndexOf("${");
        List<string> bindingResults = new();

        while (textBindingsIndex > -1)
        {
            // Find full text binding
            int bindingEndIndex = finalLine.IndexOf('}', textBindingsIndex + 2);
            if (bindingEndIndex == -1) throw new Exception($"Unclosed textBindings found in line \"{line}\"");

            // Extract binding key and arguments
            string cmd = finalLine.Substring(textBindingsIndex + 2, bindingEndIndex - textBindingsIndex - 2);
            string[] sections = cmd.Split("?=");
            string[] args = sections.Length > 1 ? sections[1].Split(",") : null;

            string result = CallTextBinding(sections[0], args);

            // Replace occurrences with indexed placeholders
            while (finalLine.Contains("${" + cmd + "}"))
                finalLine = finalLine.Replace("${" + cmd + "}", "{" + bindingResults.Count + "}");

            bindingResults.Add(result);

            textBindingsIndex = finalLine.IndexOf("${");
        }

        return string.Format(finalLine, bindingResults.ToArray());
    }

    #endregion
}
