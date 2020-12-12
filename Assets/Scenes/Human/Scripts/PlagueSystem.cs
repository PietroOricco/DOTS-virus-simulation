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

        JobHandle jobHandle = Entities//.WithChangeFilter<InfectionComponent>()
            .ForEach((Entity entity, int nativeThreadIndex, ref SpriteSheetAnimation_Data spriteSheetAnimationData, in Translation translation, in InfectionComponent ic)=>{
            float uvOffsetY = 0.8f;

            if(ic.status == Status.recovered){
                uvOffsetY = 0.0f;
            }

            if (ic.status == Status.infectious && ic.symptomatic)
            {
                uvOffsetY = 0.2f;
            }

            if (ic.status == Status.infectious && !ic.symptomatic)
            {
                uvOffsetY = 0.4f;
            }

            if (ic.status == Status.exposed)
            {
                uvOffsetY = 0.6f;
            }

            float uvWidth = 1f;
            float uvHeight = 1f/5;
            float uvOffsetX = 0f;
            
            spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);

            Vector3 position = translation.Value;
            spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
        }).ScheduleParallel(Dependency);

        jobHandle.Complete();

        ecbSystem.AddJobHandleForProducer(jobHandle);
    }
}