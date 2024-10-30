using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Select Node", menuName = "Level Select Node")]
public class LevelSelectNode : ScriptableObject
{
    public Level level;
    public string title;
    public string description;
    public Sprite levelImage;
}
