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
        _gameManager.OnLevelEnd = (int _) =>
        {
            StopAllCoroutines();
        };
    }

    public void ActivateCutscene(CutsceneType type, float duration)
    { 
        switch(type) 
        { 
            case CutsceneType.Tutorial:
                StartCoroutine(TutorialCutscene(duration));
                break;
            case CutsceneType.ScreenTransition:
                StartCoroutine(ScreenTransitionCutscene(duration));
                break;
        }
    }

    IEnumerator TutorialCutscene(float cutsceneDuration)
    {
        var bigBaddy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Previewable>();
        _gameManager.MovePreviewableOffScreenToTile(bigBaddy, bigBaddy.CurrentTile, cutsceneDuration / 2);
        yield return new WaitForSeconds(cutsceneDuration / 2);
        var lasers = FindObjectsOfType<BossTutorialBullet>();
        foreach (var item in lasers)
        {
            item.DestroyObject();
        }
        yield return new WaitForSeconds(cutsceneDuration / 2);
        _gameManager.EffectsSystem.PerformEffect(EffectType.ScrambleAmount, 3);
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
}
