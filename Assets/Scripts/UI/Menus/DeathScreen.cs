using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    public static DeathScreen Instance { get; private set; }

    [Header("UI Prefabs")]
    [SerializeField, Tooltip("Drag the Death Menu Prefab here")] 
    private GameObject _deathMenuPrefab;

    // This keeps track of the specific menu currently on screen
    private GameObject _activeMenuInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowDeathScreen()
    {
        // Prevent multiple menus from spawning if called twice
        if (_activeMenuInstance != null) return;

        //Instantiate the prefab
        _activeMenuInstance = Instantiate(_deathMenuPrefab);
        _activeMenuInstance.transform.SetParent(this.transform, false);

        //Find and setup buttons on this specific instance
        InitializeButtons(_activeMenuInstance.transform);
    }

    private void InitializeButtons(Transform menuTransform)
    {
        // Finding children by name on the NEWLY instantiated object
        Button retryBtn = menuTransform.Find("_retryButton")?.GetComponent<Button>();
        Button levelBtn = menuTransform.Find("_levelSelectButton")?.GetComponent<Button>();
        Button menuBtn = menuTransform.Find("_mainMenuButton")?.GetComponent<Button>();

        if (retryBtn == null || levelBtn == null || menuBtn == null)
        {
            Debug.LogError("DeathScreen: Buttons missing on instantiated prefab! Check naming.");
            return;
        }

        // Hook up listeners
        retryBtn.onClick.AddListener(OnRetryClicked);
        levelBtn.onClick.AddListener(OnLevelSelectClicked);
        menuBtn.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnRetryClicked()
    {
        Cleanup();
        LevelManager.Instance.LoadLastCheckpoint();
    }

    private void OnLevelSelectClicked()
    {
        Cleanup();
        LevelManager.Instance.LoadLevelSelect();
    }

    private void OnMainMenuClicked()
    {
        Cleanup();
        LevelManager.Instance.LoadMainMenu();
    }

    private void Cleanup()
    {
        if (_activeMenuInstance != null)
        {
            Destroy(_activeMenuInstance);
            _activeMenuInstance = null;
        }
    }
}