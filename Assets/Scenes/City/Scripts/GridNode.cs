using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapEnum;

public class GridNode {

    private Grid<GridNode> grid;
    private int x;
    private int y;

    private bool isWalkable;

    private TileMapSprite tileType;

    public GridNode(TileMapSprite tileType, Grid<GridNode> grid, int x, int y) {
        this.grid = grid;
        this.tileType = tileType;
        this.x = x;
        this.y = y;
        if(tileType==TileMapSprite.Road)
            isWalkable = true;
        else isWalkable = false;
    }

    public bool IsWalkable() {
        return isWalkable;
    }

    public void SetIsWalkable(bool isWalkable) {
        this.isWalkable = isWalkable;
        grid.TriggerGridObjectChanged(x, y);
    }

    public TileMapSprite GetTileType() {
        return tileType;
    }

}
