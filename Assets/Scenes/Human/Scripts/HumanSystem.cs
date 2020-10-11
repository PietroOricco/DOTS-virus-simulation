using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using System;
using Unity.Mathematics;
using Unity.Collections;



public class HumanSystem : SystemBase{
    private EndSimulationEntityCommandBufferSystem ecbSystem;

    [ReadOnly]
    private NativeArray<TileMapEnum.TileMapSprite> Grid;
    [ReadOnly]
    private float CellSize;
    [ReadOnly]
    private int Width;
 
    protected override void OnCreate(){
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning(){
        Grid = Testing.Instance.grid.GetGridByValue((GridNode gn)=>{return gn.GetTileType();});
        CellSize = Testing.Instance.grid.GetCellSize();
        Width = Testing.Instance.grid.GetWidth();
    }

    //Handles increment and decrement of parameters of HumanComponent
    protected override void OnUpdate(){
        var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

        float deltaTime = Time.DeltaTime;

        var width = Width;
        var cellSize = CellSize;
        var grid = Grid;

        JobHandle jobhandle = Entities.ForEach((ref Translation t, ref HumanComponent hc) =>{

            //increment of 1 value per second for each HumanComponent parameters
            hc.hunger = math.min(hc.hunger + 1f * deltaTime, 100f);
            hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, 100f);
            hc.sociality = math.min(hc.sociality + 1f * deltaTime, 100f);
            hc.sportivity = math.min(hc.sportivity + 1f * deltaTime, 100f);

            //retrieve entity position
            GetXY(t.Value, Vector3.zero, cellSize, out int currentX, out int currentY); //TODO fix hardcoded origin

            //decrement based to position:
            //home -> decrement fatigue
            //park -> decrement sociality and sportivity
            //pub -> decrement hunger and sociality
            //road -> decrement sportivity
            switch (grid[currentX+currentY*width]){
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

        //cycle all the entities without a NeedComponent and assign it according to parameters
        JobHandle jobhandle1 = Entities.WithNone<NeedComponent>().ForEach((Entity entity, int nativeThreadIndex, in HumanComponent hc) =>{
            
            //set searchRadius for retrieving areas in the map included in that radius if the need is over a certain threshold
            if (hc.hunger > 75f){
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needForFood
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex , entity, new NeedPathParams{
                    searchRadius=2
                });
            }
            else if (hc.fatigue > 75f){
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needToRest
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex , entity, new NeedPathParams{
                    searchRadius=2
                });
            }
            else if (hc.sportivity > 75f){
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needForSport
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex , entity, new NeedPathParams{
                    searchRadius=2
                });
            }
            else if (hc.sociality > 75f){
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needForSociality
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex , entity, new NeedPathParams{
                    searchRadius=2
                });
            }
        }).ScheduleParallel(jobhandle);

        jobhandle1.Complete();

        //manage satisfied needs, when value for a parameter decreases under 25 as threshold 
        JobHandle jobhandle2 = Entities.ForEach((Entity entity, int nativeThreadIndex, in HumanComponent hc, in NeedComponent needComponent) =>{
            if (needComponent.currentNeed == NeedType.needForFood && hc.hunger < 25f)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needToRest && hc.fatigue < 25f)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needForSport && hc.sportivity < 25f)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needForSociality && hc.sociality < 25f)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
        }).ScheduleParallel(jobhandle1);

        jobhandle2.Complete();

        ecbSystem.AddJobHandleForProducer(jobhandle2);
    }

    protected override void OnStopRunning(){
        Grid.Dispose();
    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y) {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }
}