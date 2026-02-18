using FMODUnity;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Dialogue;


/// <summary>
/// The class created to handle the dialog text bubble logic and rendering
/// 
/// Author: Chandler Van
/// </summary>

[RequireComponent(typeof(SimpleAudioEmitter))]
public class TextBubble : MonoBehaviour
{
    [System.Serializable]
    public struct UIReference
    {
        public TMP_Text lineText;
        public ContentSizeFitter centerSizeFitter;
        public HorizontalLayoutGroup centerLayoutGroup;
        public Canvas textBubbleCanvas;
    }

    [Header("Visual References")]
    public UIReference uiReferences;
    public Dialogue.Line Line { get; set; }

    [Header("Visual Settings")]
    public float typewriterSpeed = 60;

    /// <summary>
    /// Gets whether or not the text bubble is done showing its typing animation
    /// </summary>
    public bool IsFinishedTyping { get { return currentCoroutine == null; }  }

    private Coroutine currentCoroutine;
    private SimpleAudioEmitter audioEmitter;

    private void Awake()
    {
        audioEmitter = GetComponent<SimpleAudioEmitter>();
        Init(Line);
    }

    /// <summary>
    /// Initializes the text bubble and starts its processes
    /// </summary>
    public void Init(Dialogue.Line line)
    {
        Line = line;
        uiReferences.lineText.text = "";

        StartTyping();
    }

    /// <summary>
    /// Finishes the line early if still displaying its typing animation
    /// </summary>
    public void Finish()
    {
        if (currentCoroutine == null) // already finished
            return;

        StopCoroutine(currentCoroutine);
    }

    /// <summary>
    /// Closes the typing bubble 
    /// </summary>
    public void Close()
    {
        while (currentCoroutine != null)
            Finish();

        // Stop voiceline
        audioEmitter.StopSound();

        // TODO: Add closing animation if needed
        Destroy(gameObject);
    }

    private void StartTyping()
    {
        currentCoroutine = StartCoroutine(TypingCoroutine());

        if (Line.voiceLine != null)
        {
            audioEmitter.settings.eventReference = (EventReference)Line.voiceLine;
            audioEmitter.PlaySound();
        }
    }

    #region Coroutines

    private IEnumerator PopupCoroutine()
    {
        // TODO: Add Pop up animation if needed
        yield return null;
    }

    private IEnumerator TypingCoroutine()
    {
        try
        {
            uiReferences.lineText.text = "";

            yield return PopupCoroutine();

            for (int i = 0; i < Line.text.Length; i++)
            {
                uiReferences.lineText.text += Line.text[i];
                yield return new WaitForSeconds(1f / typewriterSpeed);
            }
        }
        finally // runs when line is finished or when coroutine is stopped via StopCoroutine(coroutine)
        {
            uiReferences.lineText.text = Line.text;
            uiReferences.textBubbleCanvas.transform.localScale = Vector3.one;

            currentCoroutine = null;
        }
    }

    #endregion

}
