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
        systemSettingParameters = new SystemSettingParameters()
        { 
            musicVolume = 1,
            sfxVolume = 1,
            coinsPerPlay = 2,
            isFreeplay = false
        };
    }
}