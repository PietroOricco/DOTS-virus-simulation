using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using JetBrains.Annotations;
using System;


[UpdateAfter(typeof(QuadrantSystem))]
public class ContagionSystem : SystemBase
{
    //copy of the grid, used to know where is each entity
    public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap2;

    private const float contagionThreshold = 15f;

    protected override void OnCreate()
    {
        quadrantMultiHashMap2 = QuadrantSystem.quadrantMultiHashMap;
    }

    protected override void OnUpdate(){
        var quadrantMultiHashMap = quadrantMultiHashMap2;
        float deltaTime = Time.DeltaTime;

        //job -> each element, if not infected, check if there are infected in its same quadrant
        Entities.ForEach((Entity entity, int nativeThreadIndex, Translation t, ref QuadrantEntity qe, ref HumanComponent humanComponent, ref InfectionComponent ic) =>{
            float symptomsProbability;
            float deathProbability;
            //for non infected entities, a check in the direct neighbours is done for checking the presence of infected 
            if (ic.status == Status.susceptible)
            {
                //not infected-> look for infected in same cell
                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(t.Value);
                //Debug.Log("Infected false");
                QuadrantData quadrantData;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
                {
                    //cycle through all entries oh hashmap with the same Key
                    do
                    {
                        //Debug.Log(quadrantData.position);                       
                        if (math.distance(t.Value, quadrantData.position) < 2f)
                        {
                            //TODO consider also social resp other human involved
                            //increment infectionCounter if one/more infected are in close positions (infected at quadrantData.position) 
                            //infection counter is determined by time and human responsibility
                            ic.contagionCounter += 1f * deltaTime * (1 - humanComponent.socialResposibility);
                        }
                        //TODO we could add a cap here to speed up the check
                    } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                }
                else
                {
                    //no infected in same cell -> human is safe
                    ic.contagionCounter = 0f;
                }
            }
            //infection happened
            if (ic.contagionCounter >= contagionThreshold && ic.status == Status.susceptible)
            {
                //human become infected
                Counter.infectedCounter++;
                qe.typeEnum = QuadrantEntity.TypeEnum.exposed;
                ic.status = Status.exposed;
                   
                ic.exposedCounter = 0;
            }

            //Infectious status -> symptoms vs non-symptoms
            if (ic.exposedCounter > ic.exposedThreshold  && ic.status == Status.exposed)
            {
                qe.typeEnum = QuadrantEntity.TypeEnum.infectious;
                ic.status = Status.infectious;

                if (ic.humanSymptomsProbability > (100 - ic.globalSymptomsProbability))
                {
                    //symptomatic -> lasts between 0.5 and 1.5 day
                    ic.symptomatic = true;
                        
                    ic.infectiousCounter = 0;
                }
                else
                {
                    //asymptomatic
                    ic.symptomatic = false;

                    ic.infectiousCounter = 0;
                }
            }

            if(ic.infectiousCounter > ic.infectiousThreshold && ic.status == Status.infectious)
            {
                if(ic.humanDeathProbability > (100-ic.globalDeathProbability))
                {
                    //remove entity
                    Death_counter.deathCounter++;
                    ic.status = Status.removed;
                    qe.typeEnum = QuadrantEntity.TypeEnum.removed;
                }
                else
                {
                    //recovery time set up
                    ic.recoveredCounter = 0;
                }
            }

            if(ic.recoveredCounter > ic.recoveredThreshold && ic.status == Status.recovered)
            {
                Counter.infectedCounter--;
                ic.status = Status.susceptible;
                qe.typeEnum = QuadrantEntity.TypeEnum.susceptible;
            }

            if (ic.status == Status.exposed)
            {
                ic.exposedCounter += 1f * deltaTime;
            }
            if (ic.status == Status.infectious)
            {
                ic.infectiousCounter += 1f * deltaTime;
            }
            if (ic.status == Status.recovered)
            {
                ic.recoveredCounter += 1f * deltaTime;
            }

        }).WithReadOnly(quadrantMultiHashMap).WithoutBurst().ScheduleParallel();
    }
}


