using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : Condition
{
    public const int RespawnDuration = 2;
    public override bool OnPlayerHit()
    {
        return false;
    }
}
