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
    private const float contagionThreshold = 900f;

    protected override void OnCreate()
    {
        quadrantMultiHashMap2 = QuadrantSystem.quadrantMultiHashMap;
    }

    protected override void OnUpdate(){

        float deltaTime = Time.DeltaTime;
       
       
        var quadrantMultiHashMap = quadrantMultiHashMap2;

        //job -> each element, if not infected, check if there are infected in its same quadrant
        Entities.ForEach((Entity entity, int nativeThreadIndex, Translation t, ref QuadrantEntity qe, ref HumanComponent humanComponent, ref InfectionComponent ic) =>{
            float symptomsProbability;
            float deathProbability;
            //for non infected entities, a check in the direct neighbours is done for checking the presence of infected 
            if (ic.status == Status.susceptible){
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
                            ic.contagionCounter += 1f * deltaTime*(1-humanComponent.socialResposibility);
                        }
                    //TODO we could add a cap here to speed up the check
                    } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                }
                else{
                    //no infected in same cell -> human is safe
                    ic.contagionCounter = 0f;
                }

                //infection happened
                if (ic.contagionCounter >= contagionThreshold && ic.status == Status.susceptible)
                {
                    //human become infected
                    Counter.infectedCounter++;
                    qe.typeEnum = QuadrantEntity.TypeEnum.exposed;
                    ic.status = Status.exposed;
                   
                    float mean = (3 + 6) * 3600 * 24 / 2;
                    float sigma = (6*3600*24 - mean) / 3;

                    ic.exposedThreshold = GenerateNormalRandom(mean, sigma, 3 * 3600 * 24 , 6 * 3600 * 24);
                    ic.exposedCounter = 0;
                }

                //Infectious status -> symptoms vs non-symptoms
                if (ic.exposedCounter > ic.exposedThreshold  && ic.status == Status.exposed)
                {
                    qe.typeEnum = QuadrantEntity.TypeEnum.infectious;
                    ic.status = Status.infectious;
                    symptomsProbability = UnityEngine.Random.Range(0, 100);

                    if (symptomsProbability > (1 - ic.symptomsProbability))
                    {
                        //symptomatic -> lasts between 0.5 and 1.5 day
                        ic.symptomatic = true;
                        float mean = (0.5f + 1.5f) * 3600 * 24 / 2;
                        float sigma = (1.5f * 3600 * 24 - mean) / 3;

                        ic.infectiousThreshold = GenerateNormalRandom(mean, sigma, 0.5f * 24 * 3600, 1.5f * 24 * 3600);
                        ic.infectiousCounter = 0;
                    }
                    else
                    {
                        //asymptomatic
                        ic.symptomatic = false;

                        float mean = (2 + 4) * 3600 * 24 / 2;
                        float sigma = (4 * 3600 * 24 - mean) / 3;

                        ic.infectiousThreshold = GenerateNormalRandom(mean, sigma, 2 * 24 * 3600, 4 * 24 * 3600);
                        ic.infectiousCounter = 0;
                    }
                }

                if(ic.infectiousCounter > ic.infectiousThreshold && ic.status == Status.infectious)
                {
                    deathProbability = UnityEngine.Random.Range(0, 100);
                    if(deathProbability > (1-ic.deathProbability))
                    {
                        //remove entity
                        ic.status = Status.removed;
                        qe.typeEnum = QuadrantEntity.TypeEnum.removed;
                    }
                    else
                    {
                        //recovery time set up
                        ic.recoveredCounter = 0;

                        float mean = (30 + 45) * 3600 * 24 / 2;
                        float sigma = (45 * 3600 * 24 - mean) / 3;

                        ic.recoveredThreshold = GenerateNormalRandom(mean, sigma, 30 * 24 * 3600, 45 * 24 * 3600);
                    }
                }

                if(ic.recoveredCounter > ic.recoveredThreshold && ic.status == Status.recovered)
                {
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


            }
        }).WithReadOnly(quadrantMultiHashMap).WithoutBurst().ScheduleParallel();
    }

    public static float GenerateNormalRandom(float mean, float sigma, float min, float max)
    {
        float rand1 = UnityEngine.Random.Range(0.0f, 1.0f);
        float rand2 = UnityEngine.Random.Range(0.0f, 1.0f);

        float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

        float generatedNumber = (mean + sigma * n);

        generatedNumber = Mathf.Clamp(generatedNumber, min, max);

        return generatedNumber;
    }
}
