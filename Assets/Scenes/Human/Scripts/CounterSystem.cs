using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CounterSystem : SystemBase
{
    EntityQuery query;
    // Start is called before the first frame update
    protected override void OnCreate()
    {
         
    }

    // Update is called once per frame
    protected override void OnUpdate()
    {
        query = GetEntityQuery(ComponentType.ReadOnly<InfectionComponent>());
        int count = query.CalculateEntityCount();
    }
}
