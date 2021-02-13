using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using System;
using Unity.Mathematics;
using Unity.Collections;

[UpdateAfter(typeof(ContagionSystem))] 
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

        var lockdown = Human.conf.lockdown;

        JobHandle jobhandle = Entities.ForEach(( ref HumanComponent hc, in InfectionComponent ic) =>{

            if (!lockdown)
            {
                if(ic.symptomatic&&hc.socialResposibility>0.5){
                    hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, 17 * 60);
                }
                else{
                    //increment of 1 value per second for each HumanComponent parameters
                    hc.hunger = math.min(hc.hunger + 1f * deltaTime, 7 * 60);
                    hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, 17 * 60);
                    hc.sociality = math.min(hc.sociality + 1f * deltaTime, 11 * 60);
                    hc.sportivity = math.min(hc.sportivity + 1f * deltaTime, 23 * 60);
                    hc.grocery = math.min(hc.grocery + 1f * deltaTime, 3 * 25 * 60);
                    hc.work = math.min(hc.work + 1f * deltaTime, 17 * 60);
                }
            }
            else{
                if(ic.symptomatic){
                    hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, 17 * 60);
                }
                else{
                    hc.hunger = math.min(hc.hunger + 1f * deltaTime, 7 * 60);
                    hc.fatigue = math.min(hc.fatigue + 1f * deltaTime, 17 * 60);
                    hc.work = math.min(hc.work + hc.jobEssentiality * deltaTime, 17 * 60);
                    hc.grocery = math.min(hc.grocery + 0.5f * deltaTime, 3 * 25 * 60);
                    hc.sociality = math.min(hc.sociality + (1-hc.socialResposibility)*0.1f * deltaTime, 11 * 60);
                    hc.sportivity = math.min(hc.sportivity + (1 - hc.socialResposibility) * 0.1f * deltaTime, 23 * 60);
                }
                
            }
        }).ScheduleParallel(Dependency);
        jobhandle.Complete();

        //cycle all the entities without a NeedComponent and assign it according to parameters
        JobHandle jobhandle1 = Entities.WithNone<NeedComponent>().ForEach((Entity entity, int nativeThreadIndex, in HumanComponent hc) =>{
            
            //set searchRadius for retrieving areas in the map included in that radius if the need is over a certain threshold
            if (hc.hunger > 60*6){
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needForFood
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex , entity, new NeedPathParams{
                    searchRadius=2
                });
            }
            else if (hc.fatigue > 16*60){
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needToRest
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex , entity, new NeedPathParams{
                    searchRadius=2
                });
            }
            else if (hc.sportivity > 23*60){
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needForSport
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex , entity, new NeedPathParams{
                    searchRadius=2
                });
            }
            else if (hc.sociality > 10*60){
                ecb.AddComponent<NeedComponent>(nativeThreadIndex , entity, new NeedComponent{
                    currentNeed=NeedType.needForSociality
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex , entity, new NeedPathParams{
                    searchRadius=2
                });
            }
            else if (hc.grocery > 3*24 * 60)
            {
                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needForGrocery
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }
            else if (hc.work > 16 * 60)
            {
                ecb.AddComponent<NeedComponent>(nativeThreadIndex, entity, new NeedComponent
                {
                    currentNeed = NeedType.needToWork
                });
                ecb.AddComponent<NeedPathParams>(nativeThreadIndex, entity, new NeedPathParams
                {
                    searchRadius = 2
                });
            }
        }).ScheduleParallel(jobhandle);

        jobhandle1.Complete();

        //manage satisfied needs, when value for a parameter decreases under 25% as threshold 
        JobHandle jobhandle2 = Entities.ForEach((Entity entity, int nativeThreadIndex, ref HumanComponent hc, in Translation t, in NeedComponent needComponent) =>{
            //retrieve entity position
            GetXY(t.Value, Vector3.zero, cellSize, out int currentX, out int currentY); //TODO fix hardcoded origin

            //decrement based to position:
            //home -> decrement fatigue
            //park -> decrement sociality and sportivity
            //pub -> decrement hunger and sociality
            //road -> decrement sportivity
            switch (grid[currentX + currentY * width])
            {
                case TileMapEnum.TileMapSprite.Home:
                case TileMapEnum.TileMapSprite.Home2:
                    if (hc.homePosition.x == currentX && hc.homePosition.y == currentY){
                        if(needComponent.currentNeed== NeedType.needToRest)
                            hc.fatigue = Math.Max(0, hc.fatigue - (2f+1f) * deltaTime);
                        else if (needComponent.currentNeed == NeedType.needForFood)
                            hc.hunger = Math.Max(0, hc.hunger - (7f+1f) * deltaTime);
                    }
                    else
                        hc.sociality = Math.Max(0, hc.sociality - (5f+1f) * deltaTime);
                    break;
                case TileMapEnum.TileMapSprite.Park:
                    if (needComponent.currentNeed == NeedType.needForSport)
                        hc.sportivity = Math.Max(0, hc.sportivity - (15f+1f) * deltaTime);
                    else if (needComponent.currentNeed == NeedType.needForSociality)
                        hc.sociality = Math.Max(0, hc.sociality - (5f+1f) * deltaTime);
                    break;
                case TileMapEnum.TileMapSprite.Pub:
                    if (needComponent.currentNeed == NeedType.needForFood)
                        hc.hunger = Math.Max(0, hc.hunger - (7f+1f) * deltaTime);
                    else if (needComponent.currentNeed == NeedType.needForSociality)
                        hc.sociality = Math.Max(0, hc.sociality - (5f+1f) * deltaTime);
                    break;
                case TileMapEnum.TileMapSprite.Supermarket:
                    hc.grocery = Math.Max(0, hc.grocery - (3*24f+1f) * deltaTime);
                    break;
                case TileMapEnum.TileMapSprite.Office:
                    hc.work = Math.Max(0, hc.work - (2f+1f) * deltaTime);
                    break;

                case TileMapEnum.TileMapSprite.RoadHorizontal:
                case TileMapEnum.TileMapSprite.RoadVertical:
                case TileMapEnum.TileMapSprite.RoadCrossing:
          
                    break;
            }

            if (needComponent.currentNeed == NeedType.needForFood && hc.hunger < 25f* 7 * 0.6)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needToRest && hc.fatigue < 25f*17*0.6)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needForSport && hc.sportivity < 25f*23*0.6)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needForSociality && hc.sociality < 25f*11*0.6)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needForGrocery && hc.grocery < 25f * 25 *3 * 0.6)
                ecb.RemoveComponent<NeedComponent>(nativeThreadIndex, entity);
            else if (needComponent.currentNeed == NeedType.needToWork && hc.work < 25f * 17 * 0.6)
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