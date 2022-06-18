using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBulb : MonoBehaviour
{
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    public Light _light;
    public Renderer _renderer;

    public void SetIntensity(float intensity)
    {
        _light.intensity = intensity;
        _renderer.material.SetFloat(IntensityId, intensity);
    }

    public void SetReverseIntensity(float intensity) => SetIntensity(1 - intensity);

    public void On() => SetIntensity(1);
    public void Off() => SetIntensity(0);
}
