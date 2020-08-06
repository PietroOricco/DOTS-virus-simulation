using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

public class PathFollowSystem : JobComponentSystem {

    private Unity.Mathematics.Random random;

    protected override void OnCreate() {
        random = new Unity.Mathematics.Random(56);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        float deltaTime = Time.DeltaTime;

        float cellSize = Testing.Instance.grid.GetCellSize();

        return Entities.ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuffer, ref Translation translation, ref PathFollow pathFollow) => {
            if (pathFollow.pathIndex >= 0) {
                // Has path to follow
                PathPosition pathPosition = pathPositionBuffer[pathFollow.pathIndex];

                float3 targetPosition = new float3(pathPosition.position.x*cellSize+cellSize*0.5f, pathPosition.position.y*cellSize+cellSize*0.5f, 0);
                float3 moveDir = math.normalizesafe(targetPosition - translation.Value);
                float moveSpeed = 3f;

                translation.Value += moveDir * moveSpeed * deltaTime;
                
                if (math.distance(translation.Value, targetPosition) < .1f) {
                    // Next waypoint
                    pathFollow.pathIndex--;
                }
            }
        }).Schedule(inputDeps);
    }

    private void ValidateGridPosition(ref int x, ref int y) {
        x = math.clamp(x, 0, Testing.Instance.grid.GetWidth() - 1);
        y = math.clamp(y, 0, Testing.Instance.grid.GetHeight() - 1);
    }

}

[UpdateAfter(typeof(PathFollowSystem))]
public class PathFollowGetNewPathSystem : JobComponentSystem {
    
    private Unity.Mathematics.Random random;

    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    protected override void OnCreate() {
        random = new Unity.Mathematics.Random(56);

        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        int mapWidth = Testing.Instance.grid.GetWidth();
        int mapHeight = Testing.Instance.grid.GetHeight();
        float3 originPosition = float3.zero;
        float cellSize = Testing.Instance.grid.GetCellSize();
        Unity.Mathematics.Random random = new Unity.Mathematics.Random(this.random.NextUInt(1, 10000));
        
        EntityCommandBuffer.Concurrent entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        JobHandle jobHandle = Entities.WithNone<PathfindingParams>().ForEach((Entity entity, int entityInQueryIndex, in PathFollow pathFollow, in Translation translation) => { 
            if (pathFollow.pathIndex == -1) {

                Debug.Log("Get new path");
                
                GetXY(translation.Value + new float3(1, 1, 0) * cellSize * +.5f, originPosition, cellSize, out int startX, out int startY);

                ValidateGridPosition(ref startX, ref startY, mapWidth, mapHeight);

                int endX = random.NextInt(0, mapWidth);
                int endY = random.NextInt(0, mapHeight);

                entityCommandBuffer.AddComponent(entityInQueryIndex, entity, new PathfindingParams { 
                    startPosition = new int2(startX, startY), endPosition = new int2(endX, endY) 
                });
            }
        }).Schedule(inputDeps);

        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }

    private static void ValidateGridPosition(ref int x, ref int y, int width, int height) {
        x = math.clamp(x, 0, width - 1);
        y = math.clamp(y, 0, height - 1);
    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y) {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }

}
