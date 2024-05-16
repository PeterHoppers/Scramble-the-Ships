using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ship Info", menuName = "Ship Info")]
public class ShipInfo : ScriptableObject
{
    [SerializeField]
    public Bullet bullet;
    [SerializedDictionary("Input", "Sprite to Represent")]
    public SerializedDictionary<InputValue, Sprite> inputsForSprites; 
    [SerializeField]
    public ParticleSystem deathVFX;
}