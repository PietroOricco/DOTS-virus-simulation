using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System;
using UnityEditor;
using Unity.Jobs;
using Unity.Collections;

[UpdateAfter(typeof(PathFollowSystem))]
public class GetNeedPathSystem : SystemBase {

	private float CellSize;
    private int Width;
	private int Height;
	private NativeArray<TileMapEnum.TileMapSprite> Grid;

	private EndSimulationEntityCommandBufferSystem ecbSystem;
 
    protected override void OnCreate(){
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

	protected override void OnStartRunning(){
        CellSize = Testing.Instance.grid.GetCellSize();
    	Width = Testing.Instance.grid.GetWidth();
		Height = Testing.Instance.grid.GetHeight();
		Grid = Testing.Instance.grid.GetGridByValue((GridNode gn)=>{return gn.GetTileType();});
    }

	protected override void OnUpdate() {
		var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

		var cellSize = this.CellSize;
		var height = this.Height;
		var width = this.Width;
		var grid = this.Grid;

		
		//int2 directions = new int2[] {(1, 0), (0, -1), (-1, 0), (0, 1)};
		NativeArray<int2> directions = new NativeArray<int2>(4, Allocator.Temp);
		directions.CopyFrom(new int2[] {new int2(1, 0), new int2(0, -1), new int2(-1, 0), new int2(0, 1)});
		//start_offset_x = ;
		//start_offset_y = new (int, int)[] {(-1, 1), (1, 1), (1, -1), (-1, -1)};
		NativeArray<int2> start_offset = new NativeArray<int2>(4, Allocator.Temp);
		start_offset.CopyFrom(new int2[] {new int2(-1, 1), new int2(1, 1), new int2(1, -1), new int2(-1, -1)});

		Unity.Mathematics.Random rnd = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0,420));
		
        JobHandle jobHandle = Entities.ForEach((Entity entity, int nativeThreadIndex, ref NeedPathParams needPathParams, in Translation translation, in NeedComponent needComponent) => {

			GetXY(translation.Value, Vector3.zero, cellSize, out int startX, out int startY);

			//FIXME validation removed!

			//int pos = FindTarget(startX, startY, hc.status, range, grid, width, height);

			int i, j, endX = -1, endY = -1;
			bool found = false;

			NativeArray<TileMapEnum.TileMapSprite> result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
			switch(needComponent.currentNeed){
				case NeedType.needForFood:
					result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
					result[0]=TileMapEnum.TileMapSprite.Supermarket;
					result[1]=TileMapEnum.TileMapSprite.Pub;
					break;
				case NeedType.needForSociality:
					result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
					result[0]=TileMapEnum.TileMapSprite.Park;
					result[1]=TileMapEnum.TileMapSprite.Pub;
					break;
				case NeedType.needForSport:
					result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
					result[0]=TileMapEnum.TileMapSprite.Park;
					break;
				case NeedType.needToRest:
					result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
					result[0]=TileMapEnum.TileMapSprite.Home;
					break;	
			}

			for(int l=0; l < result.Length; l++){
				if (result[l]==grid[startX + startY * width]){
					endX = startX;
					endY = startY;
					found = true;
				}
			}

			//TODO this could esplode... keep an eye on this
			for(int range = 1; !found; range++){
				//random number selection
				int starting_edge = rnd.NextInt(0, 4);
				int pos_step = rnd.NextInt(0, range*2+1);
				i = start_offset[starting_edge].x*range + directions[starting_edge].x*pos_step;
				j = start_offset[starting_edge].y*range + directions[starting_edge].y*pos_step;
				for(int turns = 0, tot_step = 0; turns<5&&tot_step<8*range; turns++){
					//TODO random starting step?
					var edge = (turns+starting_edge)%4;
					for(int steps = pos_step; steps<range*2+1-pos_step && !found; steps++){
						i += directions[edge].x;
						j += directions[edge].y;
						if(i>=0&&i<width&&j>=0&&j<height)
							for(int l=0; l < result.Length && !found; l++)
								if (result[l]==grid[i+j*width]){
									endX = i;
									endY = j;
									found = true;
								}
					}
					pos_step = 0;
				}
			}

			result.Dispose();

			ecb.RemoveComponent<NeedPathParams>(nativeThreadIndex, entity);

			ecb.AddComponent<PathfindingParams>(nativeThreadIndex , entity, new PathfindingParams{
				startPosition = new int2(startX, startY),
				endPosition = new int2(endX, endY)
			});
	    }).Schedule(Dependency);

        jobHandle.Complete();

		ecbSystem.AddJobHandleForProducer(jobHandle);
    }

	protected override void OnStopRunning(){
        Grid.Dispose();
    }

	private NativeArray<TileMapEnum.TileMapSprite> GetPlacesForStatus(NeedType currentNeed){
		NativeArray<TileMapEnum.TileMapSprite> result;
		switch(currentNeed){
			case NeedType.needForFood:
				result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
				result[0]=TileMapEnum.TileMapSprite.Supermarket;
				result[1]=TileMapEnum.TileMapSprite.Pub;
				return result;
			case NeedType.needForSociality:
				result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
				result[0]=TileMapEnum.TileMapSprite.Park;
				result[1]=TileMapEnum.TileMapSprite.Pub;
				return result;
			case NeedType.needForSport:
				result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
				result[0]=TileMapEnum.TileMapSprite.Park;
				return result;
			case NeedType.needToRest:
				result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
				result[0]=TileMapEnum.TileMapSprite.Home;
				return result;
			default:
				return new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
		}
	}


	private int FindTarget(int startX, int startY, NeedType status, int range, NativeArray<TileMapEnum.TileMapSprite> grid, int width, int height) {
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