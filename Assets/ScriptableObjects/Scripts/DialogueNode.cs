using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DialogueNode
{
    public DialogueSpeaker textSpeaker;
    [TextArea(2, 4)]
    public string textToDisplay;
}

public enum DialogueSpeaker
{
    Control,
    Player,
    Alien
}
