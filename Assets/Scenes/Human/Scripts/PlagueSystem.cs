using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using System;
using Unity.Collections;
using Unity.Jobs;


public class PlagueSystem : SystemBase{

    private EndSimulationEntityCommandBufferSystem ecbSystem;
 
    protected override void OnCreate(){
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate(){
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        JobHandle jobHandle = Entities.WithChangeFilter<InfectionComponent>()
            .ForEach((Entity entity, int nativeThreadIndex, in InfectionComponent ic)=>{
            if(ic.infected){
                ecb.SetSharedComponent<RenderMesh>(nativeThreadIndex, entity, new RenderMesh{
                    mesh = Human.Instance.mesh, material = Human.Instance.sickMaterial
                });
            }
            else{
                ecb.SetSharedComponent<RenderMesh>(nativeThreadIndex, entity, new RenderMesh{
                    mesh = Human.Instance.mesh, material = Human.Instance.healthyMaterial
                });
            }
            
        }).ScheduleParallel(Dependency);

        jobHandle.Complete();

        ecbSystem.AddJobHandleForProducer(jobHandle);
    }
}