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
    public TMP_Text time_text;
    public TMP_Text best_time_text;
    public TMP_Text health_text;
    public TMP_Text power_ups_text;

    // results menu set to false until level is completed sucessfully
    void Start()
    {
        resultsPanel.SetActive(false);
    }

    public void OnLevelCompleted()
    {
        resultsPanel.SetActive(true);
    }

    public void ShowResults(float time, float best_time, int health, string power_ups)
    {
        resultsPanel.SetActive(true);

        time_text.text = "Time Taken: " + time.ToString("F2");
        best_time_text.text = "Best Time: " + best_time.ToString("F2");
        health_text.text = "Health: " + health.ToString("");
        power_ups_text.text = "Power ups: " + power_ups;
        
        Time.timeScale = 0f;
    }

    // buttons
    public void OnNextLevel()
    {

    }

    public void OnRetry()
    {

    }

    public void OnLevelSelect()
    {

    }

    public void OnMainMenu()
    {

    }
}
