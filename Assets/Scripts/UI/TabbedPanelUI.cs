using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TabbedPanelUI : MonoBehaviour
{
    public SerializedDictionary<SettingPanelTypes, GameObject> panels;

    private void Start()
    {
        OnPanelButtonClicked(0);
    }

    private void OnEnable()
    {
        var firstButton = GetComponentInChildren<Button>();
        if (firstButton != null && firstButton.gameObject.activeInHierarchy) 
        {
            OnPanelButtonClicked(0);
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }

    public void OnPanelButtonClicked(int panelId)
    { 
        var panelType = (SettingPanelTypes)panelId;
        foreach (var panelPair in panels) 
        { 
            var type = panelPair.Key;
            var panel = panelPair.Value;
            var isTargettingPanel = (panelType == type);

            panel.SetActive(isTargettingPanel);
        }
    }
}

[System.Serializable]
public enum SettingPanelTypes
{ 
    Game,
    System,
    Energy
}
