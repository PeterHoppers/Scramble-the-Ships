using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneSystem : MonoBehaviour
{
    [SerializeField]
    private Animator _fullscreenAnimator;
    GameManager _gameManager;
    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _gameManager.OnLevelEnd = (int _, float _) =>
        {
            StopAllCoroutines();
        };
    }

    public void ActivateCutscene(CutsceneType type, float duration)
    { 
        switch(type) 
        { 
            case CutsceneType.Tutorial:
                StartCoroutine(HackingCutscene(duration, new List<Effect>
                { 
                    new()
                    { 
                        type = EffectType.ScrambleAmount,
                        amount = 3f
                    }
                }));
                break;
            case CutsceneType.ShootingHacking:
                StartCoroutine(HackingCutscene(duration, new List<Effect>
                {
                    new()
                    {
                        type = EffectType.ScrambleAmount,
                        amount = 5f
                    }
                }));
                break;
            case CutsceneType.ScreenTransition:
                StartCoroutine(ScreenTransitionCutscene(duration));
                break;
        }
    }

    IEnumerator HackingCutscene(float cutsceneDuration, List<Effect> effectsToApply)
    {
        var effects = _gameManager.EffectsSystem;
        var bigBaddy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Previewable>();
        _gameManager.MovePreviewableOffScreenToTile(bigBaddy, bigBaddy.CurrentTile, cutsceneDuration / 2);

        effects.PerformEffect(EffectType.DigitalGlitchIntensity, .8f);
        effects.PerformEffect(EffectType.ScanLineJitter, .2f);
        effects.PerformEffect(EffectType.VerticalJump, .05f);
        effects.PerformEffect(EffectType.HorizontalShake, 1);
        effects.PerformEffect(EffectType.ColorDrift, .3f);
        
        yield return new WaitForSeconds(cutsceneDuration);
        
        var lasers = FindObjectsOfType<BossTutorialBullet>();
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
}

public enum CutsceneType
{ 
    Tutorial,
    ScreenTransition,
    ShootingHacking
}
