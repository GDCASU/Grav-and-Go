using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/* -----------------------------------------------------------
 * Author:
 * Rika Vuong
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

public class ResultsMenuSuccess : MonoBehaviour 
{
    public GameObject resultsPanel;
    public TMP_Text timeText;
    public TMP_Text healthText;
    public TMP_Text powerUpsText;

    // set to false until level is successfully completed
    void Start()
    {
        resultsPanel.SetActive(false);
    }

    public void ShowResults(float time, int health, string powerUps)
    {
        resultsPanel.SetActive(true);

        timeText.text = "Time: " + time.ToString("F2") + "s";
        healthText.text = "Health: " + health.ToString("");
        powerUpsText.text = "Power ups: " + powerUps;
        
        Time.timeScale = 0f;
    }

    // buttons
    public void OnRetry()
    {
        Time.timeScale = 1f;
    }
    
    public void OnLevelSelect()
    {

    }

    public void OnMainMenu()
    {

    }

    public void OnNextLevel()
    {

    }
}
