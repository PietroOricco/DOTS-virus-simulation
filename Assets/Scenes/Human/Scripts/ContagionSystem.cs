using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using System.Threading;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using System.IO;

[UpdateAfter(typeof(QuadrantSystem))]
//[UpdateAfter(typeof(PathFollowSystem))]
[BurstCompile]
public class ContagionSystem : SystemBase
{
    //copy of the grid, used to know where is each entity
    public NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap2;

    private const float contagionThreshold = 15f;

    Configuration conf;

    EndInitializationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    public static long infectedCounter;
    public static long totalInfectedCounter;
    public static long symptomaticCounter;
    public static long asymptomaticCounter;
    public static long recoveredCounter;
    public static long totalRecoveredCounter;
    public static long deathCounter;
    public static long populationCounter;
    private StreamWriter writer;
    public static string logPath = "statistics/log.txt";

    protected override void OnCreate()
    {
        conf = Configuration.CreateFromJSON();
        quadrantMultiHashMap2 = QuadrantSystem.quadrantMultiHashMap;
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        infectedCounter = conf.numberOfInfects;
        symptomaticCounter = 0;
        asymptomaticCounter = 0;
        recoveredCounter = 0;
        deathCounter = 0;
        populationCounter = conf.numberOfHumans;
        writer = new StreamWriter(logPath, false); // false is for overwrite existing file
        writer.WriteLine("Population\tExposed\tTotalExposed\tSymptomatic\tAsymptomatic\tDeath\tRecovered\tTotalRecovered\tMinutesPassed");
    }

    protected override void OnUpdate(){
        var quadrantMultiHashMap = quadrantMultiHashMap2;
        float deltaTime = Time.DeltaTime;

        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

        NativeArray<long> localInfectedCounter = new NativeArray<long>(1, Allocator.TempJob);
        localInfectedCounter[0]=0;

        NativeArray<long> localTotalInfectedCounter = new NativeArray<long>(1, Allocator.TempJob);
        localTotalInfectedCounter[0]=0;
        
        NativeArray<long> localSymptomaticCounter = new NativeArray<long>(1, Allocator.TempJob);
        localSymptomaticCounter[0]=0;

        NativeArray<long> localAsymptomaticCounter = new NativeArray<long>(1, Allocator.TempJob);
        localAsymptomaticCounter[0] = 0;

        NativeArray<long> localDeathCounter = new NativeArray<long>(1, Allocator.TempJob);
        localDeathCounter[0] = 0;

        NativeArray<long> localRecoveredCounter = new NativeArray<long>(1, Allocator.TempJob);
        localRecoveredCounter[0] = 0;

        NativeArray<long> localTotalRecoveredCounter = new NativeArray<long>(1, Allocator.TempJob);
        localTotalRecoveredCounter[0] = 0;

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
                    Interlocked.Increment(ref ((long *)localTotalInfectedCounter.GetUnsafePtr())[0]);
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
                ic.infected=true;

                if (ic.humanSymptomsProbability > (100 - ic.globalSymptomsProbability))
                {
                    //symptomatic -> lasts between 0.5 and 1.5 day
                    ic.symptomatic = true;
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localSymptomaticCounter.GetUnsafePtr())[0]);    
                    }
                    ic.infectiousCounter = 0;
                }
                else
                {
                    //asymptomatic
                    ic.symptomatic = false;
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localAsymptomaticCounter.GetUnsafePtr())[0]);
                    }
                    ic.infectiousCounter = 0;
                }
            }

            if(ic.infectiousCounter > ic.infectiousThreshold && ic.status == Status.infectious)
            {
                if(ic.humanDeathProbability > (100-ic.globalDeathProbability))
                {
                    //remove entity
                    unsafe
                    {
                        Interlocked.Increment(ref ((long*)localDeathCounter.GetUnsafePtr())[0]);
                        Interlocked.Decrement(ref ((long *)localInfectedCounter.GetUnsafePtr())[0]);
                    }


                    if (ic.symptomatic)
                        unsafe
                        {
                            Interlocked.Decrement(ref ((long*)localSymptomaticCounter.GetUnsafePtr())[0]);
                        }
                    else
                        unsafe
                        {
                            Interlocked.Decrement(ref ((long*)localAsymptomaticCounter.GetUnsafePtr())[0]);
                        }
                    ic.status = Status.removed;
                    qe.typeEnum = QuadrantEntity.TypeEnum.removed;
                    ecb.DestroyEntity(nativeThreadIndex, entity);
                }
                else
                {
                    //recovery time set up
                    unsafe{
                        Interlocked.Decrement(ref ((long *)localInfectedCounter.GetUnsafePtr())[0]);
                        Interlocked.Increment(ref ((long*)localRecoveredCounter.GetUnsafePtr())[0]);
                        Interlocked.Increment(ref ((long*)localTotalRecoveredCounter.GetUnsafePtr())[0]);
                    }
                    if (ic.symptomatic)
                        unsafe
                        {
                            Interlocked.Decrement(ref ((long*)localSymptomaticCounter.GetUnsafePtr())[0]);
                        }
                    else
                        unsafe
                        {
                            Interlocked.Decrement(ref ((long*)localAsymptomaticCounter.GetUnsafePtr())[0]);
                        }
                
                    ic.status = Status.recovered;
                    ic.infected=false;
                    //qe.typeEnum = QuadrantEntity.TypeEnum.recovered;
                    ic.recoveredCounter = 0;
                }
            }

            if(ic.recoveredCounter > ic.recoveredThreshold && ic.status == Status.recovered)
            {
                ic.status = Status.susceptible;
                qe.typeEnum = QuadrantEntity.TypeEnum.susceptible;
                unsafe{
                    Interlocked.Decrement(ref ((long*)localRecoveredCounter.GetUnsafePtr())[0]);
                }
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
            Interlocked.Add(ref totalInfectedCounter, Interlocked.Read(ref ((long *)localTotalInfectedCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref symptomaticCounter, Interlocked.Read(ref ((long*)localSymptomaticCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref asymptomaticCounter, Interlocked.Read(ref ((long*)localAsymptomaticCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref recoveredCounter, Interlocked.Read(ref ((long*)localRecoveredCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref deathCounter, Interlocked.Read(ref ((long*)localDeathCounter.GetUnsafePtr())[0]));
            Interlocked.Add(ref populationCounter, -Interlocked.Read(ref ((long*)localDeathCounter.GetUnsafePtr())[0]));
        }

        //Write some text to the test.txt file
        writer.WriteLine(Interlocked.Read(ref populationCounter)+"\t"
                            +Interlocked.Read(ref infectedCounter)+"\t"
                            +Interlocked.Read(ref totalInfectedCounter)+"\t"
                            +Interlocked.Read(ref symptomaticCounter)+"\t"
                            +Interlocked.Read(ref asymptomaticCounter)+"\t"
                            +Interlocked.Read(ref deathCounter)+"\t"
                            +Interlocked.Read(ref recoveredCounter)+"\t"
                            +Interlocked.Read(ref totalRecoveredCounter)+"\t"
                            +(int)Datetime.total_minutes);

        localInfectedCounter.Dispose();
        localTotalInfectedCounter.Dispose();
        localAsymptomaticCounter.Dispose();
        localSymptomaticCounter.Dispose();
        localRecoveredCounter.Dispose();
        localTotalRecoveredCounter.Dispose();
        localDeathCounter.Dispose();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        writer.Close();
    }
}


