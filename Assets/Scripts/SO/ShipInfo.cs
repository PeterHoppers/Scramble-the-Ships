using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ship Info", menuName = "Ship Info")]
public class ShipInfo : ScriptableObject
{
    [Header("Related Objects")]
    [SerializeField]
    public Bullet bullet;
    [Header("Sprites")]
    public Sprite shipSprite;
    [SerializedDictionary("Input", "Sprite to Represent")]
    public SerializedDictionary<InputValue, Sprite> inputsForSprites;
    [Header("Effects")]
    [SerializeField]
    public ParticleSystem deathVFX;
}