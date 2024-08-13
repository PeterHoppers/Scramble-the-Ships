using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper;
using AYellowpaper.SerializedCollections;
using UnityEngine.UI;

public class TabbedPanelUI : MonoBehaviour
{
    public SerializedDictionary<SettingPanelTypes, GameObject> panels;

    private void Start()
    {
        OnPanelButtonClicked(0);
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
    System
}
