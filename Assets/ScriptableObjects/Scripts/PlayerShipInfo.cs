using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Info", menuName = "Player Info")]
public class PlayerShipInfo : ShipInfo
{   
    //only items that should be constimized by a per player basis should exist in here
    [Header("Additional Sprites")]
    public Sprite cutsceneSprite;
    public Sprite actionButtonSprite;
    [SerializedDictionary("Input", "Sprite to Represent")]
    public SerializedDictionary<InputValue, Sprite> inputsForSprites;
}
