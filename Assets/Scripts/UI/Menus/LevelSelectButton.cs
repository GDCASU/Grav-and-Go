using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UI;

/*
-------------------------------
Authors:
Jacob, Rebekah
-------------------------------
*/

public class LevelSelectButton : MonoBehaviour
{
    // Whether or not this button will progress index /go back
    [Header("Button goes for/back")]
    [SerializeField] private bool moveForward; 

    // chapterdata scriptable object
    public ChapterData chapterData;

    // for referencing the script in the actual level button 
    public LevelButton levelButton;

    //not used
    //private Sprite currentLevelPreview;

    // final string to send for loadlevel function
    private string selectedLevelName;

    /////////////////////////////////////////////////////////////
    
    private void Start()

    {
        ///Defining the scriptableo jbect
        chapterData = Resources.Load<ChapterData>("LevelSelectScreen/ChapterData");

        ////define level button instance as the script in the level button
        levelButton = gameObject.transform.parent.GetComponentInChildren<LevelButton>();

        chapterData.selectedLevelIndex = 1; //Begin at 1
        chapterData.selectedChapterIndex = 1; //begin at 1 (function should already compensates for array start at 0)

    }

    private void getCabinetSprite(GameObject storedChapterCabinet)
    {
        Sprite cabinetSprite;
        string spriteName = "Chapter " + chapterData.selectedChapterIndex + " Cabinet";
        cabinetSprite = Resources.Load<Sprite>("LevelSelectScreen/Cabinet Sprites/" + spriteName);

         if (cabinetSprite != null)

        {
            //If valid cabinet sprite then apply it the the image component
            Image image = storedChapterCabinet.GetComponent<Image>();
            image.sprite = cabinetSprite;
        }
        /*
        else

        {
            // No such image exists in folder or there is no preview for this level
        }
        */
        
    }

    private void getLevelPreview(string levelName)

    {
        /*Search for an level preview/image of the currently selected level in 
        the appropriate folder, it should have the same name as the level that 
        it reprsents. so if it exists it will be found by searching for it with 
        levelname string */
        Sprite levelPreview;
        levelPreview = Resources.Load<Sprite>("LevelSelectScreen/Level Previews/" + levelName);

        if (levelPreview != null)

        {

            ///If does exist/was found set image component of button to be it.
            Image image = levelButton.gameObject.GetComponent<Image>();
            image.sprite = levelPreview;

        }
        /*
        else

        {
            // No such image exists in folder or there is no preview for this level
        }
        */
    }

    /*
    private void zoomCanvas(bool zoomIn, GameObject canvas, GameObject objectParent)
    {
        GameObject parentObject = objectParent.GetComponent<Transform>().parent.gameObject;
        float step = 40f * Time.deltaTime;
        float desiredScale;
        float desiredYpos = -500f;

        if (zoomIn)
        {
            desiredScale = 4f;

        }
        else
        {
            desiredScale = 1f;
        }

        IEnumerator Zoom()
        {
            while (parentObject.GetComponent<RectTransform>().localScale.x != desiredScale)
            {
                parentObject.GetComponent<RectTransform>().localScale = UnityEngine.Vector3.MoveTowards(parentObject.GetComponent<RectTransform>().localScale, new UnityEngine.Vector3(desiredScale, desiredScale, desiredScale), step);
                //parentObject.GetComponent<RectTransform>().anchoredPosition = UnityEngine.Vector3.MoveTowards(parentObject.GetComponent<RectTransform>().anchoredPosition, new UnityEngine.Vector3(0, desiredYpos, 0), step);
                //
                yield return new WaitForSeconds(0.01f);
            }
            //At end of routine
            parentObject.GetComponent<RectTransform>().anchoredPosition = new UnityEngine.Vector3(0, desiredYpos, 0);
        }
        StartCoroutine(Zoom());
    }

    */
    private void playCabinetAnimation(GameObject storedChapterCabinet, GameObject currentChapterCabinet, LevelSelectButton[] interactableButtons, GameObject objectParent)
    {
        // Zoom out of the canvas first
        GameObject parentObject = objectParent.GetComponent<Transform>().parent.gameObject;
        float stepZoom = 40f * Time.deltaTime;
        float zoomDesiredScale = 1f; // 1
        float zoomDesiredYpos = 0f; // y pos to 0

        IEnumerator ZoomOut()
        {
            //Fix the position BEFORE zooming out so the canvas doesn't go off screen basically
            parentObject.GetComponent<RectTransform>().anchoredPosition = new UnityEngine.Vector3(0, zoomDesiredYpos, 0);

            while (parentObject.GetComponent<RectTransform>().localScale.x != zoomDesiredScale)
            {
                parentObject.GetComponent<RectTransform>().localScale = UnityEngine.Vector3.MoveTowards(parentObject.GetComponent<RectTransform>().localScale, new UnityEngine.Vector3(zoomDesiredScale, zoomDesiredScale, zoomDesiredScale), stepZoom);
                //parentObject.GetComponent<RectTransform>().anchoredPosition = UnityEngine.Vector3.MoveTowards(parentObject.GetComponent<RectTransform>().anchoredPosition, new UnityEngine.Vector3(0, desiredYpos, 0), step);
                //
                yield return new WaitForSeconds(0.01f);
            }
            //At end of routine
            
            //Cabinet movement routine starts after this one is done. should wait for this one to finish otherwise it will move before this one is done.
            StartCoroutine(ArcadeCabinetMoveLoop());
        }
        ///////
        
        //Cabinet Animation//

        float step = 4000 * Time.deltaTime;
        float distance = UnityEngine.Vector3.Distance(storedChapterCabinet.GetComponent<RectTransform>().anchoredPosition, new UnityEngine.Vector3(0f, 0f, 0f));
        float direction;

        /*direction value is just arbitrary position off screen relative to canvas, should be fine but if the sprite for the final
        image is bigger then might need to be more off screen*/
        if (moveForward)
        {
            direction = 1200f;
        }
        else
        {
            direction = -1200f;
        }

        //Move the stored one and other to appropriate side, it's just whichever is not the current one, but the side should be different depending on direction
        storedChapterCabinet.GetComponent<RectTransform>().anchoredPosition = new UnityEngine.Vector3(direction, 0f, 0f);
        currentChapterCabinet.GetComponent<RectTransform>().anchoredPosition = new UnityEngine.Vector3(0f, 0f, 0f);

        IEnumerator ArcadeCabinetMoveLoop()
        {
            while (distance > 0)
            {
                storedChapterCabinet.GetComponent<RectTransform>().anchoredPosition = UnityEngine.Vector3.MoveTowards(storedChapterCabinet.GetComponent<RectTransform>().anchoredPosition, new UnityEngine.Vector3(0f, 0f, 0f), step);
                currentChapterCabinet.GetComponent<RectTransform>().anchoredPosition = UnityEngine.Vector3.MoveTowards(currentChapterCabinet.GetComponent<RectTransform>().anchoredPosition, new UnityEngine.Vector3(-direction, 0f, 0f), step);
                //
                distance = UnityEngine.Vector3.Distance(storedChapterCabinet.GetComponent<RectTransform>().anchoredPosition, new UnityEngine.Vector3(0f, 0f, 0f));
                //
                yield return new WaitForSeconds(0.01f);
            }
            //At end of routine, re-enable buttons, also zoom back out
            foreach (LevelSelectButton currentComponent in interactableButtons)
            {
                currentComponent.gameObject.GetComponent<Image>().enabled = true;
                currentComponent.gameObject.GetComponent<Button>().interactable = true;
            }
            levelButton.gameObject.GetComponent<Image>().enabled = true;
            levelButton.gameObject.GetComponent<Button>().interactable = true;
            
            StartCoroutine(ZoomIn()); // Zoom back out
            ////
        }

        //Zoom in 
        
        IEnumerator ZoomIn()
        {
            zoomDesiredScale = 4f;
            zoomDesiredYpos = -500f;

            while (parentObject.GetComponent<RectTransform>().localScale.x != zoomDesiredScale)
            {
                parentObject.GetComponent<RectTransform>().localScale = UnityEngine.Vector3.MoveTowards(parentObject.GetComponent<RectTransform>().localScale, new UnityEngine.Vector3(zoomDesiredScale, zoomDesiredScale, zoomDesiredScale), stepZoom);
                //parentObject.GetComponent<RectTransform>().anchoredPosition = UnityEngine.Vector3.MoveTowards(parentObject.GetComponent<RectTransform>().anchoredPosition, new UnityEngine.Vector3(0, desiredYpos, 0), step);
                //
                yield return new WaitForSeconds(0.01f);
            }
            //At end of routine
            
            //Correct screen position
             parentObject.GetComponent<RectTransform>().anchoredPosition = new UnityEngine.Vector3(0, zoomDesiredYpos, 0);
        }

        StartCoroutine(ZoomOut()); // Start zoom out
    }
    

    // For the animation when the chapter changes (move chapters button is selected)
    private void updateChapterCabinet()
    {
        ///Getting all of the buttons to control enabling/disabling
        GameObject levelButtonObject = levelButton.gameObject;
        GameObject levelSelectParent = levelButtonObject.transform.parent.gameObject;
        GameObject canvas = levelSelectParent.transform.parent.gameObject;
        LevelSelectButton[] interactableButtons = levelSelectParent.GetComponentsInChildren<LevelSelectButton>();

        GameObject objectParent = null;
        GameObject currentChapterCabinet = null;
        GameObject storedChapterCabinet = null;
        levelButtonObject.GetComponent<Image>().enabled = false;

        /*hidie all of the imagecomponents so buttons disappear and then
        Make them uninteractable*/
        foreach (LevelSelectButton currentComponent in interactableButtons)
        {

            currentComponent.gameObject.GetComponent<Image>().enabled = false;
            currentComponent.gameObject.GetComponent<Button>().interactable = false;

        }
        /*Player should not be able to interact with the button 
        while it's being moved to the next cabinet*/
        levelButtonObject.GetComponent<Button>().interactable = false;
        
        //for finding the arcade object parent
        foreach (Transform currentComponent in canvas.GetComponent<Transform>())
        {
            if (currentComponent.TryGetComponent<CanvasGroup>(out var T) != false)
            {
                objectParent = currentComponent.gameObject;
                break;
            }
        }

        //define which object is currently in use/not
        //which cabinet is currently the one is use will be determined by which is NOT at the center of screen
        foreach (Transform currentComponent in objectParent.GetComponent<Transform>())
        {
            if (currentComponent.GetComponent<RectTransform>().anchoredPosition.x == 0f)
            {

                currentChapterCabinet = currentComponent.gameObject;
            }
            else
            {
                storedChapterCabinet = currentComponent.gameObject;
            }
        }
        getCabinetSprite(storedChapterCabinet);
        playCabinetAnimation(storedChapterCabinet, currentChapterCabinet, interactableButtons, objectParent);
    }

    public void ChangeSelectedLevel()

    {
        //
        if (moveForward)
        {
            //maxlevels = value of this index, so index 0 (ch.1) = #levels
            int maxLevels = chapterData.TotalLevelsInChapter[chapterData.selectedChapterIndex - 1];

            ///Changing the index but make surei t cannot go past the actual # of levels or negative
            chapterData.selectedLevelIndex = Math.Clamp(chapterData.selectedLevelIndex + 1, 1, maxLevels); // cannot go past the max level of #s for the current chapter

            //Creating the string for the name, all existing levels currently follow format of "Chapter n Level n", so can just put in the index
            selectedLevelName = "Chapter " + chapterData.selectedChapterIndex + " Level " + chapterData.selectedLevelIndex;

            /*the reference in level button script in the level button has name set to this, 
            so the right level is called when that script uses the loadLevelIfUnlocked function */
            levelButton._levelReference.name = selectedLevelName;

            getLevelPreview(selectedLevelName);

            /*
            print(chapterData.selectedLevelIndex);
            print(maxLevels);
            print(selectedLevelName);*/
        }

        else // If set to move backward instead, same but index - 1
        {
            int maxLevels = chapterData.TotalLevelsInChapter[chapterData.selectedChapterIndex - 1];

            chapterData.selectedLevelIndex = Math.Clamp(chapterData.selectedLevelIndex - 1, 1, maxLevels);

            selectedLevelName = "Chapter " + chapterData.selectedChapterIndex + " Level " + chapterData.selectedLevelIndex;

            levelButton._levelReference.name = selectedLevelName;

            getLevelPreview(selectedLevelName);

            /*
            print(chapterData.selectedLevelIndex);
            print(maxLevels);
            print(selectedLevelName);*/
        }
        //

    }

    public void ChangeSelectedChapter()
    {
        int lastSelected;
        if (moveForward)
        {
            //total # of chapters = total # of entries in the array
            int maxChapters = chapterData.TotalLevelsInChapter.Length;

            lastSelected = chapterData.selectedChapterIndex;

            chapterData.selectedChapterIndex = Math.Clamp(chapterData.selectedChapterIndex + 1, 1, maxChapters);

            /*level index and selected level goes back to 1 
            when chapter is changed, because chapters might 
            have varying levels, so want to make sure the
            index is within the max #levels in the CURRENT chapter*/
            chapterData.selectedLevelIndex = 1;
            selectedLevelName = "Chapter " + chapterData.selectedChapterIndex + " Level " + chapterData.selectedLevelIndex;

            levelButton._levelReference.name = selectedLevelName;

            getLevelPreview(selectedLevelName);

            /*print(chapterData.selectedChapterIndex);
             print(maxChapters);
             print(selectedLevelName);*/
        }

        else // Same as levels, if set to move backward instead

        {
            int maxChapters = chapterData.TotalLevelsInChapter.Length;

            lastSelected = chapterData.selectedChapterIndex;

            chapterData.selectedChapterIndex = Math.Clamp(chapterData.selectedChapterIndex - 1, 1, maxChapters);

            chapterData.selectedLevelIndex = 1;

            selectedLevelName = "Chapter " + chapterData.selectedChapterIndex + " Level " + chapterData.selectedLevelIndex;

            levelButton._levelReference.name = selectedLevelName;

            getLevelPreview(selectedLevelName);

            /*print(chapterData.selectedChapterIndex);
            print(maxChapters);
            print(selectedLevelName);*/
        }
        if (lastSelected != chapterData.selectedChapterIndex)//dont play if valued was clamped to the bounds/is the same 
        {
            updateChapterCabinet();
        }
        
    }



}

 
