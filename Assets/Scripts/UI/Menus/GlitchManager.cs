using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlitchManager : MonoBehaviour
{
    [SerializeField] Material glitchMat;
    [SerializeField] Renderer2DData data;

    FullScreenPassRendererFeature feature;

    private void Start()
    {
        feature = new FullScreenPassRendererFeature();
        
        feature.passMaterial = glitchMat;
        feature.name = "Glitch"; 
        
        data.rendererFeatures.Add(feature);
        feature.SetActive(false);
    }

    private void OnDestroy()
    {
        data.rendererFeatures.Remove(feature);
    }

    public void StartGlitch()
    {
        feature.SetActive(true);
    }
    public void StopGlitch()
    {
        feature.SetActive(false);
    }
}
