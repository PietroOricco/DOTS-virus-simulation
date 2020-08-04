using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using System;


public class PlagueSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Material tmpMaterial = material;
        Entities.ForEach((ref PlagueComponent pc) =>
        {
            if (pc.infectionProb % 2 == 0)
                
        }).Schedule();
    }
}
 

