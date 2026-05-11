using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using TMPro;
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
 * Backend controller for the sound showcase/testing scene.
 * Plays through a list of named FMOD tracks with UI controls:
 *   - Left/right arrow buttons to cycle tracks
 *   - Play/Stop button
 *   - Display-only progress slider
 *   - Track name and duration labels
 *
 * Wire up all UI references in the inspector.
 * Does NOT require SoundManager — operates independently so
 * it can be used in a standalone scene.
 */// --------------------------------------------------------

/// <summary>
/// Stores the data for a single showcase track
/// </summary>
[Serializable]
public class ShowcaseTrack
{
    public string trackName = "Unnamed Track";
    [TextArea(1, 2)]
    public string duration = "";   // Optional override (e.g. "2:34"). Leave blank to auto-detect from FMOD.
    public EventReference eventReference;
}

/// <summary>
/// Controls the sound showcase UI scene
/// </summary>
public class SoundShowcaseController : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector — Tracks
    // -------------------------------------------------------

    [Header("Tracks")]
    [SerializeField] private ShowcaseTrack[] _tracks;

    // -------------------------------------------------------
    // Inspector — UI References
    // -------------------------------------------------------

    [Header("UI — Navigation")]
    [Tooltip("Left arrow button — cycles to the previous track.")]
    [SerializeField] private Button _prevButton;
    [Tooltip("Right arrow button — cycles to the next track.")]
    [SerializeField] private Button _nextButton;

    [Header("UI — Playback")]
    [Tooltip("Button that toggles between Play and Stop.")]
    [SerializeField] private Button _playStopButton;
    [Tooltip("Label inside the play/stop button (e.g. '▶' / '■').")]
    [SerializeField] private TextMeshProUGUI _playStopButtonLabel;

    [Header("UI — Track Info")]
    [Tooltip("Displays the current track name.")]
    [SerializeField] private TextMeshProUGUI _trackNameLabel;
    [Tooltip("Displays elapsed and total duration (e.g. '1:02 / 3:45').")]
    [SerializeField] private TextMeshProUGUI _durationLabel;

    [Header("UI — Progress")]
    [Tooltip("Read-only slider showing playback progress. Set Interactable = false in the inspector.")]
    [SerializeField] private Slider _progressSlider;

    // -------------------------------------------------------
    // Inspector — Settings
    // -------------------------------------------------------

    [Header("Settings")]
    [Tooltip("Text shown on the button when a track is stopped.")]
    [SerializeField] private string _playLabel  = "▶";
    [Tooltip("Text shown on the button when a track is playing.")]
    [SerializeField] private string _stopLabel  = "■";
    [Tooltip("How often (in seconds) the progress bar and timer update.")]
    [Range(0.05f, 0.5f)]
    [SerializeField] private float _progressPollRate = 0.1f;

    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;

    // -------------------------------------------------------
    // State
    // -------------------------------------------------------

    private int           _currentIndex    = 0;
    private EventInstance _currentInstance;
    private bool          _isPlaying       = false;
    private Coroutine     _progressRoutine;

    // Cached duration of the current FMOD event in milliseconds (-1 = unknown)
    private int _currentDurationMs = -1;

    // -------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------

    private void Awake()
    {
        // Guard against empty track list
        if (_tracks == null || _tracks.Length == 0)
        {
            Debug.LogWarning("[SoundShowcase] No tracks assigned.");
            enabled = false;
            return;
        }
    }

    private void OnDisable()
    {
        // Resume the persistent music player when leaving this scene
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.ResumeForScene();
    }

    private void Start()
    {
        // Wire button listeners
        if (_prevButton)      _prevButton.onClick.AddListener(OnPrevPressed);
        if (_nextButton)      _nextButton.onClick.AddListener(OnNextPressed);
        if (_playStopButton)  _playStopButton.onClick.AddListener(OnPlayStopPressed);

        // Draw initial state
        RefreshTrackUI();
        SetProgressUI(0f, 0, 0);
        SetPlayStopButtonState(false);
        
        // Silence the persistent music player while this scene is active
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.SilenceForScene();
    }

    private void OnDestroy()
    {
        StopCurrentTrack();

        if (_prevButton)      _prevButton.onClick.RemoveListener(OnPrevPressed);
        if (_nextButton)      _nextButton.onClick.RemoveListener(OnNextPressed);
        if (_playStopButton)  _playStopButton.onClick.RemoveListener(OnPlayStopPressed);
    }

    // -------------------------------------------------------
    // Button Callbacks
    // -------------------------------------------------------

    private void OnPrevPressed()
    {
        StopCurrentTrack();
        _currentIndex = (_currentIndex - 1 + _tracks.Length) % _tracks.Length;
        RefreshTrackUI();
        Log($"Cycled to previous: [{_currentIndex}] {_tracks[_currentIndex].trackName}");
    }

    private void OnNextPressed()
    {
        StopCurrentTrack();
        _currentIndex = (_currentIndex + 1) % _tracks.Length;
        RefreshTrackUI();
        Log($"Cycled to next: [{_currentIndex}] {_tracks[_currentIndex].trackName}");
    }

    private void OnPlayStopPressed()
    {
        if (_isPlaying)
            StopCurrentTrack();
        else
            PlayCurrentTrack();
    }

    // -------------------------------------------------------
    // Playback
    // -------------------------------------------------------

    private void PlayCurrentTrack()
    {
        if (_tracks == null || _tracks.Length == 0) return;

        ShowcaseTrack track = _tracks[_currentIndex];

        // Create and start the FMOD instance
        _currentInstance = RuntimeManager.CreateInstance(track.eventReference);
        _currentInstance.start();
        _isPlaying = true;

        // Cache the event duration from FMOD (milliseconds)
        _currentDurationMs = GetEventDurationMs(track);
        Log($"Playing '{track.trackName}' | duration: {_currentDurationMs}ms");

        // Update button state
        SetPlayStopButtonState(true);

        // Start the progress polling coroutine
        if (_progressRoutine != null) StopCoroutine(_progressRoutine);
        _progressRoutine = StartCoroutine(ProgressRoutine());
    }

    private void StopCurrentTrack()
    {
        // Stop polling
        if (_progressRoutine != null)
        {
            StopCoroutine(_progressRoutine);
            _progressRoutine = null;
        }

        // Stop and release the FMOD instance
        if (_currentInstance.isValid())
        {
            _currentInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _currentInstance.release();
            _currentInstance = default;
        }

        _isPlaying = false;
        _currentDurationMs = -1;

        // Reset UI
        SetPlayStopButtonState(false);
        SetProgressUI(0f, 0, 0);
    }

    // -------------------------------------------------------
    // Progress Coroutine
    // -------------------------------------------------------

    /// <summary>
    /// Polls FMOD timeline position at _progressPollRate and updates the
    /// slider and duration label.
    /// Does NOT auto-advance on natural track end — the player must use
    /// the prev/next buttons. This also means looping events are handled
    /// gracefully: the slider will follow the timeline position as-is
    /// (looping back to 0 each time the loop region repeats).
    /// </summary>
    private IEnumerator ProgressRoutine()
    {
        var wait = new WaitForSeconds(_progressPollRate);

        while (_currentInstance.isValid())
        {
            // Get current position in milliseconds
            _currentInstance.getTimelinePosition(out int positionMs);

            // Calculate normalised progress [0,1]
            float progress = 0f;
            if (_currentDurationMs > 0)
                progress = Mathf.Clamp01((float)positionMs / _currentDurationMs);

            SetProgressUI(progress, positionMs, _currentDurationMs);

            yield return wait;
        }
    }

    // -------------------------------------------------------
    // UI Helpers
    // -------------------------------------------------------

    /// <summary>
    /// Updates the track name label and resets progress/duration display.
    /// </summary>
    private void RefreshTrackUI()
    {
        if (_tracks == null || _tracks.Length == 0) return;

        ShowcaseTrack track = _tracks[_currentIndex];

        if (_trackNameLabel)
            _trackNameLabel.text = track.trackName;

        // Show duration label from override string if provided,
        // otherwise show placeholder until playback starts
        if (_durationLabel)
        {
            if (!string.IsNullOrWhiteSpace(track.duration))
                _durationLabel.text = $"0:00 / {track.duration}";
            else
                _durationLabel.text = "0:00 / --:--";
        }

        if (_progressSlider)
            _progressSlider.value = 0f;
    }

    /// <summary>
    /// Updates the progress slider and duration label from raw millisecond values.
    /// </summary>
    private void SetProgressUI(float normalizedProgress, int positionMs, int totalMs)
    {
        if (_progressSlider)
            _progressSlider.value = normalizedProgress;

        if (_durationLabel)
        {
            string elapsed = FormatMs(positionMs);
            string total   = totalMs > 0
                ? FormatMs(totalMs)
                : (!string.IsNullOrWhiteSpace(_tracks[_currentIndex].duration)
                    ? _tracks[_currentIndex].duration
                    : "--:--");

            _durationLabel.text = $"{elapsed} / {total}";
        }
    }

    /// <summary>
    /// Swaps the play/stop button label based on playback state.
    /// </summary>
    private void SetPlayStopButtonState(bool playing)
    {
        if (_playStopButtonLabel)
            _playStopButtonLabel.text = playing ? _stopLabel : _playLabel;
    }

    // -------------------------------------------------------
    // FMOD Helpers
    // -------------------------------------------------------

    /// <summary>
    /// Attempts to retrieve the event duration in milliseconds from FMOD.
    /// Falls back to -1 if the event description can't provide it
    /// (e.g. adaptive/looping events with no fixed length).
    /// </summary>
    private int GetEventDurationMs(ShowcaseTrack track)
    {
        if (track.eventReference.IsNull) return -1;

        try
        {
            EventDescription description = RuntimeManager.GetEventDescription(track.eventReference);
            description.getLength(out int lengthMs);
            return lengthMs > 0 ? lengthMs : -1;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SoundShowcase] Could not fetch duration for '{track.trackName}': {e.Message}");
            return -1;
        }
    }

    // -------------------------------------------------------
    // Utilities
    // -------------------------------------------------------

    /// <summary>
    /// Converts milliseconds to a M:SS string.
    /// </summary>
    private static string FormatMs(int ms)
    {
        if (ms <= 0) return "0:00";
        int totalSeconds = ms / 1000;
        int minutes      = totalSeconds / 60;
        int seconds      = totalSeconds % 60;
        return $"{minutes}:{seconds:D2}";
    }

    private void Log(string msg)
    {
        if (_doDebugLog)
            Debug.Log($"[SoundShowcase] {msg}");
    }
}