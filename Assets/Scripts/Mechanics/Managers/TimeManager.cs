using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Cody Quinn
 *
 * Modified By:
 *
 */// --------------------------------------------------------

public class TimeManager : MonoBehaviour
{
    private bool _inLevel = false;
    private float _time = 0F;

    void Start()
    {
        LevelManager.Instance.OnLevelStart.AddListener(OnLevelStart);
        LevelManager.Instance.OnLevelComplete.AddListener(OnLevelComplete);
    }

    void OnLevelStart(LevelName levelName)
    {
        // Reset everything
        _inLevel = true;
        _time = 0F;
    }

    void OnLevelComplete(LevelName levelName)
    {
        _inLevel = false;
        LevelManager.Instance.UpdateLevelBestTime(levelName, _time);
    }

    void Update()
    {
        if (Time.timeScale > 0 && _inLevel)
        {
            _time += Time.deltaTime;
        }
    }

}
