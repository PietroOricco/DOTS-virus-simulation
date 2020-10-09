using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

//system handling the contagion logic

[UpdateAfter(typeof(QuadrantSystem))]
public class ContagionSystem : SystemBase
{
    //copy of the grid, used to know where is each entity
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

        //job -> each element, if not infected, check if there are infected in its same quadrant
        Entities.ForEach((Entity entity, int nativeThreadIndex, Translation t, ref QuadrantEntity qe, ref HumanComponent humanComponent, ref InfectionComponent ic) =>{
            if(ic.infected==false){
                //not infected-> look for infected in same cell
                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(t.Value);
                //Debug.Log("Infected false");
                QuadrantData quadrantData;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)){
                    do{
                        //Debug.Log(quadrantData.position);
                        //found infected in same quadrant -> if too close, increment infection Counter
                        if (math.distance(t.Value, quadrantData.position) < 2f){
                            //infection counter is determined by time and human responsibility
                            humanComponent.infectionCounter += 5f * deltaTime*(1-humanComponent.socialResposibility);
                            //Debug.Log(humanComponent.infectionCounter);
                        }
                    } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                }
                else{
                    //no infected in same cell -> human is safe
                    humanComponent.infectionCounter = 0f;
                }

                if (humanComponent.infectionCounter >= threshold)
                {
                    //human become infected
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
