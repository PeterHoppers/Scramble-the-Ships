using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PregameUIManager : MonoBehaviour
{
    public SerializedDictionary<GlobalGameStateStatus, GameObject> UIs;
    private void OnEnable()
    {
        GlobalGameStateManager.Instance.OnStateChange += OnStateChange;
    }

    private void OnDisable()
    {
        GlobalGameStateManager.Instance.OnStateChange -= OnStateChange;
    }

    void OnStateChange(GlobalGameStateStatus newStatus)
    {
        foreach (var ui in UIs)
        {
            ui.Value.SetActive(false);
        }

        var targetUI = UIs[newStatus];

        targetUI.SetActive(true);
        var firstInput = targetUI.GetComponentInChildren<Button>();
        
        if (firstInput != null)
        {
            EventSystem.current.SetSelectedGameObject(firstInput.gameObject);
        }
    }

    public void PlayTutorial()
    {
        GlobalGameStateManager.Instance.PlayTutorial();
    }

    public void SkipTutorial()
    {
        GlobalGameStateManager.Instance.SkipTutorial();
    }
}
