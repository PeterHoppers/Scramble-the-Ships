using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneSystem : MonoBehaviour
{
    GameManager _gameManager;
    private void Awake()
    {
        _gameManager = GetComponent<GameManager>();
    }

    public void ActivateCutscene(CutsceneType type)
    { 
        switch(type) 
        { 
            case CutsceneType.Tutorial:
                StartCoroutine(TutorialCutscene());
                break;
        }
    }

    IEnumerator TutorialCutscene()
    {
        var bigBaddy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Previewable>();
        _gameManager.MovePreviewableOffScreenToTile(bigBaddy, bigBaddy.CurrentTile, 1f);
        yield return new WaitForSeconds(1f);
        var lasers = FindObjectsOfType<BossTutorialBullet>();
        foreach (var item in lasers)
        {
            item.DestroyObject();
        }
        yield return new WaitForSeconds(1f);
        _gameManager.EffectsSystem.PerformEffect(new Effect() 
        { 
            type = EffectType.ScrambleAmount,
            amount = 3
        });
        _gameManager.EndedCutscene();
    }
}

public enum CutsceneType
{ 
    Tutorial
}
