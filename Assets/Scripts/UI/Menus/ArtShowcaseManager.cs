using UnityEngine;
using UnityEngine.UI;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 *
 * Modified By:
 *
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Purpose:
 * Manages an art showcase scene by cycling through a list of
 * GameObjects (pages/panels), activating one at a time.
 * Loops at both ends of the list.
 * Wire up the next/prev buttons and page objects in the inspector.
 */// --------------------------------------------------------

public class ArtShowcaseManager : MonoBehaviour
{
    [Header("Pages")]
    [Tooltip("GameObjects to cycle through. Only one will be active at a time.")]
    [SerializeField] private GameObject[] _pages;

    [Header("UI")]
    [SerializeField] private Button _prevButton;
    [SerializeField] private Button _nextButton;

    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;

    // Current page index
    private int _currentIndex = 0;

    private void Start()
    {
        if (_pages == null || _pages.Length == 0)
        {
            Debug.LogWarning("[ArtShowcaseManager] No pages assigned.");
            enabled = false;
            return;
        }

        _prevButton?.onClick.AddListener(OnPrevPressed);
        _nextButton?.onClick.AddListener(OnNextPressed);

        ShowPage(_currentIndex);
    }

    private void OnDestroy()
    {
        _prevButton?.onClick.RemoveListener(OnPrevPressed);
        _nextButton?.onClick.RemoveListener(OnNextPressed);
    }

    // -------------------------------------------------------
    // Button Callbacks
    // -------------------------------------------------------

    private void OnPrevPressed()
    {
        _currentIndex = (_currentIndex - 1 + _pages.Length) % _pages.Length;
        ShowPage(_currentIndex);
    }

    private void OnNextPressed()
    {
        _currentIndex = (_currentIndex + 1) % _pages.Length;
        ShowPage(_currentIndex);
    }

    // -------------------------------------------------------
    // Page Display
    // -------------------------------------------------------

    /// <summary>
    /// Deactivates all pages then activates only the one at the given index.
    /// </summary>
    private void ShowPage(int index)
    {
        for (int i = 0; i < _pages.Length; i++)
        {
            if (_pages[i] != null)
                _pages[i].SetActive(i == index);
        }

        Log($"Showing page [{index + 1}/{_pages.Length}]: {_pages[index]?.name}");
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private void Log(string msg)
    {
        if (_doDebugLog)
            Debug.Log($"[ArtShowcaseManager] {msg}");
    }
}