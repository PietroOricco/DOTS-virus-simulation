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
    //copy of the grid, used to know where is each entity
    public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap2;
    private const float threshold = 20f;

    protected override void OnCreate()
    {
        quadrantMultiHashMap2 = QuadrantSystem.quadrantMultiHashMap;
    }

    protected override void OnUpdate(){

        float deltaTime = Time.DeltaTime;
        var quadrantMultiHashMap = quadrantMultiHashMap2;

        //job -> each element, if not infected, check if there are infected in its same quadrant
        Entities.ForEach((Entity entity, int nativeThreadIndex, Translation t, ref QuadrantEntity qe, ref HumanComponent humanComponent, ref InfectionComponent ic) =>{
            
            //for non infected entities, a check in the direct neighbours is done for checking the presence of infected 
            if(ic.infected==false){
                //not infected-> look for infected in same cell
                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(t.Value);
                //Debug.Log("Infected false");
                QuadrantData quadrantData;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)){
                    //cycle through all entries oh hashmap with the same Key
                    do{
                        //Debug.Log(quadrantData.position);                       
                        if (math.distance(t.Value, quadrantData.position) < 2f){
                            //TODO consider also social resp other human involved
                            //increment infectionCounter if one/more infected are in close positions (infected at quadrantData.position) 
                            //infection counter is determined by time and human responsibility
                            humanComponent.infectionCounter += 5f * deltaTime*(1-humanComponent.socialResposibility);
                        }
                    } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                }
                else{
                    //no infected in same cell -> human is safe
                    humanComponent.infectionCounter = 0f;
                }

                //infection happened
                if (humanComponent.infectionCounter >= threshold)
                {
                    //human become infected
                    qe.typeEnum = QuadrantEntity.TypeEnum.Sick;
                    ic.infected = true;
                }
            }
        }).WithReadOnly(quadrantMultiHashMap).ScheduleParallel();
    }
}
