using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Info", menuName = "Player Info")]
public class PlayerShipInfo : ShipInfo
{
    public AudioClip moveSFX;
    public AudioClip fireSFX;
    public AudioClip scrambleSFX;
    public AudioClip exitSFX;

    [Header("Additional Sprites")]
    public Sprite cutsceneSprite;
    [SerializedDictionary("Input", "Sprite to Represent")]
    public SerializedDictionary<InputValue, Sprite> inputsForSprites;
}
