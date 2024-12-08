using AYellowpaper.SerializedCollections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ship Info", menuName = "Ship Info")]
public class ShipInfo : ScriptableObject
{
    [Header("Related Objects")]
    [SerializeField]
    public Fireable fireable;
    [ColorUsage(true, true)]
    public Color baseColor;
    [Header("Sprites")]
    public Sprite shipSprite;
    public Sprite bulletSprite;
    [Header("Effects")]
    [SerializeField]
    public ParticleSystem deathVFX;
}