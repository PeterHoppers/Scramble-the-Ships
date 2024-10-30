using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using URPGlitch.Runtime.AnalogGlitch;
using URPGlitch.Runtime.DigitalGlitch;

public class GlitchAdapter : MonoBehaviour
{
    [SerializeField]
    private Volume _globalVolume;

    private DigitalGlitchVolume _digitalGlitchVolume;
    private AnalogGlitchVolume _analogueGlichVolume;

    private void Awake()
    {
        _globalVolume.profile.TryGet<DigitalGlitchVolume>(out var digitalGlitchVolume);
        _digitalGlitchVolume = digitalGlitchVolume;
        _globalVolume.profile.TryGet<AnalogGlitchVolume>(out var analogGlitchVolume);
        _analogueGlichVolume = analogGlitchVolume;
    }

    public void SetDigitalGlitchIntensity(float intensity)
    {
        _digitalGlitchVolume.intensity.value = intensity;
    }

    public void SetScanLineJitterIntensity(float intensity)
    {
        _analogueGlichVolume.scanLineJitter.value = intensity;
    }

    public void SetVerticalJumpIntensity(float intensity)
    {
        _analogueGlichVolume.verticalJump.value = intensity;
    }

    public void SetHorizontalShakeIntensity(float intensity)
    {
        _analogueGlichVolume.horizontalShake.value = intensity;
    }

    public void SetColorDriftIntensity(float intensity)
    {
        _analogueGlichVolume.colorDrift.value = intensity;
    }

    public void PerformDefaultGlitchTransitionEffect()
    {
        SetScanLineJitterIntensity(.5F);
        SetColorDriftIntensity(.35f);
        SetHorizontalShakeIntensity(.45f);
        SetVerticalJumpIntensity(.025f);
    }

    public void ClearGlitchEffects()
    {
        _digitalGlitchVolume.intensity.value = 0;
        _analogueGlichVolume.scanLineJitter.value = 0;
        _analogueGlichVolume.verticalJump.value = 0;
        _analogueGlichVolume.horizontalShake.value = 0;
        _analogueGlichVolume.colorDrift.value = 0;
    }
}
