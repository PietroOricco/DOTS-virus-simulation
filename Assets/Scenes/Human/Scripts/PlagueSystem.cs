using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using System;


public class PlagueSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref PlagueComponent pc, in Translation t) =>
        {



        }).Schedule();
    }
}
 

