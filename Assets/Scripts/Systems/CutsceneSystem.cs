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
        
    }
}

public enum CutsceneType
{ 
    Tutorial
}
