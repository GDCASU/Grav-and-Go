using UnityEngine;
using TMPro; // Required for TextMeshPro

/* -----------------------------------------------------------
 * Author:
 * Joshua Wright
 * 
 * Modified By:
 * 
 * 
 */// --------------------------------------------------------
public class LevelTimer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;

    // The total time elapsed in seconds
    public float TotalTime { get; private set; }

    private bool isRunning = true;

    void Update()
    {
        // Time.deltaTime automatically becomes 0 when Time.timeScale is 0 (Paused)
        if (isRunning)
        {
            TotalTime += Time.deltaTime;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            timerText.text = GetFormattedTime();
        }
    }

    public void ResetTimer()
    {
        TotalTime = 0f;
        UpdateUI();
    }

    public void ToggleTimer(bool running)
    {
        isRunning = running;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(TotalTime / 60F);
        int seconds = Mathf.FloorToInt(TotalTime % 60F);
        int milliseconds = Mathf.FloorToInt((TotalTime * 1000F) % 1000F); // Updated to 3 digits for precision

        return string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }
}