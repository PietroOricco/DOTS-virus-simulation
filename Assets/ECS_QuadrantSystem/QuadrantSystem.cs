/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;


public struct QuadrantEntity : IComponentData {
    public TypeEnum typeEnum;

    public enum TypeEnum {
        Healthy,
        Sick
    }
}

public struct QuadrantData {
    public Entity entity;
    public float3 position;
    public QuadrantEntity quadrantEntity;
}

public class QuadrantSystem : SystemBase {

    public static NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap;

    public const int quadrantYMultiplier = 1000;
    public const float quadrantCellSize = 10f;

    public static int GetPositionHashMapKey(float3 position) {
        return (int) (math.floor(position.x / quadrantCellSize) + (quadrantYMultiplier * math.floor(position.y / quadrantCellSize)));
    }


    private static int GetEntityCountInHashMap(NativeMultiHashMap<int, QuadrantData> quadrantMultiHashMap, int hashMapKey) {
        QuadrantData quadrantData;
        NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
        int count = 0;
        if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator)) {
            do {
                count++;
            } while (quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
        }
        return count;
    }

    /*[BurstCompile]
    private struct SetQuadrantDataHashMapJob : IJobForEachWithEntity<Translation, QuadrantEntity> {

        public NativeMultiHashMap<int, QuadrantData>.Concurrent quadrantMultiHashMap;

        public void Execute(Entity entity, int index, ref Translation translation, ref QuadrantEntity quadrantEntity) {
            int hashMapKey = GetPositionHashMapKey(translation.Value);
            quadrantMultiHashMap.Add(hashMapKey, new QuadrantData {
                entity = entity,
                position = translation.Value,
                quadrantEntity = quadrantEntity
            });
        }

    }*/

    protected override void OnCreate() {
        quadrantMultiHashMap = new NativeMultiHashMap<int, QuadrantData>(250000, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy() {
        quadrantMultiHashMap.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate() {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantEntity));

        quadrantMultiHashMap.Clear();
        if (entityQuery.CalculateEntityCount() > quadrantMultiHashMap.Capacity) {
            quadrantMultiHashMap.Capacity = entityQuery.CalculateEntityCount();
        }
         
        JobHandle jobHandle = Entities.ForEach((Entity entity, Translation t, ref QuadrantEntity qe, in InfectionComponent ic) =>{
            NativeMultiHashMap<int, QuadrantData>.ParallelWriter quadrantMultiHashMap2 = quadrantMultiHashMap.AsParallelWriter();

            int hashMapKey = GetPositionHashMapKey(t.Value);
            //Debug.Log(hashMapKey);
            quadrantMultiHashMap2.Add(hashMapKey, new QuadrantData{
                entity = entity,
                position = t.Value,
                quadrantEntity = qe
            });
            
        }).ScheduleParallel(Dependency);
        jobHandle.Complete();
        //Debug.Log(GetEntityCountInHashMap(quadrantMultiHashMap, GetPositionHashMapKey(new float3(0f,0f,0))));
    }

}
