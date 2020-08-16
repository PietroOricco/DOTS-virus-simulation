using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

public class ContagionSystem : SystemBase
{
    public static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap = QuadrantSystem.quadrantMultiHashMap;
    private const float threshold = 20f;
    private EndSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        float deltaTime = Time.DeltaTime;
        Entities.ForEach((Entity entity, int nativeThreadIndex, Translation t, ref QuadrantEntity qe, ref HumanComponent humanComponent, ref InfectionComponent ic) =>
        {
            if(ic.infected==false){
                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(t.Value);

                QuadrantData quadrantData;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)){
                    do{
                        if (math.distance(t.Value, quadrantData.position) < 2f){
                            humanComponent.infectionCounter += 5f * deltaTime*(1-humanComponent.socialResposibility);
                        }
                    } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                }
                else{
                    humanComponent.infectionCounter = 0f;
                }

                if (humanComponent.infectionCounter >= threshold)
                {
                    qe.typeEnum = QuadrantEntity.TypeEnum.Sick;
                    ecb.SetComponent<InfectionComponent>(nativeThreadIndex, entity, new InfectionComponent{
                        infected = true
                    });
                }
            }
        }).ScheduleParallel();
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
