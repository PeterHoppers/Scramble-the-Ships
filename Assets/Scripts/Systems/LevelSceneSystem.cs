using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneSystem : MonoBehaviour
{
    public const int PREVIEW_SCENE_INDEX = 0;
    public const int GAME_SCENE_INDEX = 1;

    public void LoadGameScene()
    { 
        SceneManager.LoadScene(GAME_SCENE_INDEX);
    }
}
