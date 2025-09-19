using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Studio = FMOD.Studio; // alias

public class MusicControllerFMOD : MonoBehaviour
{
    [SerializeField] private EventReference levelMusic;
    private EventInstance _inst;

    void OnEnable()
    {
        _inst = RuntimeManager.CreateInstance(levelMusic);
        _inst.start();
        _inst.release();
    }

    public void SetParam(string name, float value) => _inst.setParameterByName(name, value);

    public void StopMusic(bool fade = true)
        => _inst.stop(fade ? Studio.STOP_MODE.ALLOWFADEOUT : Studio.STOP_MODE.IMMEDIATE);
}

