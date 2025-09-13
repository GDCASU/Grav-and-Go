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
    private int _time;

    void Awake()
    {
        LevelManager.Instance.OnLevelStart.AddListener(OnLevelStart);
        LevelManager.Instance.OnLevelEnd.AddListener(OnLevelEnd);
    }

    void OnLevelStart(LevelName levelName)
    {
        // Reset everything
        _time = 0;
    }

    void OnLevelEnd(LevelName levelName)
    {
        LevelManager.Instance.UpdateLevelBestTime(levelName, _time);
    }

    void Update()
    {

    }

}
