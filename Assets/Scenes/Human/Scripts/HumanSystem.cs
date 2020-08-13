using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using System;
using Unity.Mathematics;
using Unity.Collections;


//Handles increment and decrement of status
public class HumanSystem : SystemBase{
    private EndSimulationEntityCommandBufferSystem ecbSystem;
 
    protected override void OnCreate(){
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate(){
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        NativeArray<TileMapEnum.TileMapSprite> grid = Testing.Instance.grid.GetGridByValue((GridNode gn)=>{return gn.GetTileType();});

        float cellSize = Testing.Instance.grid.GetCellSize();
        int width = Testing.Instance.grid.GetWidth();

        float deltaTime = Time.DeltaTime;

        JobHandle jobhandle = Entities.ForEach((ref Translation t, ref HumanComponent hc) =>{
            hc.hunger = math.min(hc.hunger + 1f * deltaTime, 100f);
            hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, 100f);
            hc.sociality = math.min(hc.sociality + 1f * deltaTime, 100f);
            hc.sportivity = math.min(hc.sportivity + 1f * deltaTime, 100f);

            GetXY(t.Value, Vector3.zero, cellSize, out int currentX, out int currentY); //TODO fix hardcoded origin

            switch(grid[currentX+currentY*width]){
                case TileMapEnum.TileMapSprite.Home:
                    hc.fatigue = Math.Max(0, hc.fatigue-5f* deltaTime);
                    break;
                case TileMapEnum.TileMapSprite.Park:
                    hc.sportivity = Math.Max(0, hc.sportivity-5f* deltaTime);
                    hc.sociality = Math.Max(0, hc.sociality-5f* deltaTime);
                    break;
                case TileMapEnum.TileMapSprite.Pub:
                    hc.hunger = Math.Max(0, hc.hunger-3f* deltaTime);
                    hc.sociality = Math.Max(0, hc.sociality-5f* deltaTime);
                    break;
                case TileMapEnum.TileMapSprite.Supermarket:
                    hc.hunger = Math.Max(0, hc.hunger-5f* deltaTime);
                    break;
                case TileMapEnum.TileMapSprite.Road:
                    hc.sportivity = Math.Max(0, hc.sportivity-2f* deltaTime);
                    break;
            }
        }).ScheduleParallel(Dependency);
        jobhandle.Complete();
        grid.Dispose();

        JobHandle jobhandle1 = Entities.WithNone<NeedComponent>().ForEach((Entity entity, int nativeThreadIndex, in HumanComponent hc) =>{
            if (hc.hunger > 75f)
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needForFood
                });
            else if (hc.fatigue > 75f)
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needToRest
                });
            else if (hc.sportivity > 75f)
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needForSport
                });
            else if (hc.sociality > 75f)
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needForSociality
                });
        }).ScheduleParallel(jobhandle);

        Entities.ForEach((Entity entity, int nativeThreadIndex, in HumanComponent hc, in NeedComponent needComponent) =>{
            if (needComponent.currentNeed == NeedType.needForFood && hc.hunger < 25f)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needToRest && hc.fatigue < 25f)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needForSport && hc.sportivity < 25f)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needForSociality && hc.sociality < 25f)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
        }).ScheduleParallel(jobhandle1);

        ecbSystem.AddJobHandleForProducer(Dependency);
    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y) {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }
}