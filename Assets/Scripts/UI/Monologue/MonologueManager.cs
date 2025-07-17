using System;
using System.Collections.Generic;
using UnityEngine;

public class MonologueManager : MonoBehaviour
{
    public TextAsset test;

    #region Text Binding Functions

    private Dictionary<string, Func<string>> textBindings = new();

    public void LoadTextBinding(string key, Func<string> func)
    {
        textBindings[key] = func;
    }

    public void UnloadTextBinding(string key)
    {
        if(!textBindings.ContainsKey(key)) { throw new KeyNotFoundException($"{key} was not found as a text binding"); }

        textBindings[key] = null;
    }

    public string CallTextBinding(string key)
    {
        if (!textBindings.ContainsKey(key)) { throw new KeyNotFoundException($"{key} was not found as a text binding"); }

        return textBindings[key].Invoke();
    }

    #endregion

    #region Monologue Functions

    public string[] LoadMonologueFile(TextAsset monolog)
    {
        string[] lines = monolog.text.Split('\n');

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
