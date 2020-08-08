using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System;


public class UnitMoveOrderSystem : SystemBase {

	protected override void OnUpdate() {
		int range;
		int endX, endY;

		endX = endY = -1;

		EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

	    Entities.ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuffer, ref Translation translation, ref HumanComponent hc) => {
            if (!hc.goingToNeedPlace)
            {
                Testing.Instance.grid.GetXY(translation.Value, out int startX, out int startY);

                ValidateGridPosition(ref startX, ref startY);

                range = 2;
                FindTarget(startX, startY, ref endX, ref endY, hc.status, range);
                while (endX == -1 && endY == -1)
                {
                    range *= 2;
                    FindTarget(startX, startY, ref endX, ref endY, hc.status, range);
                }


                entityManager.AddComponentData(entity, new PathfindingParams
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(endX, endY)
                });
                hc.goingToNeedPlace = true;
            }
	    }).WithStructuralChanges().Run();
        
    }

    private void ValidateGridPosition(ref int x, ref int y) {
        x = math.clamp(x, 0, Testing.Instance.grid.GetWidth() - 1);
        y = math.clamp(y, 0, Testing.Instance.grid.GetHeight() - 1);
    }


	private void FindTarget(int startX, int startY, ref int endX, ref int endY, HumanComponent.need status, int range) {
		int i, j;

		endY = endX = -1;

		if (status == HumanComponent.need.needForFood)
		{
			for (i = startX - range; i < startX + range; i++) 
			{
				for (j = startY - range; j < startY + range; j++) 
				{
					if (i >= 0 && j >= 0 && i < Testing.Instance.grid.GetWidth() && j < Testing.Instance.grid.GetHeight() )
						if (Testing.Instance.grid.GetGridObject(i, j).GetTileType() == TileMapEnum.TileMapSprite.Supermarket) 
						{
							endX = i;
							endY = j;
							return;
						}
				}
			}
			return;
		}
		else if (status == HumanComponent.need.needForSport)
		{
			for (i = startX - range; i < startX + range; i++)
			{
				for (j = startY - range; j < startY + range; j++)
				{
					if (i >= 0 && j >= 0 && i < Testing.Instance.grid.GetWidth() && j < Testing.Instance.grid.GetHeight())
						if (Testing.Instance.grid.GetGridObject(i, j).GetTileType() == TileMapEnum.TileMapSprite.Park)
						{
							endX = i;
							endY = j;
							return;
						}
				}
			}
			return;
		}
		else if (status == HumanComponent.need.needForSociality)
		{
			for (i = startX - range; i < startX + range; i++)
			{
				for (j = startY - range; j < startY + range; j++)
				{
					if (i >= 0 && j >= 0 && i < Testing.Instance.grid.GetWidth() && j < Testing.Instance.grid.GetHeight())
						if (Testing.Instance.grid.GetGridObject(i, j).GetTileType() == TileMapEnum.TileMapSprite.Pub)
						{
							endX = i;
							endY = j;
							return;
						}
				}
			}
            return;
		}
		else if (status == HumanComponent.need.needToRest)
		{
			for (i = startX - range; i < startX + range; i++)
			{
				for (j = startY - range; j < startY + range; j++)
				{
					if (i >= 0 && j >= 0 && i < Testing.Instance.grid.GetWidth() && j < Testing.Instance.grid.GetHeight())
						if (Testing.Instance.grid.GetGridObject(i, j).GetTileType() == TileMapEnum.TileMapSprite.Home)
						{
							endX = i;
							endY = j;
							return;
						}
				}
			}
			return;
		}
		else {
			return;		
		}
	}
}
