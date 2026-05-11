using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/* -----------------------------------------------------------
 * Author:
 * Ian Fletcher
 * 
 * Modified By:
 * 
 */// --------------------------------------------------------

/* -----------------------------------------------------------
 * Purpose:
 * Sits on the persistent scene object alongside SoundManager.
 * Plays through a list of FMOD music events in a shuffled order,
 * re-shuffles the list each time the full playlist has been
 * played through, then repeats indefinitely.
 */// --------------------------------------------------------

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    // -------------------------------------------------------
    // Inspector
    // -------------------------------------------------------

    [Header("Playlist")]
    [SerializeField] private EventReference[] _tracks;

    [Header("Settings")]
    [Tooltip("Delay in seconds between the end of one track and the start of the next.")]
    [Range(0f, 10f)]
    [SerializeField] private float _trackGapSeconds = 0f;

    [Tooltip("Start playing automatically on Awake.")]
    [SerializeField] private bool _playOnAwake = true;

    [Header("Debugging")]
    [SerializeField] private bool _doDebugLog;

    // -------------------------------------------------------
    // State
    // -------------------------------------------------------

    private EventInstance    _currentInstance;
    private List<int>        _shuffledIndices = new();   // shuffled index order into _tracks
    private int              _playlistCursor  = 0;       // position inside _shuffledIndices
    private Coroutine        _playbackRoutine;
    private bool             _isPaused        = false;

    // -------------------------------------------------------
    // Unity Messages
    // -------------------------------------------------------

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (_tracks == null || _tracks.Length == 0)
        {
            Debug.LogWarning("[MusicPlayer] No tracks assigned — player will not start.");
            return;
        }

        BuildShuffledIndices();

        if (_playOnAwake)
            StartPlayback();
    }

    private void OnEnable()
    {
        SoundManager.OnGamePaused  += HandleGamePaused;
        SoundManager.OnGameResumed += HandleGameResumed;
    }

    private void OnDisable()
    {
        SoundManager.OnGamePaused  -= HandleGamePaused;
        SoundManager.OnGameResumed -= HandleGameResumed;
    }

    private void OnDestroy()
    {
        StopPlayback();
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary> Begin playing from the start of the (newly shuffled) playlist. </summary>
    public void StartPlayback()
    {
        StopPlayback();
        _playlistCursor  = 0;
        _playbackRoutine = StartCoroutine(PlaylistRoutine());
    }

    /// <summary> Stop all music immediately. </summary>
    public void StopPlayback()
    {
        if (_playbackRoutine != null)
        {
            StopCoroutine(_playbackRoutine);
            _playbackRoutine = null;
        }
        ReleaseCurrentInstance(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }


    /// <summary>
    /// Silences the music player for a scene that manages its own audio
    /// (e.g. the sound showcase scene). Stops playback but remembers the
    /// current playlist position so it can resume from the next track.
    /// </summary>
    public void SilenceForScene()
    {
        if (_playbackRoutine != null)
        {
            StopCoroutine(_playbackRoutine);
            _playbackRoutine = null;
        }
        ReleaseCurrentInstance(FMOD.Studio.STOP_MODE.IMMEDIATE);
        Log("Silenced for scene.");
    }

    /// <summary>
    /// Resumes playback after returning from a scene that silenced the player.
    /// Picks up from the next track in the shuffled playlist.
    /// </summary>
    public void ResumeForScene()
    {
        if (_tracks == null || _tracks.Length == 0) return;
        if (_playbackRoutine != null) return; // already playing

        // Advance past the track that was interrupted
        _playlistCursor++;
        if (_playlistCursor >= _shuffledIndices.Count)
        {
            Shuffle(_shuffledIndices);
            _playlistCursor = 0;
        }

        _playbackRoutine = StartCoroutine(PlaylistRoutine());
        Log("Resumed after scene.");
    }

    /// <summary> Skip the current track and move to the next one. </summary>
    public void SkipTrack()
    {
        // Stopping the current instance will let the playlist coroutine
        // detect STOPPED state and advance on its own — but we kick it
        // immediately by restarting from the next cursor position.
        int nextCursor = _playlistCursor + 1;   // +1 because PlaylistRoutine increments after each track
        StopPlayback();

        if (_tracks == null || _tracks.Length == 0) return;

        // If we've gone past the end, re-shuffle and wrap
        if (nextCursor >= _shuffledIndices.Count)
        {
            Shuffle(_shuffledIndices);
            nextCursor = 0;
        }

        _playlistCursor  = nextCursor;
        _playbackRoutine = StartCoroutine(PlaylistRoutine());
    }

    /// <summary> Pause the current track. </summary>
    public void Pause()
    {
        if (!_isPaused && _currentInstance.isValid())
        {
            _currentInstance.setPaused(true);
            _isPaused = true;
        }
    }

    /// <summary> Resume a paused track. </summary>
    public void Resume()
    {
        if (_isPaused && _currentInstance.isValid())
        {
            _currentInstance.setPaused(false);
            _isPaused = false;
        }
    }

    // -------------------------------------------------------
    // Core Coroutine
    // -------------------------------------------------------

    /// <summary>
    /// Loops forever: plays through the shuffled index list,
    /// re-shuffles when exhausted, then repeats.
    /// </summary>
    private IEnumerator PlaylistRoutine()
    {
        while (true)
        {
            // Clamp cursor safety (shouldn't be needed, but defensive)
            if (_playlistCursor >= _shuffledIndices.Count)
            {
                Shuffle(_shuffledIndices);
                _playlistCursor = 0;
            }

            int trackIndex = _shuffledIndices[_playlistCursor];
            EventReference trackRef = _tracks[trackIndex];

            // Create and start the instance
            _currentInstance = RuntimeManager.CreateInstance(trackRef);
            _currentInstance.start();

            // Poll until stopped
            yield return StartCoroutine(WaitForInstanceToStop(_currentInstance));

            // Release cleanly
            _currentInstance.release();
            _currentInstance = default;

            // Advance cursor
            _playlistCursor++;

            // End of list — re-shuffle and reset
            if (_playlistCursor >= _shuffledIndices.Count)
            {
                Log("Playlist complete — reshuffling.");
                Shuffle(_shuffledIndices);
                _playlistCursor = 0;
            }

            // Optional gap between tracks
            if (_trackGapSeconds > 0f)
                yield return new WaitForSeconds(_trackGapSeconds);
        }
    }

    /// <summary>
    /// Waits until the given EventInstance reaches STOPPED state.
    /// Handles pausing gracefully by not advancing while paused.
    /// </summary>
    private IEnumerator WaitForInstanceToStop(EventInstance instance)
    {
        PLAYBACK_STATE state;

        while (true)
        {
            if (!instance.isValid()) yield break;

            instance.getPlaybackState(out state);
            if (state == PLAYBACK_STATE.STOPPED) yield break;

            yield return null;
        }
    }

    // -------------------------------------------------------
    // Shuffle
    // -------------------------------------------------------

    /// <summary>
    /// Populate _shuffledIndices with [0 .. _tracks.Length-1] then shuffle.
    /// </summary>
    private void BuildShuffledIndices()
    {
        _shuffledIndices.Clear();
        for (int i = 0; i < _tracks.Length; i++)
            _shuffledIndices.Add(i);

        Shuffle(_shuffledIndices);
    }

    /// <summary>
    /// In-place Fisher-Yates shuffle using UnityEngine.Random.
    /// Operates on the index list so _tracks is never mutated.
    /// </summary>
    private static void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // -------------------------------------------------------
    // Event Handlers
    // -------------------------------------------------------

    private void HandleGamePaused()  => Pause();
    private void HandleGameResumed() => Resume();

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private void ReleaseCurrentInstance(FMOD.Studio.STOP_MODE mode)
    {
        if (_currentInstance.isValid())
        {
            _currentInstance.stop(mode);
            _currentInstance.release();
            _currentInstance = default;
        }
    }

    private void Log(string msg)
    {
        if (_doDebugLog)
            Debug.Log($"[MusicPlayer] {msg}");
    }
}