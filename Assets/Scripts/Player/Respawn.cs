using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : Condition
{    
    // Start is called before the first frame update
    void Start()
    {
        Duration = 2;
    }

    public override bool OnPlayerHit()
    {
        return false;
    }
}
