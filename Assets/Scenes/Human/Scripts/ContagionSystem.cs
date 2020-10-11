using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

//System for contagion calculation

[UpdateAfter(typeof(QuadrantSystem))]
public class ContagionSystem : SystemBase
{
    public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap2;
    private const float threshold = 20f;
    private EndSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        quadrantMultiHashMap2 = QuadrantSystem.quadrantMultiHashMap;
    }

    protected override void OnUpdate()
    {
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        float deltaTime = Time.DeltaTime;

        var quadrantMultiHashMap = quadrantMultiHashMap2;

        Entities.ForEach((Entity entity, int nativeThreadIndex, Translation t, ref QuadrantEntity qe, ref HumanComponent humanComponent, ref InfectionComponent ic) =>{
            
            //for non infected entities, a check in the direct neighbours is done for checking the presence of infected 
            if(ic.infected==false){
                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(t.Value);
                //Debug.Log("Infected false");
                QuadrantData quadrantData;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)){
                    //cycle through all entries oh hashmap with the same Key
                    do{
                        if (math.distance(t.Value, quadrantData.position) < 2f){
                            //increment infectionCounter if one/more infected are in close positions (infected at quadrantData.position) 
                            humanComponent.infectionCounter += 5f * deltaTime*(1-humanComponent.socialResposibility);
                        }
                    } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                }
                else{
                    humanComponent.infectionCounter = 0f;
                }

                //infection happened
                if (humanComponent.infectionCounter >= threshold)
                {
                    qe.typeEnum = QuadrantEntity.TypeEnum.Sick;
                    ic.infected = true;
                    //Debug.Log("Contagion");
                    /*ecb.SetComponent<InfectionComponent>(nativeThreadIndex, entity, new InfectionComponent{
                        infected = true
                    });*/
                }
            }
        }).WithReadOnly(quadrantMultiHashMap).ScheduleParallel();
        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
