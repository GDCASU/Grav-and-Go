using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Cutscene : MonoBehaviour
{
    [SerializeField] Sprite[] backgroundImages;
    [SerializeField] Scene[] scenes;

    Image currentImage;


    Dialogue.Block currentBlock;
    int sceneIndex = 0;

    bool sceneTransitioning;

    private void Start()
    {
        currentImage = GetComponent<Image>();
    }


    public void UpdateScene()
    {
        if (sceneTransitioning) return;

        if (scenes[sceneIndex].UpdateDialogue())
        {
            // continue
        }
        else
        {
            StartCoroutine(SceneTransition());
        }
    }

    IEnumerator SceneTransition()
    {
        sceneTransitioning = true;
        sceneIndex++;
        currentImage.sprite = backgroundImages[sceneIndex];
        scenes[sceneIndex].UpdateDialogue();
        sceneTransitioning = false;

        yield return null;
    }
}
