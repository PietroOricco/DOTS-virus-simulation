using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;

public class HumanSystem : SystemBase
{
    protected override void OnUpdate(){
        float deltaTime = Time.DeltaTime;
        Entities.ForEach((ref HumanComponent hc) =>
        {
            if(hc.hunger < 100f)
                hc.hunger += 1f * deltaTime;
            if (hc.fatigue < 100)
                hc.fatigue += 1f * deltaTime;
            if (hc.sociality < 100)
                hc.sociality += 1f * deltaTime;
            if (hc.sportivity < 100)
                hc.sportivity += 1f * deltaTime;
        }).Schedule();
    }
}
