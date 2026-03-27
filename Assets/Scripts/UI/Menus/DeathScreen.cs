using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance { get; private set; }

    [Header("UI References")]
    [SerializeField, Tooltip("Drag the Death Menu GameObject here")] 
    private GameObject _deathMenu;

    private Button _retryButton;
    private Button _levelSelectButton;
    private Button _mainMenuButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializeButtons();
        _deathMenu.SetActive(false); // Start disabled
    }

    private void InitializeButtons()
    {
        // Find buttons on the death menu
        _retryButton = _deathMenu.transform.Find("_retryButton")?.GetComponent<Button>();
        _levelSelectButton = _deathMenu.transform.Find("_levelSelectButton")?.GetComponent<Button>();
        _mainMenuButton = _deathMenu.transform.Find("_mainMenuButton")?.GetComponent<Button>();

        if (_retryButton == null || _levelSelectButton == null || _mainMenuButton == null)
        {
            Debug.LogError("DeathScreen: Buttons missing on prefab! Check naming.");
            return;
        }

        // Hook up listeners
        _retryButton.onClick.AddListener(OnRetryClicked);
        _levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void ShowDeathScreen()
    {
        _deathMenu.SetActive(true);
    }

    public void HideDeathScreen()
    {
        _deathMenu.SetActive(false);
    }

    private void OnRetryClicked()
    {
        HideDeathScreen();
        LevelManager.Instance.LoadLastCheckpoint();
    }

    private void OnLevelSelectClicked()
    {
        HideDeathScreen();
        LevelManager.Instance.LoadLevelSelect();
    }

    private void OnMainMenuClicked()
    {
        HideDeathScreen();
        LevelManager.Instance.LoadMainMenu();
    }
}