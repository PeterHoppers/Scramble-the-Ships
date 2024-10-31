using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CutsceneSystem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _cutsceneText;
    [SerializeField]
    private Animator _fullscreenAnimator;
    [SerializeField]
    private AudioClip _rewindClip;
    [SerializeField]
    private AudioClip _hackedClip;

    GameManager _gameManager;
    EffectsSystem _effectsSystem;
    GameInputProgression _scrambleType = GameInputProgression.SimpleMovement;
    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _gameManager.OnLevelEnd = (int _, int _, int _) =>
        {
            StopAllCoroutines();
        };

        _effectsSystem = _gameManager.EffectsSystem;
        _effectsSystem.OnGameInputProgressionChanged += (GameInputProgression newType) => _scrambleType = newType;
    }

    public void ActivateCutscene(CutsceneType type, float duration)
    { 
        switch(type) 
        { 
            case CutsceneType.Hacking:
                StartCoroutine(HackingCutscene(duration, GetEffectsByScrambleType(_scrambleType)));
                break;            
            case CutsceneType.ScreenTransition:
                StartCoroutine(ScreenTransitionCutscene(duration));
                break;
            default:
                break;
        }
    }    

    List<Effect> GetEffectsByScrambleType(GameInputProgression type)
    { 
        switch (type) 
        { 
            case GameInputProgression.SimpleMovement:
                return new List<Effect>
                {
                    new()
                    {
                        type = EffectType.ScrambleAmount,
                        amount = 4f
                    },                    
                    new()
                    {
                        type = EffectType.ScrambleVarience,
                        amount = 0f
                    },
                    new()
                    {
                        type = EffectType.GameInputProgression,
                        amount = 1f
                    },
                };
            case GameInputProgression.MoveAndShooting:
                return new List<Effect>
                {
                    new()
                    {
                        type = EffectType.ScrambleAmount,
                        amount = 5f
                    },                    
                    new()
                    {
                        type = EffectType.ScrambleVarience,
                        amount = 0f
                    },
                    new()
                    {
                        type = EffectType.GameInputProgression,
                        amount = 3f
                    },
                };
            case GameInputProgression.ScrambledShooting:
                return new List<Effect>
                {
                    new()
                    {
                        type = EffectType.ScrambleAmount,
                        amount = 5f
                    },                    
                    new()
                    {
                        type = EffectType.ScrambleVarience,
                        amount = 0f
                    },
                    new()
                    {
                        type = EffectType.GameInputProgression,
                        amount = 4f
                    },
                };
            default:
                return new List<Effect>();
        }
    }

    IEnumerator HackingCutscene(float cutsceneDuration, List<Effect> effectsToApply)
    {
        var effects = _gameManager.EffectsSystem;
        var bigBaddy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Previewable>();
        _gameManager.MovePreviewableOffScreenToTile(bigBaddy, bigBaddy.CurrentTile, cutsceneDuration / 2);

        GlobalAudioManager.Instance.PlayAudioSFX(_hackedClip, true);
        effects.PerformEffect(EffectType.DigitalGlitchIntensity, .8f);
        effects.PerformEffect(EffectType.ScanLineJitter, .2f);
        effects.PerformEffect(EffectType.VerticalJump, .05f);
        effects.PerformEffect(EffectType.HorizontalShake, 1);
        effects.PerformEffect(EffectType.ColorDrift, .3f);
        
        yield return new WaitForSeconds(cutsceneDuration);
        
        var lasers = FindObjectsOfType<HackingLaser>();
        foreach (var item in lasers)
        {
            item.DestroyObject();        
        }

        effects.PerformEffect(EffectType.DigitalGlitchIntensity, .2f);
        effects.PerformEffect(EffectType.HorizontalShake, 0);

        yield return new WaitForSeconds(cutsceneDuration);

        effects.PerformEffect(EffectType.DigitalGlitchIntensity, 0);
        effects.PerformEffect(EffectType.VerticalJump, 0);
        effects.PerformEffect(EffectType.ColorDrift, .1f);

        yield return new WaitForSeconds(cutsceneDuration / 2);

        GlobalAudioManager.Instance.StopAudioSFX();
        effects.ClearCameraEffects();
        effects.PerformEffect(EffectType.ScanLineJitter, .05f);

        effectsToApply.ForEach(effect =>
        {
            effects.PerformEffect(effect);
        });
        
        _gameManager.ToggleIsPlaying(true);
    }

    IEnumerator ScreenTransitionCutscene(float cutsceneDuration)
    {
        _fullscreenAnimator.SetFloat("animSpeed", cutsceneDuration); //since our animations are set to being 1.0s, this will change our animation to be whatever the tick duration is
        _fullscreenAnimator.Play("pan");
        yield return new WaitForSeconds(cutsceneDuration);
        _fullscreenAnimator.Play("close");
    }

    public void PerformRewindEffect()
    {
        _effectsSystem.PerformEffect(EffectType.DigitalGlitchIntensity, .5f);
        _effectsSystem.PerformEffect(EffectType.HorizontalShake, .125f);
        _effectsSystem.PerformEffect(EffectType.ScanLineJitter, .25f);

        GlobalAudioManager.Instance.PlayAudioSFX(_rewindClip);
    }

    public IEnumerator PlayLevelIntro(string text, float duration = 3f)
    {
        _cutsceneText.text = text;
        _fullscreenAnimator.Play("intro");
        yield return new WaitForSeconds(duration);
        _fullscreenAnimator.Play("panelFadeOut");
    }
}

public enum CutsceneType
{
    Enterance = 2,
    Hacking = 0,
    ScreenTransition = 1    
}
