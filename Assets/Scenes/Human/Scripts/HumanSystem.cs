using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using System;
using Unity.Mathematics;
using Unity.Collections;

public class HumanSystem : SystemBase
{
    protected override void OnUpdate(){
        float deltaTime = Time.DeltaTime;
        NativeArray<TileMapEnum.TileMapSprite> grid = Testing.Instance.grid.GetGridByValue((GridNode gn)=>{return gn.GetTileType();});
        float cellSize = Testing.Instance.grid.GetCellSize();
        int width = Testing.Instance.grid.GetWidth();

        JobHandle jobhandle = Entities.ForEach((ref Translation t, ref HumanComponent hc) =>{
            if(hc.hunger < 100f)
                hc.hunger += 1f * deltaTime;
            if (hc.fatigue < 100f)
                hc.fatigue += 1f * deltaTime;
            if (hc.sociality < 100f)
                hc.sociality += 1f * deltaTime;
            if (hc.sportivity < 100f)
                hc.sportivity += 1f * deltaTime;

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

            if (hc.hunger > 75f && hc.status == HumanComponent.need.none)
            {
                hc.status = HumanComponent.need.needForFood;
            }
            else if (hc.fatigue > 75f && hc.status == HumanComponent.need.none) 
            {
                hc.status = HumanComponent.need.needToRest;
            }
            else if (hc.sportivity > 75f && hc.status == HumanComponent.need.none)
            {
                hc.status = HumanComponent.need.needForSport;
            }
            else if (hc.sociality > 75f && hc.status == HumanComponent.need.none)
            {
                hc.status = HumanComponent.need.needForSociality;
            }

            if (hc.status!= HumanComponent.need.none)
            {
                if (hc.status == HumanComponent.need.needForFood && hc.hunger < 25f)
                    hc.status = HumanComponent.need.none;
                else if (hc.status == HumanComponent.need.needToRest && hc.fatigue < 25f)
                    hc.status = HumanComponent.need.none;
                else if (hc.status == HumanComponent.need.needForSport && hc.sportivity < 25f)
                    hc.status = HumanComponent.need.none;
                else if (hc.status == HumanComponent.need.needForSociality && hc.sociality < 25f)
                    hc.status = HumanComponent.need.none;
            }

        }).ScheduleParallel(this.Dependency);
        jobhandle.Complete();
        grid.Dispose();
    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y) {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }
}
