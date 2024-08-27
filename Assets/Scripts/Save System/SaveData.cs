using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public GameSettingParameters gameSettingParameters;
    public SystemSettingParameters systemSettingParameters;

    public SaveData()
    {
        gameSettingParameters = new GameSettingParameters();
        systemSettingParameters = new SystemSettingParameters();
    }
}