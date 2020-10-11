using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;

[UpdateAfter(typeof(QuadrantSystem))]
public class ContagionSystem : SystemBase
{
    public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap2;
    private const float threshold = 20f;

    protected override void OnCreate()
    {
        quadrantMultiHashMap2 = QuadrantSystem.quadrantMultiHashMap;
    }

    protected override void OnUpdate(){

        float deltaTime = Time.DeltaTime;
        var quadrantMultiHashMap = quadrantMultiHashMap2;

        Entities.ForEach((Entity entity, int nativeThreadIndex, Translation t, ref QuadrantEntity qe, ref HumanComponent humanComponent, ref InfectionComponent ic) =>{
            if(ic.infected==false){
                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(t.Value);
                //Debug.Log("Infected false");
                QuadrantData quadrantData;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)){
                    do{
                        //Debug.Log(quadrantData.position);
                        if (math.distance(t.Value, quadrantData.position) < 2f){
                            //TODO consider also social resp other human involved
                            humanComponent.infectionCounter += 5f * deltaTime*(1-humanComponent.socialResposibility);
                            //Debug.Log(humanComponent.infectionCounter);
                        }
                    } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                }
                else{
                    humanComponent.infectionCounter = 0f;
                }

                if (humanComponent.infectionCounter >= threshold)
                {
                    qe.typeEnum = QuadrantEntity.TypeEnum.Sick;
                    ic.infected = true;
                }
            }
        }).WithReadOnly(quadrantMultiHashMap).ScheduleParallel();
    }
}
