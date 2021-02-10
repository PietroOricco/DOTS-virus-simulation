using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System;
using UnityEditor;
using Unity.Jobs;
using Unity.Collections;

[UpdateAfter(typeof(ContagionSystem))]
[UpdateAfter(typeof(PathFollowSystem))]
public class GetNeedPathSystem : SystemBase {

	private float CellSize;
    private int Width;
	private int Height;
	private NativeArray<TileMapEnum.TileMapSprite> Grid;

	private NativeArray<int2> directions;
	private NativeArray<int2> start_offset;
	private Unity.Mathematics.Random rnd;

	private EndSimulationEntityCommandBufferSystem ecbSystem;
	private NativeArray<Vector2Int> myHouses;
 
    protected override void OnCreate(){
        ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

	protected override void OnStartRunning(){
        CellSize = Testing.Instance.grid.GetCellSize();
    	Width = Testing.Instance.grid.GetWidth();
		Height = Testing.Instance.grid.GetHeight();
		Grid = Testing.Instance.grid.GetGridByValue((GridNode gn)=>{return gn.GetTileType();});
		directions = new NativeArray<int2>(4, Allocator.Persistent);
		directions.CopyFrom(new int2[] {new int2(1, 0), new int2(0, -1), new int2(-1, 0), new int2(0, 1)});
		start_offset = new NativeArray<int2>(4, Allocator.Persistent);
		start_offset.CopyFrom(new int2[] {new int2(-1, 1), new int2(1, 1), new int2(1, -1), new int2(-1, -1)});
		rnd = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1,420));
		myHouses = Human.Instance.houses;
    }

	protected override void OnUpdate() {
		var ecb = ecbSystem.CreateCommandBuffer().ToConcurrent();

		var cellSize = this.CellSize;
		var height = this.Height;
		var width = this.Width;
		var grid = this.Grid;
		var rnd = this.rnd;
		var start_offset = this.start_offset;
		var directions = this.directions;

		var houses = myHouses;
		var lockdown = Human.conf.lockdown;
		
        JobHandle jobHandle = Entities.ForEach((Entity entity, int nativeThreadIndex, ref NeedPathParams needPathParams, in Translation translation, in NeedComponent needComponent, in HumanComponent humanComponent) => {

			GetXY(translation.Value, Vector3.zero, cellSize, out int startX, out int startY);

			//FIXME validation removed!

			//int pos = FindTarget(startX, startY, hc.status, range, grid, width, height);

			int i, j, endX = -1, endY = -1;
			bool found = false;

			NativeArray<TileMapEnum.TileMapSprite> result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
			switch(needComponent.currentNeed){
				case NeedType.needForFood:
					if(rnd.NextDouble()<0.20&&!lockdown){
						result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
						result[0]=TileMapEnum.TileMapSprite.Pub;
					}
					else{
						// Go home to eat most of the times
						result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
						found = true;
						endX = humanComponent.homePosition.x;
						endY = humanComponent.homePosition.y;
					}
					break;
				case NeedType.needForGrocery:
					result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
					result[0]=TileMapEnum.TileMapSprite.Supermarket;
					break;
				case NeedType.needForSociality:
					if((rnd.NextDouble()<0.20&&!lockdown)||(rnd.NextDouble()<0.20*humanComponent.socialResposibility&&lockdown)){
						result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
						found = true;
						int friendIndex;
						do{
							friendIndex = rnd.NextInt(0, houses.Length);
						}while(houses[friendIndex].x==humanComponent.homePosition.x||houses[friendIndex].y==humanComponent.homePosition.y);
						endX = houses[friendIndex].x;
						endY = houses[friendIndex].y;
					}
					else if(!lockdown){
						result = new NativeArray<TileMapEnum.TileMapSprite>(2, Allocator.Temp);
						result[0]=TileMapEnum.TileMapSprite.Park;
						result[1]=TileMapEnum.TileMapSprite.Pub;
					}
					else{
						result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
						result[0]=TileMapEnum.TileMapSprite.Park;
					}

					break;
				case NeedType.needForSport:
					result = new NativeArray<TileMapEnum.TileMapSprite>(1, Allocator.Temp);
					result[0]=TileMapEnum.TileMapSprite.Park;
					break;
				case NeedType.needToRest:
					result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
					found = true;
					endX = humanComponent.homePosition.x;
					endY = humanComponent.homePosition.y;
					break;
				case NeedType.needToWork:
					result = new NativeArray<TileMapEnum.TileMapSprite>(0, Allocator.Temp);
					found = true;
					endX = humanComponent.officePosition.x;
					endY = humanComponent.officePosition.y;
					break;
			}

			for(int l=0; l < result.Length && !found; l++){
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
				i = startX + start_offset[starting_edge].x*range + directions[starting_edge].x*pos_step;
				j = startY + start_offset[starting_edge].y*range + directions[starting_edge].y*pos_step;
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
	    }).WithReadOnly(houses).ScheduleParallel(Dependency);

        jobHandle.Complete();

		ecbSystem.AddJobHandleForProducer(jobHandle);
    }

	protected override void OnStopRunning(){
        Grid.Dispose();
		directions.Dispose();
		start_offset.Dispose();
    }

	private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y) {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }
}