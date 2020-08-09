using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System;
using UnityEditor;
using Unity.Jobs;
using Unity.Collections;


[UpdateAfter(typeof(PathFollowSystem))]
public class UnitMoveOrderSystem : SystemBase {

	private EndSimulationEntityCommandBufferSystem ecbSystem;
 
    protected override void OnCreate(){
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

	protected override void OnUpdate() {
		var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

		var cellSize = Testing.Instance.grid.GetCellSize();
        var width = Testing.Instance.grid.GetWidth();
		var height = Testing.Instance.grid.GetHeight();
		var grid = Testing.Instance.grid.GetGridByValue((GridNode gn)=>{return gn.GetTileType();});
        JobHandle jobHandle = Entities.ForEach((Entity entity, int nativeThreadIndex, DynamicBuffer <PathPosition> pathPositionBuffer, ref PathFollow pathFollow, ref Translation translation, ref HumanComponent hc) => {
            if(pathFollow.pathIndex==-1){
				int range = 2;

                GetXY(translation.Value, Vector3.zero, cellSize, out int startX, out int startY);

				//FIXME validation removed!

                //int pos = FindTarget(startX, startY, hc.status, range, grid, width, height);

				int i, j, endX = -1, endY = -1;
				bool found = false;

				NativeArray<TileMapEnum.TileMapSprite> result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
				switch(hc.status){
					case HumanComponent.need.needForFood:
						result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
						result[0]=TileMapEnum.TileMapSprite.Supermarket;
						result[1]=TileMapEnum.TileMapSprite.Pub;
						break;
					case HumanComponent.need.needForSociality:
						result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
						result[0]=TileMapEnum.TileMapSprite.Park;
						result[1]=TileMapEnum.TileMapSprite.Pub;
						break;
					case HumanComponent.need.needForSport:
						result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
						result[0]=TileMapEnum.TileMapSprite.Park;
						break;
					case HumanComponent.need.needToRest:
						result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
						result[0]=TileMapEnum.TileMapSprite.Home;
						break;	
				}


                if (ArrayUtility.Contains(result.ToArray(), grid[startX + startY * width])){
                    endX = startX;
                    endY = startY;
                    found = true;
                }

                for (i = startX - range; i < startX + range && !found ; i++) {
					for (j = startY - range; j < startY + range && !found; j++) {
						if (i >= 0 && j >= 0 && i < width && j < height )
							if (ArrayUtility.Contains(result.ToArray(), grid[i+j*width])){
								endX = i;
								endY = j;
								found = true;
							}
					}
				}

                for(int tmax = 0; tmax < 5 && !found; tmax++){
                    range *= 2;
                    //pos = FindTarget(startX, startY, hc.status, range, grid, width, height);
					for (i = startX - range; i < startX + range && !found ; i++) {
						for (j = startY - range; j < startY + range && !found; j++) {
							if (i >= 0 && j >= 0 && i < width && j < height )
								if (ArrayUtility.Contains(result.ToArray(), grid[i+j*width])){
									endX = i;
									endY = j;
									found = true;
								}
						}
					}
                }


				ecb.AddComponent<PathfindingParams>(nativeThreadIndex , entity, new PathfindingParams{
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(endX, endY)
                });
            }
	    }).Schedule(Dependency);

        jobHandle.Complete();
        grid.Dispose();
    }

	private NativeArray<TileMapEnum.TileMapSprite> GetPlacesForStatus(HumanComponent.need status){
		NativeArray<TileMapEnum.TileMapSprite> result;
		switch(status){
			case HumanComponent.need.needForFood:
				result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
				result[0]=TileMapEnum.TileMapSprite.Supermarket;
				result[1]=TileMapEnum.TileMapSprite.Pub;
				return result;
			case HumanComponent.need.needForSociality:
				result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
				result[0]=TileMapEnum.TileMapSprite.Park;
				result[1]=TileMapEnum.TileMapSprite.Pub;
				return result;
			case HumanComponent.need.needForSport:
				result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
				result[0]=TileMapEnum.TileMapSprite.Park;
				return result;
			case HumanComponent.need.needToRest:
				result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
				result[0]=TileMapEnum.TileMapSprite.Home;
				return result;
			default:
				return new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
		}
	}


	private int FindTarget(int startX, int startY, HumanComponent.need status, int range, NativeArray<TileMapEnum.TileMapSprite> grid, int width, int height) {
		int i, j;

		for (i = startX - range; i < startX + range; i++) {
			for (j = startY - range; j < startY + range; j++) {
				if (i >= 0 && j >= 0 && i < width && j < height )
					//if (ArrayUtility.Contains(GetPlacesForStatus(status), grid[i+j*width])){
						return i+j*width;
					//}
			}
		}
		return -1;
	}

	private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y) {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }
}