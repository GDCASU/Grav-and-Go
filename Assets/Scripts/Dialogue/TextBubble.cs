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

    enum BubbleState
    {
        PoppingIn,
        Typing,
        Finished,
        ClosingOut,
        Closed
    }

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
    public float popinTime = 0.5f;
    public float popoutTime = 0.5f;

    /// <summary>
    /// Gets whether or not the text bubble is done showing its typing animation
    /// </summary>

    private BubbleState state = BubbleState.PoppingIn;
    private SimpleAudioEmitter audioEmitter;
    private float elapsedTimer = 0;

    private void Awake()
    {
        audioEmitter = GetComponent<SimpleAudioEmitter>();
        DialogueManager.Instance.onContinue.AddListener(OnDialogueContinue);
    }

    private void OnDestroy()
    {
        DialogueManager.Instance.onContinue.RemoveListener(OnDialogueContinue);
    }

    /// <summary>
    /// Initializes the text bubble and starts its processes
    /// </summary>
    public void Init(Speaker speaker, Dialogue.Line line)
    {
        Line = line;
        uiReferences.lineText.text = "";
        uiReferences.lineText.color = speaker.textColor;
    }


    private void OnDialogueContinue()
    {
        SwitchState(state + 1);
    }

    private void Update()
    {
        switch (state)
        {
            case BubbleState.PoppingIn:
                if(elapsedTimer <= popinTime)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, elapsedTimer / popinTime);
                }
                else
                {
                    transform.localScale = Vector3.one;
                    SwitchState(BubbleState.Typing);
                }

                elapsedTimer += Time.deltaTime;

                break;

            case BubbleState.Typing:

                if(uiReferences.lineText.text != Line.text)
                {
                    if(elapsedTimer > 1f / typewriterSpeed)
                    {
                        uiReferences.lineText.text += Line.text[uiReferences.lineText.text.Length];
                        elapsedTimer = 0f;
                    }
                    else
                    {
                        elapsedTimer += Time.deltaTime;
                    }
                }
                else
                {
                    SwitchState(BubbleState.Finished);
                }

                break;

            case BubbleState.Finished:
                break;

            case BubbleState.ClosingOut:
                if (elapsedTimer <= popinTime)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, elapsedTimer / popoutTime);
                    elapsedTimer += Time.deltaTime;
                }
                else
                {
                    transform.localScale = Vector3.zero;
                    SwitchState(BubbleState.Closed);
                }

                break;

        }
    }

    private void SwitchState(BubbleState newState)
    {
        switch (newState)
        {
            case BubbleState.PoppingIn:
                transform.localScale = Vector3.zero;
                break;

            case BubbleState.Typing:
                transform.localScale = Vector3.one;
                uiReferences.lineText.text = "";
                break;

            case BubbleState.Finished:
                uiReferences.lineText.text = Line.text;
                break;

            case BubbleState.ClosingOut:
                break;

            case BubbleState.Closed:
                Destroy(gameObject);
                break;
        }

        state = newState;
    }
}
