using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class UnitMoveOrderSystem : SystemBase {

    protected override void OnUpdate() {
        if (Input.GetMouseButtonDown(0)) {
	        Vector3 mousePosition = new Vector3(0,0,0);

	        float cellSize = Testing.Instance.grid.GetCellSize();

	        Testing.Instance.grid.GetXY(mousePosition + new Vector3(1, 1) * cellSize * +.5f, out int endX, out int endY);
	
	        ValidateGridPosition(ref endX, ref endY);
			endX = 1;
			endY = 1;

			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

	        Entities.ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuffer, ref Translation translation) => {
		        Testing.Instance.grid.GetXY(translation.Value + new float3(1, 1, 0) * cellSize * +.5f, out int startX, out int startY);

		        ValidateGridPosition(ref startX, ref startY);

		        entityManager.AddComponentData(entity, new PathfindingParams { 
			        startPosition = new int2(startX, startY), 
                    endPosition = new int2(endX, endY) 
		        });
	        }).WithStructuralChanges().Run();
        }
    }

    private void ValidateGridPosition(ref int x, ref int y) {
        x = math.clamp(x, 0, Testing.Instance.grid.GetWidth() - 1);
        y = math.clamp(y, 0, Testing.Instance.grid.GetHeight() - 1);
    }

}
