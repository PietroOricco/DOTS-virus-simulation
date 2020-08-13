using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using System;
using Unity.Collections;


public class PlagueSystem : SystemBase{

    private EndSimulationEntityCommandBufferSystem ecbSystem;
 
    protected override void OnCreate(){
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate(){
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        Entities.WithSharedComponentFilter(new RenderMesh{
            mesh = Human.Instance.mesh,
            material = Human.Instance.healthyMaterial
        }).ForEach((Entity entity, int nativeThreadIndex, InfectionComponent ic)=>{
            // https://forum.unity.com/threads/burst-error-adding-component-frozenrenderscenetag.810753/
            ecb.AddSharedComponent<RenderMesh>(nativeThreadIndex, entity, new RenderMesh{
                mesh = Human.Instance.mesh, material = Human.Instance.sickMaterial
            });
        }).ScheduleParallel();

        Entities.WithNone<InfectionComponent>().WithSharedComponentFilter(new RenderMesh{
            mesh = Human.Instance.mesh,
            material = Human.Instance.sickMaterial
        }).ForEach((Entity entity, int nativeThreadIndex) =>{
            // https://forum.unity.com/threads/burst-error-adding-component-frozenrenderscenetag.810753/
            ecb.AddSharedComponent<RenderMesh>(nativeThreadIndex, entity, new RenderMesh{
                mesh = Human.Instance.mesh, material = Human.Instance.sickMaterial
            });
        }).ScheduleParallel();

        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
 

