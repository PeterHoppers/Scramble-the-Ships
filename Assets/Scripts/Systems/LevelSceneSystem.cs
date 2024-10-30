using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneSystem : MonoBehaviour
{
    public const int PREVIEW_SCENE_INDEX = 0;
    public const int GAME_SCENE_INDEX = 1;
    public const int CUTSCENE_SCENE_INDEX = 2;
    public const int LEVEL_SELECT_INDEX = 3;

    public void LoadPreviewScene()
    {
        SceneManager.LoadScene(PREVIEW_SCENE_INDEX);
    }

    public bool IsPreviewScene()
    { 
        var sceneIndex = SceneManager.GetActiveScene().buildIndex;
        return (sceneIndex == PREVIEW_SCENE_INDEX);
    }

    public void LoadGameScene()
    { 
        SceneManager.LoadScene(GAME_SCENE_INDEX);
    }

    public void LoadCutsceneScene()
    {
        SceneManager.LoadScene(CUTSCENE_SCENE_INDEX);
    }

    public void LoadLevelSelectScene()
    {
        SceneManager.LoadScene(LEVEL_SELECT_INDEX);
    }

    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
