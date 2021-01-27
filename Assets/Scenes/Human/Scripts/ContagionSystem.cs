using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using System.Threading;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;

[UpdateAfter(typeof(QuadrantSystem))]
[UpdateAfter(typeof(PathFollowSystem))]
[BurstCompile]
public class ContagionSystem : SystemBase
{
    //copy of the grid, used to know where is each entity
    public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap2;

    private const float contagionThreshold = 15f;

    EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    public static long infectedCounter = 0;

    protected override void OnCreate()
    {
        quadrantMultiHashMap2 = QuadrantSystem.quadrantMultiHashMap;
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        infectedCounter = 0;
    }

    protected override void OnUpdate(){
        var quadrantMultiHashMap = quadrantMultiHashMap2;
        float deltaTime = Time.DeltaTime;

        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

        NativeArray<long> localInfectedCounter = new NativeArray<long>(1, Allocator.TempJob);
        localInfectedCounter[0]=0;

        // TODO remove
        int localSynthomaticCounter = synthomatic_counter.synthomatic;
        int localAsynthomaticCounter = Asynthomatic_counter.asynthomatic;
        int localDeathCounter = Death_counter.deathCounter;
        int localPopulationCounter = Population_counter.population;
        int localRecoveredCounter = Recovered_counter.recovered;

        //job -> each element, if not infected, check if there are infected in its same quadrant
        var jobHandle = Entities.ForEach((Entity entity, int nativeThreadIndex, Translation t, ref QuadrantEntity qe, ref HumanComponent humanComponent, ref InfectionComponent ic) =>{
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
                unsafe{
                    Interlocked.Increment(ref ((long *)localInfectedCounter.GetUnsafePtr())[0]);
                }
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
                    Interlocked.Increment(ref localSynthomaticCounter);    
                    ic.infectiousCounter = 0;
                }
                else
                {
                    //asymptomatic
                    ic.symptomatic = false;
                    Interlocked.Increment(ref localAsynthomaticCounter);
                    ic.infectiousCounter = 0;
                }
            }

            if(ic.infectiousCounter > ic.infectiousThreshold && ic.status == Status.infectious)
            {
                if(ic.humanDeathProbability > (100-ic.globalDeathProbability))
                {
                    //remove entity
                    Interlocked.Increment(ref localDeathCounter);
                    unsafe{
                        Interlocked.Decrement(ref ((long *)localInfectedCounter.GetUnsafePtr())[0]);
                    }
                    Interlocked.Decrement(ref localPopulationCounter);
                    if (ic.symptomatic)
                        Interlocked.Decrement(ref localSynthomaticCounter);
                    else
                        Interlocked.Decrement(ref localAsynthomaticCounter);
                    ic.status = Status.removed;
                    qe.typeEnum = QuadrantEntity.TypeEnum.removed;
                    ecb.DestroyEntity(nativeThreadIndex, entity);
                }
                else
                {
                    //recovery time set up
                    unsafe{
                        Interlocked.Decrement(ref ((long *)localInfectedCounter.GetUnsafePtr())[0]);
                    }
                    Interlocked.Increment(ref localRecoveredCounter);
                    if (ic.symptomatic)
                        Interlocked.Decrement(ref localSynthomaticCounter);
                    else
                        Interlocked.Decrement(ref localAsynthomaticCounter);
                    ic.status = Status.recovered;
                    //qe.typeEnum = QuadrantEntity.TypeEnum.recovered;
                    ic.recoveredCounter = 0;
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

        }).WithReadOnly(quadrantMultiHashMap).ScheduleParallel(Dependency);

        m_EndSimulationEcbSystem.AddJobHandleForProducer(jobHandle);
        this.Dependency = jobHandle;

        jobHandle.Complete();
        
        unsafe{
            Interlocked.Add(ref infectedCounter, Interlocked.Read(ref ((long *)localInfectedCounter.GetUnsafePtr())[0]));
        }
        
        // TODO remove
        synthomatic_counter.synthomatic = localSynthomaticCounter;
        Asynthomatic_counter.asynthomatic = localAsynthomaticCounter;
        Death_counter.deathCounter = localDeathCounter;
        Population_counter.population = localPopulationCounter;
        Recovered_counter.recovered = localRecoveredCounter;
        // end TODO remove

        localInfectedCounter.Dispose();
    }
}


